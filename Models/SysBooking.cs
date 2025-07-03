using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysBooking
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdPromotion { get; set; }

    public string? FullNameGuest { get; set; }

    public string? PhoneGuest { get; set; }

    public string? EmailGuest { get; set; }

    public int IdCategories { get; set; }

    public int BookingItemId { get; set; }

    public string? ListItemServices { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int? GuestsNumber { get; set; }

    public string? DesQr { get; set; }

    public decimal Price { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime? BookingDate { get; set; }

    public DateTime? CheckInDate { get; set; }

    public decimal? Deposit { get; set; }

    public string? Note { get; set; }

    public int Status { get; set; }

    public virtual CatCategory IdCategoriesNavigation { get; set; } = null!;

    public virtual SysUser? IdUserNavigation { get; set; }
}
