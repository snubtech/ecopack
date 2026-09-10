/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ChatPromptBuilder (질문 글 조립)
 * ==============================================================================
 *
 * 1. 왜 이 파일이 필요한가
 *    - 생성형 AI 서버의 /api/chat 과 /api/chat_stream 은 요청 항목이 question 하나뿐입니다.
 *      역할 지시문·대화 이력·작업 상태를 따로 담아 보낼 자리가 없습니다.
 *    - 그래서 이 세 가지를 question 이라는 글 하나에 순서대로 적어 보냅니다.
 *
 * 2. 무엇을 어떤 순서로 담나 (순서가 중요하다)
 *    ① 요청        — 사용자가 실제로 입력한 문장
 *    ② 작업 상태   — ProjectContextBuilder 가 모아 온 전 과정 내용
 *    ③ 직전 대화   — 같은 세션의 최근 주고받은 말
 *    ④ 지시문      — 위 자료만 근거로 답하라는 당부. 반드시 맨 끝에 온다.
 *
 *    AI 서버 앞단에는 질문을 걸러 내는 장치가 있어서, 보낸 글이 사용자 질문으로 끝나면
 *    답을 만들기도 전에 "이전 대화 내용을 찾을 수 없습니다"로 되돌려보낼 때가 있습니다.
 *    (실측: 0.1초 만에 돌아옵니다. 생성에 실패한 것이 아니라 앞단에서 막힌 것입니다)
 *    요청을 맨 앞에 두고 맨 끝을 지시문으로 닫으면 같은 질문도 정상으로 통과합니다.
 *    그래서 ①~④ 순서를 지킵니다.
 *
 *    그래도 막히는 질문이 남아 있어, 막히면 LocalAnswerBuilder 가 DB 값으로 직접 답합니다.
 *
 * 3. 분량을 줄이는 이유
 *    - 서버가 벡터 DB(RAG)와 Text2SQL 을 함께 쓰기 때문에, 질문 글이 지나치게 길면
 *      정작 사용자가 물은 내용이 묻힐 수 있습니다.
 *    - 작업 상태는 MaxContextChars, 대화 이력은 HistoryTurns 로 잘라서 싣습니다.
 *    - 작업 상태를 아예 빼고 순수 질문만 보내고 싶으면
 *      설정에서 IncludeWorkContext 를 false 로 두면 됩니다.
 *
 * 4. DB 에는 무엇이 남나
 *    - 여기서 만든 긴 글이 아니라, 사용자가 입력한 원래 질문만 대화 이력에 남깁니다.
 *      화면에 되돌려 보여 줄 것도 원래 질문입니다.
 * ==============================================================================
 */
using System.Text;

namespace ecopack.Api.Support
{
    /// <summary>대화 한 줄(과거 이력을 실어 보낼 때 쓴다)</summary>
    public record ChatTurn(string Role, string Content);

    /// <summary>
    /// 역할 지시문 · 작업 상태 · 직전 대화 · 이번 질문을
    /// 서버가 받는 question 한 항목으로 합친다.
    /// </summary>
    public static class ChatPromptBuilder
    {
        /// <summary>
        /// 서버에 보낼 question 문자열을 만든다.
        /// </summary>
        /// <param name="question">사용자가 입력한 원래 질문</param>
        /// <param name="workContextText">ProjectContextBuilder 가 모아 온 작업 상태(없으면 null)</param>
        /// <param name="history">같은 세션의 직전 대화(오래된 것부터)</param>
        /// <param name="opt">채팅 설정</param>
        public static string Build(
            string question,
            string? workContextText,
            IReadOnlyList<ChatTurn> history,
            LlmChatOptions opt)
        {
            // 작업 상태도 대화 이력도 없으면 굳이 감싸지 않고 질문만 보낸다.
            // 서버의 RAG 답변을 그대로 받는 편이 낫다.
            var hasContext = opt.IncludeWorkContext && !string.IsNullOrWhiteSpace(workContextText);
            var hasHistory = history.Count > 0;
            if (!hasContext && !hasHistory)
            {
                return question;
            }

            var sb = new StringBuilder();

            // ── 순서가 중요하다 ────────────────────────────────────
            // AI 서버 앞단에는 질문을 걸러 내는 장치가 있어서, 글이 사용자 질문으로 끝나면
            // "이전 대화 내용을 찾을 수 없습니다"라며 답을 만들기도 전에 돌려보낼 때가 있다.
            // 실측해 보니 [요청]을 맨 앞에 두고 자료를 넣은 뒤 맨 끝을 지시문으로 닫으면
            // 같은 질문도 정상으로 통과한다. 그래서 이 순서를 지킨다.
            //   ① 요청 → ② 작업 상태 → ③ 직전 대화 → ④ 지시문
            // 그래도 막히는 질문이 남아 있어, 막히면 LocalAnswerBuilder 가 대신 답한다.

            // ── ① 요청 ───────────────────────────────────────────
            sb.AppendLine($"요청: {question.Trim()}");
            sb.AppendLine();

            // ── ② 작업 상태 ──────────────────────────────────────
            if (hasContext)
            {
                var ctx = workContextText!.Trim();
                if (ctx.Length > opt.MaxContextChars)
                {
                    ctx = ctx[..opt.MaxContextChars] + "\n...(작업 상태가 길어 이후 내용은 생략함)";
                }

                sb.AppendLine("[현재 작업 상태] — 이 사용자의 DB 에 실제로 저장된 값입니다");
                sb.AppendLine(ctx);
                sb.AppendLine();
            }

            // ── ③ 직전 대화 ──────────────────────────────────────
            if (hasHistory)
            {
                sb.AppendLine("[직전 대화]");
                foreach (var turn in history)
                {
                    if (string.IsNullOrWhiteSpace(turn.Content)) continue;

                    var who = turn.Role == "assistant" ? "AI" : "사용자";
                    var body = turn.Content.Trim();
                    if (body.Length > opt.MaxHistoryCharsPerTurn)
                    {
                        body = body[..opt.MaxHistoryCharsPerTurn] + "…";
                    }
                    // 줄바꿈이 섞이면 어디까지가 한 사람의 말인지 흐려져 한 줄로 편다
                    body = body.Replace("\r", " ").Replace("\n", " ");
                    sb.AppendLine($"{who}: {body}");
                }
                sb.AppendLine();
            }

            // ── ④ 지시문 (반드시 맨 끝) ──────────────────────────
            sb.AppendLine("위 자료만 근거로 맨 위의 요청에 답해 주세요.");
            sb.AppendLine("자료에 없는 값은 지어내지 말고 \"아직 입력되지 않았습니다\"라고 알려 주세요.");
            sb.AppendLine("한국어로, 실무자에게 말하듯 간결하게 정리해 주세요.");

            return sb.ToString();
        }
    }
}
