using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysAdvetise
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Urlimg { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? Status { get; set; }
}
