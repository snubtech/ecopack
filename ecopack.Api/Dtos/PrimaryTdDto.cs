/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - PrimaryTdDto (기술문서 주고받기용 자료 묶음)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 기술문서 화면과 서버가 주고받는 항목을 담습니다. primary_td 전체 컬럼과 1:1로 맞췄습니다.
 * 
 * 2. 타입을 모두 string? 으로 둔 이유
 *    - 화면의 [insert] 항목은 전부 글자 입력이라, DB가 숫자(decimal/int)인 컬럼도
 *      화면에서는 '2.4 kg' 처럼 단위가 섞여 들어올 수 있습니다.
 *    - 그래서 여기서는 글자로 받고, 컨트롤러가 DB 타입에 맞게 변환합니다. 빈 문자열은 null 로 저장됩니다.
 * 
 * 3. 행이 늘어나는 표
 *    - 재질구성·BOM·시험·품질관리·첨부문서처럼 행을 더할 수 있는 표는
 *      DB 컬럼 슬롯 개수만큼 항목을 미리 열어 두었습니다. (예: BOM 은 1~8)
 * ==============================================================================
 */
namespace ecopack.Api.Dtos
{
    /// <summary>
    /// 1차포장 기술문서(primary_td / 모듈 A) 화면 CRUD DTO.
    /// 화면의 모든 [insert] 항목은 텍스트 입력으로 다루기 때문에
    /// DB가 decimal/int 인 컬럼도 여기서는 string? 으로 받고 컨트롤러에서 파싱한다.
    /// (빈 문자열 → null 로 저장)
    ///
    /// 행 추가/삭제가 가능한 표는 DB 슬롯 최대치까지 프로퍼티를 열어 둔다.
    /// </summary>
    public class PrimaryTdDto
    {
        // ── 키 ────────────────────────────────────────────────
        /// <summary>1차포장기술문서 ID. 신규 저장 시 서버에서 TD-1-{yyyyMMddHHmmssfff} 로 채번</summary>
        public string? Pkg1TechDocId { get; set; }

        /// <summary>작성일. 저장 시 서버가 현재 타임스탬프로 갱신한다.</summary>
        public DateTime? LastWrtDtm { get; set; }

        // ── 1. 제품 식별 정보 ─────────────────────────────────
        public string? PrjId { get; set; }
        public string? PrjfNm { get; set; }
        public string? BizNm { get; set; }
        public string? CntryNm { get; set; }
        public string? DocNo { get; set; }
        public string? RevNo { get; set; }

        // ── 2. 제품 설명 ──────────────────────────────────────
        public string? PrdExplPhrsCntn { get; set; }
        public string? PrdIdfyCntn { get; set; }
        public string? MainMatVal { get; set; }
        public string? DsgnFeatCntn { get; set; }

        // ── 3. 제품 사양 및 제조 도면 (항목별 고정 컬럼) ──────
        public string? PrdExtDimSpecVal { get; set; }
        public string? PrdIntDimSpecVal { get; set; }
        public string? PrdWtSpecVal { get; set; }
        public string? PrdMatSpecVal { get; set; }
        public string? PrdClrSpecVal { get; set; }
        public string? PrdUseTempSpecVal { get; set; }
        public string? PrdLoadWtSpecVal { get; set; }
        public string? PrdDsgnLifeSpecVal { get; set; }
        public string? MfrDrwUrl { get; set; }

        // ── 4-1. 재질구성 합계행 ──────────────────────────────
        public string? MatCompTot { get; set; }
        public string? MatNmTot { get; set; }
        public string? WtValTot { get; set; }
        public string? WtRtTot { get; set; }

        // ── 5. 재사용성 평가 — 문단/성능 ──────────────────────
        public string? PrdDsgnPhrsCntn { get; set; }
        public string? ReusePerfRsltVal { get; set; }

        // ── 6. 재활용성 평가 — 문단 ───────────────────────────
        public string? RcycDsgnPrncPhrsCntn { get; set; }
        public string? RcycPathPhrsCntn { get; set; }

