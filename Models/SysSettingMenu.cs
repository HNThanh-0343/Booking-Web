using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysSettingMenu
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdModule { get; set; }

    public bool? IsVisible { get; set; }
}
