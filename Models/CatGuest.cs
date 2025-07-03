using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatGuest
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Name { get; set; }

    public bool? Status { get; set; }
}
