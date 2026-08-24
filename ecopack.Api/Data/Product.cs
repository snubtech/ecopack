using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal CarbonEmission { get; set; }
}
