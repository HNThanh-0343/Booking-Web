using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysRoom
{
    public int Id { get; set; }

    public int? IdHotel { get; set; }

    public string? Name { get; set; }

    public int? TypeRoom { get; set; }

    public string? IdTypeBed { get; set; }

    public string? ContentBed { get; set; }

    public string? Description { get; set; }

    public int? AdultsMax { get; set; }

    public int? ChildrenMax { get; set; }

    public int? TotalRoom { get; set; }

    public int? Floor { get; set; }

    public int? NumRoom { get; set; }

    public string? ListImg { get; set; }

    public string? ListAminities { get; set; }

    public string? AmenitiesShort { get; set; }

    public string? AmenitiesLong { get; set; }

    public double? Price { get; set; }

    public bool? Feature { get; set; }

    public bool? Status { get; set; }

    public virtual CatTypeRoom? TypeRoomNavigation { get; set; }
}
