using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // 👈 이 부분이 반드시 포함되어야 합니다.

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

    /// <summary>
    /// 프로젝트 상세 정보 신규/수정 저장용 DTO
    /// </summary>
    public class ProjectDetailSaveDto
    {
        [Required(ErrorMessage = "프로젝트 ID는 필수입니다.")]
        public string PrjId { get; set; } = null!;

        [Required(ErrorMessage = "포장차수는 필수입니다.")]
        public string PackLevel { get; set; } = null!;

        public string? PrjRevNo { get; set; }
        public string? PackLevelNm { get; set; }
        public string? AppliedMaterial { get; set; }
        public string? AppliedMaterialNm { get; set; }
        public string? MatUse { get; set; }
        public string? MatUseNm { get; set; }
        public string? MatType { get; set; }
        public string? MatTypeNm { get; set; }
        public string? MatForm { get; set; }
        public string? MatFormNm { get; set; }
        public string? PackDsgnTplId { get; set; }
        public string? Projstatus { get; set; }
        public string? PrdExpCntry { get; set; }
        public string? PrdExpCntryNm { get; set; }
        public string? Prjuserid { get; set; }
    }

    /// <summary>
    /// 1. 물성 정보 응답 DTO (getmaterial)
    /// </summary>
    public class GetMaterialListDto
    {
        public string? PackLevel { get; set; }
        public string? PackLevelNm { get; set; }         // 포장차수명
        public string? AppliedMaterial { get; set; }
        public string? AppliedMaterialNm { get; set; }   // 적용소재명
        public string? MatType { get; set; }
        public string? MatTypeNm { get; set; }           // 사용환경명
        public string? Item { get; set; }
        public string? ItemName { get; set; }            // 성능항목명 (itemNm)
        public string? UnitNm { get; set; }              // 단위명
        public string? Unit { get; set; }                // 단위
        public decimal? AcceptableRange { get; set; }    // 기준값 범위 최솟값 (min(acceptableRange))
    }

    /// <summary>
    /// 2. 환경규제 정보 응답 DTO (getenvironment)
    /// </summary>
    public class GetEnvironmentListDto
    {
        public string? RelatedReg { get; set; }   // 관련규정 (relatedReg)
        public string? RegItem { get; set; }      // 규제항목 (regItem)
        public string? DtlCont { get; set; }      // 규제내용 (dtlCont)
    }

    /// <summary>
    /// 3. 공정도 정보 응답 DTO (getprocessflow)
    /// </summary>
    public class GetProcessFlowListDto
    {
        public string? MatComp { get; set; }     // 구성요소 코드
        public string? MatCompNm { get; set; }   // 구성요소명
        public string? MemoImg { get; set; }     // 메모 이미지
        public string? FileData { get; set; }    // 파일 데이터
    }

    /// <summary>
    /// 4. 탄소배출량 정보 응답 DTO (getcarconinfo)
    /// </summary>
    public class GetCarconInfoListDto
    {
        public string? PackLevel { get; set; }
        public string? AppliedMaterial { get; set; }
        public string? Matform { get; set; }
        public decimal? MassCo2Mat { get; set; }     // 중량당 탄소배출량-원료
        public decimal? MassCo2Proc { get; set; }    // 중량당 탄소배출량-제조
        public decimal? MassCo2Scrap { get; set; }   // 중량당 탄소배출량-폐기
        public decimal? MassCo2Sum { get; set; }     // 중량당 탄소배출량-합계
        public decimal? UnitCo2Mat { get; set; }     // 단위당 탄소배출량-원료
        public decimal? UnitCo2Proc { get; set; }    // 단위당 탄소배출량-제조
        public decimal? UnitCo2Scrap { get; set; }   // 단위당 탄소배출량-폐기
        public decimal? UnitCo2Sum { get; set; }     // 단위당 탄소배출량-합계
    }


    /// <summary>
    /// 프로젝트 상세 리포트 신규/수정 저장용 DTO
    /// </summary>
    public class ProjectDetailReportSaveDto
    {
        [Required(ErrorMessage = "프로젝트 ID는 필수입니다.")]
        public string PrjId { get; set; } = null!;

        [Required(ErrorMessage = "포장차수는 필수입니다.")]
        public string PackLevel { get; set; } = null!;

        public string? Prjuserid { get; set; }
        public string? PackLevelNm { get; set; }
        public string? AppliedMaterial { get; set; }
        public string? AppliedMaterialNm { get; set; }
        public string? PrdExpCntry { get; set; }
        public string? PrdExpCntryNm { get; set; }
        public string? MatType { get; set; }
        public string? MatTypeNm { get; set; }
        public string? MatForm { get; set; }
        public string? MatFormNm { get; set; }

        // [참고] 기존 단일 필드들은 호환성을 위해 남겨둘 수 있으나, 
        // 여러 행을 처리할 때는 아래의 Materials와 Environments 리스트가 핵심으로 사용됩니다.
        public string? Item { get; set; }
        public string? ItemNm { get; set; }
        public string? Unit { get; set; }
        public string? UnitNm { get; set; }
        public string? AcceptableRange { get; set; }
        public string? RelatedReg { get; set; }
        public string? RegItem { get; set; }
        public string? DtlCont { get; set; }

        public string? MatComp { get; set; }
        public string? MatCompNm { get; set; }
        public string? MemoImg { get; set; }
        public string? FileData { get; set; }
        public string? MassCo2Mat { get; set; }
        public string? MassCo2Proc { get; set; }
        public string? MassCo2Scrap { get; set; }
        public string? MassCo2Sum { get; set; }
        public string? UnitCo2Mat { get; set; }
        public string? UnitCo2Proc { get; set; }
        public string? UnitCo2Scrap { get; set; }
        public string? UnitCo2Sum { get; set; }

        // === [추가] 여러 줄의 데이터를 받기 위한 리스트 속성 ===
        /// <summary>
        /// 1. 물성 정보 목록 (여러 줄)
        /// </summary>
        public List<ProjectMaterialItemDto>? Materials { get; set; }

        /// <summary>
        /// 2. 환경 규제 정보 목록 (여러 줄)
        /// </summary>
        public List<ProjectEnvironmentItemDto>? Environments { get; set; }
    }

    /// <summary>
    /// 물성 행 데이터 DTO
    /// </summary>
    public class ProjectMaterialItemDto
    {
        public string? Item { get; set; }
        public string? ItemName { get; set; }
        public string? Unit { get; set; }
        public string? UnitNm { get; set; }
        public string? AcceptableRange { get; set; }
    }

    /// <summary>
    /// 환경규제 행 데이터 DTO
    /// </summary>
    public class ProjectEnvironmentItemDto
    {
        public string? RelatedReg { get; set; }
        public string? RegItem { get; set; }
        public string? DtlCont { get; set; }
    }

}