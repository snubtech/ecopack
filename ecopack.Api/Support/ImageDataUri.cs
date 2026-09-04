/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ImageDataUri (수집한 이미지 데이터 처리)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 참조 시스템에서 받아온 이미지 데이터를 화면에서 안전하게 쓸 수 있는 형태로 바꿉니다.
 *    - 공정도와 디자인 템플릿 화면의 상세 조회가 함께 씁니다.
 * 
 * 2. FromFile — 첨부 이미지
 *    - base64 문자열을 파일 확장자에 맞는 형식(image/png 등)으로 감싸 화면이 바로 그릴 수 있게 합니다.
 * 
 * 3. ExtractAll — 설명 이미지(memoImg)
 *    - memoImg 는 <img src="data:image/..."> 가 들어간 HTML 입니다.
 *    - 이 HTML 을 화면에 그대로 넣으면 위험하므로(HTML 주입),
 *      이미지 주소만 골라내어 <img> 로만 그리게 합니다.
 * ==============================================================================
 */
using System.Text.RegularExpressions;

namespace ecopack.Api.Support
{
    /// <summary>
    /// 참조 시스템에서 수집한 이미지 데이터를 화면에서 안전하게 쓸 수 있는 형태로 바꾼다.
    /// 라이브러리 화면(공정도·디자인 템플릿)이 공용으로 사용한다.
    /// </summary>
    public static class ImageDataUri
    {
        /// <summary>base64 문자열을 파일 확장자에 맞는 data URI 로 감싼다.</summary>
        public static string? FromFile(string? fileNm, string? base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
            {
                return null;
            }

            var mime = Path.GetExtension(fileNm ?? "").ToLowerInvariant() switch
            {
                ".png"  => "image/png",
                ".gif"  => "image/gif",
                ".webp" => "image/webp",
                ".svg"  => "image/svg+xml",
                ".bmp"  => "image/bmp",
                _       => "image/jpeg"
            };
            return $"data:{mime};base64,{base64}";
        }

        private static readonly Regex DataUriPattern = new(
            @"data:image/(?<type>png|jpe?g|gif|webp|bmp);base64,(?<data>[A-Za-z0-9+/=\s]+?)(?=[""'\)])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// memoImg 는 &lt;img src="data:image/...;base64,..."&gt; 가 들어간 HTML 이다.
        /// HTML 을 화면에 그대로 주입하지 않도록 이미지 data URI 만 뽑아낸다.
        /// </summary>
        public static List<string> ExtractAll(string? html)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(html))
            {
                return list;
            }

            foreach (Match m in DataUriPattern.Matches(html))
            {
                // 값 안에 줄바꿈/공백이 섞여 있을 수 있어 제거한 뒤 사용한다
                var data = Regex.Replace(m.Groups["data"].Value, @"\s+", "");
                if (data.Length == 0) continue;

                var type = m.Groups["type"].Value.ToLowerInvariant();
                if (type == "jpg") type = "jpeg";
                list.Add($"data:image/{type};base64,{data}");
            }

            return list;
        }
    }
}
