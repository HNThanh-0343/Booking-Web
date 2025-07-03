using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;


namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class HoatDongController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public HoatDongController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        [HttpGet("hoatdong")]
        public IActionResult Index(int? page)
        {
            try
            {
                var hoatdongs = _unitOfWork.Repository<SysActivity>().GetAll(filter: (m => m.Status == true));


                ViewBag.hoatdongs = hoatdongs;
                ViewBag.hoatdongsCountByUser = hoatdongs.Count();
                #region Page
                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 9;
                var pageListView = hoatdongs.ToPagedList(page ?? 1, pageSize);
                #endregion
                return View(pageListView);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        [HttpGet("hoatdong/{nameactivity}")]
        public IActionResult dsHoatDong(int? page, string nameactivity, int ks)
        {
            try
            {
                var user = _unitOfWork.Repository<SysUser>().GetAll(filter: (u => u.Id == ks)).FirstOrDefault();
                if (user == null)
                {
                    return NotFound();
                }
                var hoatdongs = _unitOfWork.Repository<SysActivity>().GetAll(filter: (m => m.IdUser == user.Id && m.Status == true));
                ViewBag.hoatdongsCountByUser = hoatdongs.Count();
                ViewBag.userName = user?.Name;
                #region Page
                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 9;
                var pageListView = hoatdongs.ToPagedList(page ?? 1, pageSize);
                #endregion
                return View("Index", pageListView);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        [HttpGet("hoatdong/{namehotel}/nameroom")]
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
    }
}
