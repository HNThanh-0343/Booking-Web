using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatContry
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Image { get; set; }

    public int? Code { get; set; }

    public bool? Featured { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysHotel> SysHotels { get; set; } = new List<SysHotel>();
}
