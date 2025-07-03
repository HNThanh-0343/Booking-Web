using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatBank
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? KeyBank { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
