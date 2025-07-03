using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysEvaluate
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdCategory { get; set; }

    public int? IdService { get; set; }

    public double? Avgreview { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public DateTime DateTime { get; set; }

    public string? Comment { get; set; }

    public int? Cleanliness { get; set; }

    public int? Facilities { get; set; }

    public int? ValueForMoney { get; set; }

    public int? Service { get; set; }

    public int? Location { get; set; }

    public int? LikeCount { get; set; }

    public int? DislikeCount { get; set; }
}
