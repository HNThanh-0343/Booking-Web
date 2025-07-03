using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatPromotionGuest
{
    public int Id { get; set; }

    public int? IdGuest { get; set; }

    public int? IdPromotion { get; set; }

    public DateTime? DayReceive { get; set; }

    public bool? IsStatus { get; set; }
}
