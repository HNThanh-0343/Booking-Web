using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatRoomService
{
    public int Id { get; set; }

    public int? IdHotel { get; set; }

    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public bool? Status { get; set; }
}
