using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysMenu
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Controller { get; set; }

    public int? PrentId { get; set; }

    public bool? Status { get; set; }
}
