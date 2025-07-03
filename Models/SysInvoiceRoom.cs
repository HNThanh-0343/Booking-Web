using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysInvoiceRoom
{
    public int Id { get; set; }

    public int IdHotel { get; set; }

    public DateTime DateCreate { get; set; }

    public decimal? TotalMoney { get; set; }

    public int? IdUser { get; set; }

    public string? FullNameGuest { get; set; }

    public string? PhoneGuest { get; set; }

    public string? EmailGuest { get; set; }

    public string ListIdRoomBooking { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Note { get; set; }

    public bool? Status { get; set; }
}
