using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatTypeActivity
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysActivity> SysActivities { get; set; } = new List<SysActivity>();

    public virtual ICollection<SysTour> SysTours { get; set; } = new List<SysTour>();
}
