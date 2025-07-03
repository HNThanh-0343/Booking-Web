using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysTour
{
    public int Id { get; set; }

    public int? IdCategory { get; set; }

    public int? IdType { get; set; }

    public string? Name { get; set; }

    public string? LocalText { get; set; }

    public string? LocalIframe { get; set; }

    public double? Price { get; set; }

    public string? Description { get; set; }

    public string? WhattoExpect { get; set; }

    public int? MaxGuest { get; set; }

    public string? MaxPeople { get; set; }

    public string? MinAge { get; set; }

    public string? Wifi { get; set; }

    public DateTime? Time { get; set; }

    public string? DateLine { get; set; }

    public string? TimeLine { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Pickup { get; set; }

    public int? IdItinerary { get; set; }

    public int? IdUser { get; set; }

    public string? ListImg { get; set; }

    public bool? Featured { get; set; }

    public bool? Like { get; set; }

    public string? Phone { get; set; }

    public double? Score { get; set; }

    public int? Reviews { get; set; }

    public int? NumStar { get; set; }

    public bool? Status { get; set; }

    public int? IdPromotion { get; set; }

    public int? IdContry { get; set; }

    public virtual CatCategory? IdCategoryNavigation { get; set; }

    public virtual SysItinerary? IdItineraryNavigation { get; set; }

    public virtual SysPromotion? IdPromotionNavigation { get; set; }

    public virtual CatTypeActivity? IdTypeNavigation { get; set; }
}
