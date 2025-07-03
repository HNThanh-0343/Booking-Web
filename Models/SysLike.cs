using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysLike
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public bool Like { get; set; }

    public int Idcategory { get; set; }

    public int LikeItemId { get; set; }
}
