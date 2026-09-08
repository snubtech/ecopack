/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ProjectContextBuilder (작업 상태 수집기)
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 우측 AI 채팅창이 답을 만들 때 참고할 "지금까지의 작업 내용"을 한 덩어리로 모읍니다.
 *    - 화면 한 장만 보는 것이 아니라, 프로젝트를 처음 만든 순간부터
 *      기본사항 → 디자인 템플릿 → 모의평가 → 기술문서(TD) → 적합성 선언서(DOC) 까지
 *      전 과정을 차수(1차/2차/3차)별로 훑어서 담습니다.
 *
 * 2. 왜 전 과정을 다 담나
 *    - 사용자가 TD 화면에서 "이 소재로 괜찮아?" 라고 물으면
 *      기본사항에서 고른 소재와 모의평가 점수를 같이 봐야 제대로 답할 수 있습니다.
 *    - 그래서 화면이 알려 준 현재 메뉴(curMenuId)는 "지금 어디를 보고 있는지" 표시로만 쓰고,
 *      내용은 항상 전 단계를 함께 넘깁니다.
 *
 * 3. 분량 관리
 *    - 표 형태 데이터(BOM·모의평가 문항 등)는 항목 수를 잘라서 담습니다.
 *    - 마지막에 전체 길이를 한 번 더 잘라, 프롬프트가 지나치게 커지지 않게 합니다.
 *
 * 4. 접근 제한
 *    - 프로젝트 소유자 확인은 호출하는 쪽(LlmChatController)이 ProjectAccess 로 먼저 합니다.
 *      여기서는 이미 확인된 프로젝트만 들어온다고 보고 읽기만 합니다.
 * ==============================================================================
 */
using System.Text;
using Microsoft.EntityFrameworkCore;
using ecopack.Api.Data;

namespace ecopack.Api.Support
{
    /// <summary>프로젝트의 전 과정 작업 상태를 모아 LLM 프롬프트용 텍스트로 만든다.</summary>
    public static class ProjectContextBuilder
    {
        /// <summary>프롬프트에 담을 작업 상태 전문의 최대 길이(글자)</summary>
        private const int MaxTextLength = 24000;

        /// <summary>모의평가 문항은 차수당 이 개수까지만 담는다</summary>
        private const int MaxEvalItemsPerLevel = 40;

        /// <summary>화면에서 쓰는 메뉴 ID → 사람이 읽는 이름</summary>
        private static readonly Dictionary<string, string> MenuNames = new()
        {
            ["project-history"] = "프로젝트 현황",
            ["start-project"] = "기본사항",
            ["prjdefault"] = "기본사항",
            ["prjtemplate"] = "디자인 템플릿 선택",
            ["prjdefaultresult"] = "기본사항 결과",
            ["prjeval"] = "모의평가",
            ["mock-assessment"] = "모의평가",
            ["prjevalresult"] = "모의평가 결과",
            ["td"] = "TD (기술문서)",
            ["doc"] = "DOC (적합성 선언서)",
            ["material"] = "라이브러리 > 소재물성",
            ["process-map"] = "라이브러리 > 공정도",
            ["carbon"] = "라이브러리 > 탄소배출량",
            ["regulation"] = "라이브러리 > 환경규제",
            ["template"] = "라이브러리 > 디자인 템플릿",
        };

        /// <summary>
        /// 작업 상태를 모은다.
        /// </summary>
        /// <param name="db">DB 컨텍스트</param>
        /// <param name="repCustId">로그인한 고객 ID</param>
        /// <param name="prjId">현재 프로젝트 ID(없을 수 있다)</param>
        /// <param name="packLevel">현재 보고 있는 포장차수</param>
        /// <param name="curMenuId">현재 중앙 화면의 메뉴 ID</param>
        public static async Task<ProjectWorkContext> BuildAsync(
            AppDbContext db,
            string repCustId,
            string? prjId,
            string? packLevel,
            string? curMenuId,
            CancellationToken ct = default)
        {
            var sb = new StringBuilder();
            var steps = new List<string>();

            // ── 0. 지금 보고 있는 화면 ─────────────────────────────────
            var menuName = !string.IsNullOrWhiteSpace(curMenuId) && MenuNames.TryGetValue(curMenuId, out var mn)
                ? mn
                : (curMenuId ?? "(알 수 없음)");
            sb.AppendLine("## 지금 사용자가 보고 있는 화면");
            sb.AppendLine($"- 중앙 화면: {menuName}" + (string.IsNullOrWhiteSpace(curMenuId) ? "" : $" (menuId={curMenuId})"));
            sb.AppendLine();

            // ── 1. 회원(제조사) 정보 ───────────────────────────────────
            var member = await db.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.RepCustId == repCustId, ct);
            if (member != null)
            {
                sb.AppendLine("## 로그인한 회원(제조사) 정보");
                sb.AppendLine($"- 고객 ID: {member.RepCustId}");
                AppendIf(sb, "회사(법인/개인사업)명", member.BizNm);
                AppendIf(sb, "대표자명", member.RoleNm);
                AppendIf(sb, "담당자/직책", member.RepNm);
                AppendIf(sb, "업종", member.IndstNm);
                AppendIf(sb, "국가", member.CntryNm);
                AppendIf(sb, "이메일", member.EmlAddr);
                sb.AppendLine();
            }

