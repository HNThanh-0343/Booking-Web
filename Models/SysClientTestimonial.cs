using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysClientTestimonial
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Avatar { get; set; }

    public string? Role { get; set; }

    public bool? Status { get; set; }
}
