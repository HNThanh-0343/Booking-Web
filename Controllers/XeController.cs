using Microsoft.AspNetCore.Mvc;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class XeController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public XeController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        [HttpGet("xe")]
        public IActionResult Index(int? page)
        {
            try
            {
                var cars = _unitOfWork.Repository<SysCar>().GetAll(filter: (m => m.Status == true));

              
                ViewBag.Cars = cars;
                ViewBag.HotelsCountByUser = cars.Count();
                #region Page
                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 9;
                var pageListView = cars.ToPagedList(page ?? 1, pageSize);
                #endregion
                return View(pageListView);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        [HttpGet("xe/{namecar}")]
        public IActionResult dsXe(int? page, string namecar, int ks)
        {
            try
            {
                var user = _unitOfWork.Repository<SysUser>().GetAll(filter: (u => u.Id == ks)).FirstOrDefault();
                if (user == null)
                {
                    return NotFound();
                }
                var cars = _unitOfWork.Repository<SysCar>().GetAll(filter: (m => m.IdUser == user.Id && m.Status == true));

               
                ViewBag.CarsCountByUser = cars.Count();
                ViewBag.userName = user?.Name;
                ViewBag.Cars = cars;
                #region Page
                page = page == null ? 1 : page;
                page = page < 1 ? 1 : page;
                var pageSize = 9;
                var pageListView = cars.ToPagedList(page ?? 1, pageSize);
                #endregion
                return View("Index",pageListView);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
