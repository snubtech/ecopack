namespace ecopack.Api.Dtos
{
    public class ProjecttemplateListDto
    {
        public long Idx { get; set; }
        public string PackDsgnTplId { get; set; } = null!;
        public string? PackLevelNm { get; set; }
        public string? MatTypeNm { get; set; }
        public string? Subject { get; set; }
        public string? DsgnTypeNm { get; set; }
        public string? DsgnTypeCdVal { get; set; }
        public string? DsgnExpCon { get; set; }
        public string? AppliedMaterialNm { get; set; }
        public string? DsgnFeatDscr { get; set; }
        public string? OperDscr { get; set; }
        public string? MemoImg { get; set; }
        public string? PackLevel { get; set; }
        public string? MatType { get; set; }
        public string? AppliedMaterial { get; set; }
        public string? FileNm { get; set; }
        //public byte[]? FileData { get; set; }
        public string? FileData { get; set; }
        //public DateTime? CreatedAt { get; set; }
    }
}