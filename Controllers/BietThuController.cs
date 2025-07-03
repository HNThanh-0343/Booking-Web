using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;


namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class BietThuController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public BietThuController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public IActionResult Index(int? page)
        {
            var villa = _unitOfWork.Repository<SysVilla>().GetAll(filter: (m => m.Status == true));
            ViewBag.Villas = villa;
            ViewBag.VillaCount = villa.Count();
            #region Page
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 9;
            var pageListView = villa.ToPagedList(page ?? 1, pageSize);
            #endregion
            return View(pageListView);
        }
        [HttpGet("bietthu/{namevilla}")]
        public IActionResult dsBietThu(int? page, string namevilla, int ks)
        {
            var user = _unitOfWork.Repository<SysUser>().GetAll(filter: (m=>m.Id == ks)).FirstOrDefault();
            if(user == null)
            {
                NotFound();
            }
            var villa = _unitOfWork.Repository<SysVilla>().GetAll(filter: (m => m.Status == true && m.IdUser == user.Id));
            ViewBag.VillaCount = villa.Count();
            ViewBag.UserName = user?.Name;
            ViewBag.Villas = villa;
            #region Page
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 9;
            var pageListView = villa.ToPagedList(page ?? 1, pageSize);
            #endregion
            return View("Index",pageListView);
        }
    }
}
