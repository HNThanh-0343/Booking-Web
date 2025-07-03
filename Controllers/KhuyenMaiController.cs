using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class KhuyenMaiController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public KhuyenMaiController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult loadKhuyenMai(int? page)
        {
            try
            {
                var km = _unitOfWork.Repository<SysPromotion>().GetAll(filter: k => k.Status == true).OrderByDescending(a => a.StartDate).ToList();
                #region Page
                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 6;
                var pageListView = km.ToPagedList(page ?? 1, pageSize);
                #endregion
                return PartialView("loadKhuyenMai", pageListView);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
