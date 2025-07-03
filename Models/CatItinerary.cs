using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatItinerary
{
    public int Id { get; set; }

    public int? IdTour { get; set; }

    public int? Day { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }
}
