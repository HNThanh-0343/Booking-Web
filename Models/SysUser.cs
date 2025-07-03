using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysUser
{
    public int Id { get; set; }

    public int? IdRole { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Local { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public int? CardName { get; set; }

    public string? CardNumber { get; set; }

    public string? Ccv { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTill { get; set; }

    public bool? Status { get; set; }

    public DateTime? Time { get; set; }

    public int? PartnerId { get; set; }

    public virtual CatBank? CardNameNavigation { get; set; }

    public virtual SysRole? IdRoleNavigation { get; set; }

    public virtual ICollection<SysBlog> SysBlogs { get; set; } = new List<SysBlog>();

    public virtual ICollection<SysBooking> SysBookings { get; set; } = new List<SysBooking>();
}
