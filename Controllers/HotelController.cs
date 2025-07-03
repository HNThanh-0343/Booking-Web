using Microsoft.AspNetCore.Mvc;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class HotelController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public HotelController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        [HttpGet("hotelss/{slug}")]
        public IActionResult Detail(string slug, int Id)
        {
            var getAllCate = _unitOfWork.Repository<SysHotel>().GetById(Id);
            if (getAllCate == null)
            {
                return NotFound();
            }
            ViewBag.GetAllHotel = (from a in _unitOfWork.Repository<SysHotel>().GetAll(filter: (m => m.Status == true), take: 10, orderBy: (m => m.OrderByDescending(d => d.Id)))
                                   select new HotelViewDetail()
                                   {
                                       Id = a.Id,
                                       //Price = a.Price,
                                       Name = a.Name,
                                       Discount = "",
                                       Image = !string.IsNullOrEmpty(a.ListImg) && a.ListImg.Contains(",")
                                                       ? a.ListImg.Split(',')[0]
                                                       : (!string.IsNullOrEmpty(a.ListImg) ? a.ListImg : ""),
                                       Url = $"/hotel/{Common.GenerateSlug(a.Name)}?id={a.Id}",
                                       //Like = a.Like ?? false,
                                       Featured = a.Featured ?? false,
                                       Amenities = a.Amenities
                                   }).ToList();
            return View(getAllCate);
        }
    }
}
