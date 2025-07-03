using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysHotel
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdCategory { get; set; }

    public int? IdContry { get; set; }

    public int? IdPromotion { get; set; }

    public string? Name { get; set; }

    public string? Local { get; set; }

    public string? Localiframe { get; set; }

    public string? ListImg { get; set; }

    public decimal? PriceMin { get; set; }

    public string? Description { get; set; }

    public string? Amenities { get; set; }

    public bool? Featured { get; set; }

    public double? Score { get; set; }

    public int? Reviews { get; set; }

    public int? NumStar { get; set; }

    public string? Phone { get; set; }

    public DateTime? TimeCreate { get; set; }

    public string? HouseRules { get; set; }

    public string? ListManagerId { get; set; }

    public double? LocalX { get; set; }

    public double? LocalY { get; set; }

    public bool? Status { get; set; }

    public virtual CatCategory? IdCategoryNavigation { get; set; }

    public virtual CatContry? IdContryNavigation { get; set; }

    public virtual SysPromotion? IdPromotionNavigation { get; set; }
}
