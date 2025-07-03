using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class ThanhPhoController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public ThanhPhoController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public IActionResult Index()
        {
            return View(); // Trả về giao diện có nút
        }
        [HttpPost]
        public async Task<IActionResult> Sync()
        {
            var success = await SyncProvincesAsync();
            if (!success)
            {
                TempData["SyncError"] = "Không thể cập nhật dữ liệu từ API. Vui lòng thử lại sau.";
            }
            else
            {
                TempData["SyncSuccess"] = "Cập nhật dữ liệu thành công!";
            }

            return RedirectToAction("Index");
        }
        public async Task<bool> SyncProvincesAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("https://provinces.open-api.vn/api/p/");
                if (!response.IsSuccessStatusCode) return false;

                var content = await response.Content.ReadAsStringAsync();
                var provinces = JsonSerializer.Deserialize<List<ProvinceDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (provinces == null) return false;

                foreach (var item in provinces)
                {
                    var byCode = _unitOfWork.Repository<CatContry>().GetAll(c => c.Code == item.Code).FirstOrDefault();
                    var byName = _unitOfWork.Repository<CatContry>().GetAll(c => c.Name == item.Name).FirstOrDefault();

                    if (byCode != null && byCode.Name != item.Name)
                    {
                        byCode.Name = item.Name;
                    }
                    else if (byName != null && byName.Code != item.Code)
                    {
                        byName.Code = item.Code;
                    }
                    else if (byCode == null && byName == null)
                    {
                        _unitOfWork.Repository<CatContry>().Insert(new CatContry
                        {
                            Name = item.Name,
                            Code = item.Code,
                            Status = true,
                            Featured = false
                        });
                    }
                }

                _unitOfWork.Save();
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần thiết
                return false;
            }
        }
    }
}
