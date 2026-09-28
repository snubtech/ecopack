namespace ecopack.Api.Support
{
    /// <summary>
    /// 기준정보 연계 API(에코 패키징 디자인 데이터베이스 시스템) 설정.
    /// appsettings.json 의 "EdbApi" 구역에서 읽는다.
    /// </summary>
    public class EdbApiOptions
    {
        public const string SectionName = "EdbApi";

        // ─────────────────────────────────────────────────────────
        // 접속 정보
        // ─────────────────────────────────────────────────────────

        /// <summary>기준정보 시스템 주소. 비어 있으면 배치를 돌리지 않는다.</summary>
        public string BaseUrl { get; set; } = "";

        /// <summary>IF005 환경영향평가정보 목록 조회 경로</summary>
        public string If005Path { get; set; } = "/api/rest/in/getRestEdbEnvImpactAssessmentList";

        /// <summary>
        /// 계정별로 발급되는 API 키.
        /// ⚠️ 여기에 직접 적으면 Git 에 그대로 올라간다.
        ///    비워 두면 환경변수 ECOPACK_EDB_API_KEY 를 읽는다. 환경변수 사용을 권한다.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>조회 시작일(yyyyMMdd). 전체를 받아 통째로 교체하므로 충분히 이른 날로 둔다.</summary>
        public string StartCredat { get; set; } = "20000101";

        /// <summary>조회 종료일(yyyyMMdd). 명세서 예시값을 그대로 쓴다.</summary>
        public string EndCredat { get; set; } = "99991231";

        /// <summary>
        /// 허용할 서버 인증서 지문(SHA-1, 콜론 없이 대문자).
        /// 기준정보 서버가 자체 서명 인증서를 써서 기본 검증을 통과하지 못한다.
        /// 검증을 통째로 끄지 않고, 이 지문과 같은 인증서일 때만 예외로 받아 준다.
        /// 비워 두면 기본 검증만 한다.
        /// </summary>
        public string? CertThumbprint { get; set; }

        /// <summary>서버 호출 제한 시간(초)</summary>
        public int TimeoutSeconds { get; set; } = 60;

        // ─────────────────────────────────────────────────────────
        // 배치
        // ─────────────────────────────────────────────────────────

        /// <summary>IF005 자동 수집 주기(시간). 0 이하면 자동 실행하지 않는다.</summary>
        public int If005SyncIntervalHours { get; set; } = 24;
    }
}