            // 프로젝트가 지정되지 않았으면 목록만 요약해 주고 끝낸다
            if (string.IsNullOrWhiteSpace(prjId))
            {
                var recent = await db.Project.AsNoTracking()
                    .Where(p => p.RepCustId == repCustId || p.Prjuserid == repCustId)
                    .OrderByDescending(p => p.PrjId)
                    .Take(10)
                    .ToListAsync(ct);

                sb.AppendLine("## 선택된 프로젝트 없음");
                sb.AppendLine("사용자가 아직 프로젝트를 열지 않았다. 이 회원이 가진 최근 프로젝트는 다음과 같다.");
                if (recent.Count == 0)
                {
                    sb.AppendLine("- (등록된 프로젝트가 없다. 먼저 '기본사항' 화면에서 프로젝트를 만들어야 한다)");
                    steps.Add("프로젝트 없음 — 기본사항에서 신규 생성 필요");
                }
                else
                {
                    foreach (var p in recent)
                    {
                        sb.AppendLine($"- [{p.PrjId}] {p.PrjNm} / {p.PackLevel}차 / 생성일 {p.PrjFcrtDt:yyyy-MM-dd}");
                    }
                    steps.Add($"프로젝트 {recent.Count}건 보유 — 미선택 상태");
                }

                return Finish(sb, steps, new { menuId = curMenuId, prjId = (string?)null, projectCount = recent.Count });
            }

            // ── 2. 프로젝트 기본 정보 (① 프로젝트 생성) ────────────────
            var prj = await db.Project.AsNoTracking().FirstOrDefaultAsync(p => p.PrjId == prjId, ct);
            if (prj == null)
            {
                sb.AppendLine($"## 프로젝트 {prjId}");
                sb.AppendLine("- DB 에서 찾을 수 없다.");
                return Finish(sb, steps, new { menuId = curMenuId, prjId, found = false });
            }

            sb.AppendLine("## ① 프로젝트 생성 (project)");
            sb.AppendLine($"- 프로젝트 번호: {prj.PrjId}");
            sb.AppendLine($"- 프로젝트명(판매제품명): {prj.PrjNm}");
            AppendIf(sb, "생성일", prj.PrjFcrtDt?.ToString("yyyy-MM-dd"));
            AppendIf(sb, "수정번호", prj.PrjRevNo);
            AppendIf(sb, "제조국가", prj.CntryNm);
            AppendIf(sb, "메모", prj.Prjmemo);

            var exportCountries = ExportCountries(prj);
            sb.AppendLine($"- 수출 대상국: {(exportCountries.Count > 0 ? string.Join(", ", exportCountries) : "선택 없음")}");

            var declaredLevels = DeclaredPackLevels(prj);
            sb.AppendLine($"- 진행하기로 선언한 포장차수: {(declaredLevels.Count > 0 ? string.Join(", ", declaredLevels.Select(l => l + "차")) : "선언 없음")}");
            sb.AppendLine($"- 지금 화면이 보고 있는 차수: {(string.IsNullOrWhiteSpace(packLevel) ? prj.PackLevel : packLevel)}차");
            sb.AppendLine();
            steps.Add($"① 프로젝트 생성 — 완료 ({prj.PrjFcrtDt:yyyy-MM-dd}, {prj.PrjNm})");

            // 전 과정을 훑을 차수 목록. 선언된 차수 + 실제 데이터가 있는 차수를 모두 본다.
            var levels = new List<string> { "1", "2", "3" };

            // ── 3. 차수별 기본사항 / 템플릿 (② ③) ──────────────────────
            var details = await db.ProjectDetail.AsNoTracking()
                .Where(d => d.PrjId == prjId)
                .ToListAsync(ct);

