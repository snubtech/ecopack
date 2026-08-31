namespace ecopack.Api.Dtos
{
    public class MaterialPropertyDto
    {
        public string? AppliedMaterial { get; set; }
        public string? AppliedMaterialNm { get; set; }
        public string? MatUse { get; set; }
        public string? MatUseNm { get; set; }
        public string? MatType { get; set; }
        public string? MatTypeNm { get; set; }
        public string? MatForm { get; set; }
        public string? MatFormNm { get; set; }
        public string? PackLevel { get; set; }
        public string? PackLevelNm { get; set; }
    }
    public class PackLevelDto
    {
        public string PackLevel { get; set; }
        public string PackLevelNm { get; set; }
    }
    public class MattypePropertyDto
    {
        public string MatType { get; set; }
        public string MatTypeNm { get; set; }
    }
}