using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysPromotion
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Code { get; set; }

    public string? Image { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? Type { get; set; }

    public string? Describe { get; set; }

    public string? Condition { get; set; }

    public decimal? ConditionNumber { get; set; }

    public int? SaleOff { get; set; }

    public int? Quantity { get; set; }

    public int? QuantityUse { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysHotel> SysHotels { get; set; } = new List<SysHotel>();

    public virtual ICollection<SysVilla> SysVillas { get; set; } = new List<SysVilla>();
}
