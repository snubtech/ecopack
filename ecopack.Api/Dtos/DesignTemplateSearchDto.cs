namespace ecopack.Api.Dtos
{
    /// <summary>디자인 템플릿 화면의 조회조건 셀렉트 목록</summary>
    public class DesignTemplateFiltersDto
    {
        public List<CodeNameDto> PackLevels { get; set; } = new();        // 포장차수
        public List<CodeNameDto> AppliedMaterials { get; set; } = new();  // 적용소재
        public List<CodeNameDto> MatTypes { get; set; } = new();          // 포장재 구분
    }

    /// <summary>디자인 템플릿 목록 한 행</summary>
    public class DesignTemplateRowDto
    {
        /// <summary>내부 관리용 일련번호 (화면 미표시, 행 key 용)</summary>
        public long Idx { get; set; }

        /// <summary>패키징디자인템플릿 ID (상세 조회 키)</summary>
        public string? PackDsgnTplId { get; set; }

        public string? PackLevelNm { get; set; }        // 포장차수
        public string? AppliedMaterialNm { get; set; }  // 적용소재
        public string? MatTypeNm { get; set; }          // 포장재 구분
        public string? Subject { get; set; }            // 탬플릿명
        public string? DsgnTypeNm { get; set; }         // 디자인유형명
        public string? DsgnTypeCdVal { get; set; }      // 디자인유형코드
        public DateTime? CreatedAt { get; set; }        // 수집일시
    }

    /// <summary>디자인 템플릿 목록 조회 결과 (페이징)</summary>
    public class DesignTemplatePageDto
    {
        public List<DesignTemplateRowDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 디자인 템플릿 상세 — 참조 화면의 행 펼침 영역과 같은 항목을 담는다.
    /// memoImg 는 이미지가 포함된 HTML 이므로 화면에 그대로 주입하지 않고
    /// 서버에서 data URI 만 뽑아 전달한다.
    /// </summary>
    public class DesignTemplateDetailDto
    {
        public string? PackDsgnTplId { get; set; }
        public string? Subject { get; set; }

        public string? DsgnExpCon { get; set; }     // 디자인설명내용
        public string? DsgnFeatDscr { get; set; }   // 디자인 특징
        public string? OperDscr { get; set; }       // 제품의 설명

        /// <summary>이미지 파일명 (확장자 포함)</summary>
        public string? FileNm { get; set; }

        /// <summary>이미지 파일 data URI</summary>
        public string? FileImageUri { get; set; }

        /// <summary>memoImg 안에서 추출한 이미지 data URI 목록</summary>
        public List<string> MemoImageUris { get; set; } = new();
    }
}
