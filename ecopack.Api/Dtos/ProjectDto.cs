using System;

namespace ecopack.Api.Dtos
{
    /// <summary>
    /// 대시보드 목록 조회용 DTO (전체 필드 포함)
    /// </summary>
    public class ProjectListDto
    {
        public string PrjId { get; set; } = null!;
        public string? PrjNm { get; set; }
        public string? RepCustId { get; set; }
        public string? BizNo { get; set; }
        public string? BizNm { get; set; }
        public string? RepNm { get; set; }
        public string? RoleNm { get; set; }
        public string? IndstNm { get; set; }
        public string? CntryNm { get; set; }
        public string? AddrCd { get; set; }
        public string? DtlAddr1 { get; set; }
        public string? DtlAddr2 { get; set; }
        public string? EmlAddr { get; set; }
        public string? RepTelNo { get; set; }
        public string? MblTelNo { get; set; }

        // 수출 국가 여부 플래그 (1~8)
        public string? PrdExpCntryNm1 { get; set; }
        public string? PrdExpCntryNm2 { get; set; }
        public string? PrdExpCntryNm3 { get; set; }
        public string? PrdExpCntryNm4 { get; set; }
        public string? PrdExpCntryNm5 { get; set; }
        public string? PrdExpCntryNm6 { get; set; }
        public string? PrdExpCntryNm7 { get; set; }
        public string? PrdExpCntryNm8 { get; set; }

        // 포장 차수 여부 플래그 (1~3)
        public string? PrdPkgSeq1 { get; set; }
        public string? PrdPkgSeq2 { get; set; }
        public string? PrdPkgSeq3 { get; set; }

        public string? PrjRevNo { get; set; }
		public string? Prjuserid { get; set; }
        public string? Prjmemo { get; set; }
        public string? PackLevel { get; set; }
        public DateOnly? PrjFcrtDt { get; set; }
    }

    /// <summary>
    /// 신규 프로젝트 등록용 DTO
    /// </summary>
    public class ProjectCreateDto
    {
        
        public string? PrjId { get; set; }
        public string? PrjNm { get; set; }
        public string? RepCustId { get; set; }
        public string? BizNo { get; set; }
        public string? BizNm { get; set; }
        public string? RepNm { get; set; }
        public string? RoleNm { get; set; }
        public string? IndstNm { get; set; }
        public string? CntryNm { get; set; }
        public string? AddrCd { get; set; }
        public string? DtlAddr1 { get; set; }
        public string? DtlAddr2 { get; set; }
        public string? EmlAddr { get; set; }
        public string? RepTelNo { get; set; }
        public string? MblTelNo { get; set; }

        // 수출 국가 여부 플래그 (1~8)
        public string? PrdExpCntryNm1 { get; set; }
        public string? PrdExpCntryNm2 { get; set; }
        public string? PrdExpCntryNm3 { get; set; }
        public string? PrdExpCntryNm4 { get; set; }
        public string? PrdExpCntryNm5 { get; set; }
        public string? PrdExpCntryNm6 { get; set; }
        public string? PrdExpCntryNm7 { get; set; }
        public string? PrdExpCntryNm8 { get; set; }

        // 포장 차수 여부 플래그 (1~3)
        public string? PrdPkgSeq1 { get; set; }
        public string? PrdPkgSeq2 { get; set; }
        public string? PrdPkgSeq3 { get; set; }

		public string? PrjRevNo { get; set; }
		public string? Prjuserid { get; set; }
        public string? Prjmemo { get; set; }
        public string? PackLevel { get; set; }
    }
}