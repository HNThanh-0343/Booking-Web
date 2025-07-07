using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;

using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class TrangChuController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;
        private readonly CustomPasswordHasher _passwordHasher;
        public TrangChuController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
            _passwordHasher = new CustomPasswordHasher();
        }
        public IActionResult Index()
        {
            var contry = _unitOfWork.Repository<CatContry>().GetAll(filter: c => c.Status == true &&
            c.Featured == true).Take(5).ToList();
            ViewBag.GetAminitise = GetAminitise();
            #region hiển thị hotel nổi bật ra
            var LocalStorages = new List<LocalStorage>();
            var cookie = Request.Cookies["LocalStorages"];
            if (!string.IsNullOrEmpty(cookie))
            {
                LocalStorages = JsonConvert.DeserializeObject<List<LocalStorage>>(cookie);
            }
            ViewBag.LocalStorages = LocalStorages;
            #endregion
            return View("Index", contry);
        }
        public ActionResult loadCategoryHead(string tabId)
        {
            try
            {
                if (string.IsNullOrEmpty(tabId))
                {
                    tabId = "pills-Khách sạn"; // Tab mặc định bạn muốn chọn khi mới vào trang
                }
                var name = tabId.Replace("pills-", "").Trim();
                var TinhThanh = getTinhThanh();
                var hotels = _unitOfWork.Repository<SysHotel>().GetAll(filter: (m => m.Status == true));
                ViewBag.TinhThanh = TinhThanh.Select(country => new CountryHotelCountViewModel
                {
                    Id = country.Id,
                    Name = country.Name
                }).ToList();

                ViewBag.HotelName = hotels.Select(m => new HotelViewModel
                {
                    Name = m.Name,
                }).ToList();
                var getAllCate = _unitOfWork.Repository<CatCategory>().GetAll(filter: c => c.Status == true).ToList();
                var filteredCategory = getAllCate
                .Where(c => c.Name != null && c.Name.Trim().ToLower().Contains(name.ToLower()))
                .ToList();

                ViewBag.FilteredCategory = filteredCategory;
                return PartialView("loadCategoryHead", getAllCate);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private List<CountryViewModel> getTinhThanh()
        {
            try
            {
                return _unitOfWork.Repository<CatContry>().GetAll(filter: (m => m.Status == true))
                                                                       .Select(m => new CountryViewModel
                                                                       {
                                                                           Id = m.Id,
                                                                           Name = m.Name,
                                                                           Featured = m.Featured
                                                                       }).OrderByDescending(m => m.Featured).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }
        private List<ModelHotel> GetHotel(DateTime? TimeStar, DateTime? TimeEnd, int phong = 0, int Adults = 0, int Children = 0)
        {
            try
            {
                var today = DateTime.Today;
                var startDate = TimeStar ?? today;
                var endDate = TimeEnd ?? today.AddDays(1);

                // Lấy dữ liệu phòng và số booking trong khoảng thời gian
                var rooms = (from room in _unitOfWork.Repository<SysRoom>().GetAll()
                             join booking in (
                                 from b in _unitOfWork.Repository<SysBooking>().GetAll()
                                 where b.Status != 0
                                       && b.IdCategories == 1
                                       && b.StartDate <= startDate
                                       && b.EndDate > endDate
                                 group b by b.BookingItemId into g
                                 select new
                                 {
                                     BookingItemId = g.Key,
                                     TotalBooked = g.Count()
                                 }
                             ) on room.Id equals booking.BookingItemId into bookingsGroup
                             from bg in bookingsGroup.DefaultIfEmpty()
                             let bookedCount = bg != null ? bg.TotalBooked : 0
                             where (room.TotalRoom ?? 0) > bookedCount
                             select new
                             {
                                 room.IdHotel,
                                 room.Id,
                                 MaxAdults = room.AdultsMax ?? 0,
                                 MaxChildren = room.ChildrenMax ?? 0,
                                 RoomsAvailable = (room.TotalRoom ?? 0) - bookedCount
                             })
                             .ToList();

                var roomsGroupedByHotel = rooms.GroupBy(r => r.IdHotel);

                var suitableHotels = new List<int>();

                foreach (var hotelGroup in roomsGroupedByHotel)
                {
                    // Tính tổng số phòng trống trong khách sạn
                    int totalRoomsAvailable = hotelGroup.Sum(r => r.RoomsAvailable);

                    // Tính tổng sức chứa người lớn và trẻ em theo số phòng trống
                    int totalAdultsCapacity = hotelGroup.Sum(r => r.MaxAdults * r.RoomsAvailable);
                    int totalChildrenCapacity = hotelGroup.Sum(r => r.MaxChildren * r.RoomsAvailable);

                    bool passPhong = phong <= 0 || totalRoomsAvailable >= phong;
                    bool passAdults = Adults <= 0 || totalAdultsCapacity >= Adults;
                    bool passChildren = Children <= 0 || totalChildrenCapacity >= Children;

                    if (passPhong && passAdults && passChildren)
                    {
                        suitableHotels.Add((int)hotelGroup.Key);
                    }
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var defaultImagePath = "/AppData/no-image.png";

                var getHotel = _unitOfWork.Repository<SysHotel>()
                    .GetAll(m => suitableHotels.Contains(m.Id))
                    .AsEnumerable()
                    .Select(h =>
                    {
                        string firstImg = null;

                        if (!string.IsNullOrEmpty(h.ListImg))
                        {
                            firstImg = h.ListImg.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                            if (!string.IsNullOrEmpty(firstImg))
                            {
                                var cleanedFirstImg = firstImg.TrimStart('/', '\\');
                                var fullPath = Path.Combine(uploadsFolder, cleanedFirstImg);
                                if (!System.IO.File.Exists(fullPath))
                                {
                                    firstImg = null;
                                }
                            }
                        }

                        return new ModelHotel
                        {
                            IdHotel = h.Id,
                            IdContry = h.IdContry,
                            Name = h.Name,
                            PriceMin = h.PriceMin,
                            IMG = firstImg ?? defaultImagePath,
                            Local = h.Local,
                            Amenities = h.Amenities,
                            Featured = h.Featured,
                            NumStar = h.NumStar
                        };
                    })
                    .ToList();

                return getHotel;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult loadTrending()
        {
            try
            {
                var getAllCate = _unitOfWork.Repository<CatCategory>().GetAll(filter: c => c.Status == true).ToList();

                // Khởi tạo danh sách khách sạn
                var getHotel = new List<HotelViewHome>();
                var rooms = _unitOfWork.Repository<SysRoom>().GetAll(filter: r => r.Status == true);
                var listUserIds = rooms
                                         .Where(h => h.IdHotel.HasValue)
                                         .Select(h => h.IdHotel.Value)
                                         .Distinct()
                                         .ToList();
                var hotels = _unitOfWork.Repository<SysHotel>().GetAll(filter: m => m.Status == true && m.Featured == true);
                var hotelss = hotels.Where(u => listUserIds.Contains(u.Id)).ToList();
                var promotions = _unitOfWork.Repository<SysPromotion>().GetAll(filter: m => m.Status == true);
                var promotionDict = promotions.ToDictionary(
                                p => p.Id,
                                p => new promoInfo
                                {
                                    Type = p.Type,
                                    Sale = p.SaleOff,
                                }
                            );
                //var PriceDict = rooms
                //        .Where(h => h.IdHotel.HasValue && h.Price.HasValue)
                //        .GroupBy(h => h.IdHotel.Value)
                //        .ToDictionary(g => g.Key, g => g.Min(h => h.Price.Value));

                #region Lọc danh sách tim lên đầu
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.User = userId;
                if (userId.HasValue)
                {
                    var likedHotelIds = _unitOfWork.Repository<SysLike>()
                        .GetAll(l => l.IdUser == userId.Value && l.Idcategory == 1 && l.Like == true)
                        .Select(l => l.LikeItemId)
                        .ToList();

                    ViewBag.LikedHotelIds = likedHotelIds;

                    hotelss = hotelss
                    .OrderByDescending(h => likedHotelIds.Contains(h.Id))
                    .ThenBy(h => h.Name)
                    .ToList();
                }
                #endregion

                getHotel = hotelss.Select(hotel =>
                {
                    var promo = hotel.IdPromotion.HasValue && promotionDict.ContainsKey(hotel.IdPromotion.Value)
                    ? promotionDict[hotel.IdPromotion.Value]
                    : null;
                    return new HotelViewHome
                    {
                        Id = hotel.Id,
                        Image = hotel.ListImg?.Split(',').FirstOrDefault() ?? "",
                        Local = hotel.Local ?? "",
                        Name = hotel.Name ?? "",
                        NumberStar = (int)(hotel.NumStar ?? 0),
                        Url = $"/khach-san/{Common.GenerateSlug(hotel.Name)}?ks={hotel.Id}",
                        Price = hotel.PriceMin ?? 0,
                        Amenities = hotel.Amenities,
                        Sale = promo?.Sale ?? 0,
                        type = promo?.Type ?? null
                    };
                }).Take(8).ToList();
                if (getHotel == null)
                {
                    return PartialView("loadTrending", getAllCate);
                }
                ViewBag.getHotel = getHotel;
                ViewBag.GetAminitise = GetAminitise();// lấy toàn bộ tiện ích


                return PartialView("loadTrending", getAllCate);
            }
            catch (Exception ex)
            {
                // Ghi log hoặc trả về view lỗi
                Console.WriteLine("Error in loadTrending: " + ex.Message);
                return PartialView("loadTrending", new List<CatCategory>());
            }
        }

        public ActionResult loadPostIdCate(int IdPost)
        {
            try
            {
                var getHotel = new List<HotelViewHome>();
                ViewBag.GetAminitise = GetAminitise();// lấy toàn bộ tiện ích
                var promotions = _unitOfWork.Repository<SysPromotion>().GetAll(filter: m => m.Status == true);
                var promotionDict = promotions.ToDictionary(
                                p => p.Id,
                                p => new promoInfo
                                {
                                    Type = p.Type,
                                    Sale = p.SaleOff,
                                }
                            );
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.User = userId;
                switch (IdPost)
                {
                    case 1:// hotel
                        //var allHotels = _unitOfWork.Repository<SysHotel>().GetAll(filter: (m => m.Status == true && m.Featured ==true && m.IdCategory == IdPost));
                        var rooms = _unitOfWork.Repository<SysRoom>().GetAll(filter: r => r.Status == true);
                        var listUserIds = rooms
                                                 .Where(h => h.IdHotel.HasValue)
                                                 .Select(h => h.IdHotel.Value)
                                                 .Distinct()
                                                 .ToList();
                        var hotelss = _unitOfWork.Repository<SysHotel>()
                                        .GetAll(filter: m => m.Status == true && m.Featured == true && m.IdCategory == IdPost)
                                        .Where(u => listUserIds.Contains(u.Id))
                                        .ToList();
                        #region Lọc danh sách tim lên đầu
                        if (userId.HasValue)
                        {
                            var likedHotelIds = _unitOfWork.Repository<SysLike>()
                                .GetAll(l => l.IdUser == userId.Value && l.Idcategory == 1 && l.Like == true)
                                .Select(l => l.LikeItemId)
                                .ToList();

                            ViewBag.LikedHotelIds = likedHotelIds;

                            hotelss = hotelss
                            .OrderByDescending(h => likedHotelIds.Contains(h.Id))
                            .ThenBy(h => h.Name)
                            .ToList();
                        }
                        #endregion
                        //var PriceDict = rooms
                        //        .Where(h => h.IdHotel.HasValue && h.Price.HasValue)
                        //        .GroupBy(h => h.IdHotel.Value)
                        //        .ToDictionary(g => g.Key, g => g.Min(h => h.Price.Value));
                        getHotel = hotelss.Select(hotels =>
                        {

                            var promo = hotels.IdPromotion.HasValue && promotionDict.ContainsKey(hotels.IdPromotion.Value)
                                    ? promotionDict[hotels.IdPromotion.Value]
                                    : null;
                            return new HotelViewHome
                            {
                                Id = hotels.Id,
                                Image = hotels.ListImg?.Split(',').FirstOrDefault() ?? "",
                                Local = hotels.Local ?? "",
                                Name = hotels.Name ?? "",
                                NumberStar = (int)(hotels.NumStar ?? 0),
                                Url = $"/khach-san/{Common.GenerateSlug(hotels.Name)}?ks={hotels.Id}",
                                Amenities = hotels.Amenities,
                                Price = hotels.PriceMin ?? 0,
                                Sale = promo?.Sale ?? 0,
                                type = promo?.Type ?? null
                            };
                        }).Take(8).ToList();
                        break;
                    //case 2://Homestay
                    //    var allActivity = _unitOfWork.Repository<SysActivity>().GetAll(filter: (m => m.Status == true && m.Featured == true && m.IdCategory == IdPost));
                    //    getHotel = allActivity.Select(activity => new HotelViewHome
                    //    {
                    //        Id = activity.Id,
                    //        Image = activity.ListImg.Split(",").FirstOrDefault(),
                    //        Local = activity.LocalText,
                    //        Name = activity.Name,
                    //        NumberStar = (int)activity.NumStar,
                    //        Discount = (int)activity.Reviews,
                    //        Url = $"/hoatdong/{Common.GenerateSlug(activity.Name)}?hd={activity.Id}",
                    //        Price = (float)activity.Price
                    //    }).Take(8).ToList();
                    //    break;

                    //case 3://Rental
                    //    var allVilla = _unitOfWork.Repository<SysVilla>().GetAll(filter: (m => m.Status == true && m.Featured == true && m.IdCategory == IdPost));
                    //    getHotel = allVilla.Select(villa => new HotelViewHome
                    //    {
                    //        Id = villa.Id,
                    //        Image = villa.ListImg.Split(",").FirstOrDefault(),
                    //        Local = villa.LocalText,
                    //        Name = villa.Name,
                    //        NumberStar = (int)villa.NumStar,
                    //        Discount = (int)villa.Reviews,
                    //        Url = $"/bietthu/{Common.GenerateSlug(villa.Name)}?bt={villa.Id}",
                    //        Price = (float)villa.Price
                    //    }).Take(8).ToList();
                    //    break;

                    //case 4://Restaurants
                    //    var allRestaurant = _unitOfWork.Repository<SysRestaurant>().GetAll(filter: (m => m.Status == true && m.Featured == true && m.IdCategory == IdPost));

                    //    getHotel = allRestaurant.Select(restaurant => new HotelViewHome
                    //    {
                    //        Id = restaurant.Id,
                    //        Image = restaurant.ListImg.Split(",").FirstOrDefault(),
                    //        Local = restaurant.LocalText,
                    //        Name = restaurant.Name,
                    //        NumberStar = (int)restaurant.NumStar,
                    //        Discount = (int)restaurant.Reviews,
                    //        Url = $"/nhahang/{Common.GenerateSlug(restaurant.Name)}?nh={restaurant.Id}",
                    //        Price = (float)restaurant.Price
                    //    }).Take(8).ToList();
                    //    break;

                    default:
                        break;
                }
                return PartialView("loadPostIdCate", getHotel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult loadBlog()
        {
            try
            {
                var getPostBlog = new List<PostViewHome>();
                var today = DateTime.Today;
                getPostBlog = (from a in _unitOfWork.Repository<SysBlog>().GetAll(filter: (a => a.Status == true)).OrderByDescending(a => a.DateCreate).ToList()
                               select new PostViewHome()
                               {
                                   Id = a.Id,
                                   Name = a.Name,
                                   Image = !string.IsNullOrEmpty(a.ListImg) && a.ListImg.Contains(",")
                                           ? a.ListImg.Split(',')[0]
                                           : (!string.IsNullOrEmpty(a.ListImg) ? a.ListImg : ""),
                                   Content = a.ContentsShort,
                                   Date = a.DateCreate,
                                   //Url = $"/blog/{Common.GenerateSlug(a.Name)}",
                               }).Take(9).ToList();
                return PartialView("loadBlog", getPostBlog);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult loadKM()
        {
            try
            {
                var today = DateTime.Today;
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.User = userId;
                // BƯỚC 1: Cập nhật các khuyến mãi hết hạn
                var expiredPromos = _unitOfWork.Repository<SysPromotion>()
                    .GetAll(p => p.EndDate < today && p.Status == true)
                    .ToList();

                foreach (var promo in expiredPromos)
                {
                    promo.Status = false;
                    _unitOfWork.Repository<SysPromotion>().Update(promo);
                }

                _unitOfWork.Save();

                // BƯỚC 2: Lấy danh sách khuyến mãi hợp lệ
                var activePromos = _unitOfWork.Repository<SysPromotion>()
                    .GetAll(p => p.StartDate <= today && p.EndDate >= today && p.Status == true)
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => new PostViewHome
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Image = !string.IsNullOrEmpty(p.Image) ? p.Image : "/AppData/KhuyenMai/KhuyenMaiMacDinh.jpg",
                        Content = p.Code
                    })
                    .Take(9)
                    .ToList();

                return PartialView("loadKM", activePromos);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private List<CatAminitieseRoom> GetAminitise()
        {
            try
            {
                return _unitOfWork.Repository<CatAminitieseRoom>().GetAll(filter: (m => m.Status == true)).ToList();

            }
            catch (Exception)
            {

                throw;
            }
        }
        #region phần khuyến mãi theo tài khoản tạm thời đóng
        //[HttpPost]
        //public JsonResult SavePromotionCode(int id)
        //{
        //    var userId = Account.GetGuest().Id;
        //    if (userId == null)
        //    {
        //        return Json(new { success = false, message = "Bạn cần đăng nhập để lưu mã" });
        //    }

        //    // Kiểm tra đã nhận mã chưa
        //    var existed = _unitOfWork.Repository<CatPromotionGuest>()
        //        .GetAll()
        //        .Any(x => x.IdGuest == userId && x.IdPromotion == id);

        //    if (existed)
        //    {
        //        return Json(new { success = false, message = "Bạn đã lưu mã này rồi" });
        //    }

        //    var promotion = new CatPromotionGuest
        //    {
        //        IdGuest = userId,
        //        IdPromotion = id,
        //        DayReceive = DateTime.Now,
        //        IsStatus = true
        //    };

        //    _unitOfWork.Repository<CatPromotionGuest>().Insert(promotion);
        //    _unitOfWork.Save();

        //    return Json(new { success = true });
        //}
        //[HttpPost]
        //public JsonResult CheckSavedCode(int id)
        //{
        //    var userId = Account.GetGuest().Id;
        //    if (userId == null)
        //        return Json(new { alreadySaved = false });

        //    var existed = _unitOfWork.Repository<CatPromotionGuest>()
        //        .GetAll()
        //        .Any(x => x.IdGuest == userId && x.IdPromotion == id);
        //    return Json(new { alreadySaved = existed });
        //}
        //[HttpPost]
        //public IActionResult UseCode(int id)
        //{
        //    var promo = _unitOfWork.Repository<SysPromotion>().GetById(id);
        //    if (promo == null) return NotFound();

        //    // Nếu đã hết mã
        //    if (promo.QuantityUse >= promo.Quantity)
        //        return Json(new { success = false, message = "Hết mã khuyến mãi" });

        //    // Tăng số lượng sử dụng
        //    promo.QuantityUse += 1;
        //    _unitOfWork.Save();

        //    // Kiểm tra lại sau khi tăng
        //    bool isOutOfCode = promo.QuantityUse >= promo.Quantity;

        //    return Json(new
        //    {
        //        success = true,
        //        isOutOfCode = isOutOfCode,
        //        quantityUse = promo.QuantityUse,
        //        quantity = promo.Quantity
        //    });
        //}
        #endregion

        [Route("TrangChu/Error404")]
        public IActionResult Error404()
        {
            Response.StatusCode = 404;
            return View("404");
        }
        public IActionResult DangNhap(int role)
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            ViewBag.IdRole = role;
            AccountLoginForm accountLoginForm = new AccountLoginForm();
            if (Request.Cookies.ContainsKey("RememberUsername"))
            {
                accountLoginForm.Username = Request.Cookies["RememberUsername"];
                accountLoginForm.RememberMe = true;
            }
            return PartialView("_DangNhap", accountLoginForm);
        }
        public IActionResult DangKy(int id)
        {
            TempData["ReturnUrl"] = Request.Query["ReturnUrl"];

            // If user is already logged in, redirect to home page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            ViewBag.Idrole = id;
            AccountForm accountForm = new AccountForm();

            return PartialView("_DangKy", accountForm);
        }
        public IActionResult QuenMatKhau(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            ViewBag.Role = id;
            AccountForgotForm accountForgotForm = new AccountForgotForm();

            return PartialView("_QuenMatKhau", accountForgotForm);
        }
        public IActionResult ThongTin()
        {
            var idUser = HttpContext.Session.GetInt32("UserId");
            if (idUser == null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            var guest = _unitOfWork.Repository<SysGuest>().GetById((int)idUser);

            return PartialView("_ThongTin", guest);
        }
        [HttpGet]
        public IActionResult CapNhatThongTin()
        {
            var idUser = HttpContext.Session.GetInt32("UserId");
            if (idUser == null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            var guest = _unitOfWork.Repository<SysGuest>().GetById((int)idUser);

            return PartialView("_CapNhatThongTin", guest);
        }
        [HttpPost]
        public IActionResult CapNhatThongTin(SysGuest guest, IFormFile AvatarFile)
        {
            var previousUrl = Request.Headers["Referer"].ToString();
            // Xử lý lưu thông tin
            var existingGuest = _unitOfWork.Repository<SysGuest>().GetById(guest.Id);
            if (existingGuest == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
                return PartialView("_ThongTin", guest);
            }

            existingGuest.Name = guest.Name;
            existingGuest.Email = guest.Email;
            existingGuest.Phone = guest.Phone;
            existingGuest.Local = guest.Local;

            if (AvatarFile != null)
            {
                string wwwPath = this.Environment.WebRootPath;
                var urlImg = "\\AppData\\Guest\\" + existingGuest.Id + "\\";
                var path = wwwPath + urlImg;
                var pathdefault = Common.SaveUrlImg(existingGuest.Id, wwwPath, urlImg, AvatarFile);
                if (pathdefault != null)
                {
                    existingGuest.Avatar = pathdefault;
                }


            }

            _unitOfWork.Repository<SysGuest>().Update(existingGuest);
            _unitOfWork.Save();
            SetUserSession(existingGuest, "Guest");
            ViewBag.SuccessMessage = "Cập nhật thông tin thành công!";
            return Redirect(previousUrl);

        }
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            var idUser = HttpContext.Session.GetInt32("UserId");

            if (idUser == null)
            {
                return RedirectToAction("Index", "TrangChu");
            }

            return PartialView("_DoiMatKhau");
        }
        [HttpPost]
        public IActionResult DoiMatKhau(string Password, string NewPassword)
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var type = HttpContext.Session.GetString("UserType");
            var previousUrl = Request.Headers["Referer"].ToString();
            if (type == "Guest")
            {
                var guest = _unitOfWork.Repository<SysGuest>().GetById((int)id);
                if (guest.Password == _passwordHasher.CreateBase64(Password))
                {
                    guest.Password = _passwordHasher.CreateBase64(NewPassword);
                }

                _unitOfWork.Repository<SysGuest>().Update(guest);
                _unitOfWork.Save();
                return RedirectToAction("Index", "TrangChu");
            }
            else if(type == "User")
            {
                var user = _unitOfWork.Repository<SysUser>().GetById((int)id);
                if (user.Password == _passwordHasher.CreateBase64(Password))
                {
                    user.Password = _passwordHasher.CreateBase64(NewPassword);
                }

                _unitOfWork.Repository<SysUser>().Update(user);
                _unitOfWork.Save();
                return RedirectToAction("Index", "TrangChu");
            }
            return RedirectToAction("Index", "TrangChu");

        }
        private void SetUserSession(dynamic user, string userType)
        {
            HttpContext.Session.SetString("TaiKhoan", (string)System.Text.Json.JsonSerializer.Serialize(user));
            HttpContext.Session.SetInt32("UserId", (int)user.Id);
            HttpContext.Session.SetString("UserType", userType);
        }
    }
}
