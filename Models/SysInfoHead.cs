using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysInfoHead
{
    public int Id { get; set; }

    public string? Line1 { get; set; }

    public string? Line2 { get; set; }

    public string? Line3 { get; set; }

    public string? UrlVideo { get; set; }

    public bool? Status { get; set; }
}