        // ── 7. 중금속 — 문단 / 합계행 ─────────────────────────
        public string? SocHvyMetMngFixPhrsCntn { get; set; }
        public string? SocHvyMetMngRsltTot { get; set; }
        public string? SocHvyMetMngTestRsltTot { get; set; }
        public string? SocHvyMetMngRegCritTot { get; set; }
        public string? SocHvyMetMngTestMthdCntn6 { get; set; }
        public string? SocHvyMetMngTestRsltPhrs { get; set; }

        // ── 8. 제조 공정 ──────────────────────────────────────
        public string? MfrPrcsUrl { get; set; }
        public string? MfrPrcsCntn { get; set; }

        // ── 10. 준수 선언 ─────────────────────────────────────
        public string? CmplDclCntn { get; set; }

        // ── 12. 책임자 정보 ───────────────────────────────────
        public string? BizNm2 { get; set; }
        public string? RepNm { get; set; }
        public string? RoleNm { get; set; }
        public string? EmlAddr { get; set; }
        public string? MbTelNo { get; set; }
        public string? TechDocLastPhrsCntn { get; set; }

        // ── 4-1. 재질구성 (최대 4행) ──────────────────────────
        public string? MatComplItem1 { get; set; }
        public string? MatComplItem2 { get; set; }
        public string? MatComplItem3 { get; set; }
        public string? MatComplItem4 { get; set; }
        public string? MatNm1 { get; set; }
        public string? MatNm2 { get; set; }
        public string? MatNm3 { get; set; }
        public string? MatNm4 { get; set; }
        public string? WtVal1 { get; set; }
        public string? WtVal2 { get; set; }
        public string? WtVal3 { get; set; }
        public string? WtVal4 { get; set; }
        public string? WtRt1 { get; set; }
        public string? WtRt2 { get; set; }
        public string? WtRt3 { get; set; }
        public string? WtRt4 { get; set; }

        // ── 4-2. BOM (최대 8행) ───────────────────────────────
        public string? BomCmpnNm1 { get; set; }
        public string? BomCmpnNm2 { get; set; }
        public string? BomCmpnNm3 { get; set; }
        public string? BomCmpnNm4 { get; set; }
        public string? BomCmpnNm5 { get; set; }
        public string? BomCmpnNm6 { get; set; }
        public string? BomCmpnNm7 { get; set; }
        public string? BomCmpnNm8 { get; set; }
        public string? BomMatNm1 { get; set; }
        public string? BomMatNm2 { get; set; }
        public string? BomMatNm3 { get; set; }
        public string? BomMatNm4 { get; set; }
        public string? BomMatNm5 { get; set; }
        public string? BomMatNm6 { get; set; }
        public string? BomMatNm7 { get; set; }
        public string? BomMatNm8 { get; set; }
        public string? BomMatStdCd1 { get; set; }
        public string? BomMatStdCd2 { get; set; }
        public string? BomMatStdCd3 { get; set; }
        public string? BomMatStdCd4 { get; set; }
        public string? BomMatStdCd5 { get; set; }
        public string? BomMatStdCd6 { get; set; }
        public string? BomMatStdCd7 { get; set; }
        public string? BomMatStdCd8 { get; set; }
        public string? BomQtyVal1 { get; set; }
        public string? BomQtyVal2 { get; set; }
        public string? BomQtyVal3 { get; set; }
        public string? BomQtyVal4 { get; set; }
        public string? BomQtyVal5 { get; set; }
        public string? BomQtyVal6 { get; set; }
        public string? BomQtyVal7 { get; set; }
        public string? BomQtyVal8 { get; set; }
        public string? BomUnitWtVal1 { get; set; }
        public string? BomUnitWtVal2 { get; set; }
        public string? BomUnitWtVal3 { get; set; }
        public string? BomUnitWtVal4 { get; set; }
        public string? BomUnitWtVal5 { get; set; }
        public string? BomUnitWtVal6 { get; set; }
        public string? BomUnitWtVal7 { get; set; }
        public string? BomUnitWtVal8 { get; set; }
        public string? BomTotWtVal1 { get; set; }
        public string? BomTotWtVal2 { get; set; }
        public string? BomTotWtVal3 { get; set; }
        public string? BomTotWtVal4 { get; set; }
        public string? BomTotWtVal5 { get; set; }
        public string? BomTotWtVal6 { get; set; }
        public string? BomTotWtVal7 { get; set; }
        public string? BomTotWtVal8 { get; set; }
        public string? BomWtRtVal1 { get; set; }
        public string? BomWtRtVal2 { get; set; }
        public string? BomWtRtVal3 { get; set; }
        public string? BomWtRtVal4 { get; set; }
        public string? BomWtRtVal5 { get; set; }
        public string? BomWtRtVal6 { get; set; }
        public string? BomWtRtVal7 { get; set; }
        public string? BomWtRtVal8 { get; set; }

