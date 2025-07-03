using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysModule
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? NameController { get; set; }

    public string? Icon { get; set; }

    public string? PrentId { get; set; }

    public int? Order { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysRule> SysRules { get; set; } = new List<SysRule>();
}