            sb.AppendLine("## ② 기본사항 · 디자인 템플릿 (project_detail) — 차수별 전체");
            if (details.Count == 0)
            {
                sb.AppendLine("- 아직 저장된 기본사항이 없다. (기본사항 화면에서 소재·사용환경 등을 저장해야 다음 단계로 갈 수 있다)");
                steps.Add("② 기본사항 — 미작성");
            }
            else
            {
                foreach (var lv in levels)
                {
                    var d = details.FirstOrDefault(x => x.PackLevel == lv);
                    if (d == null) continue;

                    sb.AppendLine($"### {lv}차 ({d.PackLevelNm})");
                    AppendIf(sb, "적용소재", Pair(d.AppliedMaterial, d.AppliedMaterialNm));
                    AppendIf(sb, "사용환경", Pair(d.MatUse, d.MatUseNm));
                    AppendIf(sb, "포장재 구분", Pair(d.MatType, d.MatTypeNm));
                    AppendIf(sb, "소재의 구성", Pair(d.MatForm, d.MatFormNm));
                    AppendIf(sb, "디자인 템플릿 ID", d.PackDsgnTplId);
                    AppendIf(sb, "수출국", Pair(d.PrdExpCntry, d.PrdExpCntryNm));
                    AppendIf(sb, "진행단계(projstatus)", d.Projstatus);
                    AppendIf(sb, "최종 저장", d.Updatedate?.ToString("yyyy-MM-dd HH:mm"));
                    sb.AppendLine();

                    steps.Add($"② 기본사항 {lv}차 — 저장됨 (소재 {d.AppliedMaterialNm ?? d.AppliedMaterial}, 단계 {d.Projstatus})");
                    if (!string.IsNullOrWhiteSpace(d.PackDsgnTplId))
                    {
                        steps.Add($"③ 디자인 템플릿 {lv}차 — 선택됨 ({d.PackDsgnTplId})");
                    }
                }
            }
            sb.AppendLine();

            // ── 4. 차수별 모의평가 (④) ─────────────────────────────────
            var evals = await db.AiPkgEvalInfoBscs.AsNoTracking()
                .Where(e => e.Prjid == prjId)
                .OrderBy(e => e.PackLevel).ThenBy(e => e.AsmtQstId)
                .ToListAsync(ct);

