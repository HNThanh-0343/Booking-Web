using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysRole
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? Status { get; set; }

    public int? IdUserPrent { get; set; }

    public virtual ICollection<SysRule> SysRules { get; set; } = new List<SysRule>();

    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
