using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysGuest
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

    public string? CardName { get; set; }

    public string? CardNumber { get; set; }

    public string? Ccv { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTill { get; set; }

    public bool? Status { get; set; }

    public DateTime? Time { get; set; }
}
