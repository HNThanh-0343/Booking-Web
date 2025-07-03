using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;

namespace WEBSITE_TRAVELBOOKING.Areas.Admin.ViewComponents
{
    public class MenuPartnerViewComponent : ViewComponent
    {
        public IUnitOfWork _unitOfWork;
        public MenuPartnerViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        //public IViewComponentResult Invoke()
        //{
        //    var getTaiKhoan = HttpContext.Session.GetString("TaiKhoan");

        //    if (!string.IsNullOrEmpty(getTaiKhoan))
        //    {
        //        var getAccout = JsonSerializer.Deserialize<SysUser>(getTaiKhoan);

        //        var allModules = _unitOfWork.Repository<SysModule>().GetAll(m => m.Status == true).ToList();

        //        var settings = _unitOfWork.Repository<SysSettingMenu>()
        //                  .GetAll(s => s.IdUser == getAccout.Id)
        //                  .ToList();
        //        // Nếu chưa có thiết lập, mặc định tất cả đều hiện
        //        if (!settings.Any())
        //        {
        //            foreach (var item in allModules)
        //            {
        //                _unitOfWork.Repository<SysSettingMenu>().Insert(new SysSettingMenu()
        //                {
        //                    IdUser = getAccout.Id,
        //                    IdModule = item.Id,
        //                    IsVisible = true
        //                });
        //            }
        //        }

        //        var visibleModuleIds = settings.Where(s => (bool)s.IsVisible).Select(s => s.IdModule).ToList();
        //        var filteredModules = allModules.Where(m => visibleModuleIds.Contains(m.Id)).ToList();
        //        var tree = BuildTree(filteredModules);
        //        return View("_MenuPartner", tree);

        //    }

        //    //var GetRole = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
        //    return View("NoPermission"); // Chưa đăng nhập
        //}
        public IViewComponentResult Invoke()
        {
            var getTaiKhoan = HttpContext.Session.GetString("TaiKhoan");

            if (!string.IsNullOrEmpty(getTaiKhoan))
            {
                var getAccout = JsonSerializer.Deserialize<SysUser>(getTaiKhoan);
                var getRule = _unitOfWork.Repository<SysRule>().GetAll(filter: (m => m.IdRole == getAccout.IdRole)).ToList();
                if (getRule == null)
                {
                    // không có quyền
                    // Không có quyền => trả View rỗng hoặc thông báo
                    return View("NoPermission"); // View rỗng hoặc thông báo
                }
                var listIdModule = getRule.Where(x => x.IdModule.HasValue).Select(x => x.IdModule.Value).ToList();
                var listModule = _unitOfWork.Repository<SysModule>().GetAll(filter: m => listIdModule.Contains(m.Id), includeProperties: "SysRules", orderBy: (m => m.OrderBy(d => d.Order))).ToList();
                var tree = BuildTree(listModule);
                return View("_MenuPartner", tree);
            }

            //var GetRole = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
            return View("NoPermission"); // Chưa đăng nhập
        }
        public List<MenuItem> BuildTree(List<SysModule> modules)
        {
            // Bước 1: Convert sang MenuItem
            var allItems = modules.Select(m => new MenuItem
            {
                Id = m.Id,
                Name = m.Name,
                NameController = m.NameController,
                Icon = m.Icon,
                PrentId = m.PrentId,
                Order = m.Order
            }).ToList();

            // Bước 2: Tạo dictionary theo Id
            var lookup = allItems.ToDictionary(item => item.Id.ToString(), item => item);

            // Bước 3: Gắn Children
            var rootItems = new List<MenuItem>();
            foreach (var item in allItems)
            {
                if (!string.IsNullOrEmpty(item.PrentId) && lookup.ContainsKey(item.PrentId))
                {
                    lookup[item.PrentId].Children.Add(item);
                }
                else
                {
                    rootItems.Add(item);
                }
            }

            // Bước 4: Sắp xếp theo Order
            SortMenuItems(rootItems);

            return rootItems;
        }

        private void SortMenuItems(List<MenuItem> items)
        {
            items.Sort((a, b) => (a.Order ?? 0).CompareTo(b.Order ?? 0));
            foreach (var item in items)
            {
                if (item.Children.Any())
                    SortMenuItems(item.Children);
            }
        }

    }
}
