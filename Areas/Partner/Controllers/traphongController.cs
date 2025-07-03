using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Principal;
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
    public class traphongController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;

        public traphongController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
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
        private List<SysRoom> GetRooms(List<SysHotel> getHotel, string numberRoom)
        {
            var getIdHotel = getHotel.Select(h => h.Id).Distinct().ToList();

            List<int> roomArray = string.IsNullOrWhiteSpace(numberRoom) ? new List<int>() : numberRoom
                                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                                        .Select(x => int.TryParse(x, out var id) ? id : -1)
                                                                        .Where(id => id != -1)
                                                                        .ToList();

            List<SysRoom> listRoom = string.IsNullOrEmpty(numberRoom)
               ? _unitOfWork.Repository<SysRoom>().GetAll(r => r.IdHotel.HasValue && getIdHotel.Contains(r.IdHotel.Value) && r.Status == true, includeProperties: "TypeRoomNavigation").ToList()
               : _unitOfWork.Repository<SysRoom>().GetAll(r => roomArray.Contains((int)r.NumRoom) && getIdHotel.Contains(r.IdHotel.Value) && r.Status == true, includeProperties: "TypeRoomNavigation").ToList();

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
        public IActionResult traphong(List<int> IdBooking)
        {
            try
            {
                var getAccount = Account.GetAccount();
                var getBooking = _unitOfWork.Repository<SysBooking>().GetAll(m => IdBooking.Contains(m.Id), includeProperties: "IdUserNavigation");
                if (getBooking == null)
                {
                    return PartialView("Index");
                }
                //List<RoomAndServiceItem> listRoomAndServiceItem = new List<RoomAndServiceItem>();
                //foreach (var itemServiceRoom in getBooking)
                //{
                //    if (!string.IsNullOrEmpty(itemServiceRoom?.ListItemServices))
                //    {
                //        listRoomAndServiceItem.Add(new RoomAndServiceItem()
                //        {
                //            roomServiceItems = Common.DeserializeServiceList(itemServiceRoom?.ListItemServices),
                //            IdBooking = itemServiceRoom.Id
                //        });
                //    }
                //}
                var getRoom = _unitOfWork.Repository<SysRoom>().GetById(getBooking.First().BookingItemId);
                if (getRoom == null)
                {
                    return PartialView("Index");
                }
                ViewBag.ListService = _unitOfWork.Repository<CatRoomService>().GetAll(m => m.IdHotel == getRoom.IdHotel);
                var getIdRoomInBooking = getBooking.Select(m => m.BookingItemId).Distinct();
                ViewBag.getRoom = _unitOfWork.Repository<SysRoom>().GetAll(m => getIdRoomInBooking.Contains(m.Id));

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
                var NameBank = _unitOfWork.Repository<CatBank>().GetAll(filter: (m => m.Status == true)).FirstOrDefault(m => m.Id == getAccount.CardName);
                var ConnentQRCode = new ModelQRCode()
                {
                    des = getBooking?.First()?.DesQr,
                    acc = getAccount?.CardNumber,
                    bank = NameBank?.KeyBank,
                };
                ViewBag.ConnentQRCode = JsonSerializer.Serialize(ConnentQRCode);
                return View("traphong", getBooking);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public IActionResult traphong(InvoiceRoom invoiceRoom)
        {
            try
            {
                if (string.IsNullOrEmpty(invoiceRoom.IdBooking))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy Id của phòng",
                    });
                }
                List<int> listIdBooking = invoiceRoom.IdBooking.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(idStr => int.Parse(idStr.Trim())).ToList();
                var getBooking = _unitOfWork.Repository<SysBooking>().GetAll(filter: (m => listIdBooking.Contains(m.Id))).ToList();//get room
                if (getBooking == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy Id của phòng",
                    });
                }

                var getRoomInBooking = getBooking.Select(m => m.BookingItemId).FirstOrDefault();
                if (getRoomInBooking == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy Id của phòng",
                    });
                }
                var getHotel = _unitOfWork.Repository<SysRoom>().GetAll(m => m.Id == getRoomInBooking)?.Select(m => m.IdHotel)?.Distinct()?.FirstOrDefault();
                decimal totalAllBilld = 0;
                if (getHotel != null)
                {
                    var Surcharge = invoiceRoom.Surcharge ?? 0;
                    totalAllBilld = checkTotalCheckout(getBooking, Surcharge, getHotel);
                }
                var InsertInvoiceRoom = new SysInvoiceRoom()
                {
                    IdHotel = (int)getHotel,
                    DateCreate = DateTime.Now,
                    IdUser = getBooking?.First()?.IdUser,
                    EmailGuest = getBooking?.First()?.IdUser == null ? getBooking?.First()?.EmailGuest : "",
                    FullNameGuest = getBooking?.First()?.IdUser == null ? getBooking?.First()?.FullNameGuest : "",
                    ListIdRoomBooking = invoiceRoom.IdBooking,
                    StartDate = (DateTime)(getBooking?.First()?.StartDate),
                    EndDate = (DateTime)(getBooking?.First()?.EndDate),
                    PhoneGuest = getBooking?.First()?.IdUser == null ? getBooking?.First()?.PhoneGuest : "",
                    TotalMoney = totalAllBilld,
                    Status = true,
                    Note = $"Lý do:{invoiceRoom.Note} + Tiền phụ thu:{invoiceRoom.Surcharge}",

                };
                _unitOfWork.Repository<SysInvoiceRoom>().Insert(InsertInvoiceRoom);

                if (InsertInvoiceRoom.Id > 0)
                {
                    var getIdBooking = getBooking.Select(m => m.Id).Distinct().ToList();
                    // đã thêm vào invoice, bắt đầu xóa trong booking
                    var getBookingCheckout = _unitOfWork.Repository<SysBooking>().GetAll(m => getIdBooking.Contains(m.Id));
                    foreach (var itemRemove in getBookingCheckout)
                    {
                        _unitOfWork.Repository<SysBooking>().Delete(itemRemove);
                    }
                        //getBooking
                }
                // tiến hành xóa dữ liệu bảng booking
                return Json(new
                {
                    success = true,
                    message = "Checkout thành công cho khách hàng",
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        private decimal checkTotalCheckout(List<SysBooking> sysBookings, decimal Surcharge, int? IdHotel)
        {
            try
            {
                decimal total = (decimal)(0 + Surcharge);
                foreach (var itemBooking in sysBookings)
                {
                    var nights = (itemBooking.EndDate - itemBooking.StartDate).Days;
                    total += nights * Convert.ToDecimal(itemBooking?.DiscountedPrice);
                    if (!string.IsNullOrEmpty(itemBooking.ListItemServices))
                    {
                        var converListServiceRoom = Common.DeserializeServiceList(itemBooking.ListItemServices) as List<RoomServiceItem>;
                        if (converListServiceRoom.Count > 0)
                        {
                            var getService = _unitOfWork.Repository<CatRoomService>().GetAll(filter: (m => m.IdHotel == IdHotel));
                            foreach (var itemService in converListServiceRoom)
                            {
                                var getMoneyService = getService.FirstOrDefault(m => m.Id == itemService.IdService);
                                if (getMoneyService != null)
                                {
                                    total += Convert.ToDecimal(getMoneyService.Price * itemService.Quantity);
                                }
                            }
                        }
                    }
                }
                return total;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
