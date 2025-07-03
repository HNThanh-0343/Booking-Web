using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysCallLog
{
    public int Id { get; set; }

    public int? IdCategory { get; set; }

    public int? IdService { get; set; }

    public bool? Phone { get; set; }

    public bool? Zalo { get; set; }

    public string? Ip { get; set; }

    public DateTime? Time { get; set; }

    public string? Url { get; set; }
}