            sb.AppendLine("## ④ 모의평가 결과 (ai_pkg_eval_info_bsc) — 차수별 전체");
            if (evals.Count == 0)
            {
                sb.AppendLine("- 아직 모의평가를 진행하지 않았다.");
                steps.Add("④ 모의평가 — 미진행");
            }
            else
            {
                foreach (var lv in levels)
                {
                    var rows = evals.Where(e => e.PackLevel == lv).ToList();
                    if (rows.Count == 0) continue;

                    var total = rows.Sum(r => int.TryParse(r.Asmtpoint, out var v) ? v : 0);
                    var firstDtm = rows.Min(r => r.Frstevldtm);
                    var lastDtm = rows.Max(r => r.Lastevldtm);

                    sb.AppendLine($"### {lv}차 모의평가 — 총점 {total}점 / 100점, 응답 {rows.Count}문항");
                    sb.AppendLine($"- 최초 평가 {firstDtm:yyyy-MM-dd HH:mm} / 최종 평가 {lastDtm:yyyy-MM-dd HH:mm}");

                    // 대분류(SAFETY / REDUCE 등)별 점수 합계
                    var byArea = rows
                        .GroupBy(r => r.EcoPackLarType ?? "(미분류)")
                        .Select(g => new
                        {
                            Area = g.Key,
                            AreaNm = g.First().EcoPackAreaNm,
                            Score = g.Sum(r => int.TryParse(r.Asmtpoint, out var v) ? v : 0),
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Score);

                    sb.AppendLine("- 영역별 점수:");
                    foreach (var a in byArea)
                    {
                        sb.AppendLine($"  · {a.Area}({a.AreaNm}) : {a.Score}점 / {a.Count}문항");
                    }

                    sb.AppendLine("- 문항별 응답:");
                    foreach (var r in rows.Take(MaxEvalItemsPerLevel))
                    {
                        sb.AppendLine($"  · [{r.EcoPackLarType}] {Trim(r.AsmtQstNm, 120)} → 답: {r.AsmtQstItemNm} (배점 {r.Asmtpoint}/{r.ScoringCriteria})");
                        if (!string.IsNullOrWhiteSpace(r.DsgnRecmImp))
                            sb.AppendLine($"    개선방안: {Trim(r.DsgnRecmImp, 200)}");
                        if (!string.IsNullOrWhiteSpace(r.NatRglAls))
                            sb.AppendLine($"    규제분석: {Trim(r.NatRglAls, 200)}");
                    }
                    if (rows.Count > MaxEvalItemsPerLevel)
                        sb.AppendLine($"  · ...(외 {rows.Count - MaxEvalItemsPerLevel}문항 생략)");
                    sb.AppendLine();

                    steps.Add($"④ 모의평가 {lv}차 — 완료 (총점 {total}점, {rows.Count}문항, {lastDtm:yyyy-MM-dd})");
                }
            }
            sb.AppendLine();

            // ── 5. 차수별 기술문서 TD (⑤) ──────────────────────────────
            sb.AppendLine("## ⑤ TD 기술문서 (primary/secondary/tertiary_td) — 차수별 전체");
            var tdAny = false;
            foreach (var lv in levels)
            {
                var td = await LoadTdAsync(db, prjId, lv, ct);
                if (td == null) continue;
                tdAny = true;
                sb.AppendLine($"### {lv}차 기술문서");
                sb.Append(td.Body);
                sb.AppendLine();
                steps.Add($"⑤ TD {lv}차 — 작성됨 ({td.DocNo}, {td.LastWrt})");
            }
            if (!tdAny)
            {
                sb.AppendLine("- 아직 작성된 기술문서가 없다.");
                steps.Add("⑤ TD 기술문서 — 미작성");
            }
            sb.AppendLine();

            // ── 6. 차수별 적합성 선언서 DOC (⑥) ────────────────────────
            sb.AppendLine("## ⑥ DOC 적합성 선언서 (primary/secondary/tertiary_doc) — 차수별 전체");
            var docAny = false;
            foreach (var lv in levels)
            {
                var doc = await LoadDocAsync(db, prjId, lv, ct);
                if (doc == null) continue;
                docAny = true;
                sb.AppendLine($"### {lv}차 적합성 선언서");
                sb.Append(doc.Body);
                sb.AppendLine();
                steps.Add($"⑥ DOC {lv}차 — 작성됨 ({doc.DocNo}, {doc.LastWrt})");
            }
            if (!docAny)
            {
                sb.AppendLine("- 아직 작성된 적합성 선언서가 없다.");
                steps.Add("⑥ DOC 적합성 선언서 — 미작성");
            }
            sb.AppendLine();

            // ── 7. 진행 요약 ───────────────────────────────────────────
            sb.AppendLine("## 진행 요약(처음부터 지금까지)");
            foreach (var s in steps) sb.AppendLine($"- {s}");

            return Finish(sb, steps, new
            {
                menuId = curMenuId,
                prjId,
                prjNm = prj.PrjNm,
                packLevel,
                declaredLevels,
                exportCountries,
                detailLevels = details.Select(d => d.PackLevel).ToList(),
                evalLevels = evals.Select(e => e.PackLevel).Distinct().ToList(),
                steps
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // 차수별 문서 읽기 — 1/2/3차는 테이블만 다르고 항목 구성은 같다
        // ═══════════════════════════════════════════════════════════════

        private record DocSummary(string Body, string? DocNo, string? LastWrt);

        private static async Task<DocSummary?> LoadTdAsync(AppDbContext db, string prjId, string level, CancellationToken ct)
        {
            var sb = new StringBuilder();
            string? docNo, lastWrt;

            switch (level)
            {
                case "1":
                {
                    var t = await db.PrimaryTd.AsNoTracking().FirstOrDefaultAsync(x => x.PrjId == prjId, ct);
                    if (t == null) return null;
                    docNo = t.DocNo; lastWrt = t.LastWrtDtm?.ToString("yyyy-MM-dd HH:mm");
                    AppendTdCore(sb, t.DocNo, t.RevNo, t.LastWrtDtm, t.PrjfNm, t.BizNm, t.CntryNm,
                        t.PrdExplPhrsCntn, t.PrdIdfyCntn, t.MainMatVal, t.DsgnFeatCntn,
                        t.PrdExtDimSpecVal, t.PrdIntDimSpecVal, t.PrdWtSpecVal, t.PrdMatSpecVal,
                        t.PrdClrSpecVal, t.PrdUseTempSpecVal, t.PrdLoadWtSpecVal, t.PrdDsgnLifeSpecVal,
                        t.MfrDrwUrl, t.MfrPrcsCntn, t.CmplDclCntn,
                        new[] { t.MatNm1, t.MatNm2, t.MatNm3, t.MatNm4 },
                        new[] { t.WtVal1, t.WtVal2, t.WtVal3, t.WtVal4 },
                        new[] { t.WtRt1, t.WtRt2, t.WtRt3, t.WtRt4 },
                        new[] { t.BomCmpnNm1, t.BomCmpnNm2, t.BomCmpnNm3, t.BomCmpnNm4, t.BomCmpnNm5, t.BomCmpnNm6, t.BomCmpnNm7, t.BomCmpnNm8 },
                        new[] { t.BomMatNm1, t.BomMatNm2, t.BomMatNm3, t.BomMatNm4, t.BomMatNm5, t.BomMatNm6, t.BomMatNm7, t.BomMatNm8 },
                        new[] { t.AtchDocUrl1, t.AtchDocUrl2, t.AtchDocUrl3, t.AtchDocUrl4, t.AtchDocUrl5, t.AtchDocUrl6, t.AtchDocUrl7, t.AtchDocUrl8 },
                        t.SocHvyMetMngTestRsltTot, t.ReusePerfRsltVal, t.RcycPathPhrsCntn);
                    break;
                }
                case "2":
                {
                    var t = await db.SecondaryTd.AsNoTracking().FirstOrDefaultAsync(x => x.PrjId == prjId, ct);
                    if (t == null) return null;
                    docNo = t.DocNo; lastWrt = t.LastWrtDtm?.ToString("yyyy-MM-dd HH:mm");
                    AppendTdCore(sb, t.DocNo, t.RevNo, t.LastWrtDtm, t.PrjfNm, t.BizNm, t.CntryNm,
                        t.PrdExplPhrsCntn, t.PrdIdfyCntn, t.MainMatVal, t.DsgnFeatCntn,
                        t.PrdExtDimSpecVal, t.PrdIntDimSpecVal, t.PrdWtSpecVal, t.PrdMatSpecVal,
                        t.PrdClrSpecVal, t.PrdUseTempSpecVal, t.PrdLoadWtSpecVal, t.PrdDsgnLifeSpecVal,
                        t.MfrDrwUrl, t.MfrPrcsCntn, t.CmplDclCntn,
                        new[] { t.MatNm1, t.MatNm2, t.MatNm3, t.MatNm4 },
                        new[] { t.WtVal1, t.WtVal2, t.WtVal3, t.WtVal4 },
                        new[] { t.WtRt1, t.WtRt2, t.WtRt3, t.WtRt4 },
                        new[] { t.BomCmpnNm1, t.BomCmpnNm2, t.BomCmpnNm3, t.BomCmpnNm4, t.BomCmpnNm5, t.BomCmpnNm6, t.BomCmpnNm7, t.BomCmpnNm8 },
                        new[] { t.BomMatNm1, t.BomMatNm2, t.BomMatNm3, t.BomMatNm4, t.BomMatNm5, t.BomMatNm6, t.BomMatNm7, t.BomMatNm8 },
                        new[] { t.AtchDocUrl1, t.AtchDocUrl2, t.AtchDocUrl3, t.AtchDocUrl4, t.AtchDocUrl5, t.AtchDocUrl6, t.AtchDocUrl7, t.AtchDocUrl8 },
                        t.SocHvyMetMngTestRsltTot, t.ReusePerfRsltVal, t.RcycPathPhrsCntn);
                    break;
                }
                case "3":
                {
                    var t = await db.TertiaryTd.AsNoTracking().FirstOrDefaultAsync(x => x.PrjId == prjId, ct);
                    if (t == null) return null;
                    docNo = t.DocNo; lastWrt = t.LastWrtDtm?.ToString("yyyy-MM-dd HH:mm");
                    AppendTdCore(sb, t.DocNo, t.RevNo, t.LastWrtDtm, t.PrjfNm, t.BizNm, t.CntryNm,
                        t.PrdExplPhrsCntn, t.PrdIdfyCntn, t.MainMatVal, t.DsgnFeatCntn,
                        t.PrdExtDimSpecVal, t.PrdIntDimSpecVal, t.PrdWtSpecVal, t.PrdMatSpecVal,
                        t.PrdClrSpecVal, t.PrdUseTempSpecVal, t.PrdLoadWtSpecVal, t.PrdDsgnLifeSpecVal,
                        t.MfrDrwUrl, t.MfrPrcsCntn, t.CmplDclCntn,
                        new[] { t.MatNm1, t.MatNm2, t.MatNm3, t.MatNm4 },
                        new[] { t.WtVal1, t.WtVal2, t.WtVal3, t.WtVal4 },
                        new[] { t.WtRt1, t.WtRt2, t.WtRt3, t.WtRt4 },
                        new[] { t.BomCmpnNm1, t.BomCmpnNm2, t.BomCmpnNm3, t.BomCmpnNm4, t.BomCmpnNm5, t.BomCmpnNm6, t.BomCmpnNm7, t.BomCmpnNm8 },
                        new[] { t.BomMatNm1, t.BomMatNm2, t.BomMatNm3, t.BomMatNm4, t.BomMatNm5, t.BomMatNm6, t.BomMatNm7, t.BomMatNm8 },
                        new[] { t.AtchDocUrl1, t.AtchDocUrl2, t.AtchDocUrl3, t.AtchDocUrl4, t.AtchDocUrl5, t.AtchDocUrl6, t.AtchDocUrl7, t.AtchDocUrl8 },
                        t.SocHvyMetMngTestRsltTot, t.ReusePerfRsltVal, t.RcycPathPhrsCntn);
                    break;
                }
                default:
                    return null;
            }

            return new DocSummary(sb.ToString(), docNo, lastWrt);
        }

        private static async Task<DocSummary?> LoadDocAsync(AppDbContext db, string prjId, string level, CancellationToken ct)
        {
            var sb = new StringBuilder();
            string? docNo, lastWrt;

            switch (level)
            {
                case "1":
                {
                    var d = await db.PrimaryDoc.AsNoTracking().FirstOrDefaultAsync(x => x.PrjId == prjId, ct);
                    if (d == null) return null;
                    docNo = d.Pkg1DocId; lastWrt = d.LastWrtDt?.ToString("yyyy-MM-dd");
                    AppendDocCore(sb, d.Pkg1DocId, d.RevNo, d.LastWrtDt, d.PrjfNm, d.BizNm, d.CntryNm, d.DsgnTypeNm,
                        d.DocPhrsCntn, d.ReuseReqCmplCntn, d.RcycReqCmplCntn1, d.RcycMainFeatCntn, d.RcycReqCmplCntn2,
                        d.SoCHvyMetLmtCmplCntn1, d.SoCHvyMetLmtCmplCntn2, d.MatInfoTotWtVal, d.MatInfoCntn,
                        d.ApplRuleStdCntn, d.LastDclCntn,
                        new[] { d.Mat1, d.Mat2, d.Mat3 },
                        new[] { d.Compltem1, d.Compltem2, d.Compltem3 },
                        new[] { d.EvdDocNm1, d.EvdDocNm2, d.EvdDocNm3, d.EvdDocNm4, d.EvdDocNm5, d.EvdDocNm6, d.EvdDocNm7, d.EvdDocNm8 });
                    break;
                }
                case "2":
                {
                    var d = await db.SecondaryDoc.AsNoTracking().FirstOrDefaultAsync(x => x.PrjId == prjId, ct);
                    if (d == null) return null;
                    docNo = d.Pkg2DocId; lastWrt = d.LastWrtDt?.ToString("yyyy-MM-dd");
                    AppendDocCore(sb, d.Pkg2DocId, d.RevNo, d.LastWrtDt, d.PrjfNm, d.BizNm, d.CntryNm, d.DsgnTypeNm,
                        d.DocPhrsCntn, d.ReuseReqCmplCntn, d.RcycReqCmplCntn1, d.RcycMainFeatCntn, d.RcycReqCmplCntn2,
                        d.SoCHvyMetLmtCmplCntn1, d.SoCHvyMetLmtCmplCntn2, d.MatInfoTotWtVal, d.MatInfoCntn,
                        d.ApplRuleStdCntn, d.LastDclCntn,
                        new[] { d.Mat1, d.Mat2, d.Mat3 },
                        new[] { d.Compltem1, d.Compltem2, d.Compltem3 },
                        new[] { d.EvdDocNm1, d.EvdDocNm2, d.EvdDocNm3, d.EvdDocNm4, d.EvdDocNm5, d.EvdDocNm6, d.EvdDocNm7, d.EvdDocNm8 });
                    break;
                }
                case "3":
                {
                    var d = await db.TertiaryDoc.AsNoTracking().FirstOrDefaultAsync(x => x.PrjId == prjId, ct);
                    if (d == null) return null;
                    docNo = d.Pkg3DocId; lastWrt = d.LastWrtDt?.ToString("yyyy-MM-dd");
                    AppendDocCore(sb, d.Pkg3DocId, d.RevNo, d.LastWrtDt, d.PrjfNm, d.BizNm, d.CntryNm, d.DsgnTypeNm,
                        d.DocPhrsCntn, d.ReuseReqCmplCntn, d.RcycReqCmplCntn1, d.RcycMainFeatCntn, d.RcycReqCmplCntn2,
                        d.SoCHvyMetLmtCmplCntn1, d.SoCHvyMetLmtCmplCntn2, d.MatInfoTotWtVal, d.MatInfoCntn,
                        d.ApplRuleStdCntn, d.LastDclCntn,
                        new[] { d.Mat1, d.Mat2, d.Mat3 },
                        new[] { d.Compltem1, d.Compltem2, d.Compltem3 },
                        new[] { d.EvdDocNm1, d.EvdDocNm2, d.EvdDocNm3, d.EvdDocNm4, d.EvdDocNm5, d.EvdDocNm6, d.EvdDocNm7, d.EvdDocNm8 });
                    break;
                }
                default:
                    return null;
            }

            return new DocSummary(sb.ToString(), docNo, lastWrt);
        }

        /// <summary>기술문서(TD) 한 건을 사람이 읽는 줄글로 옮긴다. 1/2/3차 공통.</summary>
        private static void AppendTdCore(
            StringBuilder sb,
            string? docNo, string? revNo, DateTime? lastWrtDtm, string? prjfNm, string? bizNm, string? cntryNm,
            string? prdExplPhrs, string? prdIdfy, string? mainMat, string? dsgnFeat,
            string? extDim, string? intDim, string? wtSpec, string? matSpec,
            string? clrSpec, string? useTemp, string? loadWt, string? dsgnLife,
            string? mfrDrwUrl, string? mfrPrcs, string? cmplDcl,
            string?[] matNms, decimal?[] wtVals, decimal?[] wtRts,
            string?[] bomCmpnNms, string?[] bomMatNms, string?[] atchDocUrls,
            string? hvyMetTot, string? reuseRslt, string? rcycPath)
        {
            AppendIf(sb, "문서번호", docNo);
            AppendIf(sb, "개정번호", revNo);
            AppendIf(sb, "최종 작성", lastWrtDtm?.ToString("yyyy-MM-dd HH:mm"));
            AppendIf(sb, "제품명", prjfNm);
            AppendIf(sb, "제조사", bizNm);
            AppendIf(sb, "제조국", cntryNm);
            AppendIf(sb, "제품 설명", Trim(prdExplPhrs, 400));
            AppendIf(sb, "제품 식별", Trim(prdIdfy, 200));
            AppendIf(sb, "주요 소재", mainMat);
            AppendIf(sb, "디자인 특징", Trim(dsgnFeat, 300));
            AppendIf(sb, "외부치수", extDim);
            AppendIf(sb, "내부치수", intDim);
            AppendIf(sb, "중량", wtSpec);
            AppendIf(sb, "재질", matSpec);
            AppendIf(sb, "색상", clrSpec);
            AppendIf(sb, "사용온도", useTemp);
            AppendIf(sb, "적재하중", loadWt);
            AppendIf(sb, "설계수명", dsgnLife);
            AppendIf(sb, "제조도면", string.IsNullOrWhiteSpace(mfrDrwUrl) ? null : "등록됨");
            AppendIf(sb, "제조공정", Trim(mfrPrcs, 300));
            AppendIf(sb, "재활용 경로", Trim(rcycPath, 200));
            AppendIf(sb, "재사용 성능 결과", Trim(reuseRslt, 200));
            AppendIf(sb, "중금속 시험결과 합계", hvyMetTot);
            AppendIf(sb, "적합성 선언 문구", Trim(cmplDcl, 300));

            // 소재 구성비
            var matLines = new List<string>();
            for (var i = 0; i < matNms.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(matNms[i])) continue;
                matLines.Add($"{matNms[i]} {wtVals[i]}g ({wtRts[i]}%)");
            }
            if (matLines.Count > 0) sb.AppendLine($"- 소재 구성: {string.Join(" / ", matLines)}");

            // BOM
            var bomLines = new List<string>();
            for (var i = 0; i < bomCmpnNms.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(bomCmpnNms[i])) continue;
                bomLines.Add($"{bomCmpnNms[i]}({bomMatNms[i]})");
            }
            if (bomLines.Count > 0) sb.AppendLine($"- BOM 구성품: {string.Join(", ", bomLines)}");

            var atchCnt = atchDocUrls.Count(u => !string.IsNullOrWhiteSpace(u));
            if (atchCnt > 0) sb.AppendLine($"- 첨부 근거문서: {atchCnt}건");
        }

        /// <summary>적합성 선언서(DOC) 한 건을 사람이 읽는 줄글로 옮긴다. 1/2/3차 공통.</summary>
        private static void AppendDocCore(
            StringBuilder sb,
            string? docId, string? revNo, DateOnly? lastWrtDt, string? prjfNm, string? bizNm, string? cntryNm, string? dsgnTypeNm,
            string? docPhrs, string? reuseReq, string? rcycReq1, string? rcycMainFeat, string? rcycReq2,
            string? hvyMet1, string? hvyMet2, string? totWt, string? matInfo,
            string? applRule, string? lastDcl,
            string?[] mats, string?[] complItems, string?[] evdDocNms)
        {
            AppendIf(sb, "문서번호", docId);
            AppendIf(sb, "개정번호", revNo);
            AppendIf(sb, "최종 작성", lastWrtDt?.ToString("yyyy-MM-dd"));
            AppendIf(sb, "제품명", prjfNm);
            AppendIf(sb, "제조사", bizNm);
            AppendIf(sb, "제조국", cntryNm);
            AppendIf(sb, "디자인 유형", dsgnTypeNm);
            AppendIf(sb, "선언 문구", Trim(docPhrs, 300));
            AppendIf(sb, "재사용 요구 충족", Trim(reuseReq, 200));
            AppendIf(sb, "재활용 요구 충족", Trim(rcycReq1, 200));
            AppendIf(sb, "재활용 주요 특징", Trim(rcycMainFeat, 200));
            AppendIf(sb, "재활용 요구 충족2", Trim(rcycReq2, 200));
            AppendIf(sb, "SoC/중금속 제한 충족", Trim(hvyMet1, 200));
            AppendIf(sb, "SoC/중금속 제한 충족2", Trim(hvyMet2, 200));
            AppendIf(sb, "소재 총중량", totWt);
            AppendIf(sb, "소재 정보", Trim(matInfo, 300));
            AppendIf(sb, "적용 규격/기준", Trim(applRule, 300));
            AppendIf(sb, "최종 선언", Trim(lastDcl, 300));

            var matList = mats.Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
            if (matList.Count > 0) sb.AppendLine($"- 소재: {string.Join(", ", matList)}");

            var complList = complItems.Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
            if (complList.Count > 0) sb.AppendLine($"- 적합 항목: {string.Join(", ", complList)}");

            var evdList = evdDocNms.Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
            if (evdList.Count > 0) sb.AppendLine($"- 근거문서: {string.Join(", ", evdList)}");
        }

        // ═══════════════════════════════════════════════════════════════
        // 작은 도구들
        // ═══════════════════════════════════════════════════════════════

        private static ProjectWorkContext Finish(StringBuilder sb, List<string> steps, object snapshot)
        {
            var text = sb.ToString();
            if (text.Length > MaxTextLength)
            {
                text = text[..MaxTextLength] + "\n\n...(작업 상태가 길어 이후 내용은 생략함)";
            }
            return new ProjectWorkContext { Text = text, Steps = steps, Snapshot = snapshot };
        }

        private static void AppendIf(StringBuilder sb, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value)) sb.AppendLine($"- {label}: {value.Trim()}");
        }

