using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using WEBSITE_TRAVELBOOKING.Core;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using X.PagedList;

namespace WEBSITE_TRAVELBOOKING.Areas.Partner.Controllers
{
    [Area("partner")]
    [Auth]
    public class quanlynhomquyenController : Controller
    {
        public IUnitOfWork _unitOfWork;
        private IWebHostEnvironment Environment;
        private SysUser getAccout;

        public quanlynhomquyenController(IUnitOfWork unitOfWork, IWebHostEnvironment _environment)
        {
            _unitOfWork = unitOfWork;
            Environment = _environment;
        }        
        public IActionResult Index()
        {
            //ViewBag.NhomQuyen = _unitOfWork.Repository<SysRole>().GetAll(filter: h => h.Status == true).ToList();
            return View();
        }

        public IActionResult childIndex(int? page, string searchValue)
        {
            var getAccout = Account.GetAccount();         
            // hiển thị quyền của partner
            var getRole = _unitOfWork.Repository<SysRole>().GetAll(filter: (m => m.IdUserPrent == getAccout.Id)).ToList();
            if (!string.IsNullOrEmpty(searchValue))
            {
                getRole = getRole.Where(m => m.Name.ToLower().Contains(searchValue.ToLower())).ToList();
            }
            #region Page
            page = page == null ? 1 : page;
            page = page < 1 ? 1 : page;
            var pageSize = 10;
            var pageListView = getRole.ToPagedList(page ?? 1, pageSize);
            #endregion
            #region ViewBag
            ViewBag.Partner = false;
            #endregion
            if (pageListView != null)
            {
                return PartialView("childIndex", pageListView);
            }
            return PartialView("childIndex");
        }

        public IActionResult themMoiNhomQuyen()
        {
            try
            {
                SysRole sysRole = new SysRole();
                return PartialView(sysRole);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult themMoiNhomQuyen(SysRole sysRole)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var key = modelStateKey;
                            var errorMessage = error.ErrorMessage;
                            ModelState.AddModelError(key, errorMessage);
                            return ValidationProblem(ModelState);
                        }
                    }
                }
                sysRole.Status = true;
                var getAccout = Account.GetAccount();
                if (getAccout.IdRole != 1)
                {
                    sysRole.IdUserPrent = Account.GetAccount()?.Id;
                }

                _unitOfWork.Repository<SysRole>().Insert(sysRole);

