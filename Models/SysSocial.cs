using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysSocial
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Icon { get; set; }

    public string? UrlSocial { get; set; }

    public bool? Status { get; set; }
}
