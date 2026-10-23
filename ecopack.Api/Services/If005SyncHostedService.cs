/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - If005SyncHostedService
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 서버가 켜진 뒤 잠시 기다렸다가 IF005(환경영향평가정보) 수집을 한 번 돌리고,
 *      그 뒤로는 설정한 주기(기본 24시간)마다 다시 돕니다.
 *
 * 2. 왜 시작 직후 바로 돌리지 않나
 *    - 서버가 막 올라온 시점에는 DB 연결 등 주변 준비가 덜 되었을 수 있어
 *      1분 정도 여유를 둡니다.
 *
 * 3. 꺼 두려면
 *    - appsettings.json 의 EdbApi:If005SyncIntervalHours 를 0 으로 두면
 *      자동 실행을 하지 않습니다. (수동 API POST api/If005Sync/Run 은 그대로 쓸 수 있습니다)
 * ==============================================================================
 */
using Microsoft.Extensions.Options;
using ecopack.Api.Support;

namespace ecopack.Api.Services
{
    public class If005SyncHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EdbApiOptions _opt;
        private readonly ILogger<If005SyncHostedService> _logger;

        public If005SyncHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<EdbApiOptions> options,
            ILogger<If005SyncHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _opt = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_opt.If005SyncIntervalHours <= 0)
            {
                _logger.LogInformation("IF005 자동 수집이 꺼져 있다 (EdbApi:If005SyncIntervalHours = 0).");
                return;
            }

            // 서버가 완전히 뜰 때까지 잠깐 기다린다
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            var interval = TimeSpan.FromHours(_opt.If005SyncIntervalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // BackgroundService 는 싱글턴이라 DbContext 를 직접 주입받을 수 없다.
                    // 실행할 때마다 새 스코프를 열어 그 안에서 꺼내 쓴다.
                    using var scope = _scopeFactory.CreateScope();
                    var sync = scope.ServiceProvider.GetRequiredService<IIf005SyncService>();

                    var result = await sync.RunAsync(stoppingToken);
                    if (!string.IsNullOrEmpty(result.ErrCntn))
                    {
                        _logger.LogWarning("IF005 자동 수집 실패 — {Err}", result.ErrCntn);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // 한 번 실패해도 서버를 내리지 않는다. 다음 주기에 다시 시도한다.
                    _logger.LogError(ex, "IF005 자동 수집 중 오류");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