        // ── 5. 제품 설계 시험 (최대 6행) ──────────────────────
        public string? PrdDsgnTestItemCntn1 { get; set; }
        public string? PrdDsgnTestItemCntn2 { get; set; }
        public string? PrdDsgnTestItemCntn3 { get; set; }
        public string? PrdDsgnTestItemCntn4 { get; set; }
        public string? PrdDsgnTestItemCntn5 { get; set; }
        public string? PrdDsgnTestItemCntn6 { get; set; }
        public string? PrdDsgnCritCntn1 { get; set; }
        public string? PrdDsgnCritCntn2 { get; set; }
        public string? PrdDsgnCritCntn3 { get; set; }
        public string? PrdDsgnCritCntn4 { get; set; }
        public string? PrdDsgnCritCntn5 { get; set; }
        public string? PrdDsgnCritCntn6 { get; set; }
        public string? PrdDsgnRsltVal1 { get; set; }
        public string? PrdDsgnRsltVal2 { get; set; }
        public string? PrdDsgnRsltVal3 { get; set; }
        public string? PrdDsgnRsltVal4 { get; set; }
        public string? PrdDsgnRsltVal5 { get; set; }
        public string? PrdDsgnRsltVal6 { get; set; }
        public string? PrdDsgnTestMthd1 { get; set; }
        public string? PrdDsgnTestMthd2 { get; set; }
        public string? PrdDsgnTestMthd3 { get; set; }
        public string? PrdDsgnTestMthd4 { get; set; }
        public string? PrdDsgnTestMthd5 { get; set; }
        public string? PrdDsgnTestMthd6 { get; set; }

        // ── 6. 재활용 설계 원칙 (최대 6행) ────────────────────
        public string? RcycDsgnPrncItemCntn1 { get; set; }
        public string? RcycDsgnPrncItemCntn2 { get; set; }
        public string? RcycDsgnPrncItemCntn3 { get; set; }
        public string? RcycDsgnPrncItemCntn4 { get; set; }
        public string? RcycDsgnPrncItemCntn5 { get; set; }
        public string? RcycDsgnPrncItemCntn6 { get; set; }
        public string? RcycDsgnPrncEvlRslt1 { get; set; }
        public string? RcycDsgnPrncEvlRslt2 { get; set; }
        public string? RcycDsgnPrncEvlRslt3 { get; set; }
        public string? RcycDsgnPrncEvlRslt4 { get; set; }
        public string? RcycDsgnPrncEvlRslt5 { get; set; }
        public string? RcycDsgnPrncEvlRslt6 { get; set; }
        public string? RcycDsgnPrncEvlTestMthdCntn1 { get; set; }
        public string? RcycDsgnPrncEvlTestMthdCntn2 { get; set; }
        public string? RcycDsgnPrncEvlTestMthdCntn3 { get; set; }
        public string? RcycDsgnPrncEvlTestMthdCntn4 { get; set; }
        public string? RcycDsgnPrncEvlTestMthdCntn5 { get; set; }
        public string? RcycDsgnPrncEvlTestMthdCntn6 { get; set; }

