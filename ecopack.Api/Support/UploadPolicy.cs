/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - UploadPolicy (첨부문서 업로드 공통 규칙)
 * ==============================================================================
 *
 * 1. 왜 필요한가
 *    - TD(기술문서) 6개 컨트롤러가 전부 같은 규칙(허용 확장자, 크기 제한,
 *      저장 위치)을 따로따로 들고 있으면 하나를 고칠 때 6곳을 다 고쳐야 한다.
 *      이 클래스 하나로 모아 둔다.
 *
 * 2. 허용 확장자 (화이트리스트)
 *    - 마이크로소프트 문서(doc/ppt/excel), 한글(hwp), pdf만 허용한다.
 *    - 실행 파일이나 html/svg 같은 스크립트 실행 가능한 파일은 받지 않는다
 *      (같은 도메인에서 파일을 열람하게 되면 저장형 XSS로 이어질 수 있다).
 *
 * 3. 저장 위치 (wwwroot 밖)
 *    - 예전엔 wwwroot/uploads 아래에 저장하고 UseStaticFiles로 그대로
 *      공개했다 — 로그인/소유자 확인 없이 URL만 알면 누구나 내려받을 수 있었다.
 *    - 지금은 wwwroot 밖(App_Data)에 저장하고, 각 컨트롤러의 Download 액션이
 *      소유자 확인(ProjectAccess.IsOwnerAsync)을 통과해야만 파일을 내려준다.
 *
 * 4. 디렉터리 구조
 *    App_Data/uploads/
 *      projects/td/{포장차수}/{프로젝트번호}/       ← 기술문서 첨부
 *      projects/doc/{포장차수}/{프로젝트번호}/      ← 적합성선언서 근거문서
 *      customers/{repCustId}/                       ← (자리만 예약) 향후 회원 개인서류
 *                                                       (사업자등록증 등)를 넣을 자리.
 *                                                       아직 실제로 쓰는 화면·API는 없다.
 * ==============================================================================
 */
namespace ecopack.Api.Support
{
    public static class UploadPolicy
    {
        /// <summary>첨부문서 1건당 최대 크기. 화면 안내 문구와 반드시 같은 값을 써야 한다.</summary>
        public const long MaxFileBytes = 20 * 1024 * 1024; // 20MB

        /// <summary>화면에 보여줄 크기 제한 문구 (예: "20MB").</summary>
        public const string MaxFileSizeDisplay = "20MB";

        /// <summary>
        /// 허용 확장자 화이트리스트. 마이크로소프트 문서(doc/ppt/excel) + 한글(hwp) + pdf.
        /// 새 형식을 추가하고 싶으면 여기 한 곳만 고치면 6개 컨트롤러에 전부 반영된다.
        /// </summary>
        public static readonly IReadOnlySet<string> AllowedExtensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ".doc", ".docx",   // 워드
            ".ppt", ".pptx",   // 파워포인트
            ".xls", ".xlsx",   // 엑셀
            ".hwp", ".hwpx",   // 한글(한글과컴퓨터)
            ".pdf",
        };

        /// <summary>화면 안내·오류 문구에 쓸, 사람이 읽기 좋은 확장자 목록 (예: "doc, docx, hwp, ...").</summary>
        public static string AllowedExtensionsDisplay =>
            string.Join(", ", AllowedExtensions.Select(e => e.TrimStart('.')).OrderBy(e => e));

        /// <summary>파일명의 확장자가 화이트리스트에 있는지 확인한다.</summary>
        public static bool IsExtensionAllowed(string fileName) =>
            AllowedExtensions.Contains(Path.GetExtension(fileName));

        /// <summary>업로드 파일이 저장되는 최상위 폴더. wwwroot 밖이라 UseStaticFiles로 노출되지 않는다.</summary>
        public static string GetUploadsRoot(IWebHostEnvironment env) =>
            Path.Combine(env.ContentRootPath, "App_Data", "uploads");

        /// <summary>
        /// 프로젝트 문서(기술문서/적합성선언서) 첨부파일이 저장될 폴더.
        /// kind: "td" 또는 "doc", level: 포장차수("1"/"2"/"3")
        /// </summary>
        public static string GetProjectDocDirectory(IWebHostEnvironment env, string kind, string level, string prjId) =>
            Path.Combine(GetUploadsRoot(env), "projects", kind, level, prjId);

        /// <summary>
        /// DB에는 절대경로 대신 업로드 루트 기준 상대경로만 저장한다 (이식성 + URL로 직접 노출 방지).
        /// 예: "projects/td/1/20260907.../3_20260907101112333.pdf"
        /// </summary>
        public static string ToRelativePath(string kind, string level, string prjId, string storedFileName) =>
            $"projects/{kind}/{level}/{prjId}/{storedFileName}";

        /// <summary>DB에 저장된 상대경로를 실제 파일 시스템 경로로 바꾼다.</summary>
        public static string ToPhysicalPath(IWebHostEnvironment env, string relativePath) =>
            Path.Combine(GetUploadsRoot(env), relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
