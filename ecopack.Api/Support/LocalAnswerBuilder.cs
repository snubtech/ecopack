/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - LocalAnswerBuilder (직접 답변 조립)
 * ==============================================================================
 *
 * 1. 왜 필요한가
 *    - 생성형 AI 서버는 앞단에 질문을 걸러 내는 장치가 있어,
 *      "현재 프로젝트에서 선택한 패키징 정보를 알려줘" 같은 물음을
 *      답을 만들기도 전에 "이전 대화 내용을 찾을 수 없습니다"로 돌려보낼 때가 있습니다.
 *      (실측: 0.1초 만에 돌아옵니다. 답변을 만들다 실패한 것이 아니라 앞단에서 막힌 것입니다)
 *    - 그런데 그 물음의 답은 이미 우리 DB 에 다 있습니다.
 *      그래서 AI 서버가 막았을 때는 우리가 가진 값으로 직접 답을 조립해 내보냅니다.
 *
 * 2. 지어낼 여지가 없다
 *    - 여기서 만드는 글은 전부 DB 에서 읽어 온 값을 그대로 옮긴 것입니다.
 *      값이 없으면 "아직 입력되지 않았습니다"라고 적습니다. 추측하지 않습니다.
 *
 * 3. 무엇을 보여 주나
 *    - 프로젝트 기본 정보 → 차수별로 고른 포장 정보(표) → 진행 상황 → 다음에 할 일
 *    - 화면이 마크다운 표를 그릴 수 있으므로 표로 정리합니다.
 * ==============================================================================
 */
using System.Text;

