using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class SysOtp
{
    public int Id { get; set; }

    public string? Email { get; set; }

    public string? Otpcode { get; set; }

    public DateTime? CrTime { get; set; }

    public DateTime? ExTime { get; set; }
}
