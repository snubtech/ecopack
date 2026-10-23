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
 *
 * 5. 파일 내용 확인 (시그니처)
 *    - 확장자는 이름만 바꾸면 속일 수 있다. 파일 맨 앞 몇 바이트(시그니처)가
 *      그 확장자의 형식과 맞는지도 함께 본다. (HasValidSignatureAsync)
 *
 * 6. 경로 검증
 *    - 프로젝트 번호나 DB 에 저장된 상대경로가 폴더 경로에 그대로 들어가므로,
 *      만들어진 경로가 업로드 폴더 안인지 항상 확인한다. 밖을 가리키면 쓰지 않는다.
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

        // 형식별 파일 시그니처(파일 맨 앞 바이트)
        private static readonly byte[] SigPdf  = "%PDF-"u8.ToArray();
        private static readonly byte[] SigZip  = { 0x50, 0x4B, 0x03, 0x04 };                          // docx / pptx / xlsx / hwpx
        private static readonly byte[] SigOle  = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };  // doc / ppt / xls / hwp
        private static readonly byte[] SigPng  = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] SigJpeg = { 0xFF, 0xD8, 0xFF };

        private static readonly IReadOnlyDictionary<string, byte[]> Signatures =
            new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = SigPdf,
                [".docx"] = SigZip, [".pptx"] = SigZip, [".xlsx"] = SigZip, [".hwpx"] = SigZip,
                [".doc"] = SigOle, [".ppt"] = SigOle, [".xls"] = SigOle, [".hwp"] = SigOle,
                [".png"] = SigPng, [".jpg"] = SigJpeg, [".jpeg"] = SigJpeg,
            };

        /// <summary>
        /// 파일 내용이 확장자가 말하는 형식과 맞는지 파일 맨 앞 바이트로 확인한다.
        /// 확장자만 바꾼 파일(예: 실행 파일을 .pdf 로 이름만 바꾼 것)을 걸러낸다.
        /// </summary>
        public static async Task<bool> HasValidSignatureAsync(IFormFile file)
        {
            if (!Signatures.TryGetValue(Path.GetExtension(file.FileName), out var expected))
            {
                return false; // 시그니처를 모르는 형식은 받지 않는다
            }

            var head = new byte[expected.Length];
            await using var stream = file.OpenReadStream();
            var read = await stream.ReadAtLeastAsync(head, head.Length, throwOnEndOfStream: false);
            return read == expected.Length && head.AsSpan().SequenceEqual(expected);
        }

        /// <summary>파일 내용이 확장자와 맞지 않을 때 돌려줄 안내 문구</summary>
        public const string SignatureMismatchMessage = "파일 내용이 확장자와 맞지 않습니다. 원본 파일을 다시 확인해 주세요.";

        /// <summary>
        /// 제조 도면처럼 "이미지 파일만" 받는 자리에 쓰는 별도 화이트리스트.
        /// 문서 화이트리스트(AllowedExtensions)와는 쓰임새가 달라 따로 둔다.
        /// SVG 는 안에 스크립트를 담을 수 있고 내용 확인(시그니처)도 할 수 없어 받지 않는다.
        /// </summary>
        public static readonly IReadOnlySet<string> AllowedImageExtensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg",
        };

        /// <summary>화면 안내·오류 문구에 쓸 이미지 확장자 목록 (예: "jpeg, jpg, png").</summary>
        public static string AllowedImageExtensionsDisplay =>
            string.Join(", ", AllowedImageExtensions.Select(e => e.TrimStart('.')).OrderBy(e => e));

        /// <summary>파일명의 확장자가 이미지 화이트리스트에 있는지 확인한다.</summary>
        public static bool IsImageExtensionAllowed(string fileName) =>
            AllowedImageExtensions.Contains(Path.GetExtension(fileName));

        /// <summary>업로드 파일이 저장되는 최상위 폴더. wwwroot 밖이라 UseStaticFiles로 노출되지 않는다.</summary>
        public static string GetUploadsRoot(IWebHostEnvironment env) =>
            Path.Combine(env.ContentRootPath, "App_Data", "uploads");

        /// <summary>
        /// 프로젝트 문서(기술문서/적합성선언서) 첨부파일이 저장될 폴더.
        /// kind: "td" 또는 "doc", level: 포장차수("1"/"2"/"3")
        /// </summary>
        public static string GetProjectDocDirectory(IWebHostEnvironment env, string kind, string level, string prjId)
        {
            // 세 값 모두 폴더 이름으로 쓰이므로 경로를 바꿀 수 있는 글자(/ \ .. 등)를 허용하지 않는다.
            // 여기를 통과하지 못하면 예외로 끊는다. 이 경로는 폴더째 지우는 데도 쓰이기 때문이다.
            if (!IsSafePathSegment(kind) || !IsSafePathSegment(level) || !IsSafePathSegment(prjId))
            {
                throw new ArgumentException("폴더 이름으로 쓸 수 없는 값입니다.");
            }

            var dir = Path.GetFullPath(Path.Combine(GetUploadsRoot(env), "projects", kind, level, prjId));
            if (!IsInsideUploadsRoot(env, dir))
            {
                throw new ArgumentException("업로드 폴더 밖의 경로입니다.");
            }
            return dir;
        }

        /// <summary>
        /// 폴더 이름 한 칸으로 써도 안전한 값인지 본다. 영문·숫자·밑줄·하이픈만 허용한다.
        /// 서버가 채번하는 프로젝트 번호는 모두 이 범위 안이다.
        /// </summary>
        public static bool IsSafePathSegment(string? value) =>
            !string.IsNullOrEmpty(value)
            && value.Length <= 50
            && value.All(c => char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-');

        /// <summary>전체 경로가 업로드 최상위 폴더 안쪽인지 확인한다.</summary>
        private static bool IsInsideUploadsRoot(IWebHostEnvironment env, string fullPath)
        {
            var root = Path.GetFullPath(GetUploadsRoot(env));
            if (!root.EndsWith(Path.DirectorySeparatorChar))
            {
                root += Path.DirectorySeparatorChar;
            }
            return fullPath.StartsWith(root, StringComparison.Ordinal);
        }

        /// <summary>
        /// DB에는 절대경로 대신 업로드 루트 기준 상대경로만 저장한다 (이식성 + URL로 직접 노출 방지).
        /// 예: "projects/td/1/20260907.../3_20260907101112333.pdf"
        /// </summary>
        public static string ToRelativePath(string kind, string level, string prjId, string storedFileName) =>
            $"projects/{kind}/{level}/{prjId}/{storedFileName}";

        /// <summary>
        /// DB에 저장된 상대경로를 실제 파일 시스템 경로로 바꾼다.
        /// 경로가 업로드 폴더 밖을 가리키면(../ 등) null 을 돌려준다.
        /// 호출부는 null 을 "파일 없음"과 똑같이 다루면 된다(File.Exists(null) 은 false).
        /// </summary>
        public static string? ToPhysicalPath(IWebHostEnvironment env, string relativePath)
        {
            string full;
            try
            {
                full = Path.GetFullPath(
                    Path.Combine(GetUploadsRoot(env), relativePath.Replace('/', Path.DirectorySeparatorChar)));
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return null;
            }

            return IsInsideUploadsRoot(env, full) ? full : null;
        }
    }
}
