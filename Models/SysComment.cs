using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysComment
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdReivew { get; set; }

    public int? Like { get; set; }

    public int? Dislike { get; set; }
}
