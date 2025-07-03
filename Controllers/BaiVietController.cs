using Azure;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Security.Principal;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class BaiVietController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public BaiVietController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        [HttpGet("baiviet")]
        public IActionResult Index(int? page)

        {
            var types = _unitOfWork.Repository<CatTypeBlog>().GetAll(filter: (t => t.Status == true)).ToList();
            var blogs = _unitOfWork.Repository<SysBlog>().GetAll(filter: (b => b.Status == true), includeProperties: "IdUserNavigation,IdTypeBlogNavigation")
                .OrderByDescending(a => a.DateCreate).ToList();


            var blogCounts = types.Select(b => new CountryHotelCountViewModel
            {
                Id = b.Id,
                Name = b.Name,
                Count = blogs.Count(h => h.IdTypeBlog == b.Id)
            }).Where(b => b.Count > 0).ToList();

            var allTags = blogs
                .Where(b => !string.IsNullOrEmpty(b.Tag))
                .SelectMany(b => b.Tag.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim())
                .Distinct()
                .ToList();

            ViewBag.AllTags = allTags;

            ViewBag.blogTotal = blogs.Count();
            ViewBag.blogCounts = blogCounts;
            ViewBag.Types = types;
            #region Page
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 6;
            var pageListView = blogs.ToPagedList(page ?? 1, pageSize);
            #endregion
            return View(pageListView);
        }
        public IActionResult loadBaiViet(int? page, string searchString, int? idLoaiBaiViet, string? tag)
        {
            try
            {
                var blogs = _unitOfWork.Repository<SysBlog>()
                    .GetAll(filter: (b => b.Status == true), includeProperties: "IdUserNavigation,IdTypeBlogNavigation")
                    .OrderByDescending(a => a.DateCreate).ToList();

                if (!string.IsNullOrEmpty(searchString) || !string.IsNullOrEmpty(tag))
                {
                    var lowerSearch = searchString?.ToLower() ?? "";
                    var lowerTag = tag?.ToLower() ?? "";

                    blogs = blogs.Where(b =>
                        (
                            (!string.IsNullOrEmpty(b.Name) &&
                                b.Name.ToLower().Contains(lowerSearch)) ||
                            (!string.IsNullOrEmpty(b.IdUserNavigation?.Name) &&
                                b.IdUserNavigation.Name.ToLower().Contains(lowerSearch))) &&
                            (string.IsNullOrEmpty(lowerTag) || (!string.IsNullOrEmpty(b.Tag) &&
                                b.Tag.Split(',', StringSplitOptions.RemoveEmptyEntries).Any(t => t.Trim().ToLower() == lowerTag)))
                    ).ToList();
                }


                if (idLoaiBaiViet.HasValue)
                {
                    blogs = blogs.Where(b => b.IdTypeBlog == idLoaiBaiViet.Value).ToList();
                }
                ViewBag.blogTotal = blogs.Count();
                ViewBag.CurrentFilter = searchString;
                ViewBag.CurrentCategory = idLoaiBaiViet;

                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 6;
                var pageListView = blogs.ToPagedList(page ?? 1, pageSize);

                return PartialView("loadBaiViet", pageListView);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult ChiTiet(string nameblog, int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var userType = HttpContext.Session.GetString("UserType");
                ViewBag.UserType = userType;
                var blog = _unitOfWork.Repository<SysBlog>().GetById(id);
                if (blog == null) return NotFound();

                var allblogs = _unitOfWork.Repository<SysBlog>().GetAll(
                    filter: b => b.Status == true && b.Id == id,
                    includeProperties: "IdTypeBlogNavigation"
                );
                ViewBag.NameType = allblogs.FirstOrDefault()?.IdTypeBlogNavigation?.Name;

                // bài viết trước
                var previousPost = _unitOfWork.Repository<SysBlog>().GetAll(
                    filter: b => b.Status == true && b.DateCreate > blog.DateCreate
                ).OrderBy(b => b.DateCreate).FirstOrDefault();

                // bài viết sau
                var nextPost = _unitOfWork.Repository<SysBlog>().GetAll(
                    filter: b => b.Status == true && b.DateCreate < blog.DateCreate
                ).OrderByDescending(b => b.DateCreate).FirstOrDefault();

                // bình luận
                var cate = _unitOfWork.Repository<SysMenu>().GetAll(m => m.Name.ToLower().Contains("bài viết")).FirstOrDefault();
                var allBinhLuan = _unitOfWork.Repository<SysEvaluate>().
                    GetAll(filter: a => a.IdService == id && a.IdCategory == cate.Id)
                    .OrderByDescending(a => a.DateTime).ToList();
                var user = _unitOfWork.Repository<SysGuest>().GetAll(filter: u => u.Status == true).ToDictionary(u => u.Id, u => u.Avatar);
                var binhLuanWithAvatars = allBinhLuan.Select(b => new BinhLuanBaiViet
                {
                    Id = b.Id,
                    Name = b.Name ?? "",
                    Comment = b.Comment ?? "",
                    DateTime = b.DateTime,
                    Avatar = b.IdUser.HasValue && user.ContainsKey(b.IdUser.Value) ? user[b.IdUser.Value] : "/assets/img/user.png"
                }).ToList();
                // bài viết trong ngày
               
                var blogInDay = _unitOfWork.Repository<SysBlog>().GetAll(filter: bl => bl.Status == true)
                .OrderByDescending(bl => bl.DateCreate).Take(4).ToList();
                ViewBag.blogInDay = blogInDay;
                ViewBag.User = userId;
                ViewBag.alllBinhLuan = binhLuanWithAvatars;
                ViewBag.PreviousPost = previousPost;
                ViewBag.NextPost = nextPost;

                return View("ChiTietBaiViet", blog);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        [HttpPost]
        public IActionResult BinhLuan(int idBaiViet, string text)
        {
            try
            {
                var baiViet = _unitOfWork.Repository<SysBlog>().GetById(idBaiViet);
                if (baiViet == null) return NotFound();

                var userType = HttpContext.Session.GetString("UserType");
                if (userType == "Guest")
                {
                    var user = Account.GetGuest();
                    if (user == null) return Unauthorized();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var cate = _unitOfWork.Repository<SysMenu>().GetAll(m => m.Name.ToLower().Contains("bài viết")).FirstOrDefault();

                        _unitOfWork.Repository<SysEvaluate>().Insert(new SysEvaluate
                        {
                            IdUser = user.Id,
                            IdService = idBaiViet,
                            IdCategory = cate?.Id,
                            Name = user.Name,
                            Email = user.Email,
                            DateTime = DateTime.Now,
                            Comment = text
                        });
                    }
                }
                else
                {
                    return Unauthorized();
                }

                var category = _unitOfWork.Repository<SysMenu>().GetAll(m => m.Name.ToLower().Contains("bài viết")).FirstOrDefault();
                var allBinhLuan = _unitOfWork.Repository<SysEvaluate>().GetAll(filter: a => a.IdService == idBaiViet && 
                                    a.IdCategory == category.Id).OrderByDescending(a => a.DateTime).ToList();


                var users = _unitOfWork.Repository<SysGuest>().GetAll(filter: u => u.Status == true)
                    .ToDictionary(u => u.Id, u => u.Avatar);

                var binhLuanWithAvatars = allBinhLuan.Select(b => new BinhLuanBaiViet
                {
                    Id = b.Id,
                    Name = b.Name ?? "",
                    Comment = b.Comment ?? "",
                    DateTime = b.DateTime,
                    Avatar = b.IdUser.HasValue && users.ContainsKey(b.IdUser.Value) ? users[b.IdUser.Value] : "/assets/img/user.png"
                }).ToList();

                // Trả về partial view chứa danh sách bình luận
                return PartialView("BinhLuan", binhLuanWithAvatars);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