                return PartialView("Index");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult chinhSuaNhomQuyen(int id)
        {
            try
            {
                var sysRole = _unitOfWork.Repository<SysRole>().GetById(id);
                if (sysRole == null)
                {
                    return PartialView("Index");
                }
                return PartialView("chinhSuaNhomQuyen", sysRole);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult chinhSuaNhomQuyen(SysRole sysRole)
        {
            try
            {
                var checksysRole = _unitOfWork.Repository<SysRole>().GetById(sysRole.Id);
                if (checksysRole == null)
                {
                    string key = "Name";
                    string errorMessage = "Tên nhóm quyền không được để trống!!";
                    ModelState.AddModelError(key, errorMessage);
                    return ValidationProblem(ModelState);
                }

                // Kiểm tra trường dữ liệu
                if (!ModelState.IsValid)
                {
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            var key = modelStateKey;
                            var errorMessage = error.ErrorMessage;
                            ModelState.AddModelError(key, errorMessage);
                            return ValidationProblem(ModelState);
                        }
                    }
                }
                checksysRole.Name = sysRole.Name;
                _unitOfWork.Repository<SysRole>().Update(checksysRole);

                TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
                return PartialView("Index");

            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult chiTietNhomQuyen(int id)
        {
            try
            {
                var SysRoles = _unitOfWork.Repository<SysRole>().GetAll(filter: (m => m.Id == id), includeProperties: "SysUsers").FirstOrDefault();
                if (SysRoles == null) return PartialView("Index");
                return PartialView(SysRoles);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult xoaNhomQuyen(int id)
        {
            try
            {

                var getRole = _unitOfWork.Repository<SysRole>().GetById(id);
                if (getRole == null) return PartialView("Index");
                _unitOfWork.Repository<SysRole>().Delete(getRole);
                TempData["SuccessMessage"] = "Xóa thành công!";
                //return PartialView("childIndex");
                return Json(new { result = true, message = "Xóa thành công!" });

            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });

            }
        }
        public IActionResult phanquyen(int id)
        {
            try
            {
                var getAccount = Account.GetAccount();
                if (getAccount == null)
                {
                    // lỗi chưa đăng nhâp
                }
                if (getAccount.IdRole == 1)// quyền admin
                {
                    var getRole = _unitOfWork.Repository<SysRole>().GetById(id);
                    if (getRole == null) return PartialView("Index");

                    var getRule = _unitOfWork.Repository<SysRule>().GetAll(filter: (m => m.IdRole == getRole.Id));
                    var getModule = _unitOfWork.Repository<SysModule>().GetAll();
                    var mapRuleModule = (from module in getModule
                                         join rule in getRule
                                         on module.Id equals rule.IdModule into gj
                                         from subRule in gj.DefaultIfEmpty()
                                         select new ModelsAndRole()
                                         {
                                             Id = subRule?.Id ?? 0,
                                             IdModule = module.Id,
                                             IdRole = subRule?.IdRole ?? getRole.Id,
                                             Name = module.Name,
                                             NameController = module.NameController,
                                             IsView = subRule?.IsView ?? false,
                                             IsCreate = subRule?.IsCreate ?? false,
                                             IsEdit = subRule?.IsEdit ?? false,
                                             IsDelete = subRule?.IsDelete ?? false,
                                             IsPermission = subRule?.IsPermission ?? false
                                         }).ToList();

                    return PartialView(mapRuleModule);
                }
                else
                {//quyền còn lại                
                    // Lấy quyền hiện tại của Partner (người đăng nhập)
                    var roleCurrent = _unitOfWork.Repository<SysRole>().GetById((int)getAccount.IdRole);
                    if (roleCurrent == null) return PartialView("Index");

                    // Lấy các Rule mà Partner hiện tại có
                    var ruleCurrent = _unitOfWork.Repository<SysRule>()
                                      .GetAll(filter: r => r.IdRole == roleCurrent.Id);

                    // Lấy danh sách module theo quyền Partner có (A, B, C)
                    var moduleIds = ruleCurrent.Select(r => r.IdModule).Distinct().ToList();
                    var modules = _unitOfWork.Repository<SysModule>()
                                  .GetAll(filter: m => moduleIds.Contains(m.Id));

                    // Lấy Rule theo Role cần chỉnh (ví dụ: phân cho user dưới quyền)
                    var roleTarget = _unitOfWork.Repository<SysRole>().GetById(id);
                    if (roleTarget == null) return PartialView("Index");

                    var ruleTarget = _unitOfWork.Repository<SysRule>()
                                     .GetAll(filter: r => r.IdRole == roleTarget.Id);

                    // Join theo quyền được phép
                    var mapRuleModule = (from module in modules
                                         join rule in ruleTarget on module.Id equals rule.IdModule into gj
                                         from subRule in gj.DefaultIfEmpty()
                                         select new ModelsAndRole()
                                         {
                                             Id = subRule?.Id ?? 0,
                                             IdModule = module.Id,
                                             IdRole = subRule?.IdRole ?? roleTarget.Id,
                                             Name = module.Name,
                                             NameController = module.NameController,
                                             IsView = subRule?.IsView ?? false,
                                             IsCreate = subRule?.IsCreate ?? false,
                                             IsEdit = subRule?.IsEdit ?? false,
                                             IsDelete = subRule?.IsDelete ?? false,
                                             IsPermission = subRule?.IsPermission ?? false
                                         }).ToList();


                    return PartialView(mapRuleModule);
                }


            }
            catch (Exception)
            {
                return Json(new { result = false, message = "Lỗi khi xóa" });

            }
        }
        [HttpPost]
        public IActionResult phanquyen([FromBody] List<ModelsAndRole> Rules)
        {
            try
            {
                var listRule = _unitOfWork.Repository<SysRule>().GetAll();
                foreach (var item in Rules)
                {
                    var checkRule = listRule.FirstOrDefault(h => h.Id == item.Id && h.IdRole == item.IdRole && h.IdModule == item.IdModule);
                    if (checkRule == null)
                    {
                        if (item.IsView == false && item.IsCreate == false && item.IsEdit == false && item.IsDelete == false)
                        {
                            continue;
                        }
                        else
                        {
                            _unitOfWork.Repository<SysRule>().Insert(new SysRule()
                            {
                                IdRole = item.IdRole,
                                IdModule = item.IdModule,
                                IsView = item.IsView,
                                IsCreate = item.IsCreate,
                                IsEdit = item.IsEdit,
                                IsDelete = item.IsDelete,
                                IsPermission = item.IsPermission,
                            });
                            continue;
                        }

                    }
                    if (item.IsView == false && item.IsCreate == false && item.IsEdit == false && item.IsDelete == false && item.IsPermission == false)
                    { // vì phải xóa đi nên phải bốc từ user ra
                        var getUser = _unitOfWork.Repository<SysUser>().GetAll(filter: (m => m.IdRole == item.IdRole)).Select(m => m.Id).Distinct().ToList();
                        if (getUser != null)
                        {
                            //tiến hành lấy lại Id của User để qua bảng Role xem thử là nhóm quyền nào
                            var getRole = _unitOfWork.Repository<SysRole>().GetAll(m => getUser.Contains(m.IdUserPrent ?? 0)).Select(m => m.Id).Distinct().ToList();
                            if (getRole != null)
                            {
                                var getRule = _unitOfWork.Repository<SysRule>().GetAll(m => getRole.Contains(m.IdRole ?? 0) && m.IdModule == item.IdModule);
                                foreach (var itemRuleDel in getRule)
                                {
                                    _unitOfWork.Repository<SysRule>().Delete(itemRuleDel);
                                }
                            }
                        }
                        _unitOfWork.Repository<SysRule>().Delete(checkRule);
                        continue;
                    }
                    checkRule.IsView = item.IsView;
                    checkRule.IsEdit = item.IsEdit;
                    checkRule.IsCreate = item.IsCreate;
                    checkRule.IsDelete = item.IsDelete;
                    checkRule.IsPermission = item.IsPermission;

                    _unitOfWork.Repository<SysRule>().Update(checkRule);
                }
                return PartialView("Index");
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
