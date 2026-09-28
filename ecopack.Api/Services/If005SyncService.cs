/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - If005SyncService
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 기준정보 시스템의 IF005(환경영향평가정보 목록) API 를 불러
 *      if005 테이블을 최신 목록으로 통째로 바꿉니다. (RunAsync)
 *
 * 2. 호출 방식
 *    - POST {BaseUrl}{If005Path} 에 startCredat / endCredat / apiKey 를 JSON 으로 보냅니다.
 *    - 응답의 restList 배열을 행 단위로 읽어 if005 컬럼에 이름 그대로 옮깁니다. (ToEntity)
 *
 * 3. 저장 방식
 *    - 한 트랜잭션 안에서 기존 행을 모두 지우고 받은 행을 넣습니다.
 *      중간에 실패하면 되돌려서 기존 데이터가 그대로 남습니다.
 *    - 응답이 오류이거나 0건이면 지우지 않습니다. (일시 장애로 화면이 비는 일을 막기 위해)
 *
 * 4. 결과
 *    - 받은 건수·저장 건수·오류 내용을 If005SyncResult 로 돌려줍니다.
 * ==============================================================================
 */
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ecopack.Api.Data;
using ecopack.Api.Support;

namespace ecopack.Api.Services
{
    public interface IIf005SyncService
    {
        Task<If005SyncResult> RunAsync(CancellationToken ct);
    }

    /// <summary>IF005 수집 1회 실행 결과</summary>
    public class If005SyncResult
    {
        /// <summary>실행 시작 일시</summary>
        public DateTime StartDtm { get; set; }

        /// <summary>API 결과코드 (0000 = 성공)</summary>
        public string? ResultCode { get; set; }

        /// <summary>API 에서 받은 건수</summary>
        public int FetchedCount { get; set; }

        /// <summary>if005 에 저장한 건수</summary>
        public int SavedCount { get; set; }

        /// <summary>오류 내용. 비어 있으면 성공</summary>
        public string? ErrCntn { get; set; }
    }

    public class If005SyncService : IIf005SyncService
    {
        public const string HttpClientName = "EdbApi";

        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpFactory;
        private readonly EdbApiOptions _opt;
        private readonly ILogger<If005SyncService> _logger;

        public If005SyncService(
            AppDbContext context,
            IHttpClientFactory httpFactory,
            IOptions<EdbApiOptions> options,
            ILogger<If005SyncService> logger)
        {
            _context = context;
            _httpFactory = httpFactory;
            _opt = options.Value;
            _logger = logger;
        }