        // ── 7. 중금속 시험 결과 (최대 5행) ────────────────────
        public string? SocHvyMetMngTestRsltSbstCntn1 { get; set; }
        public string? SocHvyMetMngTestRsltSbstCntn2 { get; set; }
        public string? SocHvyMetMngTestRsltSbstCntn3 { get; set; }
        public string? SocHvyMetMngTestRsltSbstCntn4 { get; set; }
        public string? SocHvyMetMngTestRsltSbstCntn5 { get; set; }
        public string? SocHvyMetMngTestRsltCntn1 { get; set; }
        public string? SocHvyMetMngTestRsltCntn2 { get; set; }
        public string? SocHvyMetMngTestRsltCntn3 { get; set; }
        public string? SocHvyMetMngTestRsltCntn4 { get; set; }
        public string? SocHvyMetMngTestRsltCntn5 { get; set; }
        public string? SocHvyMetMngRegCritCntn1 { get; set; }
        public string? SocHvyMetMngRegCritCntn2 { get; set; }
        public string? SocHvyMetMngRegCritCntn3 { get; set; }
        public string? SocHvyMetMngRegCritCntn4 { get; set; }
        public string? SocHvyMetMngRegCritCntn5 { get; set; }
        public string? SocHvyMetMngTestMthdCntn1 { get; set; }
        public string? SocHvyMetMngTestMthdCntn2 { get; set; }
        public string? SocHvyMetMngTestMthdCntn3 { get; set; }
        public string? SocHvyMetMngTestMthdCntn4 { get; set; }
        public string? SocHvyMetMngTestMthdCntn5 { get; set; }

        // ── 9. 품질관리 (최대 7행) ────────────────────────────
        public string? QltMngInspItemCntn1 { get; set; }
        public string? QltMngInspItemCntn2 { get; set; }
        public string? QltMngInspItemCntn3 { get; set; }
        public string? QltMngInspItemCntn4 { get; set; }
        public string? QltMngInspItemCntn5 { get; set; }
        public string? QltMngInspItemCntn6 { get; set; }
        public string? QltMngInspItemCntn7 { get; set; }
        public string? QltMngInspMthdCntn1 { get; set; }
        public string? QltMngInspMthdCntn2 { get; set; }
        public string? QltMngInspMthdCntn3 { get; set; }
        public string? QltMngInspMthdCntn4 { get; set; }
        public string? QltMngInspMthdCntn5 { get; set; }
        public string? QltMngInspMthdCntn6 { get; set; }
        public string? QltMngInspMthdCntn7 { get; set; }
        public string? QltMngFreqCntn1 { get; set; }
        public string? QltMngFreqCntn2 { get; set; }
        public string? QltMngFreqCntn3 { get; set; }
        public string? QltMngFreqCntn4 { get; set; }
        public string? QltMngFreqCntn5 { get; set; }
        public string? QltMngFreqCntn6 { get; set; }
        public string? QltMngFreqCntn7 { get; set; }

        // ── 11. 첨부 문서 Annex A~H (최대 8행) ────────────────
        public string? AtchDocNm1 { get; set; }
        public string? AtchDocNm2 { get; set; }
        public string? AtchDocNm3 { get; set; }
        public string? AtchDocNm4 { get; set; }
        public string? AtchDocNm5 { get; set; }
        public string? AtchDocNm6 { get; set; }
        public string? AtchDocNm7 { get; set; }
        public string? AtchDocNm8 { get; set; }
        public string? AtchDocUrl1 { get; set; }
        public string? AtchDocUrl2 { get; set; }
        public string? AtchDocUrl3 { get; set; }
        public string? AtchDocUrl4 { get; set; }
        public string? AtchDocUrl5 { get; set; }
        public string? AtchDocUrl6 { get; set; }
        public string? AtchDocUrl7 { get; set; }
        public string? AtchDocUrl8 { get; set; }
    }

    /// <summary>첨부문서 업로드 결과</summary>
    public class AtchDocUploadResultDto
    {
        public bool Success { get; set; }
        public int Slot { get; set; }
        public string? FileNm { get; set; }
        public string? FileUrl { get; set; }
        public string? Message { get; set; }
    }
}
