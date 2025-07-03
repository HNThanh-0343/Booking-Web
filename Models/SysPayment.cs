using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysPayment
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public string? Action { get; set; }

    public double? TotalPrice { get; set; }

    public TimeOnly? Time { get; set; }
}
