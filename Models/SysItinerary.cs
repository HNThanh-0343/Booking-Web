using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysItinerary
{
    public int Id { get; set; }

    public string? DayName { get; set; }

    public string? LocalName { get; set; }

    public string? Description { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysTour> SysTours { get; set; } = new List<SysTour>();
}
