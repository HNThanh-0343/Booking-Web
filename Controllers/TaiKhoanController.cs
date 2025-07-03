using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net;
using WEBSITE_TRAVELBOOKING.Helper;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using WEBSITE_TRAVELBOOKING.Services;
using WEBSITE_TRAVELBOOKING.Utilities;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace WEBSITE_TRAVELBOOKING.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CustomPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;

        // Định cấu hình giới hạn đăng nhập
        private const int MAX_LOGIN_ATTEMPTS = 5; // Số lần đăng nhập sai tối đa
        private const int LOCKOUT_DURATION_MINUTES = 15; // Thời gian khóa tài khoản (phút)

        // Định cấu hình yêu cầu mật khẩu và số điện thoại
        private const int MIN_PASSWORD_LENGTH = 6; // Độ dài tối thiểu của mật khẩu
        private const int PHONE_NUMBER_LENGTH = 10; // Độ dài chuẩn của số điện thoại

        public TaiKhoanController(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = new CustomPasswordHasher();
            _emailService = emailService;
        }


        /// <summary>
        /// /Google login
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public async Task<IActionResult> RegisterGoogle(int roleID, int id)
        {
            // lưu lại url
            var previousUrl = Request.Headers["Referer"].ToString();
            //await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
            //    new AuthenticationProperties
            //    {
            //        RedirectUri = Url.Action("googleresponse")
            //    });

            var redirectUrl = Url.Action("GoogleResponse", "taikhoan", new { roleID = roleID, redirectUrl = previousUrl, id = id }); // Account là tên controller
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse(int roleID, string redirectUrl, int id)
        {
            
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                return RedirectToAction("index", "trangchu"); // hoặc trang lỗi tùy ý

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims.Select(c => new
            {
                c.Type,
                c.Value
            }).ToList();
            var pictureclaims = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value;


            // Bạn có thể xử lý claims ở đây (tên, email, avatar...)
            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.Identity?.Name;
            var picture = pictureclaims;

            //Role = 0 Là đăng nhập
            if (roleID == 0)
            {
                var getSysGuest = _unitOfWork.Repository<SysGuest>().GetAll(m => m.Email != null && m.Email.ToLower() == email).FirstOrDefault();
                var getSysUser = _unitOfWork.Repository<SysUser>().GetAll(m => m.Email != null && m.Email.ToLower() == email).FirstOrDefault();

                if (id == 2)
                {
                    if (getSysGuest != null)
                    {
                        SetUserSession(getSysGuest, "Guest");

                        return Redirect(redirectUrl);
                    }
                }
                else if(id == 3)
                {
                    if (getSysUser != null)
                    {
                        SetUserSession(getSysUser, "User");

                        return Redirect(redirectUrl);
                    }
                }
                else
                {
                    if (getSysGuest == null && getSysUser != null)
                    {
                        // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
                        SetUserSession(getSysUser, "User");
                        return Redirect(redirectUrl);

                        //return RedirectToAction("index", "trangchu");
                    }
                    if (getSysGuest != null && getSysUser == null)
                    {
                        // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
                        SetUserSession(getSysGuest, "Guest");

                        return Redirect(redirectUrl);
                    }
                }
                if (getSysGuest == null && getSysUser == null)
                {
                    return RedirectToAction("dangky", "taikhoan");
                }
                return RedirectToAction("dangky", "taikhoan");
            }

            if (roleID == 2)
            {
                // Ví dụ: lưu vào DB hoặc session rồi chuyển hướng
                //return Json(new { name, email, claims });
                var getEmail = _unitOfWork.Repository<SysGuest>().GetAll(m => m.Email != null && m.Email.ToLower() == email).FirstOrDefault();
                if (getEmail == null)
                {
                    //Xử lý lưu
                    var newGuest = new SysGuest
                    {
                        Name = name,
                        Email = email,
                        Phone = "",
                        Password = _passwordHasher.CreateBase64("booking"),
                        IdRole = 2,
                        Status = true,
                        Time = DateTime.Now,
                        Avatar = picture,
                    };
                    _unitOfWork.Repository<SysGuest>().Insert(newGuest);

                    // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
                    SetUserSession(newGuest, "Guest");
                    return Redirect(redirectUrl);
                    //return RedirectToAction("index", "trangchu");
                }

                SetUserSession(getEmail, "Guest");

                return Redirect(redirectUrl);
            }
            if(roleID == 3)
            {
                // Ví dụ: lưu vào DB hoặc session rồi chuyển hướng
                //return Json(new { name, email, claims });
                var getSysUser = _unitOfWork.Repository<SysUser>().GetAll(m => m.Email != null && m.Email.ToLower() == email).FirstOrDefault();
                if (getSysUser == null)
                {
                    //Xử lý lưu
                    var newUser = new SysUser
                    {
                        Name = name,
                        Email = email,
                        Phone = "",
                        Password = _passwordHasher.CreateBase64("booking"),
                        IdRole = 3,
                        Status = true,
                        Time = DateTime.Now,
                        Avatar = picture,
                    };
                    _unitOfWork.Repository<SysUser>().Insert(newUser);

                    // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
                    SetUserSession(newUser, "User");
                    return Redirect(redirectUrl);

                    //return RedirectToAction("index", "trangchu");
                }

                SetUserSession(getSysUser, "User");

                return Redirect(redirectUrl);
            }

            return Redirect(redirectUrl);

        }


        public async Task<IActionResult> SendEmailOTP(string toEmail, int type)
        {
            try
            {
                var subject = "Đăng ký tài khoản ";

                if ( type== 1)
                {
                    subject = "Đặt lại mật khẩu ";
                    //kiểm tra bảng guest
                    var getSysGuest = _unitOfWork.Repository<SysGuest>().GetAll(m => m.Email.ToLower() == toEmail.ToLower()).FirstOrDefault();
                    
                    var getSysUser = _unitOfWork.Repository<SysUser>().GetAll(m => m.Email.ToLower() == toEmail.ToLower()).FirstOrDefault();
                    if (getSysUser == null && getSysGuest == null)
                    {
                        return Json(new { success = false, message = "Email không tồn tại!" });
                    }
                }
                if(type == 2)
                {
                    subject = "Đăng ký tài khoản";
                }

                string otpCode = OtpUtility.GenerateOtp();

                // Lưu mã OTP và thời gian vào session
                HttpContext.Session.SetString($"otpCode_{toEmail}", otpCode);
                HttpContext.Session.SetString($"otpTime_{toEmail}", DateTime.Now.ToString());

                // Gửi OTP qua email
                // subject = "Đăng ký tài khoản";
                await SendOtpByEmail(toEmail, subject, otpCode);

                // Trả về thông báo thành công
                return Json(new { success = true, message = "Đã gửi mã thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi mã OTP: " + ex.Message });
            }
        }

        /// <summary>
        /// Xác thực OTP
        /// </summary>
        private bool ValidOTP(string? email, string? otpCode)
        {
            string getotpCode = HttpContext.Session.GetString($"otpCode_{email}");
            string getotpTime = HttpContext.Session.GetString($"otpTime_{email}");

            // Kiểm tra OTP có tồn tại không
            if (string.IsNullOrEmpty(getotpCode))
            {
                return false;
            }

            // Kiểm tra OTP có khớp không
            if (getotpCode != otpCode)
            {
                return false;
            }

            // Kiểm tra thời gian hết hạn của OTP (5 phút)
            if (!string.IsNullOrEmpty(getotpTime) && DateTime.TryParse(getotpTime, out DateTime otpTime))
            {
                TimeSpan timeDifference = DateTime.Now - otpTime;
                if (timeDifference.TotalMinutes > 5)
                {
                    // Xóa OTP đã hết hạn khỏi session
                    RemoveOTP(email);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Xóa thông tin OTP khỏi session
        /// </summary>
        private void RemoveOTP(string email)
        {
            HttpContext.Session.Remove($"otpCode_{email}");
            HttpContext.Session.Remove($"otpTime_{email}");
        }

        public IActionResult dangky()
        {
            TempData["ReturnUrl"] = Request.Query["ReturnUrl"];


            // If user is already logged in, redirect to home page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            AccountForm accountForm = new AccountForm();

            return View(accountForm);
        }

        [HttpPost]
        public IActionResult dangky([Bind("IdRole,Username,Password,Phone,Email,Name,CodeOTP")] AccountForm accountForm)
        {
            try
            {
                // lưu lại url
                var previousUrl = Request.Headers["Referer"].ToString();
                if (!ModelState.IsValid)
                {
                    //ModelState.AddModelError("Name", @"Vui lòng nhập tên tài khoản!");
                    return View(accountForm);
                }
                if (!ValidOTP(accountForm.Email, accountForm.CodeOTP))
                {
                    ModelState.AddModelError("CodeOTP", @"Mã xác thực không đúng!");
                    return View(accountForm);
                }
                //Tài khoan guest
                if (accountForm.IdRole == 2)
                {
                    var getEmail = _unitOfWork.Repository<SysGuest>().GetAll(m => m.Email != null && m.Email.ToLower() == accountForm.Email).FirstOrDefault();
                    if (getEmail == null)
                    {
                        //Xử lý lưu
                        var newGuest = new SysGuest
                        {
                            Name = accountForm.Name,
                            Email = accountForm.Email,
                            Phone = accountForm.Phone,
                            Password = _passwordHasher.CreateBase64(accountForm.Password),
                            IdRole = accountForm.IdRole,
                            Status = true,
                            Time = DateTime.Now
                        };
                        _unitOfWork.Repository<SysGuest>().Insert(newGuest);

                        // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
                        SetUserSession(newGuest, "Guest");

                        if (string.IsNullOrEmpty(previousUrl))
                        {
                            return RedirectToAction("index", "trangchu");
                        }
                        // Quay về trang trước
                        return Redirect(previousUrl);
                    }

                    ModelState.AddModelError("Email", @"Tài khoản đã tồn tại!");
                    return View(accountForm);
                }
                //Tài khoản partner
                else if (accountForm.IdRole == 3)
                {
                    var getEmail = _unitOfWork.Repository<SysUser>().GetAll(m => m.Email != null && m.Email.ToLower() == accountForm.Email).FirstOrDefault();
                    if (getEmail == null)
                    {
                        //Xử lý lưu
                        //Xử lý lưu
                        var newUser = new SysUser
                        {
                            Name = accountForm.Name,
                            Email = accountForm.Email,
                            Phone = accountForm.Phone,
                            Password = _passwordHasher.CreateBase64(accountForm.Password),
                            IdRole = accountForm.IdRole,
                            Status = true,
                            Time = DateTime.Now
                        };
                        _unitOfWork.Repository<SysUser>().Insert(newUser);

                        // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
                        SetUserSession(newUser, "User");

                        if (string.IsNullOrEmpty(previousUrl))
                        {
                            return RedirectToAction("index", "trangchu");
                        }
                        // Quay về trang trước
                        return Redirect(previousUrl);

                    }

                    ModelState.AddModelError("Email", @"Tài khoản đã tồn tại!");
                    return View(accountForm);
                }

                //Lỗi phân quyền roleID
                return View(accountForm);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public IActionResult dangnhap()
        {
            // If user is already logged in, redirect to home page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            AccountLoginForm accountLoginForm = new AccountLoginForm();

            return View(accountLoginForm);
        }


        [HttpPost]
        public IActionResult dangnhap(AccountLoginForm accountLoginForm)
        {
            try
            {

                
                string hashedPassword = _passwordHasher.CreateBase64(accountLoginForm.Password);
                // lưu lại url
                var previousUrl = Request.Headers["Referer"].ToString();
                //kiểm tra bảng guest
                var getSysGuest = _unitOfWork.Repository<SysGuest>().GetAll(m => m.Email.ToLower() == accountLoginForm.Username.ToLower() || m.Phone.ToUpper() == accountLoginForm.Username.ToUpper()).FirstOrDefault();
                if (getSysGuest != null)
                {
                    if (accountLoginForm.RememberMe)
                    {
                        CookieOptions option = new CookieOptions();
                        option.Expires = DateTime.Now.AddDays(7); // Cookie tồn tại 7 ngày
                        Response.Cookies.Append("RememberUsername", accountLoginForm.Username, option);
                    }
                    else
                    {
                        Response.Cookies.Delete("RememberUsername");
                    }
                    HttpContext.Session.SetString("Username", accountLoginForm.Username);

                    if (getSysGuest.Password == hashedPassword)
                    {
                        SetUserSession(getSysGuest, "Guest");
                        
                        if (string.IsNullOrEmpty(previousUrl))
                        {
                            return RedirectToAction("index", "trangchu");
                        }
                        // Quay về trang trước
                        return Redirect(previousUrl);
                    }
                    return Redirect(previousUrl);
                    ModelState.AddModelError("Password", @"Mật khẩu không đúng!");
                    return View(accountLoginForm);

                }
                //Kiểm tra bảng user partner
                var getSysUser = _unitOfWork.Repository<SysUser>().GetAll(m => m.Email.ToLower() == accountLoginForm.Username.ToLower() || m.Phone.ToUpper() == accountLoginForm.Username.ToUpper()).FirstOrDefault();
                if (getSysUser != null)
                {
                    if (accountLoginForm.RememberMe)
                    {
                        CookieOptions option = new CookieOptions();
                        option.Expires = DateTime.Now.AddDays(7); // Cookie tồn tại 7 ngày
                        Response.Cookies.Append("RememberUsername", accountLoginForm.Username, option);
                    }
                    else
                    {
                        Response.Cookies.Delete("RememberUsername");
                    }
                    HttpContext.Session.SetString("Username", accountLoginForm.Username);

                    if (getSysUser.Password == hashedPassword && getSysUser.IdRole == 3)
                    {
                        SetUserSession(getSysUser, "User");
                        if (string.IsNullOrEmpty(previousUrl))
                        {
                            return RedirectToAction("index", "trangchu");
                        }
                        // Quay về trang trước
                        return Redirect(previousUrl);
                        //return RedirectToAction("index", "trangchu", new { area = "partner" });

                    }

                    if (getSysUser.Password == hashedPassword && getSysUser.IdRole == 1)
                    {
                        SetUserSession(getSysUser, "User");
                        //return RedirectToAction("index", "trangchu");
                        if (string.IsNullOrEmpty(previousUrl))
                        {
                            return RedirectToAction("index", "trangchu", new { Areas = "admin" });
                        }
                        // Quay về trang trước
                        return Redirect(previousUrl);
                       

                    }
                    
                    ModelState.AddModelError("Password", @"Mật khẩu không đúng!");
                    return View(accountLoginForm);

                }

                ModelState.AddModelError("Username", @"Tài khoản không tồn tại!");
                return View(accountLoginForm);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public IActionResult quenmatkhau()
        {
            // If user is already logged in, redirect to home page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            AccountForgotForm accountForgotForm = new AccountForgotForm();

            return View(accountForgotForm);
        }

        [HttpPost]
        public IActionResult quenmatkhau(AccountForgotForm accountForgotForm)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    //ModelState.AddModelError("Name", @"Vui lòng nhập tên tài khoản!");
                    return View(accountForgotForm);
                }

                if (!ValidOTP(accountForgotForm.Email, accountForgotForm.CodeOTP))
                {
                    ModelState.AddModelError("CodeOTP", @"Mã xác thực không đúng!");
                    return View(accountForgotForm);
                }
                // lưu lại url
                var previousUrl = Request.Headers["Referer"].ToString();
                var getSysGuest = _unitOfWork.Repository<SysGuest>().GetAll(m => m.Email != null && m.Email.ToLower() == accountForgotForm.Email).FirstOrDefault();
                if (getSysGuest != null)
                {
                    getSysGuest.Password = _passwordHasher.CreateBase64(accountForgotForm.Password);
                    _unitOfWork.Repository<SysGuest>().Update(getSysGuest);

                    // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
                    SetUserSession(getSysGuest, "Guest");

                    if (string.IsNullOrEmpty(previousUrl))
                    {
                        return RedirectToAction("index", "trangchu");
                    }
                    // Quay về trang trước
                    return Redirect(previousUrl);

                }
                

                var getSysUser = _unitOfWork.Repository<SysUser>().GetAll(m => m.Email != null && m.Email.ToLower() == accountForgotForm.Email).FirstOrDefault();

                if (getSysUser != null)
                {
                    getSysUser.Password = _passwordHasher.CreateBase64(accountForgotForm.Password);
                    _unitOfWork.Repository<SysUser>().Update(getSysUser);

                    // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
                    SetUserSession(getSysUser, "User");

                    if (string.IsNullOrEmpty(previousUrl))
                    {
                        return RedirectToAction("index", "trangchu");
                    }
                    // Quay về trang trước
                    return Redirect(previousUrl);

                }

                ModelState.AddModelError("Email", @"Tài khoản không tồn tại!");
                return View(accountForgotForm);
                                
            }
            catch (Exception)
            {

                throw;
            }
        }


        public IActionResult Index()
        {
            return View();
        }

        #region Đăng nhập và Đăng xuất

        // GET action for login page
        public IActionResult DangNhap1()
        {
            // If user is already logged in, redirect to home page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            return View();
        }

        [HttpPost]
        public IActionResult DangNhap1(string email, string matKhau)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ email và mật khẩu." });
            }

            // Kiểm tra xem IP hoặc email này có bị khóa tạm thời không
            if (IsAccountLockedOut(email))
            {
                return Json(new { success = false, message = $"Tạm thời không thể đăng nhập. Vui lòng thử lại sau {LOCKOUT_DURATION_MINUTES} phút." });
            }

            // Kiểm tra trong cả hai bảng: SysUser và SysGuest
            var taiKhoanRepo = _unitOfWork.Repository<SysUser>();
            var guestRepo = _unitOfWork.Repository<SysGuest>();

            var taiKhoan = taiKhoanRepo.GetAll(tk => tk.Email.ToLower() == email.ToLower()).FirstOrDefault();
            var guestAccount = guestRepo.GetAll(g => g.Email.ToLower() == email.ToLower()).FirstOrDefault();

            // Kiểm tra tài khoản SysUser trước
            if (taiKhoan != null)
            {
                return ProcessLoginAttempt(taiKhoan, matKhau, "User", email);
            }
            // Kiểm tra tài khoản SysGuest nếu không tìm thấy trong SysUser
            else if (guestAccount != null)
            {
                return ProcessLoginAttempt(guestAccount, matKhau, "Guest", email);
            }
            else
            {
                // Không tìm thấy tài khoản - tăng số lần đăng nhập sai
                IncrementFailedLoginAttempts(email);
                // Sử dụng thông báo lỗi chung, không tiết lộ lý do cụ thể
                return Json(new { success = false, message = "Thông tin đăng nhập không chính xác. Vui lòng thử lại." });
            }
        }

        // GET action for registration page
        public IActionResult DangKy1()
        {
            // If user is already logged in, redirect to home page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            return View();
        }

        // GET action for forgot password page
        public IActionResult QuenMatKhau1()
        {
            // If user is already logged in, redirect to home page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "TrangChu");
            }
            return View();
        }

        // GET action for user profile page
        public IActionResult ThongTin()
        {
            // If user is not logged in, redirect to login page
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("DangNhap");
            }
            return View();
        }

        public IActionResult DangXuat()
        {
            // Xóa mọi thông tin đăng nhập sai khi đăng xuất
            var email = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(email))
            {
                HttpContext.Session.Remove($"LoginFailCount_{email}");
                HttpContext.Session.Remove($"LoginFailTime_{email}");
            }
            // lưu lại url
            HttpContext.Session.Clear();
            var previousUrl = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(previousUrl))
            {
                return RedirectToAction("index", "trangchu");
            }
            // Quay về trang trước
            return Redirect(previousUrl);
        }

        #endregion

        #region Xác thực và OTP

        [HttpPost]
        public async Task<IActionResult> GuiOtp(string email, string type)
        {
            try
            {
                // Kiểm tra email
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Vui lòng nhập email." });
                }

                // Xác thực định dạng email
                if (!IsValidEmail(email))
                {
                    return Json(new { success = false, message = "Định dạng email không hợp lệ." });
                }

                // Kiểm tra email đã được sử dụng chưa trong cả hai bảng SysUser và SysGuest
                var taiKhoanRepo = _unitOfWork.Repository<SysUser>();
                var guestRepo = _unitOfWork.Repository<SysGuest>();

                var taiKhoan = taiKhoanRepo.GetAll(tk => tk.Email != null && tk.Email.ToLower() == email.ToLower()).FirstOrDefault();
                var guestAccount = guestRepo.GetAll(g => g.Email != null && g.Email.ToLower() == email.ToLower()).FirstOrDefault();

                if (taiKhoan != null || guestAccount != null)
                {
                    return Json(new { success = false, message = "Email này đã được sử dụng. Vui lòng sử dụng email khác." });
                }

                // Tạo mã OTP ngẫu nhiên (6 chữ số) sử dụng utility class
                string otpCode = OtpUtility.GenerateOtp();

                // Lưu mã OTP và thời gian vào session
                HttpContext.Session.SetString($"OTP_{email}_{type}", otpCode);
                HttpContext.Session.SetString($"OTP_TIME_{email}_{type}", DateTime.Now.ToString());

                // Gửi OTP qua email
                //await SendOtpByEmail(email, otpCode);

                // Trả về thông báo thành công
                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn. Mã sẽ hết hạn sau 5 phút.", otpCode = otpCode });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi mã OTP: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GuiOtpQuenMatKhau(string email)
        {
            try
            {
                // Kiểm tra email
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Vui lòng nhập email." });
                }

                // Xác thực định dạng email
                if (!IsValidEmail(email))
                {
                    return Json(new { success = false, message = "Định dạng email không hợp lệ." });
                }

                // Kiểm tra email có tồn tại trong hệ thống không
                var taiKhoanRepo = _unitOfWork.Repository<SysUser>();
                var guestRepo = _unitOfWork.Repository<SysGuest>();

                var taiKhoan = taiKhoanRepo.GetAll(tk => tk.Email.ToLower() == email.ToLower()).FirstOrDefault();
                var guestAccount = guestRepo.GetAll(g => g.Email.ToLower() == email.ToLower()).FirstOrDefault();

                if (taiKhoan == null && guestAccount == null)
                {
                    return Json(new { success = false, message = "Email này không tồn tại trong hệ thống." });
                }

                // Lưu loại tài khoản vào session để sử dụng khi đặt lại mật khẩu
                string userType = taiKhoan != null ? "User" : "Guest";
                HttpContext.Session.SetString($"RecoveryUserType_{email}", userType);

                // Tạo mã OTP ngẫu nhiên (6 chữ số) sử dụng utility class
                string otpCode = OtpUtility.GenerateOtp();

                // Lưu mã OTP và thời gian vào session
                HttpContext.Session.SetString($"OTP_{email}_recovery", otpCode);
                HttpContext.Session.SetString($"OTP_TIME_{email}_recovery", DateTime.Now.ToString());

                // Gửi OTP qua email
                //await SendOtpByEmail(email, otpCode);

                // Trả về thông báo thành công
                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn. Mã sẽ hết hạn sau 5 phút.", otpCode = otpCode });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi mã OTP: " + ex.Message });
            }
        }

        #endregion

        #region Đăng ký tài khoản

        [HttpPost]
        public IActionResult DangKyNormalUser(string name, string email, string phone, string password, string otpCode)
        {
            // Prevent duplicate form submission
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng thử lại." });
            }

            // Xác thực OTP
            if (!ValidateOtp(email, otpCode, "normal"))
            {
                return Json(new { success = false, message = "Mã OTP không hợp lệ hoặc đã hết hạn." });
            }

            // Gọi phương thức xử lý đăng ký với tham số isNormalUser = true
            var result = ProcessRegistration(name, email, phone, password, 2, true);

            // Nếu đăng ký thành công, xóa OTP khỏi session
            if (result is JsonResult jsonResult)
            {
                var resultObject = jsonResult.Value.GetType().GetProperty("success").GetValue(jsonResult.Value);
                if (resultObject != null && (bool)resultObject)
                {
                    ClearOtpSession(email, "normal");
                }
            }

            return result;
        }

        [HttpPost]
        public IActionResult DangKyPartnerUser(string pname, string pemail, string pphone, string Partnerpassword, string otpCode)
        {
            // Prevent duplicate form submission
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng thử lại." });
            }

            // Xác thực OTP
            if (!ValidateOtp(pemail, otpCode, "partner"))
            {
                return Json(new { success = false, message = "Mã OTP không hợp lệ hoặc đã hết hạn." });
            }

            // Gọi phương thức xử lý đăng ký với tham số isNormalUser = false
            var result = ProcessRegistration(pname, pemail, pphone, Partnerpassword, 3, false);

            // Nếu đăng ký thành công, xóa OTP khỏi session
            if (result is JsonResult jsonResult)
            {
                var resultObject = jsonResult.Value.GetType().GetProperty("success").GetValue(jsonResult.Value);
                if (resultObject != null && (bool)resultObject)
                {
                    ClearOtpSession(pemail, "partner");
                }
            }

            return result;
        }

        #endregion

        #region Quản lý mật khẩu

        [HttpPost]
        public IActionResult DoiMatKhau(string currentPassword, string newPassword, string confirmPassword)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType") ?? "User"; // Default to User if not set

            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thay đổi mật khẩu." });
            }

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
            }

            // Kiểm tra độ dài mật khẩu
            if (newPassword.Length < MIN_PASSWORD_LENGTH)
            {
                return Json(new { success = false, message = $"Mật khẩu phải có ít nhất {MIN_PASSWORD_LENGTH} ký tự." });
            }

            // Xác minh mật khẩu mới và xác nhận mật khẩu trùng khớp
            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu mới và xác nhận mật khẩu không khớp." });
            }

            // Kiểm tra mật khẩu mới không được trùng với mật khẩu cũ
            if (newPassword == currentPassword)
            {
                return Json(new { success = false, message = "Mật khẩu mới không được trùng với mật khẩu hiện tại." });
            }

            try
            {
                if (userType == "User")
                {
                    return ChangeUserPassword(userId.Value, currentPassword, newPassword);
                }
                else // userType == "Guest"
                {
                    return ChangeGuestPassword(userId.Value, currentPassword, newPassword);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thay đổi mật khẩu: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult QuenMatKhau1(string email, string otpCode, string newPassword, string confirmPassword)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode) ||
                    string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
                }

                // Kiểm tra mật khẩu mới và xác nhận mật khẩu
                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "Mật khẩu mới và xác nhận mật khẩu không khớp." });
                }

                // Kiểm tra độ dài mật khẩu
                if (newPassword.Length < MIN_PASSWORD_LENGTH)
                {
                    return Json(new { success = false, message = $"Mật khẩu phải có ít nhất {MIN_PASSWORD_LENGTH} ký tự." });
                }

                // Xác thực OTP
                if (!ValidateOtp(email, otpCode, "recovery"))
                {
                    // Do not clear session here to allow the user to try again
                    return Json(new { success = false, message = "Mã OTP không hợp lệ hoặc đã hết hạn." });
                }

                // Xác định loại tài khoản cần đặt lại mật khẩu
                string userType = HttpContext.Session.GetString($"RecoveryUserType_{email}") ?? "User";

                if (userType == "User")
                {
                    return ResetUserPassword(email, newPassword);
                }
                else // userType == "Guest"
                {
                    return ResetGuestPassword(email, newPassword);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật mật khẩu: " + ex.Message });
            }
            finally
            {
                // Only clear OTP after successful or final attempt
                // This is moved to ResetUserPassword and ResetGuestPassword success paths
                // ClearOtpSession(email, "recovery");
                // HttpContext.Session.Remove($"RecoveryUserType_{email}");
            }
        }

        #endregion

        #region Phương thức hỗ trợ

        /// <summary>
        /// Xử lý đăng ký cho cả người dùng thường và đối tác
        /// </summary>
        private IActionResult ProcessRegistration(string fullName, string email, string phone, string password, int roleId, bool isNormalUser)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
            }

            // Xác thực định dạng email
            if (!IsValidEmail(email))
            {
                return Json(new { success = false, message = "Định dạng email không hợp lệ. Vui lòng sử dụng định dạng email hợp lệ." });
            }

            // Kiểm tra email có phải Gmail
            if (!IsGmailAddress(email))
            {
                return Json(new { success = false, message = "Vui lòng sử dụng địa chỉ Gmail để đăng ký." });
            }

            // Kiểm tra độ dài mật khẩu
            if (password.Length < MIN_PASSWORD_LENGTH)
            {
                return Json(new { success = false, message = $"Mật khẩu phải có ít nhất {MIN_PASSWORD_LENGTH} ký tự." });
            }

            // Xác thực số điện thoại Việt Nam
            if (!IsValidVietnamesePhoneNumber(phone))
            {
                return Json(new { success = false, message = "Số điện thoại không hợp lệ. Số điện thoại phải có 10 chữ số và bắt đầu bằng đầu số Việt Nam (03, 05, 07, 08, 09)." });
            }

            // Chuẩn hóa dữ liệu đầu vào để so sánh nhất quán
            string normalizedEmail = email.ToLower().Trim();
            string normalizedPhone = phone.Trim();

            try
            {
                // Use a transaction to prevent duplicate registrations
                using (var scope = new TransactionScope())
                {
                    // Double-check if account exists inside transaction
                    // to prevent race conditions with concurrent registrations
                    if (IsAccountExistsByEmailOrPhone(normalizedEmail, normalizedPhone))
                    {
                        return Json(new { success = false, message = "Email hoặc số điện thoại đã được sử dụng." });
                    }

                    IActionResult result;
                    if (isNormalUser)
                    {
                        result = RegisterNormalUser(fullName.Trim(), normalizedEmail, normalizedPhone, password, roleId);
                    }
                    else
                    {
                        result = RegisterPartnerUser(fullName.Trim(), normalizedEmail, normalizedPhone, password, roleId);
                    }

                    // Complete the transaction
                    scope.Complete();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình đăng ký: " + ex.Message });
            }
        }

        /// <summary>
        /// Đăng ký người dùng thường
        /// </summary>
        private IActionResult RegisterNormalUser(string fullName, string email, string phone, string password, int roleId)
        {
            var guestRepo = _unitOfWork.Repository<SysGuest>();

            // Đăng ký người dùng thường vào bảng SysGuest
            var newGuest = new SysGuest
            {
                Name = fullName,
                Email = email,
                Phone = phone,
                Password = _passwordHasher.CreateBase64(password),
                IdRole = roleId,
                Status = true,
                Time = DateTime.Now
            };

            // Lưu vào cơ sở dữ liệu
            guestRepo.Insert(newGuest);
            _unitOfWork.Save();

            // Tự động đăng nhập sau khi đăng ký - lưu thông tin vào session 
            SetUserSession(newGuest, "Guest");

            return Json(new { success = true, message = "Đăng ký thành công!", data = JsonSerializer.Serialize(newGuest) });
        }

        /// <summary>
        /// Đăng ký đối tác
        /// </summary>
        private IActionResult RegisterPartnerUser(string fullName, string email, string phone, string password, int roleId)
        {
            var taiKhoanRepo = _unitOfWork.Repository<SysUser>();

            // Đăng ký đối tác vào bảng SysUser
            var newUser = new SysUser
            {
                Name = fullName,
                Email = email,
                Phone = phone,
                Password = _passwordHasher.CreateBase64(password),
                IdRole = roleId,
                Status = true,
                Time = DateTime.Now
            };

            // Lưu vào cơ sở dữ liệu
            taiKhoanRepo.Insert(newUser);
            _unitOfWork.Save();

            // Tự động đăng nhập sau khi đăng ký
            SetUserSession(newUser, "User");

            return Json(new { success = true, message = "Đăng ký thành công!", data = JsonSerializer.Serialize(newUser) });
        }

        /// <summary>
        /// Xử lý xác thực đăng nhập và theo dõi số lần đăng nhập không thành công
        /// </summary>
        private IActionResult ProcessLoginAttempt(dynamic user, string password, string userType, string email)
        {
            string hashedPassword = _passwordHasher.CreateBase64(password);

            if (hashedPassword == user.Password)
            {
                if ((bool)!user.Status)
                {
                    return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa." });
                }

                // Đăng nhập thành công - xóa bộ đếm đăng nhập sai
                ResetFailedLoginAttempts(email);

                // Lưu email để có thể xóa bộ đếm khi đăng xuất
                HttpContext.Session.SetString("UserEmail", email);

                SetUserSession(user, userType);

                return Json(new { success = true, message = "Đăng nhập thành công!", data = JsonSerializer.Serialize(user) });
            }
            else
            {
                // Đăng nhập thất bại - tăng số lần đăng nhập sai
                IncrementFailedLoginAttempts(email);

                // Lấy số lần đăng nhập sai hiện tại
                int failCount = GetFailedLoginAttempts(email);
                int remainingAttempts = MAX_LOGIN_ATTEMPTS - failCount;

                // Sử dụng thông báo lỗi chung
                string message = remainingAttempts > 0
                    ? "Thông tin đăng nhập không chính xác. Vui lòng thử lại."
                    : $"Tài khoản tạm thời bị khóa do quá nhiều lần đăng nhập không thành công. Vui lòng thử lại sau {LOCKOUT_DURATION_MINUTES} phút.";

                return Json(new { success = false, message = message });
            }
        }

        /// <summary>
        /// Kiểm tra xem tài khoản có bị khóa tạm thời do đăng nhập sai quá nhiều lần không
        /// </summary>
        private bool IsAccountLockedOut(string email)
        {
            // Lấy số lần đăng nhập sai và thời gian đăng nhập sai cuối cùng
            int failCount = GetFailedLoginAttempts(email);
            string lastFailTimeStr = HttpContext.Session.GetString($"LoginFailTime_{email}");

            // Nếu số lần đăng nhập sai vượt quá giới hạn
            if (failCount >= MAX_LOGIN_ATTEMPTS && !string.IsNullOrEmpty(lastFailTimeStr))
            {
                // Kiểm tra xem thời gian khóa đã hết chưa
                if (DateTime.TryParse(lastFailTimeStr, out DateTime lastFailTime))
                {
                    TimeSpan timeSinceLastFail = DateTime.Now - lastFailTime;

                    // Nếu thời gian khóa chưa hết
                    if (timeSinceLastFail.TotalMinutes < LOCKOUT_DURATION_MINUTES)
                    {
                        return true;
                    }
                    else
                    {
                        // Thời gian khóa đã hết - đặt lại bộ đếm
                        ResetFailedLoginAttempts(email);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tăng số lần đăng nhập sai
        /// </summary>
        private void IncrementFailedLoginAttempts(string email)
        {
            int currentCount = GetFailedLoginAttempts(email);
            HttpContext.Session.SetInt32($"LoginFailCount_{email}", currentCount + 1);
            HttpContext.Session.SetString($"LoginFailTime_{email}", DateTime.Now.ToString());
        }

        /// <summary>
        /// Lấy số lần đăng nhập sai hiện tại
        /// </summary>
        private int GetFailedLoginAttempts(string email)
        {
            return HttpContext.Session.GetInt32($"LoginFailCount_{email}") ?? 0;
        }

        /// <summary>
        /// Đặt lại bộ đếm đăng nhập sai
        /// </summary>
        private void ResetFailedLoginAttempts(string email)
        {
            HttpContext.Session.Remove($"LoginFailCount_{email}");
            HttpContext.Session.Remove($"LoginFailTime_{email}");
        }

        /// <summary>
        /// Lưu thông tin người dùng vào session
        /// </summary>
        private void SetUserSession(dynamic user, string userType)
        {
            HttpContext.Session.SetString("TaiKhoan", (string)JsonSerializer.Serialize(user));
            HttpContext.Session.SetInt32("UserId", (int)user.Id);
            HttpContext.Session.SetString("UserType", userType);
        }

        /// <summary>
        /// Thay đổi mật khẩu người dùng SysUser
        /// </summary>
        private IActionResult ChangeUserPassword(int userId, string currentPassword, string newPassword)
        {
            var taiKhoanRepo = _unitOfWork.Repository<SysUser>();
            var taiKhoan = taiKhoanRepo.GetById(userId);

            if (taiKhoan == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
            }

            // Xác minh mật khẩu hiện tại
            string hashedCurrentPassword = _passwordHasher.CreateBase64(currentPassword);
            if (hashedCurrentPassword != taiKhoan.Password)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
            }

            // Cập nhật mật khẩu
            taiKhoan.Password = _passwordHasher.CreateBase64(newPassword);
            taiKhoanRepo.Update(taiKhoan);
            _unitOfWork.Save();

            // Cập nhật phiên làm việc với dữ liệu người dùng mới
            HttpContext.Session.SetString("TaiKhoan", JsonSerializer.Serialize(taiKhoan));

            return Json(new { success = true, message = "Thay đổi mật khẩu thành công." });
        }

        /// <summary>
        /// Thay đổi mật khẩu người dùng SysGuest
        /// </summary>
        private IActionResult ChangeGuestPassword(int userId, string currentPassword, string newPassword)
        {
            var guestRepo = _unitOfWork.Repository<SysGuest>();
            var guestAccount = guestRepo.GetById(userId);

            if (guestAccount == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
            }

            // Xác minh mật khẩu hiện tại
            string hashedCurrentPassword = _passwordHasher.CreateBase64(currentPassword);
            if (hashedCurrentPassword != guestAccount.Password)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
            }

            // Cập nhật mật khẩu
            guestAccount.Password = _passwordHasher.CreateBase64(newPassword);
            guestRepo.Update(guestAccount);
            _unitOfWork.Save();

            // Cập nhật phiên làm việc với dữ liệu người dùng mới
            HttpContext.Session.SetString("TaiKhoan", JsonSerializer.Serialize(guestAccount));

            return Json(new { success = true, message = "Thay đổi mật khẩu thành công." });
        }

        /// <summary>
        /// Đặt lại mật khẩu người dùng SysUser
        /// </summary>
        private IActionResult ResetUserPassword(string email, string newPassword)
        {
            // Tìm kiếm tài khoản với email trong bảng SysUser
            var taiKhoanRepo = _unitOfWork.Repository<SysUser>();
            var taiKhoan = taiKhoanRepo.GetAll(tk => tk.Email.ToLower() == email.ToLower()).FirstOrDefault();

            if (taiKhoan == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản với email này." });
            }

            // Cập nhật mật khẩu mới
            taiKhoan.Password = _passwordHasher.CreateBase64(newPassword);
            taiKhoanRepo.Update(taiKhoan);
            _unitOfWork.Save();

            // Clear OTP session after successful password reset
            ClearOtpSession(email, "recovery");
            HttpContext.Session.Remove($"RecoveryUserType_{email}");

            return Json(new { success = true, message = "Mật khẩu đã được cập nhật thành công." });
        }

        /// <summary>
        /// Đặt lại mật khẩu người dùng SysGuest
        /// </summary>
        private IActionResult ResetGuestPassword(string email, string newPassword)
        {
            // Tìm kiếm tài khoản với email trong bảng SysGuest
            var guestRepo = _unitOfWork.Repository<SysGuest>();
            var guestAccount = guestRepo.GetAll(g => g.Email.ToLower() == email.ToLower()).FirstOrDefault();

            if (guestAccount == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản với email này." });
            }

            // Cập nhật mật khẩu mới
            guestAccount.Password = _passwordHasher.CreateBase64(newPassword);
            guestRepo.Update(guestAccount);
            _unitOfWork.Save();

            // Clear OTP session after successful password reset
            ClearOtpSession(email, "recovery");
            HttpContext.Session.Remove($"RecoveryUserType_{email}");

            return Json(new { success = true, message = "Mật khẩu đã được cập nhật thành công." });
        }

        /// <summary>
        /// Xác thực OTP
        /// </summary>
        private bool ValidateOtp(string email, string otpCode, string type)
        {
            string storedOtp = HttpContext.Session.GetString($"OTP_{email}_{type}");
            string otpTimeStr = HttpContext.Session.GetString($"OTP_TIME_{email}_{type}");

            // Kiểm tra OTP có tồn tại không
            if (string.IsNullOrEmpty(storedOtp))
            {
                return false;
            }

            // Kiểm tra OTP có khớp không
            if (storedOtp != otpCode)
            {

                return false;
            }

            // Kiểm tra thời gian hết hạn của OTP (5 phút)
            if (!string.IsNullOrEmpty(otpTimeStr) && DateTime.TryParse(otpTimeStr, out DateTime otpTime))
            {
                TimeSpan timeDifference = DateTime.Now - otpTime;
                if (timeDifference.TotalMinutes > 5)
                {
                    // Xóa OTP đã hết hạn khỏi session
                    ClearOtpSession(email, type);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Xóa thông tin OTP khỏi session
        /// </summary>
        private void ClearOtpSession(string email, string type)
        {
            HttpContext.Session.Remove($"OTP_{email}_{type}");
            HttpContext.Session.Remove($"OTP_TIME_{email}_{type}");
        }

        /// <summary>
        /// Kiểm tra tài khoản đã tồn tại bằng email hoặc số điện thoại
        /// </summary>
        private bool IsAccountExistsByEmailOrPhone(string email, string phone)
        {
            var taiKhoanRepo = _unitOfWork.Repository<SysUser>();
            var guestRepo = _unitOfWork.Repository<SysGuest>();

            // Kiểm tra email đã tồn tại trong cả 2 bảng chưa
            var existingUserByEmail = taiKhoanRepo.GetAll(tk => tk.Email != null && tk.Email.ToLower() == email).FirstOrDefault();
            var existingGuestByEmail = guestRepo.GetAll(g => g.Email != null && g.Email.ToLower() == email).FirstOrDefault();

            // Kiểm tra số điện thoại đã tồn tại trong cả 2 bảng chưa
            var existingUserByPhone = taiKhoanRepo.GetAll(tk => tk.Phone != null && tk.Phone == phone).FirstOrDefault();
            var existingGuestByPhone = guestRepo.GetAll(g => g.Phone != null && g.Phone == phone).FirstOrDefault();

            return (existingUserByEmail != null || existingGuestByEmail != null ||
                   existingUserByPhone != null || existingGuestByPhone != null);
        }

        /// <summary>
        /// Phương thức gửi OTP qua email
        /// </summary>
        private async Task SendOtpByEmail(string email, string subject, string otpCode)
        {
            try
            {
                //subject = "Mã xác thực OTP từ MyTravel";
                subject = subject;
                string body = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #4a89dc; color: white; padding: 10px 20px; text-align: center; }}
                            .content {{ padding: 20px; border: 1px solid #ddd; }}
                            .otp-code {{ font-size: 24px; font-weight: bold; text-align: center; margin: 20px 0; color: #4a89dc; letter-spacing: 5px; }}
                            .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>MyTravel - Xác thực tài khoản</h2>
                            </div>
                            <div class='content'>
                                <p>Xin chào,</p>
                                <p>Mã OTP của bạn là:</p>
                                <div class='otp-code'>{otpCode}</div>
                                <p>Mã OTP này sẽ hết hạn sau 5 phút.</p>
                                <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                                <p>Trân trọng,</p>
                                <p>Đội ngũ MyTravel</p>
                            </div>
                            <div class='footer'>
                                <p>Đây là email tự động, vui lòng không trả lời.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(email, subject, body, true);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi trong triển khai thực tế
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw; // Re-throw để xử lý tại điểm gọi
            }
        }

        /// <summary>
        /// Phương thức hỗ trợ xác thực định dạng email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            // Biểu thức chính quy để xác thực email
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        /// <summary>
        /// Phương thức hỗ trợ kiểm tra xem email có phải là địa chỉ Gmail
        /// </summary>
        private bool IsGmailAddress(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            string normalizedEmail = email.ToLower().Trim();
            return normalizedEmail.EndsWith("@gmail.com");
        }

        /// <summary>
        /// Kiểm tra số điện thoại có đúng định dạng Việt Nam không (10 số, bắt đầu bằng đầu số Việt Nam)
        /// </summary>
        private bool IsValidVietnamesePhoneNumber(string phone)
        {
            // Xác thực số điện thoại - chỉ được chứa chữ số
            if (!phone.All(char.IsDigit))
            {
                return false;
            }

            // Kiểm tra độ dài phải đúng 10 số
            if (phone.Length != PHONE_NUMBER_LENGTH)
            {
                return false;
            }

            // Kiểm tra đầu số Việt Nam (03, 05, 07, 08, 09)
            string prefix = phone.Substring(0, 2);
            string[] validPrefixes = { "03", "05", "07", "08", "09" };

            return validPrefixes.Contains(prefix);
        }

        #endregion
    }
}
