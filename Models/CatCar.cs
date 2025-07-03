using System;
using System.Collections.Generic;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class CatCar
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<SysCar> SysCars { get; set; } = new List<SysCar>();
}
