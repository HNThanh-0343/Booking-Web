using Microsoft.AspNetCore.Mvc;
using System.Buffers;
using System.Security.Principal;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Areas.Partner.Controllers
{
    [Area("partner")]
    [Auth]
    public class dichvuphongController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public dichvuphongController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ChildIndex(int? page, string searchValue)
        {
            var getAccount = Account.GetAccount();
            var now = DateTime.Now;
            // lấy ra tất cả hotel của người dùng quản lý
            var getHotel = GetHotel(getAccount);

            // lọc để lấy Id của hotel
            var getIdHotel = getHotel.Select(h => h.Id).Distinct().ToList();
            // nếu tìm kiếm phòng theo hotel thì lấy IdHotel còn không là lấy tất cả phòng của tất cả hotel mà người đó quản lý

            var listRoom = GetRooms(getHotel, searchValue); // Bao gồm TypeRoomNavigation

            var listRoomIds = listRoom.Select(r => r.Id).ToList();

            var allBookings = GetAllBookings(listRoomIds);
            ViewBag.listRoom = listRoom;
            // Pagination
            page = page ?? 1;
            page = page < 1 ? 1 : page;
            var pageSize = 16;
            var pageListView = allBookings.ToPagedList(page.Value, pageSize);
            return PartialView("childIndex", pageListView);
        }
        private List<SysHotel> GetHotel(SysUser getAccount)
        {
            try
            {
                var getHotel = new List<SysHotel>();
                if (getAccount.PartnerId != null)
                {
                    return getHotel = _unitOfWork.Repository<SysHotel>()
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
                    return getHotel = _unitOfWork.Repository<SysHotel>()
                               .GetAll(h => h.IdUser == getAccount.Id && h.Status == true).ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private List<SysRoom> GetRooms(List<SysHotel> getHotel, string numberRoom)
        {

            // lọc để lấy Id của hotel
            var getIdHotel = getHotel.Select(h => h.Id).Distinct().ToList();
            // nếu tìm kiếm phòng theo hotel thì lấy IdHotel còn không là lấy tất cả phòng của tất cả hotel mà người đó quản lý
            List<SysRoom> listRoom = string.IsNullOrEmpty(numberRoom)
               ? _unitOfWork.Repository<SysRoom>().GetAll(r => r.IdHotel.HasValue && getIdHotel.Contains(r.IdHotel.Value) && r.Status == true, includeProperties: "TypeRoomNavigation").ToList()
               : _unitOfWork.Repository<SysRoom>().GetAll(r => r.NumRoom == Convert.ToInt32(numberRoom) && getIdHotel.Contains(r.IdHotel.Value) && r.Status == true, includeProperties: "TypeRoomNavigation").ToList();
            return listRoom;
        }

        private List<SysBooking> GetAllBookings(List<int> listRoomIds)
        {
            var now = DateTime.Now;

            var allBookings = _unitOfWork.Repository<SysBooking>().GetAll(b =>
               b.IdCategories == 1 &&
               b.Status != 0 &&
               listRoomIds.Contains(b.BookingItemId) &&
               b.StartDate <= now
           ).ToList();

            return allBookings;
        }

        public IActionResult chinhSuaDichvuKhachSan(int IdBooking)
        {
            try
            {
                var getAccount = Account.GetAccount();
                var sysBooking = _unitOfWork.Repository<SysBooking>().GetById(IdBooking);
                if (sysBooking == null)
                {
                    return PartialView("Index");
                }
                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(sysBooking.BookingItemId);
                if (getRoom == null)
                {
                    return PartialView("Index");
                }
                ViewBag.getRoomService = _unitOfWork.Repository<CatRoomService>().GetAll(filter: (m => m.IdHotel == getRoom.IdHotel));
                return PartialView("chinhSuaDichvuKhachSan", sysBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult chinhSuaDichvuKhachSan(string IdCategories, int Id, List<RoomServiceItem> roomServiceItems)
        {
            try
            {
                var getAccount = Account.GetAccount();
                var getRoomService = _unitOfWork.Repository<CatRoomService>().GetAll(filter: (m => m.IdHotel == getAccount.Id));
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(Id);
                if (getBooking == null)
                {
                    //TempData["ErrorMessage"] = "Chỉnh sửa thất bại!";
                    return PartialView("chinhSuaDichvuKhachSan");
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
                    return PartialView("chinhSuaDichvuKhachSan", getBooking);
                }
                getBooking.ListItemServices = Common.SerializeServiceList(roomServiceItems.Where(m => m.Quantity > 0).ToList()); ;

                _unitOfWork.Repository<SysBooking>().Update(getBooking);
                ViewBag.getRoomService = getRoomService;
                //TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
                return PartialView(getBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult giahanphong(int IdBooking)
        {
            try
            {
                var sysBooking = _unitOfWork.Repository<SysBooking>().GetById(IdBooking);
                if (sysBooking == null)
                {
                    return PartialView("Index");
                }
                return PartialView("giahanphong", sysBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult giahanphong(ExtendCheckOutTime extendCheckOutTime)
        {
            try
            {
                var getAccount = Account.GetAccount();
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(extendCheckOutTime.IdBooking);
                if (getBooking == null)
                {
                    return PartialView("Index");
                }

                // Lấy ra thông tin phòng hiện tại
                var currentRoom = _unitOfWork.Repository<SysRoom>().GetById(getBooking.BookingItemId);
                if (currentRoom == null)
                {
                    return PartialView("Index");
                }

                // Lấy danh sách booking khác của cùng phòng
                var bookings = _unitOfWork.Repository<SysBooking>().GetAll(m =>
                    m.BookingItemId == getBooking.BookingItemId &&
                    m.Status != 0 &&
                    m.Id != getBooking.Id
                ).OrderBy(m => m.StartDate).ToList();

                // Tìm booking tiếp theo
                var nextBooking = bookings.FirstOrDefault(b => b.StartDate > getBooking.EndDate);

                if (nextBooking != null)//tìm ra có 1 booking phía sau trùng thời gian gia hạn
                {
                    var maxExtendTime = nextBooking.StartDate.AddHours(-1);

                    if (extendCheckOutTime.EndDate_ISO > maxExtendTime)
                    {
                        var getHotel = GetHotel(getAccount);
                        var getIdHotel = getHotel.Select(h => h.Id).Distinct().ToList();

                        var listRoom = GetRooms(getHotel, ""); // Bao gồm TypeRoomNavigation

                        // Lọc phòng cùng khách sạn và cùng loại phòng
                        var sameTypeRooms = listRoom
                            .Where(r =>
                                r.Id != currentRoom.Id &&
                                r.TypeRoom == currentRoom.TypeRoom &&
                                r.IdHotel == currentRoom.IdHotel
                            ).ToList();

                        // Tìm phòng trống không bị trùng thời gian với booking kế tiếp
                        var availableRooms = sameTypeRooms.Where(r =>
                            !_unitOfWork.Repository<SysBooking>().GetAll(b =>
                                b.BookingItemId == r.Id &&
                                b.Status != 0 &&
                                (
                                    b.StartDate < nextBooking.EndDate &&
                                    b.EndDate > nextBooking.StartDate
                                )
                            ).Any()
                        ).ToList();

                        if (availableRooms.Any())
                        {
                            var newRoom = availableRooms.First();

                            // Chuyển phòng cho booking sau
                            nextBooking.BookingItemId = newRoom.Id;
                            nextBooking.Note = $"Được chuyển từ phòng {currentRoom.Name} để khách trước gia hạn.";
                            _unitOfWork.Repository<SysBooking>().Update(nextBooking);
                            // đã chuyển phòng cho booking trùng với thời gian phòng A muốn thuê tiếp
                            getBooking.EndDate = extendCheckOutTime.EndDate_ISO;
                            getBooking.Note = $"Gia hạn thời gian checkout tới :{extendCheckOutTime.EndDate_ISO}";
                            _unitOfWork.Repository<SysBooking>().Update(getBooking);
                            // cập nhật thời gian gia hạn
                        }
                        else
                        {
                            var key = "EndDate";
                            var maxTimeStr = maxExtendTime.ToString("dd/MM/yyyy HH:mm");
                            var errorMessage = $"Không thể gia hạn đến {extendCheckOutTime.EndDate_ISO:dd/MM/yyyy HH:mm} vì đã có người thuê phòng sau đó. Chỉ có thể gia hạn đến {maxTimeStr}.";
                            ModelState.AddModelError(key, errorMessage);
                            return ValidationProblem(ModelState);
                        }
                    }
                    else
                    {
                        // có người thuê sau trùng phòng nhưng thời gian không trùng, tức là vẫn được
                        getBooking.EndDate = extendCheckOutTime.EndDate_ISO;
                        getBooking.Note = $"Gia hạn thời gian checkout tới :{extendCheckOutTime.EndDate_ISO}";
                        _unitOfWork.Repository<SysBooking>().Update(getBooking);
                    }
                }
                else
                {
                    getBooking.EndDate = extendCheckOutTime.EndDate_ISO;
                    getBooking.Note = $"Gia hạn thời gian checkout tới :{extendCheckOutTime.EndDate_ISO}";
                    _unitOfWork.Repository<SysBooking>().Update(getBooking);
                    // cập nhật thời gian gia hạn
                }
                return PartialView(getBooking);

            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return StatusCode(500, "Lỗi hệ thống");
            }
        }
        public IActionResult chuyenphong(int IdBooking)
        {
            try
            {
                var sysBooking = _unitOfWork.Repository<SysBooking>().GetById(IdBooking);
                if (sysBooking == null)
                {
                    return PartialView("Index");
                }
                return PartialView("giahanphong", sysBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IActionResult huyphong(int IdBooking)
        {
            try
            {
                var sysBooking = _unitOfWork.Repository<SysBooking>().GetAll(filter: (m => m.Id == IdBooking), includeProperties: "IdUserNavigation").FirstOrDefault();
                if (sysBooking == null)
                {
                    return PartialView("Index");
                }
                return PartialView("huyphong", sysBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult huyphong(SysBooking sysBooking)
        {
            try
            {
                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(sysBooking.Id);
                if (getBooking == null)
                {
                    return PartialView("Index");
                }
                getBooking.Note = sysBooking.Note;
                getBooking.Status = 0;
                _unitOfWork.Repository<SysBooking>().Update(getBooking);
                return PartialView("huyphong", getBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}