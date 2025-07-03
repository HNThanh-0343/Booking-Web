using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Linq;
using System.Text.Json;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Areas.Partner.Controllers
{
    [Area("partner")]
    [Auth]
    public class datphongController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public datphongController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        public IActionResult Index()
        {
            var getAccount = Account.GetAccount();

            // Lấy danh sách khách sạn của user
            var getHotel = _unitOfWork.Repository<SysHotel>()
                            .GetAll(h => h.IdUser == getAccount.Id && h.Status == true);
            ViewBag.getTypeRoom = _unitOfWork.Repository<CatTypeRoom>()
                            .GetAll(h => h.Status == true);

            ViewBag.ListHotel = getHotel;
            return View();
        }

        #region các hàm xử lý đặt phòng
        // Đây là hàm chính cho lễ tân xử lý mọi tình huống
        public IActionResult ChildIndex(int? page, string IdTypeRoom, string IdHotel, DateTime? DateCheckIn, DateTime? DateCheckOut, int? GuestNumber, int? TypeRoom, int adult, int child)
        {
            var getAccount = Account.GetAccount();
            var now = DateTime.Now;
            var getHotel = new List<SysHotel>();
            if (getAccount.PartnerId != null)
            {
                getHotel = _unitOfWork.Repository<SysHotel>()
                            .GetAll(h => h.Status == true) // lọc các hotel có Status true
                            .AsEnumerable() // chuyển sang LINQ in-memory
                            .Where(h => (h.ListManagerId ?? "")
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(id => id.Trim())
                                .Contains(getAccount.Id.ToString()))
                            .ToList();
            }
            else
            {
                getHotel = _unitOfWork.Repository<SysHotel>()
                           .GetAll(h => h.IdUser == getAccount.Id && h.Status == true).ToList();
            }
            // lấy ra tất cả hotel của người dùng quản lý

            // 1. Load dữ liệu phòng và booking hiện tại
            var listRoom = GetRooms(getHotel, IdHotel); // Bao gồm TypeRoomNavigation
            if (adult > 0)// tìm kiếm trường hợp nhập người lớn và trẻ em
            {
                listRoom = listRoom.Where(m => m.AdultsMax >= adult && m.ChildrenMax >= child).ToList();
            }
            var listRoomIds = listRoom.Select(r => r.Id).ToList();
            var allBookings = GetAllBookings(listRoomIds);
            ViewBag.ListHotel = getHotel;
            // Nếu không chọn ngày, xử lý như khách muốn ở ngay
            if (!DateCheckIn.HasValue || !DateCheckOut.HasValue)
            {
                var roomStatusList = HandleRealtimeBooking(now, listRoom, allBookings);
                // Pagination
                page = page ?? 1;
                page = page < 1 ? 1 : page;
                var pageSize = 16;
                var pageListView = roomStatusList.ToPagedList(page.Value, pageSize);
                return PartialView("childIndex", pageListView);

            }

            // Kiểm tra xem khoảng thời gian có booking phù hợp không
            var availableRooms = GetAvailableRooms(DateCheckIn.Value, DateCheckOut.Value, listRoom, allBookings);

            if (availableRooms.Any())
            {
                // Có phòng trống toàn thời gian => xử lý OK
                var roomStatusList = HandleDateRangeInquiry(DateCheckIn.Value, DateCheckOut.Value, availableRooms, allBookings);
                // Pagination
                page = page ?? 1;
                page = page < 1 ? 1 : page;
                var pageSize = 16;
                var pageListView = roomStatusList.ToPagedList(page.Value, pageSize);
                return PartialView("childIndex", pageListView);
            }

            // Kiểm tra xem có phòng sắp checkout không => khách có thể chờ
            var waitingSuggestion = TrySuggestWaiting(DateCheckIn.Value, DateCheckOut.Value, listRoom, allBookings);
            if (waitingSuggestion != null)
            {
                page = page ?? 1;
                page = page < 1 ? 1 : page;
                var pageSize = 16;
                var pageListView = waitingSuggestion.ToPagedList(page.Value, pageSize);
                return PartialView("childIndex", pageListView);
            }

            // Kiểm tra xem có phương án chuyển phòng 1-2 lần
            var roomSplitOption = TrySplitRoom(DateCheckIn.Value, DateCheckOut.Value, listRoom, allBookings);
            if (roomSplitOption.Any())
            {
                ViewBag.Note = "**Không có 1 phòng trọn gói, nhưng có thể chia làm nhiều phòng tương đương hoặc tốt hơn.**";

                var roomStatusList = HandleSplitSuggestions(roomSplitOption); // chuyển về List<RoomStatusDto>

                // Pagination
                page = page ?? 1;
                page = page < 1 ? 1 : page;
                var pageSize = 16;
                var pageListView = roomStatusList.ToPagedList(page.Value, pageSize);

                return PartialView("childIndex", pageListView);
            }

            var ListAllNull = new List<RoomStatusDto>();
            page = page ?? 1;
            page = page < 1 ? 1 : page;
            var pageSizeAll = 16;
            var pageListViewAll = ListAllNull.ToPagedList(page.Value, pageSizeAll);
            // Nếu đến đây thì không có phương án nào
            return PartialView("childIndex", pageListViewAll);
        }

        // =======================
        // CÁC HÀM CON NGHIỆP VỤ
        // =======================

        private List<RoomStatusDto> HandleRealtimeBooking(DateTime now, List<SysRoom> listRoom, List<SysBooking> allBookings)
        {
            var result = new List<RoomStatusDto>();
            var today = now.Date;

            foreach (var room in listRoom)
            {
                int total = room.TotalRoom ?? 0;

                // Lọc các booking còn hiệu lực của phòng
                var roomBookings = allBookings
                    .Where(b => b.BookingItemId == room.Id && b.Status != 0)
                    .OrderBy(b => b.StartDate)
                    .ToList();

                // Booking đang chiếm phòng (kể cả quá hạn chưa checkout)
                var activeBookings = roomBookings
                    .Where(b => b.EndDate >= now || b.EndDate < now)
                    .ToList();

                int booked = activeBookings.Count;
                bool isFullyBooked = booked >= total;

                // Kiểm tra có booking nào checkout đúng hôm nay
                var bookingCheckoutToday = activeBookings
                    .FirstOrDefault(b => b.EndDate.Date == today);

                // Booking đại diện để hiển thị thông tin (ưu tiên người sắp checkout hôm nay)
                var representativeBooking = bookingCheckoutToday ??
                    activeBookings.OrderBy(b => b.StartDate).FirstOrDefault();

                // Logic xác định trạng thái
                bool soonCheckout = isFullyBooked && bookingCheckoutToday != null;
                bool isBooked = isFullyBooked && !soonCheckout;
                int available = isFullyBooked ? 0 : Math.Max(0, total - booked);

                result.Add(new RoomStatusDto
                {
                    RoomId = room.Id,
                    IdHotel = room.IdHotel,
                    Name = $"{room.Name} ({available} trống)",

                    TypeRoom = room.TypeRoom,
                    TypeRoomName = room.TypeRoomNavigation?.Name,
                    Description = room.Description,
                    AdultsMax = room.AdultsMax,
                    ChildrenMax = room.ChildrenMax,
                    Floor = room.Floor,
                    NumRoom = room.NumRoom,
                    ListImg = room.ListImg,
                    ListAminities = room.ListAminities,
                    Price = room.Price,

                    IsBooked = isBooked,
                    BookingInfo = representativeBooking,
                    SoonCheckout = soonCheckout,
                    FromDate = representativeBooking?.StartDate,
                    ToDate = representativeBooking?.EndDate,
                    IsSplitGroupHeader = false,

                    TotalRoom = total,
                    BookedRoom = booked,
                    AvailableRoom = available
                });
            }

            return result;
        }






        private List<RoomStatusDto> HandleDateRangeInquiry(DateTime checkIn, DateTime checkOut, List<SysRoom> allRooms, List<SysBooking> allBookings)
        {
            ViewBag.Note = "Có phòng trống từ ngày yêu cầu.";
            DateTime now = DateTime.Now;

            var result = new List<RoomStatusDto>();

            foreach (var room in allRooms)
            {
                int total = room.TotalRoom ?? 1;

                var roomBookings = allBookings
                    .Where(b => b.BookingItemId == room.Id && b.Status != 0)
                    .ToList();

                // Tính số booking conflict (chiếm phòng)
                int conflictCount = roomBookings.Count(b =>
                {
                    DateTime realEnd = b.EndDate < now ? now : b.EndDate;
                    return b.StartDate < checkOut && realEnd > checkIn;
                });

                int available = Math.Max(0, total - conflictCount);

                // Kiểm tra có booking nào checkout đúng ngày checkin
                bool hasCheckoutSameDay = roomBookings.Any(b =>
                {
                    DateTime realEnd = b.EndDate < now ? now : b.EndDate;
                    return realEnd.Date == checkIn.Date;
                });

                result.Add(new RoomStatusDto
                {
                    RoomId = room.Id,
                    IdHotel = room.IdHotel,
                    Name = room.Name + $" ({available} trống)",

                    TypeRoom = room.TypeRoom,
                    TypeRoomName = room.TypeRoomNavigation?.Name,
                    Description = room.Description,
                    AdultsMax = room.AdultsMax,
                    ChildrenMax = room.ChildrenMax,
                    Floor = room.Floor,
                    NumRoom = room.NumRoom,
                    ListImg = room.ListImg,
                    ListAminities = room.ListAminities,
                    Price = room.Price,

                    IsBooked = available <= 0,
                    BookingInfo = null,
                    SoonCheckout = hasCheckoutSameDay,
                    IsInterrupted = false,

                    TotalRoom = total,
                    BookedRoom = conflictCount,
                    AvailableRoom = available
                });
            }

            return result.Where(r => r.AvailableRoom > 0).ToList(); // Trả về những loại phòng còn ít nhất 1 slot
        }


        private List<List<RoomStatusDto>> TrySplitRoom(DateTime checkIn, DateTime checkOut, List<SysRoom> rooms, List<SysBooking> bookings)
        {
            var requiredMinutes = (checkOut - checkIn).TotalMinutes;
            var result = new List<List<RoomStatusDto>>();

            foreach (var roomA in rooms)
            {
                var bookingsA = bookings
                    .Where(b => b.BookingItemId == roomA.Id)
                    .OrderBy(b => b.StartDate)
                    .ToList();

                var freeSlotsA = GetFreeSlots(checkIn, checkOut, bookingsA);

                double coveredA = 0;
                DateTime cursor = checkIn;

                foreach (var slotA in freeSlotsA)
                {
                    double slotMinutes = (slotA.End - slotA.Start).TotalMinutes;
                    coveredA += slotMinutes;
                    cursor = slotA.End;

                    if (coveredA >= requiredMinutes / 3) // ít nhất 1/3
                    {
                        foreach (var roomB in rooms.Where(r => r.Id != roomA.Id))
                        {
                            if (roomB.TypeRoom < roomA.TypeRoom || roomB.Price < roomA.Price)
                                continue; // bỏ qua nếu phòng kém hơn

                            var bookingsB = bookings
                                .Where(b => b.BookingItemId == roomB.Id)
                                .OrderBy(b => b.StartDate)
                                .ToList();

                            var freeSlotsB = GetFreeSlots(cursor, checkOut, bookingsB);
                            double coveredB = freeSlotsB.Sum(s => (s.End - s.Start).TotalMinutes);

                            if (coveredA + coveredB >= requiredMinutes)
                            {
                                var group = new List<RoomStatusDto>
                        {
                            new RoomStatusDto
                            {
                                RoomId = roomA.Id,
                                IdHotel = roomA.IdHotel,
                                Name = roomA.Name,
                                TypeRoom = roomA.TypeRoom,
                                TypeRoomName = roomA.TypeRoomNavigation?.Name,
                                Description = roomA.Description,
                                AdultsMax = roomA.AdultsMax,
                                ChildrenMax = roomA.ChildrenMax,
                                Floor = roomA.Floor,
                                NumRoom = roomA.NumRoom,
                                ListImg = roomA.ListImg,
                                ListAminities = roomA.ListAminities,
                                Price = roomA.Price,
                                IsBooked = false,
                                BookingInfo = null,
                                SoonCheckout = false,
                                IsInterrupted = true,
                                FromDate = checkIn,
                                ToDate = cursor
                            },
                            new RoomStatusDto
                            {
                                RoomId = roomB.Id,
                                IdHotel = roomB.IdHotel,
                                Name = roomB.Name,
                                TypeRoom = roomB.TypeRoom,
                                TypeRoomName = roomB.TypeRoomNavigation?.Name,
                                Description = roomB.Description,
                                AdultsMax = roomB.AdultsMax,
                                ChildrenMax = roomB.ChildrenMax,
                                Floor = roomB.Floor,
                                NumRoom = roomB.NumRoom,
                                ListImg = roomB.ListImg,
                                ListAminities = roomB.ListAminities,
                                Price = roomB.Price,
                                IsBooked = false,
                                BookingInfo = null,
                                SoonCheckout = false,
                                IsInterrupted = true,
                                FromDate = cursor,
                                ToDate = checkOut
                            }
                        };

                                result.Add(group);
                            }
                        }
                    }
                }
            }

            return result;
        }


        private List<RoomStatusDto> HandleSplitSuggestions(List<List<RoomStatusDto>> suggestions)
        {
            var result = new List<RoomStatusDto>();
            int groupNumber = 1;

            foreach (var group in suggestions)
            {
                //// Thêm dòng tiêu đề ngăn cách
                //result.Add(new RoomStatusDto
                //{
                //    Name = $"-- Gợi ý chia phòng #{groupNumber}: --",
                //    IsSplitGroupHeader = true // dùng trong View để render khác
                //});

                result.AddRange(group);
                groupNumber++;
            }

            return result;
        }

        private List<RoomStatusDto> TrySuggestWaiting(DateTime checkIn, DateTime checkOut, List<SysRoom> rooms, List<SysBooking> bookings)
        {
            var result = new List<RoomStatusDto>();

            foreach (var room in rooms)
            {
                var roomBookings = bookings
                    .Where(b => b.BookingItemId == room.Id)
                    .OrderBy(b => b.StartDate)
                    .ToList();

                // Lấy booking hiện tại hoặc gần nhất
                //var lastBooking = roomBookings.FirstOrDefault(b => b.EndDate > DateTime.Now && b.EndDate <= checkIn);
                var lastBooking = roomBookings.FirstOrDefault(b =>
                                    b.EndDate >= DateTime.Now &&
                                    b.EndDate <= checkIn.AddHours(4)
                                );

                if (lastBooking != null)
                {
                    result.Add(new RoomStatusDto
                    {
                        RoomId = room.Id,
                        IdHotel = room.IdHotel,
                        Name = room.Name,
                        TypeRoom = room.TypeRoom,
                        TypeRoomName = room.TypeRoomNavigation?.Name,
                        Description = room.Description,
                        AdultsMax = room.AdultsMax,
                        ChildrenMax = room.ChildrenMax,
                        Floor = room.Floor,
                        NumRoom = room.NumRoom,
                        ListImg = room.ListImg,
                        ListAminities = room.ListAminities,
                        Price = room.Price,

                        IsBooked = true,
                        BookingInfo = lastBooking,
                        SoonCheckout = true,
                        IsInterrupted = false
                    });
                }
            }

            return result;
        }


        // ============================
        // HÀM HỖ TRỢ
        // ============================
        private List<(DateTime Start, DateTime End)> GetFreeSlots(DateTime from, DateTime to, List<SysBooking> bookings)
        {
            var slots = new List<(DateTime, DateTime)>();
            var cursor = from;

            foreach (var b in bookings)
            {
                if (cursor < b.StartDate)
                {
                    slots.Add((cursor, b.StartDate));
                }
                if (b.EndDate > cursor)
                {
                    cursor = b.EndDate;
                }
            }

            if (cursor < to)
            {
                slots.Add((cursor, to));
            }

            return slots;
        }

        private List<SysRoom> GetRooms(List<SysHotel> getHotel, string IdHotel)
        {

            // lọc để lấy Id của hotel
            var getIdHotel = getHotel.Select(h => h.Id).Distinct().ToList();           
            List<SysRoom> listRoom;

            if (string.IsNullOrEmpty(IdHotel))
            {
                listRoom = _unitOfWork.Repository<SysRoom>().GetAll(
                    r => r.IdHotel.HasValue && getIdHotel.Contains(r.IdHotel.Value) && r.Status == true,
                    includeProperties: "TypeRoomNavigation"
                )
                .Select(r =>
                {
                    r.ListImg = CheckIMGServer.TimAnhKhongTonTai(r.ListImg);
                    return r;
                })
                .ToList();
            }
            else
            {
                int idHotelInt = Convert.ToInt32(IdHotel);

                listRoom = _unitOfWork.Repository<SysRoom>().GetAll(
                    r => r.IdHotel == idHotelInt && r.Status == true,
                    includeProperties: "TypeRoomNavigation"
                )
                .Select(r =>
                {
                    r.ListImg = CheckIMGServer.TimAnhKhongTonTai(r.ListImg);
                    return r;
                })
                .ToList();
            }
            return listRoom;
        }

        private List<SysBooking> GetAllBookings(List<int> listRoomIds)
        {
            var allBookings = _unitOfWork.Repository<SysBooking>().GetAll(b => b.IdCategories == 1 &&
                b.Status != 0 &&
                listRoomIds.Contains(b.BookingItemId)).ToList();
            return allBookings;
        }

        private List<SysRoom> GetAvailableRooms(DateTime checkIn, DateTime checkOut, List<SysRoom> rooms, List<SysBooking> bookings)
        {
            DateTime now = DateTime.Now;

            return rooms.Where(room =>
            {
                int total = room.TotalRoom ?? 1;

                // Lấy toàn bộ booking còn hiệu lực của phòng đó
                var roomBookings = bookings
                    .Where(b => b.BookingItemId == room.Id && b.Status != 0);

                // Conflict: nếu thời gian giao nhau (đang chiếm phòng) ≥ total phòng, thì hết phòng
                int conflictCount = roomBookings.Count(b =>
                {
                    DateTime realEndDate = b.EndDate < now ? now : b.EndDate;
                    return b.StartDate < checkOut && realEndDate > checkIn;
                });

                return conflictCount < total;
            }).ToList();
        }


        #endregion        

        public IActionResult themMoiKhachSan()
        {
            try
            {
                SysHotel sysHotel = new SysHotel();
                return PartialView("themMoiKhachSan", sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult themMoiKhachSan(SysHotel sysHotel, List<IFormFile> postedFile)
        {
            try
            {
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    return PartialView("themMoiKhachSan", sysHotel);
                }
                sysHotel.IdUser = getAccount.Id;
                if (sysHotel.Name == null || sysHotel.Local == null)
                {
                    //TempData["ErrorMessage"] = "Thêm mới thất bại!";
                    return PartialView("themMoiKhachSan", sysHotel);
                }
                if (!ModelState.IsValid)
                {
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var key = modelStateKey;
                            var errorMessage = error.ErrorMessage;
                            ModelState.AddModelError(key, errorMessage);
                            return BadRequest(ModelState);
                        }
                    }
                    return PartialView("themMoiKhachSan", sysHotel);
                }
                sysHotel.TimeCreate = DateTime.Now;
                _unitOfWork.Repository<SysHotel>().Insert(sysHotel);

                // Lưu ảnh
                if (postedFile != null)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Hotel\\" + sysHotel.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysHotel.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ", ";
                        }
                    }

                    sysHotel.ListImg = listIMG;
                    _unitOfWork.Repository<SysHotel>().Update(sysHotel);
                }
                return PartialView(sysHotel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult chinhSuaKhachSan(int id)
        {
            try
            {
                var sysHotel = _unitOfWork.Repository<SysHotel>().GetById(id);
                if (sysHotel == null)
                {
                    return PartialView("Index");
                }
                return PartialView("chinhSuaKhachSan", sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaKhachSan(SysHotel sysHotel, List<IFormFile> postedFile)
        {
            try
            {
                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(sysHotel.Id);
                if (getHotel == null)
                {
                    //TempData["ErrorMessage"] = "Chỉnh sửa thất bại!";
                    return PartialView("chinhSuaTaiKhoan", getHotel);
                }

                if (!ModelState.IsValid)
                {
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var key = modelStateKey;
                            var errorMessage = error.ErrorMessage;
                            ModelState.AddModelError(key, errorMessage);
                            return BadRequest(ModelState);
                        }
                    }
                    return PartialView("chinhSuaKhachSan", sysHotel);
                }

                // Lưu avatar
                if (postedFile != null)
                {
                    string wwwPath = this.Environment.WebRootPath;
                    var urlImg = "\\AppData\\Hotel\\" + getHotel.Id + "\\";
                    var path = wwwPath + urlImg;
                    var listIMG = "";
                    foreach (var itemFile in postedFile)
                    {
                        var pathdefault = Common.SaveUrlImg(sysHotel.Id, wwwPath, urlImg, itemFile);
                        if (pathdefault != null)
                        {
                            listIMG += pathdefault + ", ";
                        }
                    }
                    getHotel.ListImg = listIMG;
                }

                getHotel.Name = sysHotel.Name;
                getHotel.Description = sysHotel.Description;
                getHotel.Local = sysHotel.Local;
                getHotel.Amenities = sysHotel.Amenities;
                getHotel.Featured = sysHotel.Featured;
                getHotel.Status = sysHotel.Status;
                getHotel.Phone = sysHotel.Phone;

                _unitOfWork.Repository<SysHotel>().Update(getHotel);

                //TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
                return PartialView(sysHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietphong(int id)
        {
            try
            {
                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(id);
                ViewBag.GetAminities = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: (m => m.Status == true));
                if (getRoom == null)
                {
                    return PartialView("Index");
                }
                if (getRoom != null)
                {
                    getRoom.ListImg = CheckIMGServer.TimAnhKhongTonTai(getRoom.ListImg, 2); // truyền typeReturn = 2
                }
                return PartialView(getRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaKhachSan(int id)
        {
            try
            {
                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(id);
                if (getHotel == null)
                {
                    return Json(new { result = false, message = "Không tìm thấy khách sạn" });
                }
                // Xóa ảnh
                if (!string.IsNullOrEmpty(getHotel.ListImg))
                {
                    var listIMG = getHotel.ListImg.Split(',');
                    string wwwPath = this.Environment.WebRootPath;

                    foreach (var img in listIMG)
                    {
                        var fullPath = Path.Combine(wwwPath, img.TrimStart('\\').Replace("/", "\\"));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
                _unitOfWork.Repository<SysHotel>().Delete(getHotel);
                //TempData["SuccessMessage"] = "Xóa thành công!";
                return Json(new { result = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });
            }
        }

        public IActionResult XoaAnh(string imagePath, int hotelId)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { result = false, message = "Đường dẫn ảnh không hợp lệ." });
                }

                var getHotel = _unitOfWork.Repository<SysHotel>().GetById(hotelId);
                if (getHotel == null)
                {
                    return Json(new { result = false, message = "Lỗi khi xóa ảnh." });
                }

                string trimmedPath = imagePath.Trim().TrimStart('/', '\\');
                var rootPath = Directory.GetCurrentDirectory();
                var fullPath = Path.Combine(rootPath, "wwwroot", trimmedPath);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);

                    // Cập nhật lại list ảnh
                    if (!string.IsNullOrEmpty(getHotel.ListImg))
                    {
                        var newList = getHotel.ListImg.Split(',').Where(h => h.Trim() != imagePath.Trim()).ToArray();
                        getHotel.ListImg = string.Join(",", newList);
                        _unitOfWork.Repository<SysHotel>().Update(getHotel);
                    }
                    return Json(new { result = true });
                }
                else
                {
                    return Json(new { result = false, message = "Ảnh không tồn tại." });
                }
            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa ảnh." });
            }
        }
        public IActionResult datphong(int id)
        {
            try
            {
                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(id);
                //ViewBag.GetAminities = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: (m => m.Status == true));
                if (getRoom == null)
                {
                    return PartialView("Index");
                }
                return PartialView(getRoom);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult datphong(BookRoomNoUser bookRoomNoUser)
        {
            try
            {
                //var getRoom = _unitOfWork.Repository<SysRoom>().GetById(id);
                //ViewBag.GetAminities = _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: (m => m.Status == true));
                //if (getRoom == null)
                //{
                //    return PartialView("Index");
                //}
                if (!ModelState.IsValid)
                {
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var key = modelStateKey;
                            var errorMessage = error.ErrorMessage;
                            ModelState.AddModelError(key, errorMessage);
                            return ValidationProblem(ModelState);
                        }
                    }
                    return PartialView("datphong", bookRoomNoUser);
                }
                // kiểm tra xem, lần đặt này có bị trùng hay bị dính gì với ngày đặt ngày checkout của booking khác
                // tiến hành lấy hết booking có id phòng và idcate
                var getbooking = _unitOfWork.Repository<SysBooking>().GetAll(filter: (m => m.BookingItemId == bookRoomNoUser.BookingItemId && m.IdCategories == 1));
                bool isOverlap = getbooking.Any(b => (bookRoomNoUser.StartDate_ISO < b.EndDate) && (bookRoomNoUser.EndDate_ISO > b.StartDate));
                if (isOverlap)
                {
                    ModelState.AddModelError("StartDate", "Thời gian đặt phòng bị trùng");
                    ModelState.AddModelError("EndDate", "Thời gian đặt phòng bị trùng");
                    return ValidationProblem(ModelState);
                }
                var bookRoomNew = new SysBooking()
                {
                    FullNameGuest = bookRoomNoUser.FullNameGuest,
                    PhoneGuest = bookRoomNoUser.PhoneGuest,
                    EmailGuest = bookRoomNoUser.EmailGuest,
                    IdCategories = 1,
                    BookingItemId = bookRoomNoUser.BookingItemId,
                    Price = bookRoomNoUser.Price,
                    GuestsNumber = bookRoomNoUser.GuestsNumber,
                    DiscountedPrice = bookRoomNoUser.Price,
                    DiscountAmount = 0,
                    BookingDate = DateTime.Now,
                    CheckInDate = DateTime.Now,
                    StartDate = bookRoomNoUser.StartDate_ISO,
                    EndDate = bookRoomNoUser.EndDate_ISO,
                    DesQr = Common.GenerateRandomQrCode(),
                    Status = 2// chờ thanh toán
                };
                _unitOfWork.Repository<SysBooking>().Insert(bookRoomNew);
                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(bookRoomNoUser.BookingItemId);
                //return PartialView("datphong", getRoom);
                return Json(new
                {
                    success = true,
                    message = "Lấy dữ liệu phòng thành công",
                    IdBooking = bookRoomNew.Id
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult ShowBill(int Id)
        {
            try
            {
                var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var getAccount = Account.GetAccount();
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(Id);
                if (getBooking == null)
                {
                    return PartialView("Index");
                }
                var getBank = _unitOfWork.Repository<CatBank>().GetAll(filter: (m => m.Status == true));
                var getUse = new SysUser();

                if (getAccount.PartnerId != null)
                {
                    getUse = _unitOfWork.Repository<SysUser>().GetById(Convert.ToInt32(getAccount.PartnerId));
                    if (getUse == null)
                    {
                        return PartialView("Index");
                    }
                    getAccount.CardName = getUse.CardName;
                }
                var NameBank = getBank.FirstOrDefault(m => m.Id == getAccount.CardName);
                ViewBag.getRoom = _unitOfWork.Repository<SysRoom>().GetAll(filter: (m => m.Id == getBooking.BookingItemId)).ToList().Select(room =>
                {
                    if (!string.IsNullOrWhiteSpace(room.ListImg))
                    {
                        var firstImg = room.ListImg.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        if (!System.IO.File.Exists(Path.Combine(wwwRootPath, firstImg.TrimStart('/'))))
                        {
                            room.ListImg = "/assets/img/no-image.png";
                        }
                        else
                        {
                            room.ListImg = firstImg;
                        }

                    }
                    else
                    {
                        room.ListImg = "/assets/img/no-image.png";
                    }
                    return room;
                }).ToList();
                var ConnentQRCode = new ModelQRCode()
                {
                    des = getBooking.DesQr,
                    acc = getAccount?.CardNumber,
                    bank = NameBank?.KeyBank,
                    amount = getBooking.Price,
                };
                ViewBag.ConnentQRCode = JsonSerializer.Serialize(ConnentQRCode);

                //ViewBag.urlQR = $"https://qr.sepay.vn/img?acc={getAccount.CardNumber}&bank={NameBank?.KeyBank}&amount={((int)getBooking.Price).ToString()}&des=NOI_DUNG";
                ViewBag.ListService = _unitOfWork.Repository<CatRoomService>().GetAll(filter: (m => m.Status == true));
                return PartialView(getBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        //PaymentMethod =1 đã thanh toán
        //PaymentMethod =2 chưa thanh toán
        public IActionResult ComfirmPay(int IdBooking, int PaymentMethod)
        {
            try
            {
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(IdBooking);
                if (getBooking == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Phòng đặt này không tồn tại",
                    });
                }
                else if (getBooking != null && getBooking.Status == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Phòng đặt này đã hủy",
                    });
                }
                switch (PaymentMethod)
                {
                    case 1:// đã thanh toán
                        getBooking.Status = 1;
                        break;
                    case 2:// chưa thanh toán
                        getBooking.Status = 2;
                        break;
                    default:
                        break;
                }
                _unitOfWork.Repository<SysBooking>().Update(getBooking);
                return Json(new
                {
                    success = true,
                    message = "Trạng thái thanh toán đã được lưu",
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi chưa lưu được trạng thái," + ex.Message,
                });
                throw;
            }
        }
        public IActionResult getVouver(string voucher)
        {
            try
            {
                var getvoucher = _unitOfWork.Repository<SysPromotion>().GetAll(filter: (m => m.Code.ToLower().Equals(voucher.ToLower()))).FirstOrDefault();
                if (getvoucher == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Mã không tồn tại",
                    });
                }
                var datenow = DateTime.Now;
                bool isValid = (getvoucher.StartDate <= datenow) && (datenow <= getvoucher.EndDate);
                if (!isValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Mã đã hết hạn",
                    });
                }
                return Json(new
                {
                    success = true,
                    message = "Nhập mã giảm giá thành công",
                    SaleOff = getvoucher.SaleOff
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}