        /// <summary>코드와 이름을 "이름(코드)" 형태로 묶는다. 둘 다 없으면 null.</summary>
        private static string? Pair(string? code, string? name)
        {
            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name)) return null;
            if (string.IsNullOrWhiteSpace(name)) return code;
            if (string.IsNullOrWhiteSpace(code)) return name;
            return $"{name}({code})";
        }

        private static string? Trim(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var v = value.Trim();
            return v.Length <= max ? v : v[..max] + "…";
        }

        /// <summary>project 의 수출국가 여부 컬럼 8개를 국가명 목록으로 편다.</summary>
        private static List<string> ExportCountries(Project p)
        {
            var map = new (string? Flag, string Name)[]
            {
                (p.PrdExpCntryNm1, "미국(USA)"),
                (p.PrdExpCntryNm2, "유럽(EU)"),
                (p.PrdExpCntryNm3, "중국(CHN)"),
                (p.PrdExpCntryNm4, "베트남(VNM)"),
                (p.PrdExpCntryNm5, "인도네시아(IDN)"),
                (p.PrdExpCntryNm6, "일본(JPN)"),
                (p.PrdExpCntryNm7, "호주(AUS)"),
                (p.PrdExpCntryNm8, "대한민국(KOR)"),
            };
            return map.Where(x => IsYes(x.Flag)).Select(x => x.Name).ToList();
        }

        /// <summary>project 의 포장차수 여부 컬럼 3개를 차수 목록으로 편다.</summary>
        private static List<string> DeclaredPackLevels(Project p)
        {
            var levels = new List<string>();
            if (IsYes(p.PrdPkgSeq1)) levels.Add("1");
            if (IsYes(p.PrdPkgSeq2)) levels.Add("2");
            if (IsYes(p.PrdPkgSeq3)) levels.Add("3");
            return levels;
        }

        /// <summary>화면이 'Y' / 'true' / '1' 등 여러 형태로 넣어 두어, 넉넉하게 받아 준다.</summary>
        private static bool IsYes(string? flag)
        {
            if (string.IsNullOrWhiteSpace(flag)) return false;
            var v = flag.Trim();
            return v.Equals("Y", StringComparison.OrdinalIgnoreCase)
                || v.Equals("true", StringComparison.OrdinalIgnoreCase)
                || v == "1";
        }
    }
}
