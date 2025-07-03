using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysCar
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdType { get; set; }

    public int? IdCategory { get; set; }

    public int? IdContry { get; set; }

    public string? ListImg { get; set; }

    public string? Name { get; set; }

    public string? LocalIframe { get; set; }

    public string? LocalText { get; set; }

    public string? Amenities { get; set; }

    public double? Price { get; set; }

    public string? Description { get; set; }

    public string? Mile { get; set; }

    public string? Oil { get; set; }

    public string? Regime { get; set; }

    public string? Life { get; set; }

    public string? Specifications { get; set; }

    public bool? Featured { get; set; }

    public bool? Like { get; set; }

    public bool? Status { get; set; }

    public string? Phone { get; set; }

    public int? NumStar { get; set; }

    public double? Score { get; set; }

    public int? Reviews { get; set; }

    public int? IdPromotion { get; set; }

    public int? Seat { get; set; }

    public DateTime? Time { get; set; }

    public virtual CatCategory? IdCategoryNavigation { get; set; }

    public virtual SysPromotion? IdPromotionNavigation { get; set; }

    public virtual CatCar? IdTypeNavigation { get; set; }
}
