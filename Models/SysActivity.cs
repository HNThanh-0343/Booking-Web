using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysActivity
{
    public int Id { get; set; }

    public int? IdCategory { get; set; }

    public int? IdUser { get; set; }

    public int? IdType { get; set; }

    public string? Name { get; set; }

    public string? LocalIframe { get; set; }

    public string? LocalText { get; set; }

    public string? Description { get; set; }

    public string? Experience { get; set; }

    public string? Includes { get; set; }

    public double? Price { get; set; }

    public bool? CancelationPolicy { get; set; }

    public string? Voucher { get; set; }

    public string? Duration { get; set; }

    public string? TickerType { get; set; }

    public string? TickerCollection { get; set; }

    public bool? Status { get; set; }

    public string? ListImg { get; set; }

    public bool? Featured { get; set; }

    public bool? Like { get; set; }

    public string? Phone { get; set; }

    public DateTime? Time { get; set; }

    public double? Score { get; set; }

    public int? Reviews { get; set; }

    public int? IdPromotion { get; set; }

    public int? NumStar { get; set; }

    public virtual CatCategory? IdCategoryNavigation { get; set; }

    public virtual SysPromotion? IdPromotionNavigation { get; set; }

    public virtual CatTypeActivity? IdTypeNavigation { get; set; }
}
