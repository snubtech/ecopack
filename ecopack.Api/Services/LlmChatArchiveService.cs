/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - LlmChatArchiveService (대화 이력 백업)
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 마지막 대화가 2일보다 오래된 대화 세션을 찾아 백업합니다.
 *    - 백업은 두 곳에 동시에 남깁니다.
 *      (가) DB 백업 테이블 : llm_chat_session_arch / llm_chat_msg_arch
 *      (나) JSON 파일      : App_Data/llmbackup/{yyyyMMdd}/{세션ID}.json
 *
 * 2. 왜 두 곳에 남기나
 *    - DB 쪽은 나중에 SQL 로 바로 찾아볼 수 있어 편하고,
 *      파일 쪽은 DB 와 무관하게 통째로 보관·이관할 수 있습니다.
 *
 * 3. 처리 순서 (되돌릴 수 있게)
 *    ① JSON 파일을 먼저 씁니다.
 *    ② 백업 테이블에 넣고, 원본을 지우는 것까지 하나의 트랜잭션으로 묶습니다.
 *    - 파일을 먼저 쓰는 이유: DB 작업이 실패해 되돌아가더라도 파일은 남아
 *      대화가 통째로 사라지는 일이 없게 하기 위함입니다.
 *      (다음 회차에 같은 세션을 다시 처리하면 파일은 덮어써집니다)
 *
 * 4. 중복 실행 방지
 *    - 백그라운드 작업과 관리자의 수동 실행이 겹칠 수 있어 세마포어로 한 번에 하나만 돕니다.
 * ==============================================================================
 */
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Support;

namespace ecopack.Api.Services
{
    public interface ILlmChatArchiveService
    {
        Task<LlmChatArchiveResultDto> RunAsync(CancellationToken ct = default);
    }

    public class LlmChatArchiveService : ILlmChatArchiveService
    {
        /// <summary>한 번에 처리할 세션 수. 너무 많이 쌓였어도 DB 를 오래 잡지 않게 나눠 돈다.</summary>
        private const int BatchSize = 200;

        /// <summary>백그라운드 작업과 수동 실행이 겹치지 않게 하나만 돌린다.</summary>
        private static readonly SemaphoreSlim Gate = new(1, 1);

        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly AppDbContext _db;
        private readonly LlmChatOptions _opt;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LlmChatArchiveService> _logger;

        public LlmChatArchiveService(
            AppDbContext db,
            IOptions<LlmChatOptions> options,
            IWebHostEnvironment env,
            ILogger<LlmChatArchiveService> logger)
        {
            _db = db;
            _opt = options.Value;
            _env = env;
            _logger = logger;
        }

