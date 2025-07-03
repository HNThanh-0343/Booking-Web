using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using WEBSITE_TRAVELBOOKING.Services;

namespace WEBGIS_OSM_IOT.Core
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context, IUnitOfWork unitOfWork)
        {
            try
            {
                var getUser = Account.GetAccount();
                if (getUser == null)
                {
                    await _next(context);
                    return;
                }

                var request = context.Request;
                var response = context.Response;

                // ghi log booking
                _ = GhiLogAsync(request, context, unitOfWork, getUser);                
               

                await _next(context);
            }
            catch (Exception)
            {
                await _next(context);
                throw;
            }
        }

        private string CleanUrl(PathString path)
        {
            var url = path.ToString();

            if (url.StartsWith("/Admin/", StringComparison.OrdinalIgnoreCase))
            {
                return url.Replace("/Admin/", "/", StringComparison.OrdinalIgnoreCase);
            }

            if (url.StartsWith("/Partner/", StringComparison.OrdinalIgnoreCase))
            {
                return url.Replace("/Partner/", "/", StringComparison.OrdinalIgnoreCase);
            }

            return url;
        }
        private async Task GhiLogAsync(HttpRequest request, HttpContext context, IUnitOfWork unitOfWork, SysUser getUser)
        {
            try
            {
                if (request.Method == HttpMethods.Post && request.Path.Value.ToLower().Contains("/datphong"))
                {
                    string requestBody = string.Empty;
                    BookRoomNoUser bookingData = null;

                    // Đọc request body
                    request.EnableBuffering();

                    // Bắt response
                    var originalBodyStream = context.Response.Body;
                    using (var memoryStream = new MemoryStream())
                    {
                        context.Response.Body = memoryStream;

                        await _next(context); // gọi controller

                        memoryStream.Position = 0;
                        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                        // Ghi lại response về cho client
                        memoryStream.Position = 0;
                        await memoryStream.CopyToAsync(originalBodyStream);
                        context.Response.Body = originalBodyStream;

                        try
                        {
                            var json = JsonConvert.DeserializeObject<JObject>(responseBody);
                            if (json != null && json["success"]?.Value<bool>() == true)
                            {
                                var idBooking = json["idBooking"];
                                var getBooking = unitOfWork.Repository<SysBooking>().GetById((int)idBooking);
                                if (getBooking != null)
                                {
                                    // Ghi log đặt phòng
                                    unitOfWork.Repository<SysLogPayment>().Insert(new SysLogPayment()
                                    {
                                        IdUser = getBooking.IdUser,
                                        IdCategories = 1,
                                        IdBooking = getBooking.Id,
                                        Name = getBooking.FullNameGuest,
                                        TotalPrice = (double?)bookingData.Price,
                                        Time = DateTime.Now,
                                        Url = "/datphong",
                                        Action = "POST",
                                        NumberOfGuests = bookingData.GuestsNumber?.ToString(),
                                        Status = getBooking.Status
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi xử lý response: " + ex.Message);
                        }
                    }
                }
                else if (!request.Path.Value.Contains(".") || request.Path.Value.Contains("/api")) // Ghi log bình thường
                {
                    SysLog sysLog = new SysLog()
                    {
                        IdUser = getUser.Id,
                        Url = CleanUrl(request.Path),
                        Action = request.Method,
                        Time = DateTime.Now,
                        Status = true
                    };
                    var getModule = unitOfWork.Repository<SysModule>().GetAll(filter: h => h.NameController == sysLog.Url.TrimStart('/')).FirstOrDefault();
                    if (getModule != null)
                    {
                        sysLog.Name = getModule.Name;
                    }
                    else
                    {
                        var path = context.Request.Path.ToString();
                        var segments = path.Trim('/').Split('/');
                        var controller = segments.Length > 0 ? segments[0] : string.Empty;
                        var action = segments.Length > 1 ? segments[1] : string.Empty;

                        switch (controller)
                        {
                            case "TrangChu":
                                sysLog.Name = "Trang chủ";
                                break;
                            case "hotel" or "KhachSan":
                                sysLog.Name = "Khách sạn";
                                break;
                            case "tour":
                                sysLog.Name = "Tour";
                                break;
                            case "xe":
                                sysLog.Name = "Dịch vụ thuê xe";
                                break;
                            case "baiviet":
                                sysLog.Name = "Bài viết";
                                break;
                            case "LienHe":
                                sysLog.Name = "Liên hệ";
                                break;
                            case "KhuyenMai":
                                sysLog.Name = "Khuyến mãi";
                                break;
                            default:
                                break;
                        }
                    }
                    // Đọc nội dung body nếu POST hoặc PUT
                    if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put)
                    {
                        request.EnableBuffering();

                        using (var reader = new StreamReader(request.Body, leaveOpen: true))
                        {
                            var body = await reader.ReadToEndAsync();
                            sysLog.Contents = body;
                            request.Body.Position = 0;
                        }
                    }
                    unitOfWork.Repository<SysLog>().Insert(sysLog);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }        
    }
}