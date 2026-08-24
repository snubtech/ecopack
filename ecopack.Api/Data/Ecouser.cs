using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

public partial class Ecouser
{
    public string Userno { get; set; } = null!;

    public string Usernm { get; set; } = null!;

    public string Pass { get; set; } = null!;

    public string? Companynm { get; set; }

    public string? Businessno { get; set; }

    public string? Nation { get; set; }

    public string? Rule { get; set; }

    public string? Email { get; set; }

    public string? Mobile { get; set; }
}
