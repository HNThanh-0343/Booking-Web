using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysBlog
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdTypeBlog { get; set; }

    public string? Name { get; set; }

    public string? ContentsShort { get; set; }

    public string? Contents { get; set; }

    public DateTime DateCreate { get; set; }

    public DateTime? DateEdit { get; set; }

    public string? ListImg { get; set; }

    public string? Tag { get; set; }

    public bool Status { get; set; }

    public virtual CatTypeBlog? IdTypeBlogNavigation { get; set; }

    public virtual SysUser? IdUserNavigation { get; set; }
}
