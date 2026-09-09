/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - LlmChatController (우측 AI 채팅창)
 * ==============================================================================
 *
 * 1. 담당 범위
 *    - 화면 오른쪽 AI 채팅창의 서버 쪽 기능 전부를 맡습니다.
 *    - 쓰는 테이블: llm_chat_session(세션) / llm_chat_msg(메시지)
 *                   llm_chat_session_arch, llm_chat_msg_arch(2일 지난 대화의 백업본)
 *
 * 2. 대화 세션 (요건 ①: 로그인한 동안 세션과 로그가 유지된다)
 *    - OpenSession : 로그인 직후 화면이 부릅니다. 그 회원의 열려 있는(ACTIVE) 세션이 있으면
 *                    그걸 그대로 돌려주고, 없으면 새로 만듭니다.
 *                    그래서 새로고침을 해도 하던 대화가 이어집니다.
 *    - CloseSession: 로그아웃할 때 부릅니다. 세션을 CLOSED 로 닫습니다.
 *                    닫아도 기록은 지우지 않습니다. 지난 대화 목록에서 다시 열어 볼 수 있습니다.
 *
 * 3. 질문과 답변 (요건 ③④: 이전 대화 + 중앙 화면의 전 과정 작업상태를 참조한다)
 *    - Send : ⓐ 로그인·소유자 확인
 *             ⓑ ProjectContextBuilder 로 "프로젝트 생성부터 지금까지" 전 과정을 모읍니다
 *             ⓒ 같은 세션의 직전 대화를 꺼냅니다
 *             ⓓ ⓑ+ⓒ+질문을 Claude 에 보내 답을 받습니다
 *             ⓔ 질문과 답을 각각 한 줄씩 llm_chat_msg 에 남깁니다
 *               (이때 그 순간의 작업상태 스냅샷도 함께 저장해 나중에 추적할 수 있게 합니다)
 *
 * 4. 백업 (요건 ②: 2일 지난 대화 이력의 로그를 백업한다)
 *    - Archive : 관리자가 직접 백업을 돌릴 때 씁니다.
 *                평소에는 LlmChatArchiveHostedService 가 주기적으로 알아서 돕니다.
 *
 * 5. 접근 제한
 *    - 모든 조회·저장은 repCustId(로그인한 고객 ID)를 기준으로 자기 것만 다룹니다.
 *      프로젝트를 함께 넘길 때는 ProjectAccess 로 그 프로젝트가 본인 것인지도 확인합니다.
 *      (이 프로젝트의 다른 컨트롤러들과 같은 방식입니다)
 * ==============================================================================
 */
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ecopack.Api.Data;
using ecopack.Api.Dtos;
using ecopack.Api.Services;
using ecopack.Api.Support;

