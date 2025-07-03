using System.ComponentModel.DataAnnotations;

namespace WEBSITE_TRAVELBOOKING.Models
{
    /// <summary>
    /// Đăng ký đăng nhập
    /// </summary>
    public class AccountForm
    {
        //public int Id { get; set; }

        public int? IdRole { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email!")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người dùng!")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại không hợp lệ!")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã OTP!")]
        public string? CodeOTP { get; set; }
    }

    public class AccountLoginForm
    {


        [Required(ErrorMessage = "Vui lòng nhập thông tin!")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string? Password { get; set; }
        public bool RememberMe { get; set; }

    }

    public class AccountForgotForm
    {


        [Required(ErrorMessage = "Vui lòng nhập email!")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã OTP!")]
        public string? CodeOTP { get; set; }
    }

    public class PostViewHome
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Local { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool Featured { get; set; }
        public bool Like { get; set; }
        public double? Price { get; set; }
        public string Url { get; set; }
    }
    public class HotelViewDetail
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Discount { get; set; }
        public bool Featured { get; set; }
        public string? Amenities { get; set; }
        public bool Like { get; set; }
        public double? Price { get; set; }
        public string Url { get; set; }
    }
    public class HotelViewHome
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Local { get; set; }
        public int Discount { get; set; }
        public bool Featured { get; set; }
        public string? Amenities { get; set; }
        public bool Like { get; set; }
        public decimal? Price { get; set; }
        public string Url { get; set; }
        public float NumberStar { get; set; }
        public int? Sale { get; set; }
        public bool? type { get; set; }
    }

    public class promoInfo
    {
        public bool? Type { get; set; }
        public int? Sale { get; set; }
    }

    public class CountryHotelCountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
    public class ModelsAndRole
    {
        public int Id { get; set; }
        public int IdModule { get; set; }
        public int IdRole { get; set; }
        public string Name { get; set; }
        public string NameController { get; set; }
        public bool? IsView { get; set; }
        public bool? IsCreate { get; set; }
        public bool? IsEdit { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsPermission { get; set; }
        public string? PrentId { get; set; }
        public int? Order { get; set; }  // nếu cần sort
        public List<ModelsAndRole> Children { get; set; } = new();
    }
    public class RoleAll
    {
        public int IsView { get; set; }
        public int IsAdd { get; set; }
        public int IsEdit { get; set; }
        public int IsDelete { get; set; }
    }

    public class IsAction
    {
        public Nullable<int> Rule { get; set; }
        public string urlNameAction { get; set; }
        public string NameController { get; set; }
    }
    public class HideShowAction
    {
        public IsAction IsView { get; set; }
        public IsAction IsAdd { get; set; }
        public IsAction IsEdit { get; set; }
        public IsAction IsDetails { get; set; }
        public IsAction IsDelete { get; set; }
        public IsAction IsApprove { get; set; }
        public IsAction IsDownload { get; set; }
        public IsAction IsExecutive { get; set; }
    }
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameController { get; set; }
        public string Icon { get; set; }
        public string? PrentId { get; set; }
        public int? Order { get; set; }
        public bool Status { get; set; }

        public List<MenuItem> Children { get; set; } = new();
    }
    public class TourFilterViewModel
    {
        public int? page { get; set; }
        public List<int> NumStar { get; set; }
        public List<int> IdContry { get; set; }
        public string SortOrders { get; set; }
        public string Location { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? IdType { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public int? GuestNumber { get; set; }
    }

    public class ProvinceDto
    {
        public string Name { get; set; }
        public int Code { get; set; }
    }

    public class SortItem
    {
        public string Key { get; set; }
        public string Direction { get; set; }
    }   
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public double AverageScore { get; set; }
        public string Avatar { get; set; }
        public int Cleanliness { get; set; }
        public int Facilities { get; set; }
        public int ValueForMoney { get; set; }
        public int Service { get; set; }
        public int Location { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
    }
    public class RoomStatusDto
    {
        public int RoomId { get; set; }
        public int? IdHotel { get; set; }
        public string? Name { get; set; }
        public int? TypeRoom { get; set; }
        public string? TypeRoomName { get; set; }
        public string? Description { get; set; }
        public int? AdultsMax { get; set; }
        public int? ChildrenMax { get; set; }
        public int? Floor { get; set; }
        public int? NumRoom { get; set; }
        public string? ListImg { get; set; }
        public string? ListAminities { get; set; }
        public double? Price { get; set; }

        public bool IsBooked { get; set; }
        public SysBooking? BookingInfo { get; set; }
        public bool SoonCheckout { get; set; }
        public bool IsInterrupted { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsSplitGroupHeader { get; set; } = false;
        public DateTime? CheckoutTime => BookingInfo?.EndDate;
        public RoomStatusInfo StatusDisplay
        {
            get
            {
                if (!IsBooked) return new RoomStatusInfo { Text = "Phòng trống", Color = "success" };
                if (SoonCheckout) return new RoomStatusInfo { Text = "Sắp trả phòng", Color = "warning" };
                return new RoomStatusInfo { Text = "Đang có khách", Color = "danger" };
            }
        }
        public int TotalRoom { get; set; }
        public int BookedRoom { get; set; }
        public int AvailableRoom { get; set; }
    }

    public class RoomStatusInfo
    {
        public string Text { get; set; } = "";
        public string Color { get; set; } = "";
    }

    public class BookRoomNoUser
    {
        public string? FullNameGuest { get; set; }

        public string? PhoneGuest { get; set; }

        public string? EmailGuest { get; set; }
        public int BookingItemId { get; set; }
        public decimal Price { get; set; }
        public int? GuestsNumber { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? BookingDate { get; set; }

        public DateTime? CheckInDate { get; set; }
        public DateTime StartDate_ISO { get; set; }

        public DateTime EndDate_ISO { get; set; }
    }
    public class RoomServiceItem
    {
        public int IdService { get; set; }
        public int Quantity { get; set; }
    }
    public class RoomAndServiceItem
    {
        public int IdBooking { get; set; }
        public List<RoomServiceItem> roomServiceItems { get; set; }
    }
    public class FilterDataViewHotel
    {
        public int? page { get; set; }
        public List<int> NumStar { get; set; }
        public List<int> IdContry { get; set; }
        public string SortOrders { get; set; }
        public string Location { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? Rooms { get; set; }
        public int? Adults { get; set; }
        public int? Children { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
    }
    public class ModelQRCode
    {
        public string bank { get; set; }
        public string acc { get; set; }
        public decimal amount { get; set; }
        public string des { get; set; }
    }
    public class RoleTreeContext
    {
        public int Index { get; set; }
        public int Level { get; set; }
    }
    public class InvoiceRoom
    {
        public string IdBooking { get; set; }
        public decimal? TotalMoney { get; set; }
        public decimal? Surcharge { get; set; }
        public string? Note { get; set; }
    }
    #region Sự kiện hotel nổi bật
    public class LocalStorage
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Local { get; set; }
        public string? Images { get; set; }
        public double Price { get; set; }
        public int Discount { get; set; }
        public string? Amenities { get; set; }
        public string Url { get; set; }
        public float NumberStar { get; set; }
        public int? Sale { get; set; }
        public bool? type { get; set; }

    }
    #endregion
    public class ExtendCheckOutTime
    {
        public int IdBooking { get; set; }
        public int IdRoom { get; set; }
        public DateTime EndDate_ISO { get; set; }

    }
    public partial class ModelHotel
    {
        public int IdHotel { get; set; }
        public int? IdContry { get; set; }
        public string? Name { get; set; }
        public decimal? PriceMin { get; set; }
        public string? IMG { get; set; }
        public string? Local { get; set; }
        public string? Amenities { get; set; }
        public bool? Featured { get; set; }
        public int? NumStar { get; set; }
        public string? Localiframe { get; set; }
        public int? IdPromotion { get; set; }
        public virtual SysPromotion? IdPromotionNavigation { get; set; }
    }
    public class CountryViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool? Featured { get; set; }
    }
    public class HotelViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Local { get; set; }
    }
    public class BinhLuanBaiViet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime DateTime { get; set; }
        public string Avatar { get; set; }
    }
}
