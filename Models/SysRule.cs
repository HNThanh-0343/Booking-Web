using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysRule
{
    public int Id { get; set; }

    public int? IdRole { get; set; }

    public int? IdModule { get; set; }

    public bool? IsView { get; set; }

    public bool? IsCreate { get; set; }

    public bool? IsEdit { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsPermission { get; set; }

    public virtual SysModule? IdModuleNavigation { get; set; }

    public virtual SysRole? IdRoleNavigation { get; set; }
}