namespace ecopack.Api.Controllers
{
    /// <summary>우측 AI 채팅창. 라우트: api/LlmChat</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LlmChatController : ControllerBase
    {
        /// <summary>질문 글자수 상한. 지나치게 긴 입력을 막는다.</summary>
        private const int MaxQuestionLength = 4000;

        private readonly AppDbContext _context;
        private readonly IClaudeChatService _chat;
        private readonly ILlmChatArchiveService _archive;
        private readonly LlmChatOptions _opt;

        private static readonly JsonSerializerOptions SnapshotJsonOpt = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public LlmChatController(
            AppDbContext context,
            IClaudeChatService chat,
            ILlmChatArchiveService archive,
            IOptions<LlmChatOptions> options)
        {
            _context = context;
            _chat = chat;
            _archive = archive;
            _opt = options.Value;
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/OpenSession
        // 로그인 직후 대화 세션을 연다. 이미 열려 있으면 그 세션을 그대로 쓴다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("OpenSession")]
        public async Task<IActionResult> OpenSession([FromBody] LlmChatOpenSessionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.RepCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }

            // 로그인 중에는 세션 하나를 계속 쓴다. 새로고침해도 하던 대화가 이어지도록.
            var session = await _context.LlmChatSession
                .Where(s => s.RepCustId == dto.RepCustId && s.ChatSesStatCd == "ACTIVE")
                .OrderByDescending(s => s.FrstCrtDtm)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                session = new LlmChatSession
                {
                    ChatSesId = "S" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    RepCustId = dto.RepCustId,
                    PrjId = Blank(dto.PrjId),
                    PackLevel = Blank(dto.PackLevel),
                    ChatSesStatCd = "ACTIVE",
                    ChatMsgCnt = 0,
                    LgnDtm = DateTime.Now,
                    FrstCrtDtm = DateTime.Now,
                    LastUpdDtm = DateTime.Now
                };
                _context.LlmChatSession.Add(session);
                await _context.SaveChangesAsync();
            }

            var messages = await _context.LlmChatMsg
                .AsNoTracking()
                .Where(m => m.ChatSesId == session.ChatSesId)
                .OrderBy(m => m.ChatMsgSeq)
                .Select(m => ToMsgDto(m))
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new LlmChatHistoryDto
                {
                    Session = ToSessionDto(session, archived: false),
                    Messages = messages
                }
            });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/CloseSession?chatSesId=xxx&repCustId=xxx
        // 로그아웃할 때 세션을 닫는다. 기록은 남는다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("CloseSession")]
        public async Task<IActionResult> CloseSession([FromQuery] string chatSesId, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }

            var session = await _context.LlmChatSession
                .FirstOrDefaultAsync(s => s.ChatSesId == chatSesId && s.RepCustId == repCustId);

            if (session == null)
            {
                // 이미 백업으로 넘어갔거나 없는 세션. 로그아웃을 막을 이유는 없다.
                return Ok(new { success = true, message = "닫을 세션이 없습니다." });
            }

            session.ChatSesStatCd = "CLOSED";
            session.LastUpdDtm = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "대화 세션을 닫았습니다." });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/Send
        // 질문을 보내고 답변을 받는다. 이 컨트롤러의 핵심.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody] LlmChatSendDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto?.RepCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }
            var question = (dto.Question ?? "").Trim();
            if (question.Length == 0)
            {
                return BadRequest(new { success = false, message = "질문을 입력해 주세요." });
            }
            if (question.Length > MaxQuestionLength)
            {
                return BadRequest(new { success = false, message = $"질문은 {MaxQuestionLength}자를 넘을 수 없습니다." });
            }

            // ⓐ 프로젝트를 함께 넘겼다면 본인 것인지 확인한다
            var prjId = Blank(dto.PrjId);
            if (prjId != null && !await ProjectAccess.IsOwnerAsync(_context, prjId, dto.RepCustId))
            {
                return StatusCode(403, new { success = false, message = ProjectAccess.DeniedMessage });
            }

            // 세션 확보. 화면이 세션 ID 를 안 보냈거나 그 사이 백업으로 넘어갔으면 새로 연다.
            var session = string.IsNullOrWhiteSpace(dto.ChatSesId)
                ? null
                : await _context.LlmChatSession
                    .FirstOrDefaultAsync(s => s.ChatSesId == dto.ChatSesId && s.RepCustId == dto.RepCustId, ct);

            if (session == null)
            {
                session = await _context.LlmChatSession
                    .Where(s => s.RepCustId == dto.RepCustId && s.ChatSesStatCd == "ACTIVE")
                    .OrderByDescending(s => s.FrstCrtDtm)
                    .FirstOrDefaultAsync(ct);
            }

            if (session == null)
            {
                session = new LlmChatSession
                {
                    ChatSesId = "S" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    RepCustId = dto.RepCustId,
                    PrjId = prjId,
                    PackLevel = Blank(dto.PackLevel),
                    ChatSesStatCd = "ACTIVE",
                    LgnDtm = DateTime.Now,
                    FrstCrtDtm = DateTime.Now,
                    LastUpdDtm = DateTime.Now
                };
                _context.LlmChatSession.Add(session);
                await _context.SaveChangesAsync(ct);
            }

            // ⓑ 프로젝트 생성부터 지금까지의 전 과정 작업 상태를 모은다
            var workContext = await ProjectContextBuilder.BuildAsync(
                _context, dto.RepCustId, prjId, Blank(dto.PackLevel), Blank(dto.CurMenuId), ct);

            // ⓒ 같은 세션의 직전 대화를 꺼낸다 (최근 것부터 N개 → 시간순으로 되돌림)
            var recent = await _context.LlmChatMsg
                .AsNoTracking()
                .Where(m => m.ChatSesId == session.ChatSesId)
                .OrderByDescending(m => m.ChatMsgSeq)
                .Take(_opt.HistoryTurns)
                .ToListAsync(ct);

            var history = recent
                .OrderBy(m => m.ChatMsgSeq)
                .Where(m => !string.IsNullOrWhiteSpace(m.ChatMsgCntn))
                .Select(m => new ClaudeChatTurn(m.ChatRoleCd, m.ChatMsgCntn!))
                .ToList();

            // ⓓ Claude 에 물어본다
            var answer = await _chat.AskAsync(question, workContext.Text, history, ct);

            // ⓔ 질문과 답을 각각 한 줄씩 남긴다
            var nextSeq = await _context.LlmChatMsg
                .Where(m => m.ChatSesId == session.ChatSesId)
                .MaxAsync(m => (int?)m.ChatMsgSeq, ct) ?? 0;

            var snapshotJson = SafeSerialize(workContext.Snapshot);
            var now = DateTime.Now;

            var userMsg = new LlmChatMsg
            {
                ChatSesId = session.ChatSesId,
                RepCustId = dto.RepCustId,
                ChatMsgSeq = ++nextSeq,
                ChatRoleCd = "user",
                ChatMsgCntn = question,
                PrjId = prjId,
                PackLevel = Blank(dto.PackLevel),
                CurMenuId = Blank(dto.CurMenuId),
                WrkStatSnpsCntn = snapshotJson,
                FrstCrtDtm = now
            };

            var aiMsg = new LlmChatMsg
            {
                ChatSesId = session.ChatSesId,
                RepCustId = dto.RepCustId,
                ChatMsgSeq = ++nextSeq,
                ChatRoleCd = "assistant",
                ChatMsgCntn = answer.Answer,
                PrjId = prjId,
                PackLevel = Blank(dto.PackLevel),
                CurMenuId = Blank(dto.CurMenuId),
                AiModelNm = answer.AiModelNm,
                InTknCnt = answer.InTknCnt,
                OutTknCnt = answer.OutTknCnt,
                ElpsMsVal = answer.ElpsMsVal,
                ErrCntn = answer.ErrCntn,
                FrstCrtDtm = now
            };

            _context.LlmChatMsg.AddRange(userMsg, aiMsg);

            // 세션 정보 갱신. 제목이 비어 있으면 첫 질문으로 만든다.
            if (string.IsNullOrWhiteSpace(session.ChatSesNm))
            {
                session.ChatSesNm = question.Length > 60 ? question[..60] + "…" : question;
            }
            session.ChatMsgCnt += 2;
            session.LastMsgDtm = now;
            session.LastUpdDtm = now;
            // 대화 도중 프로젝트를 열었다면 세션에도 반영해 둔다
            if (prjId != null)
            {
                session.PrjId = prjId;
                session.PackLevel = Blank(dto.PackLevel);
            }

            await _context.SaveChangesAsync(ct);

            return Ok(new
            {
                success = answer.Success,
                data = new LlmChatAnswerDto
                {
                    ChatSesId = session.ChatSesId,
                    UserMessage = ToMsgDto(userMsg),
                    AssistantMessage = ToMsgDto(aiMsg),
                    AiModelNm = answer.AiModelNm,
                    InTknCnt = answer.InTknCnt,
                    OutTknCnt = answer.OutTknCnt,
                    ElpsMsVal = answer.ElpsMsVal,
                    ContextSteps = workContext.Steps
                },
                message = answer.Success ? null : "AI 응답에 문제가 있어 안내 문구를 대신 표시합니다."
            });
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/LlmChat/GetSessions?repCustId=xxx&includeArchived=true
        // 지난 대화 목록. 백업으로 넘어간 세션도 함께 볼 수 있다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetSessions")]
        public async Task<IActionResult> GetSessions(
            [FromQuery] string? repCustId,
            [FromQuery] bool includeArchived = true,
            [FromQuery] int take = 50)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }
            if (take is < 1 or > 200) take = 50;

            var live = await _context.LlmChatSession
                .AsNoTracking()
                .Where(s => s.RepCustId == repCustId)
                .OrderByDescending(s => s.LastMsgDtm ?? s.FrstCrtDtm)
                .Take(take)
                .ToListAsync();

            var list = live.Select(s => ToSessionDto(s, archived: false)).ToList();

            if (includeArchived)
            {
                var archived = await _context.LlmChatSessionArch
                    .AsNoTracking()
                    .Where(s => s.RepCustId == repCustId)
                    .OrderByDescending(s => s.LastMsgDtm ?? s.FrstCrtDtm)
                    .Take(take)
                    .ToListAsync();

                list.AddRange(archived.Select(s => new LlmChatSessionDto
                {
                    ChatSesId = s.ChatSesId,
                    ChatSesNm = s.ChatSesNm,
                    ChatSesStatCd = s.ChatSesStatCd,
                    PrjId = s.PrjId,
                    PackLevel = s.PackLevel,
                    ChatMsgCnt = s.ChatMsgCnt,
                    LastMsgDtm = s.LastMsgDtm,
                    FrstCrtDtm = s.FrstCrtDtm ?? DateTime.MinValue,
                    Archived = true
                }));
            }

            list = list
                .OrderByDescending(s => s.LastMsgDtm ?? s.FrstCrtDtm)
                .Take(take)
                .ToList();

            return Ok(new { success = true, data = list });
        }

        // ─────────────────────────────────────────────────────────────
        // GET: api/LlmChat/GetHistory?chatSesId=xxx&repCustId=xxx
        // 세션 하나의 전체 대화. 백업된 세션이면 백업 테이블에서 읽어 온다.
        // ─────────────────────────────────────────────────────────────
        [HttpGet("GetHistory")]
        public async Task<IActionResult> GetHistory([FromQuery] string chatSesId, [FromQuery] string? repCustId)
        {
            if (string.IsNullOrWhiteSpace(repCustId))
            {
                return BadRequest(new { success = false, message = "로그인 정보(repCustId)가 필요합니다." });
            }
            if (string.IsNullOrWhiteSpace(chatSesId))
            {
                return BadRequest(new { success = false, message = "세션 ID가 필요합니다." });
            }

            // 소유자가 아니면 애초에 검색되지 않으므로, 없으면 404 로 끝난다
            var session = await _context.LlmChatSession
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ChatSesId == chatSesId && s.RepCustId == repCustId);

            if (session != null)
            {
                var messages = await _context.LlmChatMsg
                    .AsNoTracking()
                    .Where(m => m.ChatSesId == chatSesId)
                    .OrderBy(m => m.ChatMsgSeq)
                    .Select(m => ToMsgDto(m))
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new LlmChatHistoryDto { Session = ToSessionDto(session, archived: false), Messages = messages }
                });
            }

            // 살아 있는 세션에 없으면 백업 쪽을 본다
            var arch = await _context.LlmChatSessionArch
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ChatSesId == chatSesId && s.RepCustId == repCustId);

            if (arch == null)
            {
                return NotFound(new { success = false, message = "대화를 찾을 수 없습니다." });
            }

            var archMessages = await _context.LlmChatMsgArch
                .AsNoTracking()
                .Where(m => m.ChatSesId == chatSesId)
                .OrderBy(m => m.ChatMsgSeq)
                .Select(m => new LlmChatMsgDto
                {
                    ChatMsgId = m.ChatMsgId,
                    ChatMsgSeq = m.ChatMsgSeq,
                    ChatRoleCd = m.ChatRoleCd,
                    ChatMsgCntn = m.ChatMsgCntn,
                    PrjId = m.PrjId,
                    PackLevel = m.PackLevel,
                    CurMenuId = m.CurMenuId,
                    ErrCntn = m.ErrCntn,
                    FrstCrtDtm = m.FrstCrtDtm ?? DateTime.MinValue
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new LlmChatHistoryDto
                {
                    Session = new LlmChatSessionDto
                    {
                        ChatSesId = arch.ChatSesId,
                        ChatSesNm = arch.ChatSesNm,
                        ChatSesStatCd = arch.ChatSesStatCd,
                        PrjId = arch.PrjId,
                        PackLevel = arch.PackLevel,
                        ChatMsgCnt = arch.ChatMsgCnt,
                        LastMsgDtm = arch.LastMsgDtm,
                        FrstCrtDtm = arch.FrstCrtDtm ?? DateTime.MinValue,
                        Archived = true
                    },
                    Messages = archMessages
                }
            });
        }

        // ─────────────────────────────────────────────────────────────
        // POST: api/LlmChat/Archive
        // 2일 지난 대화 이력을 지금 바로 백업한다(관리자용 수동 실행).
        // 평소에는 LlmChatArchiveHostedService 가 주기적으로 알아서 돈다.
        // ─────────────────────────────────────────────────────────────
        [HttpPost("Archive")]
        public async Task<IActionResult> Archive(CancellationToken ct)
        {
            var result = await _archive.RunAsync(ct);
            return Ok(new
            {
                success = string.IsNullOrEmpty(result.ErrCntn),
                data = result,
                message = $"기준일 {result.CutoffDtm:yyyy-MM-dd} 이전 대화 {result.ArchivedSessions}건(메시지 {result.ArchivedMessages}건)을 백업했습니다."
            });
        }

        // ═════════════════════════════════════════════════════════════
        // 헬퍼
        // ═════════════════════════════════════════════════════════════

        /// <summary>화면이 빈 문자열을 보내는 경우가 많아, 값 없음으로 바꿔 준다.</summary>
        private static string? Blank(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        /// <summary>스냅샷 직렬화. 실패해도 대화 저장 자체를 막지 않는다.</summary>
        private static string? SafeSerialize(object? snapshot)
        {
            if (snapshot == null) return null;
            try
            {
                return JsonSerializer.Serialize(snapshot, SnapshotJsonOpt);
            }
            catch
            {
                return null;
            }
        }

        private static LlmChatMsgDto ToMsgDto(LlmChatMsg m) => new()
        {
            ChatMsgId = m.ChatMsgId,
            ChatMsgSeq = m.ChatMsgSeq,
            ChatRoleCd = m.ChatRoleCd,
            ChatMsgCntn = m.ChatMsgCntn,
            PrjId = m.PrjId,
            PackLevel = m.PackLevel,
            CurMenuId = m.CurMenuId,
            ErrCntn = m.ErrCntn,
            FrstCrtDtm = m.FrstCrtDtm
        };

        private static LlmChatSessionDto ToSessionDto(LlmChatSession s, bool archived) => new()
        {
            ChatSesId = s.ChatSesId,
            ChatSesNm = s.ChatSesNm,
            ChatSesStatCd = s.ChatSesStatCd,
            PrjId = s.PrjId,
            PackLevel = s.PackLevel,
            ChatMsgCnt = s.ChatMsgCnt,
            LastMsgDtm = s.LastMsgDtm,
            FrstCrtDtm = s.FrstCrtDtm,
            Archived = archived
        };
    }
}