        public async Task<LlmChatArchiveResultDto> RunAsync(CancellationToken ct = default)
        {
            var days = _opt.ArchiveAfterDays > 0 ? _opt.ArchiveAfterDays : 2;
            var cutoff = DateTime.Now.Date.AddDays(-days);

            var result = new LlmChatArchiveResultDto { CutoffDtm = cutoff };

            // 이미 돌고 있으면 그냥 빈 결과를 돌려준다(줄 서서 기다리지 않는다).
            if (!await Gate.WaitAsync(0, ct))
            {
                result.ErrCntn = "백업이 이미 실행 중이다.";
                return result;
            }

            try
            {
                // 대상: 마지막 대화가 cutoff 이전인 세션.
                // 대화가 한 줄도 없는 세션은 만들어진 시각으로 판단한다.
                var targets = await _db.LlmChatSession
                    .Where(s => (s.LastMsgDtm != null && s.LastMsgDtm < cutoff)
                             || (s.LastMsgDtm == null && s.FrstCrtDtm < cutoff))
                    .OrderBy(s => s.FrstCrtDtm)
                    .Take(BatchSize)
                    .ToListAsync(ct);

                if (targets.Count == 0)
                {
                    return result;
                }

                var baseDir = Path.Combine(_env.ContentRootPath, _opt.ArchiveDirectory);

                foreach (var session in targets)
                {
                    ct.ThrowIfCancellationRequested();

                    var messages = await _db.LlmChatMsg
                        .Where(m => m.ChatSesId == session.ChatSesId)
                        .OrderBy(m => m.ChatMsgSeq)
                        .ToListAsync(ct);

                    // ① JSON 파일 먼저 — DB 작업이 실패해도 대화 내용은 남는다
                    string? filePath = null;
                    try
                    {
                        filePath = await WriteJsonAsync(baseDir, session, messages, ct);
                        result.Files.Add(filePath);
                    }
                    catch (Exception ex)
                    {
                        // 파일을 못 써도 DB 백업은 계속한다. 대신 어디서 막혔는지 남긴다.
                        _logger.LogError(ex, "대화 백업 파일 쓰기 실패. 세션 {ChatSesId}", session.ChatSesId);
                        result.ErrCntn = $"일부 세션의 백업 파일을 쓰지 못했다: {ex.Message}";
                    }

                    // ② 백업 테이블 이관 + 원본 삭제 (한 묶음)
                    await using var tx = await _db.Database.BeginTransactionAsync(ct);
                    try
                    {
                        _db.LlmChatSessionArch.Add(new LlmChatSessionArch
                        {
                            ChatSesId = session.ChatSesId,
                            RepCustId = session.RepCustId,
                            PrjId = session.PrjId,
                            PackLevel = session.PackLevel,
                            ChatSesNm = session.ChatSesNm,
                            ChatSesStatCd = "ARCHIVED",
                            ChatMsgCnt = session.ChatMsgCnt,
                            LgnDtm = session.LgnDtm,
                            LastMsgDtm = session.LastMsgDtm,
                            FrstCrtDtm = session.FrstCrtDtm,
                            LastUpdDtm = session.LastUpdDtm,
                            ArchDtm = DateTime.Now,
                            ArchFileUrl = filePath
                        });

                        foreach (var m in messages)
                        {
                            _db.LlmChatMsgArch.Add(new LlmChatMsgArch
                            {
                                ChatMsgId = m.ChatMsgId,
                                ChatSesId = m.ChatSesId,
                                RepCustId = m.RepCustId,
                                ChatMsgSeq = m.ChatMsgSeq,
                                ChatRoleCd = m.ChatRoleCd,
                                ChatMsgCntn = m.ChatMsgCntn,
                                PrjId = m.PrjId,
                                PackLevel = m.PackLevel,
                                CurMenuId = m.CurMenuId,
                                WrkStatSnpsCntn = m.WrkStatSnpsCntn,
                                AiModelNm = m.AiModelNm,
                                InTknCnt = m.InTknCnt,
                                OutTknCnt = m.OutTknCnt,
                                ElpsMsVal = m.ElpsMsVal,
                                ErrCntn = m.ErrCntn,
                                FrstCrtDtm = m.FrstCrtDtm,
                                ArchDtm = DateTime.Now
                            });
                        }

                        _db.LlmChatMsg.RemoveRange(messages);
                        _db.LlmChatSession.Remove(session);

                        await _db.SaveChangesAsync(ct);
                        await tx.CommitAsync(ct);

                        result.ArchivedSessions++;
                        result.ArchivedMessages += messages.Count;
                    }
                    catch (Exception ex)
                    {
                        await tx.RollbackAsync(ct);

                        // 실패한 세션의 변경 추적을 걷어내야 다음 세션 처리가 오염되지 않는다
                        _db.ChangeTracker.Clear();

                        _logger.LogError(ex, "대화 백업 DB 이관 실패. 세션 {ChatSesId}", session.ChatSesId);
                        result.ErrCntn = $"일부 세션을 백업 테이블로 옮기지 못했다: {ex.Message}";
                    }
                }

                _logger.LogInformation(
                    "LLM 대화 백업 완료 — 기준일 {Cutoff:yyyy-MM-dd}, 세션 {S}건, 메시지 {M}건",
                    cutoff, result.ArchivedSessions, result.ArchivedMessages);

                return result;
            }
            finally
            {
                Gate.Release();
            }
        }

        /// <summary>세션 하나를 JSON 파일로 떨어뜨리고 그 경로를 돌려준다.</summary>
        private static async Task<string> WriteJsonAsync(
            string baseDir, LlmChatSession session, List<LlmChatMsg> messages, CancellationToken ct)
        {
            // 마지막 대화 날짜로 폴더를 나눈다. 날짜별로 통째로 옮기거나 지우기 좋다.
            var dayFolder = (session.LastMsgDtm ?? session.FrstCrtDtm).ToString("yyyyMMdd");
            var dir = Path.Combine(baseDir, dayFolder);
            Directory.CreateDirectory(dir);

            // 세션 ID 는 서버가 채번하지만, 경로 조작 문자가 섞이지 않게 한 번 더 막는다
            var safeName = string.Concat(session.ChatSesId.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            var path = Path.Combine(dir, safeName + ".json");

            var payload = new
            {
                backupAt = DateTime.Now,
                session = new
                {
                    session.ChatSesId,
                    session.RepCustId,
                    session.PrjId,
                    session.PackLevel,
                    session.ChatSesNm,
                    session.ChatSesStatCd,
                    session.ChatMsgCnt,
                    session.LgnDtm,
                    session.LastMsgDtm,
                    session.FrstCrtDtm,
                    session.LastUpdDtm
                },
                messages = messages.Select(m => new
                {
                    m.ChatMsgId,
                    m.ChatMsgSeq,
                    m.ChatRoleCd,
                    m.ChatMsgCntn,
                    m.PrjId,
                    m.PackLevel,
                    m.CurMenuId,
                    m.WrkStatSnpsCntn,
                    m.AiModelNm,
                    m.InTknCnt,
                    m.OutTknCnt,
                    m.ElpsMsVal,
                    m.ErrCntn,
                    m.FrstCrtDtm
                })
            };

            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(payload, JsonOpt), ct);
            return path;
        }
    }
}
