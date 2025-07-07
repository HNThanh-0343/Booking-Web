using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Controller { get; set; }

    public string? Icon { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysBooking> SysBookings { get; set; } = new List<SysBooking>();

    public virtual ICollection<SysHotel> SysHotels { get; set; } = new List<SysHotel>();

    public virtual ICollection<SysVilla> SysVillas { get; set; } = new List<SysVilla>();
}
