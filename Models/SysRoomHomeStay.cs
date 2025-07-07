using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysRoomHomeStay
{
    public int Id { get; set; }

    public int? IdHomeStay { get; set; }

    public string? Name { get; set; }

    public int? TypeRoom { get; set; }

    public string? IdTypeBed { get; set; }

    public string? Description { get; set; }

    public int? AdultsMax { get; set; }

    public int? ChildrenMax { get; set; }

    public int? TotalRoom { get; set; }

    public string? ListImg { get; set; }

    public string? ListAminities { get; set; }

    public double? Price { get; set; }

    public bool? Feature { get; set; }

    public bool? Status { get; set; }
}
