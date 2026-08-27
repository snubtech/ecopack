using System;

namespace ecopack.Api.Data
{
    public class PrimaryPkg
    {
        public string SubPrjId { get; set; } = null!;
        public string? PrjId { get; set; }
        public string? ApplMatNm { get; set; }
        public string? UseEnvCntn { get; set; }
        public string? PkgMatTypeNm { get; set; }
    }
}