        public async Task<If005SyncResult> RunAsync(CancellationToken ct)
        {
            var result = new If005SyncResult { StartDtm = DateTime.Now };

            var apiKey = string.IsNullOrWhiteSpace(_opt.ApiKey)
                ? Environment.GetEnvironmentVariable("ECOPACK_EDB_API_KEY")
                : _opt.ApiKey;

            if (string.IsNullOrWhiteSpace(_opt.BaseUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                result.ErrCntn = "EdbApi:BaseUrl 또는 API 키(EdbApi:ApiKey / ECOPACK_EDB_API_KEY)가 설정되지 않았습니다.";
                return result;
            }

            try
            {
                // 1) API 호출
                var http = _httpFactory.CreateClient(HttpClientName);
                http.Timeout = TimeSpan.FromSeconds(_opt.TimeoutSeconds);

                var url = _opt.BaseUrl.TrimEnd('/') + _opt.If005Path;
                using var res = await http.PostAsJsonAsync(url, new
                {
                    startCredat = _opt.StartCredat,
                    endCredat = _opt.EndCredat,
                    apiKey
                }, ct);

                var body = await res.Content.ReadAsStringAsync(ct);
                if (!res.IsSuccessStatusCode)
                {
                    result.ErrCntn = $"HTTP {(int)res.StatusCode}: {Cut(body)}";
                    return result;
                }

                // 2) 응답 해석
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // 결과코드 항목 이름이 명세서에 없어, 흔히 쓰는 이름을 차례로 찾아본다
                result.ResultCode = FindString(root, "resultCode", "rsltCode", "code");
                if (result.ResultCode != null && result.ResultCode != "0000")
                {
                    result.ErrCntn = $"결과코드 {result.ResultCode}: {FindString(root, "resultMsg", "rsltMsg", "message") ?? Cut(body)}";
                    return result;
                }

                if (!TryGetProperty(root, "restList", out var list) || list.ValueKind != JsonValueKind.Array)
                {
                    result.ErrCntn = $"응답에 restList 가 없습니다: {Cut(body)}";
                    return result;
                }

                var now = DateTime.Now;
                var rows = list.EnumerateArray()
                    .Select(item => ToEntity(item, now))
                    .Where(x => !string.IsNullOrEmpty(x.EnvImpAssId))
                    .ToList();
                result.FetchedCount = list.GetArrayLength();

                if (rows.Count == 0)
                {
                    // 0건이면 일시 장애일 수 있어 기존 데이터를 지우지 않는다
                    result.ErrCntn = "받은 데이터가 0건이라 기존 데이터를 유지했습니다.";
                    return result;
                }

                // 3) 통째로 교체 (실패 시 되돌림)
                await using var tx = await _context.Database.BeginTransactionAsync(ct);
                await _context.If005.ExecuteDeleteAsync(ct);
                _context.If005.AddRange(rows);
                result.SavedCount = await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation("IF005 수집 완료 — 받은 {F}건 / 저장 {S}건", result.FetchedCount, result.SavedCount);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IF005 수집 중 오류");
                result.ErrCntn = ex.Message;
            }

            return result;
        }

        /// <summary>restList 한 건을 if005 행으로 옮긴다. 명세서 필드명과 컬럼명이 같다.</summary>
        private static If005 ToEntity(JsonElement item, DateTime now) => new()
        {
            EnvImpAssId = Str(item, "envImpAssId") ?? "",
            PackLevelNm = Str(item, "packLevelNm"),
            AppliedMaterialNm = Str(item, "appliedMaterialNm"),
            MatTypeNm = Str(item, "matTypeNm"),
            MatFormNm = Str(item, "matFormNm"),
            MassCo2Mat = Str(item, "massCo2Mat"),
            MassCo2Proc = Str(item, "massCo2Proc"),
            MassCo2Scrap = Str(item, "massCo2Scrap"),
            MassCo2Sum = Str(item, "massCo2Sum"),
            MassCo2MgtVal = Str(item, "massCo2MgtVal"),
            UnitCo2Mat = Str(item, "unitCo2Mat"),
            UnitCo2Proc = Str(item, "unitCo2Proc"),
            UnitCo2Scrap = Str(item, "unitCo2Scrap"),
            UnitCo2Sum = Str(item, "unitCo2Sum"),
            UnitCo2MgtVal = Str(item, "unitCo2MgtVal"),
            UnitCo2Desc = Str(item, "unitCo2Desc"),
            AreaDensity = Str(item, "areaDensity"),
            PhyQntyUnit = Str(item, "phyQntyUnit"),
            MatCompCon = Str(item, "matCompCon"),
            PackLevel = Str(item, "packLevel"),
            AppliedMaterial = Str(item, "appliedMaterial"),
            MatType = Str(item, "matType"),
            MatForm = Str(item, "matForm"),
            CreatedAt = now
        };

        // ═════════════════════════════════════════════════════════════
        // 헬퍼
        // ═════════════════════════════════════════════════════════════

        /// <summary>
        /// 항목 값을 글자로 꺼낸다. 명세서는 모두 string 이지만
        /// 숫자로 오는 경우도 있어 숫자는 원문 그대로 글자로 바꾼다.
        /// </summary>
        private static string? Str(JsonElement obj, string name)
        {
            if (!TryGetProperty(obj, name, out var v)) return null;
            return v.ValueKind switch
            {
                JsonValueKind.String => string.IsNullOrWhiteSpace(v.GetString()) ? null : v.GetString(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => v.GetRawText()
            };
        }

        /// <summary>대소문자를 가리지 않고 항목을 찾는다.</summary>
        private static bool TryGetProperty(JsonElement obj, string name, out JsonElement value)
        {
            value = default;
            if (obj.ValueKind != JsonValueKind.Object) return false;
            foreach (var p in obj.EnumerateObject())
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = p.Value;
                    return true;
                }
            }
            return false;
        }

        private static string? FindString(JsonElement obj, params string[] names)
        {
            foreach (var n in names)
            {
                var v = Str(obj, n);
                if (v != null) return v.Trim('"');
            }
            return null;
        }

        /// <summary>오류 메시지에 응답 본문을 실을 때 너무 길지 않게 자른다.</summary>
        private static string Cut(string s) => s.Length <= 300 ? s : s[..300] + "…";
    }
}
