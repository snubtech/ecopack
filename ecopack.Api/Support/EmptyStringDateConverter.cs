/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - 날짜 항목의 빈 문자열 처리
 * ==============================================================================
 * 
 * 1. 왜 필요한가
 *    - 화면의 입력 폼은 모든 값을 글자로 다룹니다. 그래서 아직 값이 없는 날짜 항목은
 *      빈 문자열("")로 서버에 전달될 수 있습니다.
 *    - 기본 설정에서는 빈 문자열을 날짜로 바꾸지 못해 요청 자체가 거절되고(400),
 *      화면에는 "저장 중 오류가 발생했습니다" 만 보여 원인을 찾기 어렵습니다.
 * 
 * 2. 하는 일
 *    - 날짜 자리에 빈 문자열이나 공백만 오면 값이 없는 것(null)으로 봅니다.
 *    - 그 밖의 값은 평소대로 날짜로 바꿉니다.
 * 
 * 3. 적용 범위
 *    - Program.cs 에서 JSON 설정에 등록해 두어 모든 요청에 함께 적용됩니다.
 *      (DateTime? 과 DateOnly? 두 가지)
 * ==============================================================================
 */
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ecopack.Api.Support
{
    /// <summary>날짜/시각 항목이 빈 문자열로 오면 값 없음(null)으로 처리한다.</summary>
    public class EmptyStringDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }
                return DateTime.TryParse(text, out var parsed) ? parsed : null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue) writer.WriteStringValue(value.Value);
            else writer.WriteNullValue();
        }
    }

    /// <summary>날짜 항목이 빈 문자열로 오면 값 없음(null)으로 처리한다.</summary>
    public class EmptyStringDateOnlyConverter : JsonConverter<DateOnly?>
    {
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }
                return DateOnly.TryParse(text, out var parsed) ? parsed : null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue) writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
            else writer.WriteNullValue();
        }
    }
}
