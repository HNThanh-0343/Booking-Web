using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using System.Buffers;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web.Helpers;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using System.Web.WebPages.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http.Extensions;
//using Newtonsoft.Json;


namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class KhachSanController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public KhachSanController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;


        [HttpGet("khach-san")]
        public IActionResult Index(int? page)
        {
            try
            {
                Updatelonlat();
                var getContry = getTinhThanh();
                var getHote = GetHotel(null, null);
                if (getHote == null)
                {
                    // không tìm thấy hotel nào
                }
                var getIdContry = getHote.GroupBy(m => m.IdContry).Select(g => new { IdContry = g.Key, Count = g.Count() }).ToList();
                ViewBag.getNameContry = getContry.Where(c => getIdContry.Any(h => h.IdContry == c.Id))
                                                 .Select(country => new CountryHotelCountViewModel
                                                 {
                                                     Id = country.Id,
                                                     Name = country.Name,
                                                     Count = getIdContry.First(h => h.IdContry == country.Id).Count
                                                 }).ToList();
                var getPriceMax = _unitOfWork.Repository<SysHotel>().GetAll(filter: (m => m.Status == true)).OrderByDescending(m => m.PriceMin).Select(m => m.PriceMin).FirstOrDefault();
                //var s = getPriceMax
                ViewBag.getPriceMax = getPriceMax ?? 0;
                ViewBag.AllContry = getContry;
                var countryDictionary = getContry.ToDictionary(c => c.Id, c => c.Name);
                ViewBag.AllHotel = getHote.Select(m =>
                {
                    string countryName = null;
                    if (m.IdContry != null && countryDictionary.ContainsKey(m.IdContry.Value))
                    {
                        countryName = countryDictionary[m.IdContry.Value];
                    }
                    return new HotelViewModel
                    {
                        Id = m.IdHotel,
                        Name = m.Name,
                        Local = countryName
                    };
                }).ToList();
                #region Page
                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 12;
                var pageListView = getHote.ToPagedList(page ?? 1, pageSize);
                #endregion

                return PartialView(pageListView);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [HttpGet("/khach-san/{namehotel}")]
        public IActionResult ChiTietKS(int? page, string namehotel, int ks, int? reviewsPage)
        {
            try
            {

                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                    {
                        var user = _unitOfWork.Repository<SysUser>().GetById(userId.Value);
                        if (user != null)
                            ViewBag.User = user;
                        else
                            ViewBag.User = _unitOfWork.Repository<SysGuest>().GetById(userId.Value);
                    }
                else
                {
                    ViewBag.User = null;
                }

                var liked = _unitOfWork.Repository<SysLike>().GetAll(filter: (l => l.IdUser == userId && l.Idcategory == 1 && l.LikeItemId == ks)).FirstOrDefault(); ;
                ViewBag.Liked = liked?.Like ?? false;
                ViewBag.IDHOTEL = ks;
                var hotels = _unitOfWork.Repository<SysHotel>().GetAll(filter: (m => m.Id == ks && m.Status == true), includeProperties: "IdPromotionNavigation").FirstOrDefault();
                if (hotels == null)
                {
                    return NotFound();
                }
                var Contry = _unitOfWork.Repository<CatContry>().GetById((int)hotels.IdContry);

                var rooms = _unitOfWork.Repository<SysRoom>().GetAll(
                    filter: r => r.IdHotel == hotels.Id && r.Status == true,
                    includeProperties: "TypeRoomNavigation"
                ).ToList();
                ViewBag.RoomsList = rooms;
                ViewBag.Hotels = hotels;

                var users = _unitOfWork.Repository<SysUser>().GetAll(filter: u => u.Id == hotels.IdUser && u.Status == true).FirstOrDefault();

                // Lấy khách sạn liên quan (loại trừ khách sạn hiện tại)
                var listhotel = _unitOfWork.Repository<SysHotel>()
                    ?.GetAll(
                        filter: h => h.IdContry == Contry.Id && h.Status == true && h.Id != ks,
                        includeProperties: "IdPromotionNavigation")
                    ?.Take(10)
                    ?.ToList();

                if (listhotel.Count < 4)
                {
                    var addHotels = _unitOfWork.Repository<SysHotel>()
                        .GetAll(h => h.Status == true && h.Id != ks,
                                includeProperties: "IdPromotionNavigation")
                        .ToList();

                    foreach (var h in addHotels)
                    {
                        if (listhotel.Count >= 4) break;
                        if (!listhotel.Any(x => x.Id == h.Id))
                            listhotel.Add(h);
                    }
                }
                ViewBag.Hotelss = listhotel.Take(10).ToList();


                // Lấy đánh giá và xếp hạng theo danh mục khách sạn
                var categoryRatings = GetHotelCategoryRatings(hotels.Id);
                ViewBag.CategoryRatings = categoryRatings;

                // Lấy đánh giá khách sạn thực tế kèm thông tin người dùng
                var hotelReviews = GetHotelReviews(hotels.Id);
                ViewBag.HotelReviews = hotelReviews;

                // lấy tiện nghi của khách sạn
                ViewBag.HotelAmenities = _unitOfWork.Repository<CatAminitieseRoom>()
                    .GetAll(filter: a => a.Status == true)
                    .ToList();

                // Tìm giá phòng thấp nhất của khách sạn
                var allHotelIds = new List<int> { hotels.Id };
                if (listhotel != null)
                {
                    allHotelIds.AddRange(listhotel.Select(h => h.Id));
                }

                var allRooms = _unitOfWork.Repository<SysRoom>().GetAll(
                    filter: r => r.IdHotel.HasValue && allHotelIds.Contains(r.IdHotel.Value) && r.Status == true);

                var minPricesByHotel = allRooms
                    .Where(r => r.IdHotel.HasValue && r.Price.HasValue)
                    .GroupBy(r => r.IdHotel.Value)
                    .ToDictionary(g => g.Key, g => g.Min(r => r.Price.Value));

                ViewBag.PriceDict = minPricesByHotel;

                var CurrentHotelMinPrice = minPricesByHotel.ContainsKey(hotels.Id) ? minPricesByHotel[hotels.Id] : 0;
                ViewBag.CurrentHotelMinPrice = CurrentHotelMinPrice;

                #region Sự kiện hotel nổi bật lên lại
                var promo = _unitOfWork.Repository<SysPromotion>().GetAll(filter: p => p.Status == true &&
                                                                                  p.Id == hotels.IdPromotion).FirstOrDefault();
                var LocalStorages = new List<LocalStorage>();
                var cookie = Request.Cookies["LocalStorages"];
                if (!string.IsNullOrEmpty(cookie))
                {
                    LocalStorages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LocalStorage>>(cookie);
                }
                //kiểm tra khách sạn còn tồn tại trong csdl k
                LocalStorages = LocalStorages.Where(h => _unitOfWork.Repository<SysHotel>().GetAll().
                                            Any(db => db.Id == h.Id)).ToList();

                var existingHotel = LocalStorages.FirstOrDefault(h => h.Id == hotels.Id);
                // set động /khach-san/
                var fullPath = HttpContext.Request.Path; // ="/khach-san/..."
                string[] segments = fullPath.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
                // ="/khach-san/"
                string baseUrl = segments.Length > 0 ? $"/{segments[0]}/" : "/";

                if (existingHotel != null)
                {
                    // Nếu đã có, xóa khỏi vị trí cũ và đưa lên đầu
                    LocalStorages.Remove(existingHotel);
                    LocalStorages.Insert(0, existingHotel);
                }
                else
                // Kiểm tra nếu khách sạn chưa có trong danh sách thì thêm vào

                {
                    bool hasAvailableRooms = CheckRoomAvailability(hotels.Id);
                    if (hasAvailableRooms)
                    {
                        LocalStorages.Insert(0, new LocalStorage
                        {
                            Id = hotels.Id,
                            Name = hotels.Name,
                            Images = hotels.ListImg?.Split(",").FirstOrDefault(),
                            Local = hotels.Local,
                            Price = minPricesByHotel.ContainsKey(hotels.Id) ? minPricesByHotel[hotels.Id] : 0,
                            NumberStar = (int)(hotels?.NumStar ?? 0),
                            Discount = (int)(hotels?.Reviews ?? 0),
                            Amenities = hotels?.Amenities,
                            Sale = promo == null ? 0 : promo.SaleOff,
                            type = promo == null ? null : promo.Type,
                            Url = $"{baseUrl}{Common.GenerateSlug(hotels.Name)}?ks={hotels.Id}"
                        });
                    }

                    // Giới hạn số lượng khách sạn lưu (ví dụ: 5 khách sạn gần đây)
                    if (LocalStorages.Count > 4)
                    {
                        LocalStorages = LocalStorages.Take(4).ToList();
                    }
                }
                // Ghi lại vào Cookie
                var options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    IsEssential = true
                };
                Response.Cookies.Append("LocalStorages", Newtonsoft.Json.JsonConvert.SerializeObject(LocalStorages), options);
                #endregion 

                if (HttpContext.Request.Query.ContainsKey("ajaxReviews"))
                {
                    return PartialView("ChiTietKhachSan", hotels);
                }

                return View("ChiTietKhachSan", hotels);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        private bool CheckRoomAvailability(int hotelId)
        {

            var availableRooms = _unitOfWork.Repository<SysRoom>().GetAll()
                .Where(r => r.IdHotel == hotelId &&
                            r.Status == true).Any();

            return availableRooms;
        }

        [HttpGet("khach-san/{namehotel}/nameroom")]
        public IActionResult Detail(string namehotel, string nameroom, int ks, int room)
        {
            try
            {
                var getAllCate = _unitOfWork.Repository<SysHotel>().GetById(room);
                if (getAllCate == null)
                {
                    return NotFound();
                }
                ViewBag.GetAllHotel = (from a in _unitOfWork.Repository<SysHotel>().GetAll(filter: (m => m.Status == true), take: 10, orderBy: (m => m.OrderByDescending(d => d.Id)))
                                       select new HotelViewDetail()
                                       {
                                           Id = a.Id,

                                           Name = a.Name,
                                           Discount = "",
                                           Image = !string.IsNullOrEmpty(a.ListImg) && a.ListImg.Contains(",")
                                                           ? a.ListImg.Split(',')[0]
                                                           : (!string.IsNullOrEmpty(a.ListImg) ? a.ListImg : ""),
                                           Url = $"/hotel/{Common.GenerateSlug(a.Name)}?id={a.Id}",

                                           Featured = a.Featured ?? false,
                                           Amenities = a.Amenities
                                       }).ToList();
                return View(getAllCate);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        public ActionResult loadFilter(int? page, string ksdc, DateTime? ngayden, DateTime? ngaydi, int phong, int nguoilon, int connit, string sao, string thanhpho, string sort, string giatu, string giaden)//[FromBody] FilterDataViewHotel filter
        {
            try
            {
                if (true)
                {
                    var getHotel = GetHotel(ngayden, ngaydi, phong, nguoilon, connit);
                    var listQueryHotel = GetQuerySearch(getHotel, ksdc, sao, thanhpho, giatu, giaden);
                    ViewBag.GetAminitise = GetAminitise();// lấy toàn bộ tiện ích
                    if (!string.IsNullOrEmpty(sort))
                    {
                        switch (sort.ToLower())
                        {
                            case "tangdan":
                                listQueryHotel = listQueryHotel.OrderBy(m => m.PriceMin).ToList();
                                break;
                            case "giamdan":
                                listQueryHotel = listQueryHotel.OrderByDescending(m => m.PriceMin).ToList();
                                break;
                            default:
                                break;
                        }
                    }

                    #region Lọc danh sách tim lên đầu
                    var userId = HttpContext.Session.GetInt32("UserId");
                    if (userId.HasValue)
                    {
                        ViewBag.User = _unitOfWork.Repository<SysUser>().GetById(userId.Value);
                    }
                    else
                    {
                        ViewBag.User = null;
                    }
                    if (userId.HasValue)
                    {
                        var likedHotelIds = _unitOfWork.Repository<SysLike>()
                            .GetAll(l => l.IdUser == userId.Value && l.Idcategory == 1 && l.Like == true)
                            .Select(l => l.LikeItemId)
                            .ToList();

                        ViewBag.LikedHotelIds = likedHotelIds;

                        listQueryHotel = listQueryHotel
                        .OrderByDescending(h => likedHotelIds.Contains(h.IdHotel))
                        .ThenBy(h => h.Name)
                        .ToList();
                    }
                    #endregion

                    #region Danh sách hiển thị các khách sạn có cùng thành phố
                    var count = listQueryHotel.Count();

                    if (count < 12)
                    {
                        var suggestedHotels = new List<SysHotel>();
                        if (count == 0)
                        {
                            // Bước 1: Lấy IdContry có Featured == true
                            var featuredCountryIds = _unitOfWork.Repository<CatContry>()
                                .GetAll(c => c.Featured == true)
                                .Select(c => c.Id)
                                .ToList();

                            // Bước 2: Lấy các SysHotel có IdContry nằm trong featuredCountryIds
                            suggestedHotels = _unitOfWork.Repository<SysHotel>()
                                .GetAll(h => h.Status == true && featuredCountryIds.Contains(h.IdContry.Value),
                                        includeProperties: "IdPromotionNavigation")
                                .ToList();
                        }
                        else
                        {
                            // Lấy danh sách IdContry từ các khách sạn đã lọc
                            var ToadoList = listQueryHotel.Select(l => GetIframeSetLocal.ExtractLatLongFromIframe(l.Localiframe)).ToList();

                            // Tìm khách sạn khác có cùng IdContry, loại trừ các khách sạn đã có
                            var existingIds = listQueryHotel.Select(l => l.IdHotel).ToList();
                            suggestedHotels = KhachSanTrongToaDo(ToadoList)
                                .Where(h => !existingIds.Contains(h.Id))
                                .ToList();
                        }

                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var defaultImagePath = "/AppData/no-image.png";
                        var suggestedModelHotels = suggestedHotels.Select(h =>
                        {
                            string firstImg = null;

                            if (!string.IsNullOrEmpty(h.ListImg))
                            {
                                firstImg = h.ListImg
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .FirstOrDefault();

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
                                NumStar = h.NumStar,
                                Localiframe = h.Localiframe,
                                IdPromotion = h.IdPromotion,
                                IdPromotionNavigation = h.IdPromotionNavigation
                            };
                        }).ToList();

                        ViewBag.SuggestedHotels = suggestedModelHotels;
                    }

                    #endregion
                    #region ViewBag Page
                    ViewBag.ksdc = ksdc;
                    ViewBag.ngayden = ngayden;
                    ViewBag.ngaydi = ngaydi;
                    ViewBag.phong = phong;
                    ViewBag.nguoilon = nguoilon;
                    ViewBag.connit = connit;
                    ViewBag.sao = sao;
                    ViewBag.thanhpho = thanhpho;
                    ViewBag.sort = sort;
                    ViewBag.giatu = giatu;
                    ViewBag.giaden = giaden;
                    ViewBag.SortOptions = new SelectList(new[]
                                        {
                                            new { Value = "", Text = "Mặc định" },
                                            new { Value = "tangdan", Text = "Giá tăng dần" },
                                            new { Value = "giamdan", Text = "Giá giảm dần" },
                                        }, "Value", "Text", sort);
                    #endregion
                    #region Page
                    page = page == null ? 1 : page;
                    page = page < 1 ? 1 : page;
                    var pageSizeNoW = 12;
                    var pageListViewNoW = listQueryHotel.ToPagedList(page ?? 1, pageSizeNoW);
                    ViewBag.HotelsCount = count;
                    #endregion
                    return PartialView("loadFilter", pageListViewNoW);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static double CalculateDistance(double lat1, double lng1, double? lat2, double? lng2)
        {
            double R = 6371; // Bán kính trái đất (km)
            double dLat = (lat2.Value - lat1) * Math.PI / 180;
            double dLng = (lng2.Value - lng1) * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2.Value * Math.PI / 180) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public List<SysHotel> KhachSanTrongToaDo(List<GetIframeSetLocal.LatLongResult> Diem, double KhoangCach = 20)
        {
            var allHotel = _unitOfWork.Repository<SysHotel>().GetAll(h => h.LocalX.HasValue && h.LocalY.HasValue, includeProperties: "IdPromotionNavigation").ToList();
            var existingCoordinates = new HashSet<(double Lat, double Lng)>(
                Diem.Select(d => (d.Lat, d.Lng))
            );

            var hotelsTrongKhoanCach = allHotel
                                        .Where(h => Diem
                                        .Any(l => (CalculateDistance(l.Lat, l.Lng, h.LocalY, h.LocalX) <= KhoangCach)
                                        && !existingCoordinates.Contains((h.LocalY.Value, h.LocalX.Value)))
                                        ).ToList();
            return hotelsTrongKhoanCach;
        }
        private void Updatelonlat()
        {
            var listHotel = _unitOfWork.Repository<SysHotel>().GetAll(filter: h => h.Status == true);
            foreach (var hotel in listHotel)
            {
                if (hotel.LocalX != null && hotel.LocalY != null && hotel.LocalX != 0 && hotel.LocalY != 0)
                {
                    continue;
                }
                else
                {
                    if (hotel.Localiframe != null && !string.IsNullOrWhiteSpace(hotel.Localiframe))
                    {
                        var Toado = GetIframeSetLocal.ExtractLatLongFromIframe(hotel.Localiframe);
                        if (Toado != null)
                        {
                            hotel.LocalY = Toado.Lat;
                            hotel.LocalX = Toado.Lng;
                            _unitOfWork.Repository<SysHotel>().Update(hotel);
                        }
                    }
                }
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


        private List<ModelHotel> GetQuerySearch(List<ModelHotel> modelHotels, string ksdc, string sao, string thanhpho, string giatu, string giaden)
        {
            try
            {
                if (!modelHotels.Any())
                {
                    return new List<ModelHotel>();
                }
                //lấy toàn bộ tỉnh thành 
                var getContry = getTinhThanh();
                // tìm kiếm tên ks hoặc địa chỉ
                if (!string.IsNullOrEmpty(ksdc))
                {
                    // tìm kiếm tên tỉnh thành,
                    var timTenTinhThanh = getContry.Where(m => m.Name.ToLower().Contains(ksdc.ToLower())).Select(m => m.Id).ToList();
                    var timTenKhachSan = modelHotels.Where(m => m.Name.ToLower().Contains(ksdc.ToLower())).Select(m => m.Name.ToLower()).ToList();

                    modelHotels = modelHotels.Where(t =>
                                                    (!string.IsNullOrEmpty(t.Name) && timTenKhachSan.Contains(t.Name.ToLower())) ||
                                                    (!string.IsNullOrEmpty(t.Local) && t.Local.ToLower().Contains(ksdc.ToLower())) ||
                                                    (t.IdContry != null && timTenTinhThanh.Contains(t.IdContry.Value))
                                                ).ToList();
                }
                if (!string.IsNullOrEmpty(thanhpho))
                {
                    var listThanhPho = thanhpho.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(x => int.Parse(x.Trim()))
                                     .ToList();
                    modelHotels = modelHotels.Where(m => listThanhPho.Contains(m.IdContry.Value)).ToList();
                }
                if (!string.IsNullOrEmpty(giaden))
                {
                    modelHotels = modelHotels.Where(m => m.PriceMin.HasValue && m.PriceMin.Value >= Convert.ToDecimal(giatu) && m.PriceMin.Value <= Convert.ToDecimal(giaden)).ToList();
                }
                if (!string.IsNullOrEmpty(sao))
                {
                    var listSao = sao.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(x => int.Parse(x.Trim()))
                                     .ToList();

                    modelHotels = modelHotels.Where(m => m.NumStar.HasValue && listSao.Contains(m.NumStar.Value)).ToList();
                }

                return modelHotels;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Lấy giá trị Hotel theo bảng phòng và bảng booking
        /// </summary>
        /// <returns></returns>
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
                if (phong == 0 && Adults == 0 && Children == 0)
                {
                    foreach (var hotelGroup in roomsGroupedByHotel)
                    {
                        suitableHotels.Add((int)hotelGroup.Key);
                    }
                }
                else if (phong != 0 && Adults == 0 && Children == 0)
                {
                    foreach (var hotelGroup in roomsGroupedByHotel)
                    {
                        int totalRoomsAvailable = hotelGroup.Sum(r => r.RoomsAvailable);
                        bool passPhong = totalRoomsAvailable >= phong;
                        if (passPhong)
                        {
                            suitableHotels.Add((int)hotelGroup.Key);
                        }
                    }
                }
                else if (phong == 0 && (Adults != 0 || Children != 0))
                {
                    foreach (var hotelGroup in roomsGroupedByHotel)
                    {
                        // Tính tổng sức chứa người lớn và trẻ em theo số phòng trống
                        int totalAdultsCapacity = hotelGroup.Sum(r => r.MaxAdults * r.RoomsAvailable);
                        int totalChildrenCapacity = hotelGroup.Sum(r => r.MaxChildren * r.RoomsAvailable);
                        bool passAdults = totalAdultsCapacity >= Adults;
                        bool passChildren = totalChildrenCapacity >= Children;

                        if (passAdults && passChildren)
                        {
                            suitableHotels.Add((int)hotelGroup.Key);
                        }
                    }
                }
                else
                {
                    var selectedRooms = new List<object>(); 

                    // Giả sử roomsGroupedByHotel là danh sách các phòng đã được nhóm theo IdHotel
                    foreach (var hotelGroup in roomsGroupedByHotel)
                    {
                        int totalRoomsAvailable = hotelGroup.Sum(r => r.RoomsAvailable);
                        bool passPhong = totalRoomsAvailable >= phong;

                        if (passPhong)
                        {
                            // Khởi tạo lại danh sách phòng được chọn và số người còn lại cho mỗi khách sạn
                            selectedRooms.Clear();
                            int remainingAdults = Adults;
                            int remainingChildren = Children;

                            // Sắp xếp phòng theo MaxAdults, MaxChildren, RoomsAvailable giảm dần
                            var sortedRooms = hotelGroup
                                .OrderByDescending(room => room.MaxAdults)
                                .ThenByDescending(room => room.MaxChildren)
                                .ThenByDescending(room => room.RoomsAvailable)
                                .ToList();

                            // Chọn phòng để đáp ứng số người lớn và trẻ nhỏ
                            foreach (var room in sortedRooms)
                            {
                                int roomsNeededForAdults = (int)Math.Ceiling((double)remainingAdults / room.MaxAdults);
                                int roomsNeededForChildren = (int)Math.Ceiling((double)remainingChildren / room.MaxChildren);
                                int roomsToTake = Math.Max(roomsNeededForAdults, roomsNeededForChildren);

                                // Giới hạn số phòng lấy dựa trên RoomsAvailable và phong
                                roomsToTake = Math.Min(roomsToTake, room.RoomsAvailable);
                                if (selectedRooms.Count + roomsToTake > phong)
                                {
                                    roomsToTake = phong - selectedRooms.Count;
                                }

                                // Nếu còn cần phòng và phòng hiện tại có thể sử dụng
                                if (roomsToTake > 0)
                                {
                                    // Thêm phòng vào danh sách kết quả
                                    for (int i = 0; i < roomsToTake; i++)
                                    {
                                        selectedRooms.Add(room);
                                        remainingAdults -= room.MaxAdults;
                                        remainingChildren -= room.MaxChildren;
                                    }
                                }

                                // Nếu đã đáp ứng đủ số người hoặc đủ số phòng, thoát vòng lặp
                                if (remainingAdults <= 0 && remainingChildren <= 0 || selectedRooms.Count >= phong)
                                {
                                    break;
                                }
                            }

                            // Kiểm tra xem khách sạn có đáp ứng được yêu cầu về người lớn và trẻ nhỏ không
                            bool passAdults = remainingAdults <= 0;
                            bool passChildren = remainingChildren <= 0;

                            if (passPhong && passAdults && passChildren)
                            {
                                suitableHotels.Add((int)hotelGroup.Key);
                            }
                        }
                    }
                }


                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var defaultImagePath = "/AppData/no-image.png";

                var getHotel = _unitOfWork.Repository<SysHotel>()
                    .GetAll(m => suitableHotels.Contains(m.Id), includeProperties: "IdPromotionNavigation")
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
                            NumStar = h.NumStar,
                            Localiframe = h.Localiframe,
                            IdPromotion = h.IdPromotion,
                            IdPromotionNavigation = h.IdPromotionNavigation
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

        /// <summary>
        /// lấy toàn bộ tiện ích
        /// </summary>
        /// <returns></returns>
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
        bool TryParseDate_YY_to_YYYY(string input, out DateTime result)
        {
            result = DateTime.MinValue;
            if (DateTime.TryParseExact(input, "dd/MM/yy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var temp))
            {
                // Nếu năm nhỏ hơn 100 => ép thành 2000+năm
                if (temp.Year < 100)
                {
                    result = new DateTime(2000 + temp.Year % 100, temp.Month, temp.Day);
                }
                else
                {
                    result = temp;
                }
                return true;
            }
            return false;
        }

        [HttpPost]
        public JsonResult UpdateLike(int hotelId, bool liked)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập" });
                }

                var existingLike = _unitOfWork.Repository<SysLike>()
                   .GetAll(l => l.IdUser == userId.Value
                             && l.LikeItemId == hotelId
                             && l.Idcategory == 1)
                   .FirstOrDefault();

                if (existingLike != null)
                {
                    existingLike.Like = liked;
                    _unitOfWork.Repository<SysLike>().Update(existingLike);
                }
                else
                {
                    var newLike = new SysLike
                    {
                        IdUser = userId.Value,
                        LikeItemId = hotelId,
                        Idcategory = 1,
                        Like = liked
                    };
                    _unitOfWork.Repository<SysLike>().Insert(newLike);
                }
                _unitOfWork.Save();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Có thể ghi log ex.Message ở đây
                return Json(new { success = false, message = "Lỗi server" });
            }
        }

        public List<SysHotel> GetFilteredHotels([FromBody] FilterDataViewHotel filter)
        {
            var hotels = _unitOfWork.Repository<SysHotel>().GetAll(filter: (m => m.Status == true), includeProperties: "IdPromotionNavigation");

            #region Lọc theo khoảng tiền
            var roomsRepo = _unitOfWork.Repository<SysRoom>().GetAll();

            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue)
            {
                var filteredRoomHotelIds = roomsRepo
                    .Where(r => r.Price.HasValue && r.Price.Value >= filter.MinPrice.Value && r.Price.Value <= filter.MaxPrice.Value)
                    .Select(r => r.IdHotel)
                    .Distinct()
                    .ToList();

                hotels = hotels.Where(h => filteredRoomHotelIds.Contains(h.Id)).ToList();
            }
            #endregion

            #region Lọc theo số sao

            if (filter.NumStar != null && filter.NumStar.Count > 0)
            {
                hotels = hotels.Where(m => m.NumStar.HasValue && filter.NumStar.Contains(m.NumStar.Value)).ToList();
            }
            #endregion

            #region Lọc theo Thành phố
            if (filter.IdContry != null && filter.IdContry.Count > 0)
            {
                hotels = hotels.Where(m => m.IdContry.HasValue && filter.IdContry.Contains(m.IdContry.Value)).ToList();
            }
            #endregion

            #region Lọc theo Score, review, đặc biệt, giá, Sao
            var minPricesByHotel = GetMinRoomPricesByHotel();

            List<SortItem> sortList = new List<SortItem>();
            if (!string.IsNullOrEmpty(filter.SortOrders))
            {
                sortList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SortItem>>(filter.SortOrders);
            }

            if (sortList != null && sortList.Count > 0)
            {
                IOrderedEnumerable<SysHotel> orderedHotels = null;
                for (int i = 0; i < sortList.Count; i++)
                {
                    var sort = sortList[i];
                    Func<SysHotel, object> keySelector = sort.Key switch
                    {
                        "Featured" => h => h.Featured,
                        "Score" => h => h.Score,
                        "Reviews" => h => h.Reviews,
                        "Price" => h => minPricesByHotel.ContainsKey(h.Id) ? minPricesByHotel[h.Id] : 0,
                        "Start" => h => h.NumStar,
                        _ => h => h.Name
                    };

                    if (i == 0)
                    {
                        // Sắp xếp lần đầu tiên
                        orderedHotels = sort.Direction == "asc"
                            ? hotels.OrderBy(keySelector)
                            : hotels.OrderByDescending(keySelector);
                    }
                    else
                    {
                        // Sắp xếp các tiêu chí tiếp theo
                        orderedHotels = sort.Direction == "asc"
                            ? orderedHotels.ThenBy(keySelector)
                            : orderedHotels.ThenByDescending(keySelector);
                    }
                }

                hotels = orderedHotels.ToList();
            }
            #endregion

            #region Tìm theo tên thành phố/tỉnh thành
            if (!string.IsNullOrEmpty(filter.Location))
            {
                var listIDContry = _unitOfWork.Repository<CatContry>()
                    .GetAll(filter: c => c.Status == true && c.Name != null &&
                        c.Name.ToLower().Contains(filter.Location.ToLower()))
                    .Select(c => c.Id)
                    .ToList();

                hotels = hotels.Where(h => (!string.IsNullOrEmpty(h.Name) && h.Name.Contains(filter.Location, StringComparison.OrdinalIgnoreCase))
                                        || (!string.IsNullOrEmpty(h.Local) && h.Local.Contains(filter.Location, StringComparison.OrdinalIgnoreCase))
                                        || (h.IdContry != null && listIDContry.Contains(h.IdContry.Value))).ToList();
            }
            #endregion

            #region Tìm theo số người và số phòng
            int totalGuests = (filter.Adults ?? 0) + (filter.Children ?? 0);
            if (filter.Rooms.HasValue && filter.Rooms.Value > 0 && totalGuests > 0)
            {
                var hotelIds = _unitOfWork.Repository<SysRoom>()
                             .GetAll()
                             .GroupBy(r => r.IdHotel)
                             .Where(g =>
                             {
                                 // Lấy Rooms.Value phòng có sức chứa lớn nhất trong khách sạn này
                                 var topRooms = g
                                     .OrderByDescending(r => (r.AdultsMax ?? 0) + (r.ChildrenMax ?? 0))
                                     .Take(filter.Rooms.Value);

                                 int sumCapacity = topRooms.Sum(r => (r.AdultsMax ?? 0) + (r.ChildrenMax ?? 0));

                                 return sumCapacity >= totalGuests;
                             })
                             .Select(g => g.Key)
                             .ToList();

                hotels = hotels.Where(h => hotelIds.Contains(h.Id)).ToList();
            }
            else if (filter.Rooms.HasValue) // chỉ lọc theo số phòng
            {
                var hotelIds = _unitOfWork.Repository<SysRoom>()
                    .GetAll()
                    .GroupBy(r => r.IdHotel)
                    .Where(g => g.Count() >= filter.Rooms.Value)
                    .Select(g => g.Key)
                    .ToList();

                hotels = hotels.Where(h => hotelIds.Contains(h.Id)).ToList();
            }
            else if (totalGuests > 0) // chỉ lọc theo sức chứa tổng (tất cả phòng)
            {
                var hotelIds = _unitOfWork.Repository<SysRoom>()
                                 .GetAll()
                                 .GroupBy(r => r.IdHotel)
                                 .Where(g => g.Sum(r => (r.AdultsMax ?? 0) + (r.ChildrenMax ?? 0)) >= totalGuests)
                                 .Select(g => g.Key)
                                 .ToList();

                hotels = hotels.Where(h => hotelIds.Contains(h.Id)).ToList();
            }
            #endregion

            #region Tìm theo ngày đến - ngày đi

            DateTime startDateParsed = DateTime.MinValue;
            DateTime endDateParsed = DateTime.MinValue;


            if (!string.IsNullOrWhiteSpace(filter.StartDate) &&
                !string.IsNullOrWhiteSpace(filter.EndDate))
            {
                bool isStartValid = TryParseDate_YY_to_YYYY(filter.StartDate, out startDateParsed);
                bool isEndValid = TryParseDate_YY_to_YYYY(filter.EndDate, out endDateParsed);

                if (isStartValid && isEndValid)
                {
                    startDateParsed = startDateParsed.Date;
                    endDateParsed = endDateParsed.Date;
                    // 1. Lấy danh sách phòng đã booking trong khoảng thời gian giao nhau và status = 1
                    var bookedRoomIds = _unitOfWork.Repository<SysBooking>()
                    .GetAll(b =>
                        b.Status == 1 &&
                        b.StartDate.Date <= endDateParsed &&
                        b.EndDate.Date >= startDateParsed
                    )
                    .Select(b => b.BookingItemId)
                    .ToList();

                    // 2. Lấy tất cả phòng của các khách sạn đang lọc
                    var hotelIds = hotels.Select(h => h.Id).ToList();

                    var allRooms = _unitOfWork.Repository<SysRoom>()
                        .GetAll(r => r.IdHotel.HasValue && hotelIds.Contains(r.IdHotel.Value))
                        .ToList();

                    // 3. Nhóm các phòng chưa bị booked theo khách sạn
                    var availableRoomCountByHotel = allRooms
                        .Where(r => !bookedRoomIds.Contains(r.Id))
                        .GroupBy(r => r.IdHotel)
                        .Select(g => new
                        {
                            HotelId = g.Key,
                            AvailableRoomCount = g.Count()
                        })
                        .ToList();

                    // 4. Lọc các khách sạn có đủ số phòng còn trống
                    var filteredHotelIds = availableRoomCountByHotel
                        .Where(h => h.AvailableRoomCount >= (filter.Rooms ?? 1))
                        .Select(h => h.HotelId)
                        .ToList();

                    // 5. Cập nhật danh sách khách sạn
                    hotels = hotels.Where(h => filteredHotelIds.Contains(h.Id)).ToList();
                }
            }
            #endregion

            return hotels.ToList();
        }

        public Dictionary<int, double> GetMinRoomPricesByHotel()
        {
            var allRooms = _unitOfWork.Repository<SysRoom>().GetAll()
                .Where(r => r.Price.HasValue && r.IdHotel.HasValue)
                .GroupBy(r => r.IdHotel.Value)
                .Select(g => new
                {
                    IdHotel = g.Key,
                    MinPrice = g.Min(r => (double)r.Price.Value)
                })
                .ToList();

            return allRooms.ToDictionary(x => x.IdHotel, x => x.MinPrice);
        }

        [HttpPost]
        public IActionResult CheckLogin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return Json(new { isLoggedIn = userId.HasValue });
        }

        #region Gửi đánh giá
        [HttpPost]
        public IActionResult SubmitReview(int hotelId, string name, string email, string comment,
            int cleanliness, int facilities, int valueForMoney, int service, int location)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá", requireLogin = true });
                }

                // You could also retrieve this from database if needed
                int categoryId = 1; // Hotel category

                var review = new SysEvaluate
                {
                    IdUser = userId,
                    IdCategory = categoryId,
                    IdService = hotelId,
                    Name = name,
                    Email = email,
                    DateTime = DateTime.Now,
                    Comment = comment,
                    Cleanliness = cleanliness,
                    Facilities = facilities,
                    ValueForMoney = valueForMoney,
                    Service = service,
                    Location = location
                };

                // tính điểm trung bình từ các tiêu chí
                double avgReview = (cleanliness + facilities + valueForMoney + service + location) / 5.0;
                review.Avgreview = Math.Round(avgReview, 1); // làm tròn đến đến 1 chữ số thập phân 

                _unitOfWork.Repository<SysEvaluate>().Insert(review);
                _unitOfWork.Save();

                UpdateHotelAverageScore(hotelId);

                return Json(new
                {
                    success = true,
                    message = "Đánh giá của bạn đã được ghi nhận",
                    avgScore = review.Avgreview
                });
            }
            catch (Exception ex)
            {
                // Log the error (you may want to implement proper logging)
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi đánh giá" });
            }
        }
        #endregion

        #region cập nhật điểm đánh giá của khách sạn
        private void UpdateHotelAverageScore(int hotelId)
        {
            try
            {
                // Get all reviews for this hotel
                var reviews = _unitOfWork.Repository<SysEvaluate>()
                    .GetAll(e => e.IdService == hotelId && e.IdCategory == 1)
                    .ToList();

                if (reviews.Count > 0)
                {
                    // Calculate new average score and review count
                    double averageScore = reviews.Average(r => r.Avgreview ?? 0);
                    int reviewCount = reviews.Count;

                    // Update hotel record
                    var hotel = _unitOfWork.Repository<SysHotel>().GetById(hotelId);
                    if (hotel != null)
                    {
                        hotel.Score = Math.Round(averageScore, 1);
                        hotel.Reviews = reviewCount;

                        _unitOfWork.Repository<SysHotel>().Update(hotel);
                        _unitOfWork.Save();
                    }
                }
            }
            catch (Exception)
            {
                // Log error if necessary
            }
        }
        #endregion

        #region lấy điểm đánh giá từng danh mục của khách sạn
        private Dictionary<string, double> GetHotelCategoryRatings(int hotelId)
        {
            try
            {
                // Get all reviews for this hotel
                var hotelReviews = _unitOfWork.Repository<SysEvaluate>()
                    .GetAll(filter: e => e.IdService == hotelId && e.IdCategory == 1);

                var result = new Dictionary<string, double>
                {
                    // Default values if no reviews
                    { "cleanliness", 0 },
                    { "facilities", 0 },
                    { "valueForMoney", 0 },
                    { "service", 0 },
                    { "location", 0 },
                    { "count", 0 }
                };

                if (hotelReviews.Any())
                {
                    // Calculate averages for each category
                    result["cleanliness"] = Math.Round(hotelReviews.Average(r => r.Cleanliness ?? 0), 1);
                    result["facilities"] = Math.Round(hotelReviews.Average(r => r.Facilities ?? 0), 1);
                    result["valueForMoney"] = Math.Round(hotelReviews.Average(r => r.ValueForMoney ?? 0), 1);
                    result["service"] = Math.Round(hotelReviews.Average(r => r.Service ?? 0), 1);
                    result["location"] = Math.Round(hotelReviews.Average(r => r.Location ?? 0), 1);
                    result["count"] = hotelReviews.Count();
                }

                return result;
            }
            catch (Exception)
            {
                // Return default values if error occurs
                return new Dictionary<string, double>
                {
                    { "cleanliness", 0 },
                    { "facilities", 0 },
                    { "valueForMoney", 0 },
                    { "service", 0 },
                    { "location", 0 },
                    { "count", 0 }
                };
            }
        }
        #endregion

        #region Lấy danh sách đánh giá khách sạn
        private List<ReviewViewModel> GetHotelReviews(int hotelId)
        {
            try
            {
                // Get reviews for this hotel with category = 1 (hotels)
                var reviews = _unitOfWork.Repository<SysEvaluate>()
                    .GetAll(filter: e => e.IdService == hotelId && e.IdCategory == 1)
                    .OrderByDescending(e => e.DateTime)
                    .ToList();

                // Get all user IDs from reviews to fetch their avatars in bulk
                var userIds = reviews.Where(r => r.IdUser.HasValue).Select(r => r.IdUser.Value).Distinct().ToList();

                // Dictionary to store user avatars (key: userId, value: avatar path)
                var userAvatars = new Dictionary<int, string>();

                // Get guest users with their avatars (normal users with IdRole = 2)
                var guests = _unitOfWork.Repository<SysGuest>()
                    .GetAll(filter: g => userIds.Contains(g.Id) && g.IdRole == 2)
                    .ToList();

                foreach (var guest in guests)
                {
                    if (guest.Id > 0 && !userAvatars.ContainsKey(guest.Id))
                    {
                        userAvatars.Add(guest.Id, guest.Avatar);
                    }
                }

                // Get admin/partner users with their avatars
                var adminUsers = _unitOfWork.Repository<SysUser>()
                    .GetAll(filter: u => userIds.Contains(u.Id))
                    .ToList();

                foreach (var adminUser in adminUsers)
                {
                    if (adminUser.Id > 0 && !userAvatars.ContainsKey(adminUser.Id))
                    {
                        userAvatars.Add(adminUser.Id, adminUser.Avatar);
                    }
                }

                var result = new List<ReviewViewModel>();

                foreach (var review in reviews)
                {
                    string userAvatar = null;
                    if (review.IdUser.HasValue && userAvatars.ContainsKey(review.IdUser.Value))
                    {
                        userAvatar = userAvatars[review.IdUser.Value];
                    }

                    var reviewItem = new ReviewViewModel
                    {
                        Id = review.Id,
                        Name = review.Name ?? "Khách hàng",
                        Email = review.Email ?? "",
                        Date = review.DateTime,
                        Comment = review.Comment ?? "",
                        AverageScore = review.Avgreview ?? 0,
                        Cleanliness = review.Cleanliness ?? 0,
                        Facilities = review.Facilities ?? 0,
                        ValueForMoney = review.ValueForMoney ?? 0,
                        Service = review.Service ?? 0,
                        Location = review.Location ?? 0,
                        LikeCount = review.LikeCount ?? 0,
                        DislikeCount = review.DislikeCount ?? 0,
                        Avatar = userAvatar
                    };

                    result.Add(reviewItem);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Log the error message if needed
                // Console.WriteLine(ex.Message);

                // Return empty list if error occurs
                return new List<ReviewViewModel>();
            }
        }
        #endregion 

        #region Giao diện đặt phòng & kiểm tra khuyến mãi
        public IActionResult Datphong(string nameroom, int idp, string ci, string co, string gue)
        {
            try
            {
                // Parse dates from dd/MM/yyyy HH:mm format
                DateTime checkIn = DateTime.ParseExact(ci, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                DateTime checkOut = DateTime.ParseExact(co, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                if (HttpContext.Session.GetInt32("UserId") != null)
                {
                    var id = HttpContext.Session.GetInt32("UserId");
                    var user = _unitOfWork.Repository<SysUser>().GetById((int)id);
                    var room = _unitOfWork.Repository<SysRoom>().GetById(idp);
                    var localHotel = _unitOfWork.Repository<SysHotel>().GetById((int)room.IdHotel);

                    var promotions = _unitOfWork.Repository<SysPromotion>().GetAll(
                        filter: p =>
                            p.Status == true &&
                            p.StartDate <= DateTime.Now &&
                            p.EndDate >= DateTime.Now &&
                            p.SysHotels.Any(h => h.Id == localHotel.Id)
                    ).ToList();

                    bool hasPromotion = promotions.Any();

                    if (hasPromotion)
                    {
                        var promotion = promotions.First();
                        ViewBag.PromotionCode = promotion.Code;
                        ViewBag.SaleOff = promotion.SaleOff ?? 0;
                        ViewBag.PromotionDesc = promotion.Describe;
                    }
                    else
                    {
                        ViewBag.PromotionCode = null;
                        ViewBag.SaleOff = 0;
                        ViewBag.PromotionDesc = null;
                    }
                    if (room == null) return NotFound();
                    ViewBag.MaxGuests = (room.AdultsMax ?? 1) + (room.ChildrenMax ?? 1);

                    ViewBag.HasPromotion = hasPromotion;
                    ViewBag.localHotel = localHotel;
                    ViewBag.ThongtinRoom = room;
                    ViewBag.CI = checkIn.ToString("dd/MM/yyyy HH:mm");
                    ViewBag.CO = checkOut.ToString("dd/MM/yyyy HH:mm");
                    ViewBag.GUE = gue;

                    return View("DatKhachSan", user);
                }
                else
                {
                    return View("DatKhachSan");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult KiemTraMaKhuyenMai(string code, int tongTien)
        {
            var promotion = _unitOfWork.Repository<SysPromotion>().GetAll(
                filter: p =>
                    p.Status == true &&
                    p.StartDate <= DateTime.Now &&
                    p.EndDate >= DateTime.Now &&
                    p.Code != null &&
                    p.Code.ToLower() == code.ToLower()
            ).FirstOrDefault();
            decimal thanhTien = 0;
            decimal giamGia = 0;
            string hinhThucGiam = "";
            string idKM = "";
            if (promotion == null)
            {
                thanhTien = tongTien;
                idKM = "";
                giamGia = 0;
                return Json(new { thanhTien, idKM, giamGia, success = false, message = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn." });
            }

            if (promotion.ConditionNumber.HasValue && tongTien <= promotion.ConditionNumber.Value)
            {
                thanhTien = tongTien;
                idKM = "";
                giamGia = 0;
                return Json(new { thanhTien, idKM, giamGia, success = false, message = $"Mã này chỉ áp dụng cho hóa đơn trên {promotion.ConditionNumber.Value:N0} ₫" });
            }


            if (promotion.Type == true) // Giảm theo %
            {
                decimal sale = promotion.SaleOff ?? 0m;
                giamGia = (tongTien * sale) / 100m;
                hinhThucGiam = $"{promotion.SaleOff}%";
            }
            else // Giảm theo số tiền
            {
                giamGia = promotion.SaleOff ?? 0m;
                hinhThucGiam = $"{giamGia:N0} ₫";
            }

            thanhTien = tongTien - giamGia;

            return Json(new
            {
                idKM = promotion.Id,
                success = true,
                code = promotion.Code,
                saleOff = promotion.SaleOff,
                description = promotion.Describe,
                type = promotion.Type,
                giamGia,
                thanhTien,
                hinhThucGiam,

            });
        }
        [HttpPost]
        public JsonResult KiemTraNgay(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // Lấy danh sách các booking trùng
            var bookings = _unitOfWork.Repository<SysBooking>().GetAll(filter: b => b.BookingItemId == roomId &&
                       b.StartDate <= checkOut && b.EndDate >= checkIn).ToList();

            return Json(new { conflict = bookings });
        }

        #endregion 
        [HttpGet]
        public IActionResult ThanhToan()
        {
            try
            {
                return View("ThanhToan");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        public IActionResult ThanhToan(string firstName, string email, string phone, int userId,
                               DateTime checkin, DateTime checkout, int soNguoi, int idPhong,
                               int idKhuyenMai, decimal giaTienKM, decimal tongTien, decimal thanhTien)
        {
            try
            {
                var room = _unitOfWork.Repository<SysRoom>().GetAll(filter: r => r.Id == idPhong && r.Status == true).FirstOrDefault();
                var hotel = _unitOfWork.Repository<SysHotel>().GetAll(filter: h => h.Id == room.IdHotel && h.Status == true).FirstOrDefault();
                var user = _unitOfWork.Repository<SysUser>().GetAll(filter: u => u.Id == hotel.IdUser &&
                                                                    u.IdRole == 3 && u.Status == true).FirstOrDefault();
                var nameCard = _unitOfWork.Repository<CatBank>().GetAll(filter: n => n.Id == user.CardName && n.Status == true).FirstOrDefault();
                ViewBag.Roomsname = room.Name;

                var thongTinKhachHang = new SysBooking
                {
                    IdUser = userId,
                    FullNameGuest = firstName,
                    EmailGuest = email,
                    PhoneGuest = phone,
                    IdCategories = 1,
                    IdPromotion = idKhuyenMai,
                    StartDate = checkin,
                    EndDate = checkout,
                    GuestsNumber = soNguoi,
                    BookingDate = DateTime.Now,
                    BookingItemId = room.Id,
                    DiscountAmount = giaTienKM,
                    DiscountedPrice = thanhTien,
                    Price = tongTien,
                    DesQr = Common.GenerateRandomQrCode(),
                    Status = 2,
                };
                _unitOfWork.Repository<SysBooking>().Insert(thongTinKhachHang);
                _unitOfWork.Save();

                var getBooking = _unitOfWork.Repository<SysBooking>().GetById(thongTinKhachHang.Id);
                var ConnentQRCode = new ModelQRCode()
                {
                    des = getBooking.DesQr,
                    acc = user?.CardNumber,
                    bank = nameCard.KeyBank,
                    amount = getBooking.DiscountedPrice ?? 0,
                };
                ViewBag.ConnentQRCode = JsonSerializer.Serialize(ConnentQRCode);
                return View("ThanhToan", thongTinKhachHang);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region tìm phòng trống dựa vào ngày và số khách
        [HttpPost]
        public IActionResult SearchAvailableRooms(int hotelId, string checkIn, string checkOut, int adults, int children)
        {
            try
            {
                DateTime startDate = DateTime.ParseExact(checkIn, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(checkOut, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                // Lấy tất cả các phòng của khách sạn
                var allRooms = _unitOfWork.Repository<SysRoom>()
                .GetAll(filter: r => r.IdHotel == hotelId && r.Status == true,
                includeProperties: "TypeRoomNavigation")
                .ToList();

                // Lấy các phòng đã được đặt trong khoảng thời gian đã chọn
                var bookedRoomIds = _unitOfWork.Repository<SysBooking>()
                .GetAll(b => b.Status == 1 &&
                b.StartDate <= endDate &&
                b.EndDate >= startDate)
                .Select(b => b.BookingItemId)
                .ToList();

                // Lọc ra những phòng đã được đặt và kiẻm tra số khách
                var availableRooms = allRooms
                .Where(r => !bookedRoomIds.Contains(r.Id) &&
                (r.AdultsMax ?? 0) >= adults &&
                (r.ChildrenMax ?? 0) >= children)
                .ToList();

                ViewBag.Checkin = checkIn;
                ViewBag.Checkout = checkOut;
                ViewBag.Adults = adults;
                ViewBag.Children = children;
                ViewBag.Guests = adults + children;
                ViewBag.HotelAmenities = _unitOfWork.Repository<CatAminitieseRoom>()
                .GetAll(filter: a => a.Status == true)
                .ToList();

                return PartialView("_AvailableRooms", availableRooms);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchAvailableRooms: {ex.Message}");
                // Return error message as HTML
                return Content("<div class='alert alert-danger'>Có lỗi xảy ra khi tìm kiếm phòng. Vui lòng thử lại sau.</div>");
            }
        }
        #endregion

        #region ghi log cho cuộc gọi/Zalo 
        [HttpPost]
        public IActionResult LogCall(int dichvu, int khachsan, bool phone, bool zalo)
        {
            try
            {
                var log = new SysCallLog
                {

                    IdCategory = dichvu,
                    IdService = khachsan,
                    Phone = phone,
                    Zalo = zalo,
                    Time = DateTime.Now,
                    Ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
                    Url = !string.IsNullOrEmpty(HttpContext.Request.Headers["Referer"].ToString())
                        ? new Uri(HttpContext.Request.Headers["Referer"].ToString()).PathAndQuery : ""
                };

                _unitOfWork.Repository<SysCallLog>().Insert(log);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Ghi log thành công" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Lỗi" });
            }
        }
        #endregion
    }
}