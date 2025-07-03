using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysLogPayment
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdCategories { get; set; }

    public int? IdBooking { get; set; }

    public string? Name { get; set; }

    public double? TotalPrice { get; set; }

    public DateTime? Time { get; set; }

    public string? Url { get; set; }

    public string? Action { get; set; }

    public string? RoomType { get; set; }

    public string? NumberOfGuests { get; set; }

    public string? PaymentMethod { get; set; }

    public string? SpecialRequests { get; set; }

    public string? Contents { get; set; }

    public int? Status { get; set; }
}
