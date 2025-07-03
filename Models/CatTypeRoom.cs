using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatTypeRoom
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Icon { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysRoom> SysRooms { get; set; } = new List<SysRoom>();
}
