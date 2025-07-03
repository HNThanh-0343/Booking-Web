using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using WEBSITE_TRAVELBOOKING.Helper;
using X.PagedList;
using System.Diagnostics.Metrics;
using Microsoft.IdentityModel.Tokens;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class TourController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public TourController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        [HttpGet("tour")]
        public IActionResult Index(int? page)
        {
            try
            {
                var type = _unitOfWork.Repository<CatTypeActivity>().GetAll(filter: (t => t.Status == true)).ToList();
                ViewBag.Types = type;

                var countries = _unitOfWork.Repository<CatContry>().GetAll(filter: (c => c.Status == true)).ToList();
                ViewBag.Countries = countries;

                var TinhThanh = _unitOfWork.Repository<CatContry>().GetAll(filter: (m => m.Status == true && m.Featured == true));
                ViewBag.TinhThanh = TinhThanh.Select(l => l.Name).ToList();

                var tours = _unitOfWork.Repository<SysTour>().GetAll(filter: (m => m.Status == true), includeProperties: "IdPromotionNavigation");

                var countryCounts = countries
               .Select(c => new CountryHotelCountViewModel
               {
                   Id = c.Id,
                   Name = c.Name ?? "Không xác định",
                   Count = tours.Count(h => h.IdContry == c.Id)
               })
               .Where(c => c.Count > 0)
               .ToList();
                ViewBag.CountryCounts = countryCounts;

                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    var likedToursIds = _unitOfWork.Repository<SysLike>()
                        .GetAll(l => l.IdUser == userId.Value && l.Idcategory == 3 && l.Like == true)
                        .Select(l => l.LikeItemId)
                        .ToList();

                    ViewBag.LikedTourIds = likedToursIds;

                    tours = tours
                    .OrderByDescending(h => likedToursIds.Contains(h.Id))
                    .ThenBy(h => h.Name)
                    .ToList();
                }

                var maxPrice = _unitOfWork.Repository<SysTour>().GetAll().Max(r => r.Price);
                ViewBag.MaxTourPrice = maxPrice;

                ViewBag.tours = tours;
                ViewBag.toursCountByUser = tours.Count();
                return View();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult loadFilter([FromBody] TourFilterViewModel filter)
        {
            try
            {
                var tours = GetFilteredTours(filter);

                #region Lọc danh sách tim lên đầu
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    var likedToursIds = _unitOfWork.Repository<SysLike>()
                        .GetAll(l => l.IdUser == userId.Value && l.Idcategory == 3 && l.Like == true)
                        .Select(l => l.LikeItemId)
                        .ToList();

                    ViewBag.LikedTourIds = likedToursIds;

                    tours = tours
                    .OrderByDescending(h => likedToursIds.Contains(h.Id))
                    .ThenBy(h => h.Name)
                    .ToList();
                }
                #endregion

                #region Danh sách hiển thị các tour có cùng thành phố
                var count = tours.Count();

                if (count < 6)
                {
                    // Lấy danh sách IdContry từ các tour đã lọc
                    var contryIds = tours
                        .Where(h => h.IdContry.HasValue)
                        .Select(h => h.IdContry.Value)
                        .Distinct()
                        .ToList();

                    // Tìm khách sạn khác có cùng IdContry, loại trừ các khách sạn đã có
                    var suggestedTours = _unitOfWork.Repository<SysTour>()
                        .GetAll(h =>
                            h.IdContry.HasValue &&
                            contryIds.Contains(h.IdContry.Value) &&
                            !tours.Select(x => x.Id).Contains(h.Id)
                        )
                        .OrderByDescending(h => h.Name)
                        .ToList();

                    ViewBag.SuggestedTours = suggestedTours;
                }
                #endregion

                #region Danh sách hiển thị các tour có trong các thành phố nổi bật
                var tinhThanhIds = _unitOfWork.Repository<CatContry>()
                    .GetAll(filter: m => m.Status == true && m.Featured == true)
                    .Select(m => m.Id)
                    .ToList();
                var toursTT = _unitOfWork.Repository<SysTour>()
                    .GetAll(filter: h => tinhThanhIds.Contains(h.IdContry.Value))
                    .ToList();
                ViewBag.SuggestedtoursTT = toursTT;
                #endregion

                ViewBag.tours = tours;
                ViewBag.toursCountByUser = tours.Count();

                #region Page
                filter.page = filter.page == null ? 1 : filter.page;
                filter.page = filter.page < 1 ? 1 : filter.page;
                var pageSize = 6;
                var pageListView = tours.ToPagedList(filter.page ?? 1, pageSize);
                #endregion

                return PartialView("loadFilter", pageListView);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool TryParseDate_YY_to_YYYY(string input, out DateTime result)
        {
            result = DateTime.MinValue;
            if (DateTime.TryParseExact(input, "dd/MM/yy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var temp))
            {
                // If year is less than 100, make it 2000+year
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
        public JsonResult UpdateLike(int tourId, bool liked)
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
                             && l.LikeItemId == tourId
                             && l.Idcategory == 3)
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
                        LikeItemId = tourId,
                        Idcategory = 3,
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
        public List<SysTour> GetFilteredTours([FromBody] TourFilterViewModel filter)
        {
            var tours = _unitOfWork.Repository<SysTour>().GetAll(filter: (m => m.Status == true), includeProperties: "IdPromotionNavigation");

            #region Lọc theo khoảng tiền
            var tourRepo = _unitOfWork.Repository<SysTour>().GetAll();

            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue)
            {
                var filteredtourIds = tourRepo
                    .Where(r => r.Price.HasValue && r.Price.Value >= filter.MinPrice.Value && r.Price.Value <= filter.MaxPrice.Value)
                    .Select(r => r.Id)
                    .ToList();

                tours = tours.Where(h => filteredtourIds.Contains(h.Id)).ToList();
            }
            #endregion

            #region Lọc theo số sao

            if (filter.NumStar != null && filter.NumStar.Count > 0)
            {
                tours = tours.Where(m => m.NumStar.HasValue && filter.NumStar.Contains(m.NumStar.Value)).ToList();
            }
            #endregion

            #region Lọc theo Thành phố
            if (filter.IdContry != null && filter.IdContry.Count > 0)
            {
                tours = tours.Where(m => m.IdContry.HasValue && filter.IdContry.Contains(m.IdContry.Value)).ToList();
            }
            #endregion

            #region Lọc theo Score, review, đặc biệt, giá, Sao

            List<SortItem> sortList = new List<SortItem>();
            if (!string.IsNullOrEmpty(filter.SortOrders))
            {
                sortList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SortItem>>(filter.SortOrders);
            }

            if (sortList != null && sortList.Count > 0)
            {
                IOrderedEnumerable<SysTour> orderedTours = null;
                for (int i = 0; i < sortList.Count; i++)
                {
                    var sort = sortList[i];
                    Func<SysTour, object> keySelector = sort.Key switch
                    {
                        "Featured" => h => h.Featured,
                        "Score" => h => h.Score,
                        "Reviews" => h => h.Reviews,
                        "Price" => h => h.Price,
                        "Start" => h => h.NumStar,
                        _ => h => h.Name
                    };

                    if (i == 0)
                    {
                        // Sắp xếp lần đầu tiên
                        orderedTours = sort.Direction == "asc"
                            ? tours.OrderBy(keySelector)
                            : tours.OrderByDescending(keySelector);
                    }
                    else
                    {
                        // Sắp xếp các tiêu chí tiếp theo
                        orderedTours = sort.Direction == "asc"
                            ? orderedTours.ThenBy(keySelector)
                            : orderedTours.ThenByDescending(keySelector);
                    }
                }

                tours = orderedTours.ToList();
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

                tours = tours.Where(t => (!string.IsNullOrEmpty(t.Name) && t.Name.Contains(filter.Location, StringComparison.OrdinalIgnoreCase)) ||
                                         (!string.IsNullOrEmpty(t.LocalText) && t.LocalText.Contains(filter.Location, StringComparison.OrdinalIgnoreCase))
                                        || (t.IdContry != null && listIDContry.Contains(t.IdContry.Value))).ToList();
            }
            #endregion

            #region Tìm theo số người
            if (filter.GuestNumber.HasValue && filter.GuestNumber.Value > 0)
            {
                tours = tours.Where(t => t.MaxGuest.HasValue && t.MaxGuest >= filter.GuestNumber.Value).ToList();
            }
            #endregion

            #region Tìm theo ngày đến - ngày đi

            if (!string.IsNullOrWhiteSpace(filter.StartDate) && !string.IsNullOrWhiteSpace(filter.EndDate))
            {
                if (TryParseDate_YY_to_YYYY(filter.StartDate, out DateTime startDateParsed) &&
                    TryParseDate_YY_to_YYYY(filter.EndDate, out DateTime endDateParsed))
                {
                    startDateParsed = startDateParsed.Date;
                    endDateParsed = endDateParsed.Date;

                    // Filter tours by date availability
                    tours = tours.Where(t =>
                        (!t.StartDate.HasValue || t.StartDate.Value <= endDateParsed) &&
                        (!t.EndDate.HasValue || t.EndDate.Value >= startDateParsed)
                    ).ToList();
                }
            }
            #endregion

            #region Lọc theo type
            if (filter.IdType != null)
            {
                tours = tours.Where(m => m.IdType.HasValue && m.IdType.Value == filter.IdType).ToList();
            } 
            #endregion

            return tours.ToList();
        }

        [HttpGet("tour/{nametour}")]
        public IActionResult dsTour(int? page, string nametour, int t, int? reviewsPage)
        {
            try
            {
                var tours = _unitOfWork.Repository<SysTour>().GetById(t);
                if (tours == null)
                {
                    return NotFound();
                }

                // Get related tours with the same IdContry
                var relatedTours = _unitOfWork.Repository<SysTour>()
                    .GetAll(filter: m => m.Status == true &&
                                        m.IdContry == tours.IdContry &&
                                        m.Id != tours.Id)
                    .OrderByDescending(m => m.Featured)
                    .ThenByDescending(m => m.Score)
                    .Take(4)
                    .ToList();

                // Get tour category reviews and ratings
                var categoryRatings = GetTourCategoryRatings(tours.Id);
                ViewBag.CategoryRatings = categoryRatings;

                // Get actual tour reviews with user info
                var tourReviews = GetTourReviews(tours.Id);
                ViewBag.TourReviews = tourReviews;

                // Get tour itinerary information
                var tourItinerary = GetTourItinerary(tours.Id);
                ViewBag.TourItinerary = tourItinerary;

                // Get "Why Book With Us" data
                var whyBookWithUs = _unitOfWork.Repository<CatWhyBookWithU>()
                    .GetAll(filter: w => w.Status == true)
                    .ToList();
                ViewBag.WhyBookWithUs = whyBookWithUs;

                // Get FAQ data 
                var faqs = _unitOfWork.Repository<CatFaq>()
                    .GetAll(filter: f => f.Status == true)
                    .OrderBy(f => f.Id)
                    .ToList();
                ViewBag.TourFaqs = faqs;

                // Check if it's an AJAX request for just the reviews
                if (HttpContext.Request.Query.ContainsKey("ajaxReviews"))
                {
                    return PartialView("ChiTietTour", tours);
                }

                return View("ChiTietTour", tours);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult CheckLogin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return Json(new { isLoggedIn = userId.HasValue });
        }

        #region Gửi đánh giá
        [HttpPost]
        public IActionResult SubmitReview(int tourId, string name, string email, string comment, 
            int cleanliness, int facilities, int valueForMoney, int service, int location)
        {
            try
            {
                // Check if user is logged in
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá", requireLogin = true });
                }

                // Get the category ID for tours (3)
                int categoryId = 3; // Tour category
                
                // Create new review
                var review = new SysEvaluate
                {
                    IdUser = userId,
                    IdCategory = categoryId,
                    IdService = tourId,
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

                // Calculate average review score (from 5 criteria)
                double avgReview = (cleanliness + facilities + valueForMoney + service + location) / 5.0;
                review.Avgreview = Math.Round(avgReview, 1); // Round to 1 decimal place
                
                // Insert the review
                _unitOfWork.Repository<SysEvaluate>().Insert(review);
                _unitOfWork.Save();
                
                // Update tour's average score
                UpdateTourAverageScore(tourId);
                
                // Return success response
                return Json(new { 
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

        #region cập nhật điểm đánh giá của tour
        private void UpdateTourAverageScore(int tourId)
        {
            try
            {
                // Get all reviews for this tour
                var reviews = _unitOfWork.Repository<SysEvaluate>()
                    .GetAll(e => e.IdService == tourId && e.IdCategory == 3)
                    .ToList();
                
                if (reviews.Count > 0)
                {
                    // Calculate new average score and review count
                    double averageScore = reviews.Average(r => r.Avgreview ?? 0);
                    int reviewCount = reviews.Count;
                    
                    // Update tour record
                    var tour = _unitOfWork.Repository<SysTour>().GetById(tourId);
                    if (tour != null)
                    {
                        tour.Score = Math.Round(averageScore, 1);
                        tour.Reviews = reviewCount;
                        
                        _unitOfWork.Repository<SysTour>().Update(tour);
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

        #region lấy điểm đánh giá từng danh mục của tour
        private Dictionary<string, double> GetTourCategoryRatings(int tourId)
        {
            try
            {
                // Get all reviews for this tour
                var tourReviews = _unitOfWork.Repository<SysEvaluate>()
                    .GetAll(filter: e => e.IdService == tourId && e.IdCategory == 3);
                
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
                
                if (tourReviews.Any())
                {
                    // Calculate averages for each category
                    result["cleanliness"] = Math.Round(tourReviews.Average(r => r.Cleanliness ?? 0), 1);
                    result["facilities"] = Math.Round(tourReviews.Average(r => r.Facilities ?? 0), 1);
                    result["valueForMoney"] = Math.Round(tourReviews.Average(r => r.ValueForMoney ?? 0), 1);
                    result["service"] = Math.Round(tourReviews.Average(r => r.Service ?? 0), 1);
                    result["location"] = Math.Round(tourReviews.Average(r => r.Location ?? 0), 1);
                    result["count"] = tourReviews.Count();
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

        #region Lấy danh sách đánh giá tour
        private List<ReviewViewModel> GetTourReviews(int tourId)
        {
            try
            {
                // Get reviews for this tour with category = 3 (tours)
                var reviews = _unitOfWork.Repository<SysEvaluate>()
                    .GetAll(filter: e => e.IdService == tourId && e.IdCategory == 3)
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
                    // Try to get avatar from the combined avatar dictionary
                    string userAvatar = null;
                    if (review.IdUser.HasValue && userAvatars.ContainsKey(review.IdUser.Value))
                    {
                        userAvatar = userAvatars[review.IdUser.Value];
                    }
                    
                    // Create strongly-typed view model for review data
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

        #region Lấy thông tin hành trình tour
        private List<CatItinerary> GetTourItinerary(int tourId)
        {
            try
            {
                // Get tour details to find the itinerary ID
                var tour = _unitOfWork.Repository<SysTour>().GetById(tourId);
                if (tour == null || !tour.IdItinerary.HasValue)
                {
                    return new List<CatItinerary>();
                }

                // Get itinerary details by tour ID
                var itineraryItems = _unitOfWork.Repository<CatItinerary>()
                    .GetAll(filter: i => i.IdTour == tourId)
                    .OrderBy(i => i.Day)
                    .ToList();

                return itineraryItems;
            }
            catch (Exception)
            {
                // Return empty list if error occurs
                return new List<CatItinerary>();
            }
        }
        #endregion
    }
}
