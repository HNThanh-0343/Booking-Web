using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysContact
{
    public int Id { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Name { get; set; }

    public string? ContentMap { get; set; }

    public string? Urlfb { get; set; }

    public string? Urltwi { get; set; }

    public string? Urlinta { get; set; }

    public string? Urlin { get; set; }

    public bool? Status { get; set; }
}