namespace ecopack.Api.Support
{
    /// <summary>AI 서버가 답을 만들지 못했을 때, DB 값으로 답을 직접 조립한다.</summary>
    public static class LocalAnswerBuilder
    {
        /// <summary>
        /// 작업 상태를 사람이 읽는 답변으로 만든다.
        /// </summary>
        /// <param name="ctx">ProjectContextBuilder 가 모아 온 작업 상태</param>
        public static string Build(ProjectWorkContext ctx)
        {
            // ── 프로젝트를 아직 고르지 않은 경우 ──────────────────
            if (!ctx.HasProject)
            {
                if (ctx.ProjectCount > 0)
                {
                    return "아직 프로젝트를 열지 않으셨습니다.\n\n"
                         + $"보유하신 프로젝트가 {ctx.ProjectCount}건 있습니다. "
                         + "왼쪽 메뉴의 **기본평가 > 프로젝트 현황**에서 프로젝트를 먼저 선택해 주세요. "
                         + "선택하시면 그 프로젝트의 내용을 기준으로 답해 드립니다.";
                }
                return "아직 만들어진 프로젝트가 없습니다.\n\n"
                     + "왼쪽 메뉴의 **기본평가 > 기본사항**에서 프로젝트를 먼저 만들어 주세요.";
            }

            var sb = new StringBuilder();

            // ── 프로젝트 기본 정보 ────────────────────────────────
            sb.AppendLine($"**{ctx.PrjNm}** 프로젝트 기준으로 정리해 드립니다.");
            sb.AppendLine();
            sb.AppendLine($"- 프로젝트 번호: {ctx.PrjId}");
            if (ctx.PrjFcrtDt.HasValue)
            {
                sb.AppendLine($"- 생성일: {ctx.PrjFcrtDt:yyyy-MM-dd}");
            }
            sb.AppendLine($"- 지금 보고 있는 차수: {ctx.CurPackLevel}차");
            sb.AppendLine($"- 수출 대상국: {(ctx.ExportCountries.Count > 0 ? string.Join(", ", ctx.ExportCountries) : "선택 없음")}");
            sb.AppendLine();

            // ── 차수별로 고른 포장 정보 ───────────────────────────
            var saved = ctx.Levels.Where(l => l.DetailSaved).ToList();
            sb.AppendLine("### 선택한 포장 정보");
            if (saved.Count == 0)
            {
                sb.AppendLine();
                sb.AppendLine("아직 **기본사항이 저장되지 않았습니다.** "
                            + "왼쪽 메뉴의 **기본평가 > 기본사항**에서 적용소재·사용환경·포장재 구분·소재의 구성을 고르고 저장해 주세요.");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("| 항목 | " + string.Join(" | ", saved.Select(l => $"{l.PackLevel}차")) + " |");
                sb.AppendLine("|---|" + string.Concat(saved.Select(_ => "---|")));
                Row(sb, "포장차수명", saved, l => l.PackLevelNm);
                Row(sb, "적용소재", saved, l => l.AppliedMaterial);
                Row(sb, "사용환경", saved, l => l.MatUse);
                Row(sb, "포장재 구분", saved, l => l.MatType);
                Row(sb, "소재의 구성", saved, l => l.MatForm);
                Row(sb, "디자인 템플릿", saved, l => l.PackDsgnTplId);
                Row(sb, "수출국", saved, l => l.PrdExpCntry);
                Row(sb, "진행단계", saved, l => l.Projstatus);
                Row(sb, "최종 저장", saved, l => l.Updatedate?.ToString("yyyy-MM-dd HH:mm"));
            }
            sb.AppendLine();

            // ── 진행 상황 ─────────────────────────────────────────
            sb.AppendLine("### 진행 상황");
            sb.AppendLine();
            foreach (var step in ctx.Steps)
            {
                sb.AppendLine($"- {step}");
            }
            sb.AppendLine();

            // ── 다음에 할 일 ──────────────────────────────────────
            var next = NextAction(ctx);
            if (next != null)
            {
                sb.AppendLine("### 다음에 하실 일");
                sb.AppendLine();
                sb.AppendLine(next);
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>표의 한 줄. 모든 차수에서 값이 비어 있으면 줄 자체를 넣지 않는다.</summary>
        private static void Row(
            StringBuilder sb, string label,
            List<ProjectLevelFacts> levels, Func<ProjectLevelFacts, string?> pick)
        {
            var values = levels.Select(l => pick(l)).ToList();
            if (values.All(string.IsNullOrWhiteSpace)) return;

            sb.AppendLine($"| {label} | "
                + string.Join(" | ", values.Select(v => string.IsNullOrWhiteSpace(v) ? "미입력" : v))
                + " |");
        }

        /// <summary>
        /// 비어 있는 단계 중 가장 앞선 것을 짚어 다음에 할 일을 알려 준다.
        /// 순서는 화면의 작업 순서(기본사항 → 템플릿 → 모의평가 → TD → DOC)와 같다.
        /// </summary>
        private static string? NextAction(ProjectWorkContext ctx)
        {
            var lv = ctx.CurPackLevel ?? "1";
            var f = ctx.Levels.FirstOrDefault(x => x.PackLevel == lv);

            if (f == null || !f.DetailSaved)
            {
                return $"**기본사항**({lv}차)을 먼저 저장해 주세요. 적용소재와 사용환경을 골라야 이후 단계로 넘어갈 수 있습니다.";
            }
            if (string.IsNullOrWhiteSpace(f.PackDsgnTplId))
            {
                return $"**디자인 템플릿**({lv}차)을 선택해 주세요.";
            }
            if (f.EvalScore == null)
            {
                return $"**모의평가**({lv}차)를 진행해 주세요. 설문에 답하면 영역별 점수와 개선 방안을 받을 수 있습니다.";
            }
            if (!f.TdWritten)
            {
                return $"**TD({lv}차 기술문서)**를 작성해 주세요.";
            }
            if (!f.DocWritten)
            {
                return $"**DOC({lv}차 적합성 선언서)**를 작성해 주세요.";
            }

            return $"{lv}차는 기본사항부터 적합성 선언서까지 모두 채워져 있습니다. "
                 + "다른 포장차수를 진행하시거나, 작성한 문서를 검토해 보세요.";
        }
    }
}
