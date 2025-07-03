using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using WEBSITE_TRAVELBOOKING.Infrastructure;
using WEBSITE_TRAVELBOOKING.Models;
using Newtonsoft.Json;

namespace WEBSITE_TRAVELBOOKING.Helper
{
    public class Common
    {
        private IUnitOfWork _unitOfWork;
        public Common(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Common()
        {
        }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        public static string KeyCode()
        {
            return "xxx";
        }

        public static string GenerateSlug(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";

            // Bỏ dấu tiếng Việt
            string normalized = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            string slug = sb.ToString().Normalize(NormalizationForm.FormC);

            // Viết thường, thay thế khoảng trắng và ký tự đặc biệt
            slug = slug.ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");     // Bỏ ký tự không hợp lệ
            slug = Regex.Replace(slug, @"\s+", "-").Trim();      // Thay khoảng trắng bằng dấu gạch
            slug = Regex.Replace(slug, @"-+", "-");              // Bỏ trùng dấu gạch

            return slug;
        }
        public SysUser GetUser(IHttpContextAccessor _httpContextAccessor)
        {
            try
            {
                var CookieUser = _httpContextAccessor?.HttpContext?.Request?.Cookies["Account"]?.ToString();
                if (CookieUser != null)
                {
                    var CVCookieUser = Common.Decrypt(KeyCode(), CookieUser);
                    if (!string.IsNullOrEmpty(CVCookieUser))
                    {
                        return JsonConvert.DeserializeObject<SysUser>(CVCookieUser);
                    }
                    else { return new SysUser(); }
                }
                else
                {
                    return new SysUser();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public SysUser GetUser(string cookieJson)
        {
            try
            {
                var accountCookies = new SysUser();
                if (cookieJson != null)
                {
                    var CVCookieUser = Common.Decrypt(KeyCode(), cookieJson);
                    accountCookies = JsonConvert.DeserializeObject<SysUser>(CVCookieUser);
                    if (accountCookies.Id != 0)
                    {
                        return accountCookies;
                    }
                    else
                    {
                        return accountCookies;
                    }
                }
                else
                {
                    return new SysUser();
                }
            }
            catch (Exception ex)
            {
                return new SysUser();
            }
        }
        //Mã hóa
        public static string Encrypt(string key, string toEncrypt)
        {
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt.Trim());
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                TripleDESCryptoServiceProvider tdes =
                new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tdes.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(
                    toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch
            {

            }
            return String.Empty;
        }

        //Giải mã
        public static string Decrypt(string key, string toDecrypt)
        {
            try
            {
                if (!String.IsNullOrEmpty(toDecrypt))
                {
                    byte[] keyArray;
                    byte[] toEncryptArray = Convert.FromBase64String(toDecrypt.Trim());
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                    tdes.Key = keyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cTransform = tdes.CreateDecryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(
                    toEncryptArray, 0, toEncryptArray.Length);
                    return UTF8Encoding.UTF8.GetString(resultArray);
                }
                return "Giải mã thất bại";
            }
            catch
            {

            }
            return String.Empty;
        }

        public static string SaveUrlImg(int Id, string wwwPath, string urlImg, IFormFile postedFile)
        {
            try
            {
                var path = wwwPath + urlImg;
                var currentTimeString = DateTime.Now.ToString("ddMMyyHHmmss");
                var pathdefault = "\\AppData\\no-image.png";
                // Lưu avatar
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var pathFile = path + Id + currentTimeString + postedFile.FileName;
                using (FileStream fileStream = System.IO.File.Create(pathFile))
                {
                    postedFile.CopyTo(fileStream);
                    fileStream.Flush();
                    pathdefault = urlImg + Id + currentTimeString + postedFile.FileName;
                }
                return pathdefault;
            }
            catch (Exception)
            {

                throw;
            }
            return String.Empty;
        }
        // kiểm tra ảnh
        public static string GetImagePath(string Img)
        {
            if (string.IsNullOrWhiteSpace(Img))
            {
                return "/AppData/no-image.png";
            }

            string trimmedPath = Img.Trim().TrimStart('/', '\\');

            var rootPath = Directory.GetCurrentDirectory();
            var fullPath = Path.Combine(rootPath, "wwwroot", trimmedPath);
            var defaultPath = "/AppData/no-image.png";

            return File.Exists(fullPath) ? "/" + trimmedPath.Replace("\\", "/") : defaultPath;
        }
        public static string GetDateTimeConvert(DateTime date)
        {

            if (date != null)
            {
                return $"Ngày {date.ToString("dd")} tháng {date.ToString("MM")} năm {date.ToString("yyyy")}";
            }
            return "";
        }
        public static List<RoomServiceItem> DeserializeServiceList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<RoomServiceItem>();

            return JsonConvert.DeserializeObject<List<RoomServiceItem>>(json);
        }
        public static string SerializeServiceList(List<RoomServiceItem> services)
        {
            return services != null ? JsonConvert.SerializeObject(services) : "";
        }
        public static string GenerateRandomQrCode(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
    public class Account
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public static void Configure(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }
        public static SysUser? GetAccount()
        {
            var json = _httpContextAccessor.HttpContext?.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(json)) return null;
            return System.Text.Json.JsonSerializer.Deserialize<SysUser>(json);
        }
        public static SysGuest? GetGuest()
        {
            var json = _httpContextAccessor.HttpContext?.Session.GetString("TaiKhoan");
            if (string.IsNullOrEmpty(json)) return null;
            return System.Text.Json.JsonSerializer.Deserialize<SysGuest>(json);
        }
    }
    public class GetIframeSetLocal()
    {
        public class LatLongResult
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
        public static LatLongResult ExtractLatLongFromIframe(string iframeHtml)
        {
            if (string.IsNullOrWhiteSpace(iframeHtml))
                return null;

            // Tìm src trong thẻ iframe
            var srcMatch = Regex.Match(iframeHtml, @"src=""([^""]+)""");
            if (!srcMatch.Success)
                return null;

            var src = System.Web.HttpUtility.UrlDecode(srcMatch.Groups[1].Value);

            // Tìm tọa độ !2d (long), !3d (lat)
            var lngMatch = Regex.Match(src, @"!2d([-.\d]+)");
            var latMatch = Regex.Match(src, @"!3d([-.\d]+)");

            if (latMatch.Success && lngMatch.Success)
            {
                return new LatLongResult
                {
                    Lat = double.Parse(latMatch.Groups[1].Value), //Y
                    Lng = double.Parse(lngMatch.Groups[1].Value) //X
                };
            }

            return null;
        }
    }
    public class CheckIMGServer()
    {
        public static string TimAnhKhongTonTai(string danhSachAnh, int typeReturn = 1)
        {
            if (string.IsNullOrWhiteSpace(danhSachAnh))
                return "/assets/img/AnhMacDinh.jpg";

            var danhSach = danhSachAnh.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string thuMucAnh = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            if (typeReturn == 1)
            {
                foreach (var tenAnh in danhSach)
                {
                    var duongDanAnh = Path.Combine(thuMucAnh, tenAnh);
                    if (!File.Exists(duongDanAnh))
                    {
                        return tenAnh; // Trả về ảnh đầu tiên không tồn tại
                    }
                }
                return ""; // Tất cả ảnh đều tồn tại
            }
            else if (typeReturn == 2)
            {
                var listAnhKhongTonTai = new List<string>();
                foreach (var tenAnh in danhSach)
                {
                    var duongDanAnh = Path.Combine(thuMucAnh, tenAnh);
                    if (!File.Exists(duongDanAnh))
                    {
                        listAnhKhongTonTai.Add(tenAnh);
                    }
                }

                return listAnhKhongTonTai.Count > 0 ? string.Join(",", listAnhKhongTonTai) : "";
            }

            return ""; // Mặc định nếu typeReturn không hợp lệ
        }

    }
}
