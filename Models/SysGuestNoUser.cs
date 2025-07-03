using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysGuestNoUser
{
    public int Id { get; set; }

    public int? IdUserBooking { get; set; }

    public string? Ho { get; set; }

    public string? Ten { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }
}
