using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WEBSITE_TRAVELBOOKING.Controllers;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Models;
using static System.Collections.Specialized.BitVector32;


namespace WEBSITE_TRAVELBOOKING.Core
{
    public class AuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;

            var session = httpContext.Session;

            Services.UnitOfWork unitOfWork = new Services.UnitOfWork(new WebsiteCmsBookingContext());
            Controller controller = filterContext.Controller as Controller;
            if (controller != null)
            {
                var controllerName = controller.Url.ActionContext.ActionDescriptor.RouteValues["controller"];
                var actionName = controller.Url.ActionContext.ActionDescriptor.RouteValues["action"];
                var area = controller.Url.ActionContext.ActionDescriptor.RouteValues["area"];
                var request = filterContext.HttpContext.Request;
                var SessionUserValue = filterContext.HttpContext.Session.GetString("TaiKhoan");
                // Chưa đăng nhập
                if (String.IsNullOrEmpty(SessionUserValue))
                {
                    ReturnAction(filterContext, "TrangChu", "Index");
                }
                // Đã đăng nhập
                else
                {
                    Controller baseController = null;
                    // Chuyển json cookie Account sang model
                    string strJson = filterContext.HttpContext.Session.GetString("TaiKhoan");
                    if (strJson != "")
                    {
                        var account = JsonConvert.DeserializeObject<SysUser>(strJson);
                        if (CheckRuleRedirectAreas(filterContext, account))
                        {
                            return; // 🚨 rất quan trọng: dừng lại nếu đã redirect
                        }
                        if (account.IdRole != null)
                        {
                            if (account.IdRole == 1) // admin
                            {
                                var CheckUrlRule = new ModelsAndRole
                                {

                                    IsView = true,
                                    IsCreate = true,
                                    IsEdit = true,
                                    IsDelete = true,
                                    IsPermission = true,
                                };
                                controller.ViewData["RuleAll"] = CheckUrlRule;
                            }
                            else
                            {
                                //if (account.IdRole == 3 && area == "Admin")
                                //{
                                //    ReturnActionPartner(filterContext, "TrangChu", "Index", new { area = "Partner" });
                                //}
                                // Danh sách module
                                var listModule = unitOfWork.Repository<SysModule>().GetAll();
                                // Danh sách rule
                                var listRule = unitOfWork.Repository<SysRule>().GetAll(filter: (m => m.IdRole == account.IdRole));

                                List<ModelsAndRole> modelsAndRoles = (from a in listRule
                                                                      join b in listModule on a.IdModule equals b.Id
                                                                      select new ModelsAndRole
                                                                      {
                                                                          NameController = b.NameController,
                                                                          IsView = a.IsView,
                                                                          IsCreate = a.IsCreate,
                                                                          IsEdit = a.IsEdit,
                                                                          IsDelete = a.IsDelete,
                                                                          IsPermission = a.IsPermission,
                                                                      }).ToList();
                                // checkUrl theo quyền
                                var CheckUrlRule = modelsAndRoles.FirstOrDefault(m => m.NameController.ToLower().Contains(controllerName.ToLower()) && m.IsView == true);
                                if (CheckUrlRule == null) // không có quyền
                                {
                                    ReturnAction(filterContext, "TrangChu", "Index");
                                }
                                controller.ViewData["RuleAll"] = CheckUrlRule;
                            }
                        }
                        else
                        {
                            ReturnAction(filterContext, "TrangChu", "Index");
                        }
                    }
                    else
                    {
                        ReturnAction(filterContext, "TrangChu", "Index");
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
        public void ReturnAction(ActionExecutingContext filterContext, string NameController, string NameAction)
        {
            filterContext.Result = new RedirectToRouteResult(
                                    new RouteValueDictionary{
                                        { "controller", NameController },
                                        { "action", NameAction }
                                    });
        }
        public void ReturnActionPartner(ActionExecutingContext filterContext, string NameController, string NameAction, object routeValues)
        {
            filterContext.Result = new RedirectToRouteResult(
                                    new RouteValueDictionary(routeValues){
                                        { "controller", NameController },
                                        { "action", NameAction }
                                    });
        }
        private bool CheckRuleRedirectAreas(ActionExecutingContext context, SysUser sysUser)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;

            var currentRole = sysUser.IdRole.ToString();

            var expectedArea = sysUser.IdRole switch
            {
                1 => "admin",
                3 => "partner",
                _ => ""
            };

            var routeData = context.RouteData;
            var currentArea = routeData.Values["area"]?.ToString()?.ToLower() ?? "";
            var currentController = routeData.Values["controller"]?.ToString() ?? "TrangChu";
            var currentAction = routeData.Values["action"]?.ToString() ?? "Index";

            // Chỉ redirect nếu area không đúng với role
            if (currentArea != expectedArea)
            {
                var routeValues = new RouteValueDictionary()
                    {
                        { "area", expectedArea },
                        { "controller", currentController },
                        { "action", currentAction }
                    };

                // Giữ nguyên các tham số route hiện tại (nếu có)
                foreach (var key in routeData.Values.Keys)
                {
                    if (!routeValues.ContainsKey(key))
                        routeValues.Add(key, routeData.Values[key]);
                }

                context.Result = new RedirectToRouteResult(routeValues);
                return true; // Đã redirect, dừng xử lý action tiếp theo
            }

            // Nếu đúng area với quyền thì không làm gì, cho xử lý tiếp
            return false;
        }

    }
    //public class ValidatePartnerIdAttribute : ActionFilterAttribute
    //{
    //    public override void OnActionExecuting(ActionExecutingContext context)
    //    {
    //        var httpContext = context.HttpContext;
    //        var sessionTaiKhoan = httpContext.Session.GetString("TaiKhoan");
    //        var routePartnerId = context.RouteData.Values["TaiKhoanUserName"]?.ToString();           

    //        if (string.IsNullOrEmpty(sessionTaiKhoan))
    //        {
    //            //context.Result = new ForbidResult(); // hoặc RedirectToAction("Login", "Account");
    //            httpContext.Session.SetString("ErrorMessagePartNer", "Bạn chưa đăng nhập!!");
    //            context.Result = new RedirectToActionResult("Index", "Error", new { area = "Partner" });
    //            return;
    //        }
    //        else if (!string.IsNullOrEmpty(sessionTaiKhoan))
    //        {
    //            var account = JsonConvert.DeserializeObject<SysUser>(sessionTaiKhoan);
    //            if (account != null && !string.Equals(account.Username.ToLower(), routePartnerId.ToLower(), StringComparison.OrdinalIgnoreCase)) //so sanh
    //            {
    //                httpContext.Session.SetString("ErrorMessagePartNer", "Bạn lại tò mò rồi!!");
    //                context.Result = new RedirectToActionResult("Index", "Error", new { area = "Partner" });
    //                return;
    //            }

    //        }            

    //        base.OnActionExecuting(context);
    //    }
    //}
    public class ValidatePartnerId : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //var httpContext = context.HttpContext;
            //var sessionData = httpContext.Session.GetString("TaiKhoan");

            //if (string.IsNullOrEmpty(sessionData))
            //{
            //    httpContext.Session.SetString("ErrorMessagePartNer", "Bạn chưa đăng nhập!!");
            //    context.Result = new RedirectResult("/partner/404");
            //    return;
            //}

            //var user = JsonConvert.DeserializeObject<SysUser>(sessionData);
            //var routeUsername = context.RouteData.Values["TaiKhoanUserName"]?.ToString()?.ToLower();

            //if (!string.Equals(user.Username.ToLower(), routeUsername))
            //{
            //    httpContext.Session.SetString("ErrorMessagePartNer", "Bạn lại tò mò rồi!!");
            //    context.Result = new RedirectResult("/partner/404"); // Truy cập sai user
            //    return;
            //}

            base.OnActionExecuting(context);
        }
    }

}
