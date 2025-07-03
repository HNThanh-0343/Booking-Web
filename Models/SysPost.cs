using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysPost
{
    public int Id { get; set; }

    public int? IdEvaluate { get; set; }

    public int? IdPricingUnit { get; set; }

    public int? Idpromotion { get; set; }

    public int? IddemoPic { get; set; }

    public string? Name { get; set; }

    public string? Local { get; set; }

    public string? Price { get; set; }

    public string? Overview { get; set; }

    public bool? Like { get; set; }

    public string? Feature { get; set; }

    public string? Picture { get; set; }

    public string? GuestsNumber { get; set; }
}
