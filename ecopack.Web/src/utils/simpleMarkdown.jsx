/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - simpleMarkdown (AI 답변 서식 표시)
 * ==============================================================================
 *
 * 1. 하는 일
 *    - AI 답변에 섞여 오는 간단한 마크다운을 화면 요소로 바꿔 줍니다.
 *      제목(#), 굵게(**), 코드(`), 글머리표(-), 번호목록(1.), 표(|) 를 다룹니다.
 *
 * 2. 왜 라이브러리를 안 쓰나
 *    - 채팅 답변에 쓰이는 서식은 위 몇 가지뿐이라, 이 정도면 충분합니다.
 *      패키지를 하나 더 얹지 않으려고 짧게 직접 처리합니다.
 *
 * 3. 안전
 *    - HTML 을 그대로 넣지 않고(dangerouslySetInnerHTML 미사용) 리액트 요소로만 만듭니다.
 *      그래서 답변에 태그가 섞여 와도 화면이 망가지거나 실행되지 않습니다.
 * ==============================================================================
 */

/** 한 줄 안의 **굵게** 와 `코드` 를 처리한다. */
function renderInline(text, keyPrefix) {
    const parts = [];
    // **굵게** 또는 `코드` 를 찾는다
    const regex = /(\*\*[^*]+\*\*|`[^`]+`)/g;
    let last = 0;
    let match;
    let i = 0;

    while ((match = regex.exec(text)) !== null) {
        if (match.index > last) {
            parts.push(text.slice(last, match.index));
        }
        const token = match[0];
        if (token.startsWith('**')) {
            parts.push(<strong key={`${keyPrefix}-b${i}`}>{token.slice(2, -2)}</strong>);
        } else {
            parts.push(
                <code key={`${keyPrefix}-c${i}`} className="chat-inline-code">
                    {token.slice(1, -1)}
                </code>
            );
        }
        last = match.index + token.length;
        i += 1;
    }

    if (last < text.length) parts.push(text.slice(last));
    return parts.length > 0 ? parts : text;
}

/** 마크다운 표 한 덩어리(| 로 시작하는 연속된 줄)를 <table> 로 만든다. */
function renderTable(rows, key) {
    // 두 번째 줄이 |---|---| 형태의 구분선이면 걷어낸다
    const body = rows.filter((r) => !/^\s*\|?[\s:|-]+\|?\s*$/.test(r));
    if (body.length === 0) return null;

    const toCells = (row) =>
        row.replace(/^\s*\|/, '').replace(/\|\s*$/, '').split('|').map((c) => c.trim());

    const header = toCells(body[0]);
    const dataRows = body.slice(1).map(toCells);

    return (
        <div className="chat-table-wrap" key={key}>
            <table className="chat-table">
                <thead>
                    <tr>
                        {header.map((h, i) => (
                            <th key={i}>{renderInline(h, `${key}-h${i}`)}</th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {dataRows.map((cells, r) => (
                        <tr key={r}>
                            {cells.map((c, i) => (
                                <td key={i}>{renderInline(c, `${key}-r${r}c${i}`)}</td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

/**
 * 마크다운 문자열을 리액트 요소 배열로 바꾼다.
 * @param {string} text AI 답변 원문
 */
export function renderMarkdown(text) {
    if (!text) return null;

    const lines = String(text).split('\n');
    const out = [];
    let i = 0;
    let key = 0;

    while (i < lines.length) {
        const line = lines[i];

        // 빈 줄 → 문단 사이 여백
        if (line.trim() === '') {
            i += 1;
            continue;
        }

        // 표: | 로 시작하는 줄이 이어지는 동안 묶는다
        if (/^\s*\|/.test(line)) {
            const rows = [];
            while (i < lines.length && /^\s*\|/.test(lines[i])) {
                rows.push(lines[i]);
                i += 1;
            }
            const table = renderTable(rows, `t${key++}`);
            if (table) out.push(table);
            continue;
        }

        // 제목: #, ##, ###
        const heading = line.match(/^(#{1,3})\s+(.*)$/);
        if (heading) {
            const level = heading[1].length;
            out.push(
                <p key={`h${key++}`} className={`chat-heading chat-heading-${level}`}>
                    {renderInline(heading[2], `h${key}`)}
                </p>
            );
            i += 1;
            continue;
        }

        // 글머리표: -, *, •
        if (/^\s*[-*•]\s+/.test(line)) {
            const items = [];
            while (i < lines.length && /^\s*[-*•]\s+/.test(lines[i])) {
                items.push(lines[i].replace(/^\s*[-*•]\s+/, ''));
                i += 1;
            }
            out.push(
                <ul key={`u${key++}`} className="chat-list">
                    {items.map((it, n) => (
                        <li key={n}>{renderInline(it, `u${key}-${n}`)}</li>
                    ))}
                </ul>
            );
            continue;
        }

        // 번호목록: 1. 2. 3.
        if (/^\s*\d+\.\s+/.test(line)) {
            const items = [];
            while (i < lines.length && /^\s*\d+\.\s+/.test(lines[i])) {
                items.push(lines[i].replace(/^\s*\d+\.\s+/, ''));
                i += 1;
            }
            out.push(
                <ol key={`o${key++}`} className="chat-list">
                    {items.map((it, n) => (
                        <li key={n}>{renderInline(it, `o${key}-${n}`)}</li>
                    ))}
                </ol>
            );
            continue;
        }

        // 그 밖의 줄은 문단으로. 이어지는 일반 줄은 한 문단으로 묶는다.
        const para = [];
        while (
            i < lines.length &&
            lines[i].trim() !== '' &&
            !/^\s*\|/.test(lines[i]) &&
            !/^#{1,3}\s+/.test(lines[i]) &&
            !/^\s*[-*•]\s+/.test(lines[i]) &&
            !/^\s*\d+\.\s+/.test(lines[i])
        ) {
            para.push(lines[i]);
            i += 1;
        }
        out.push(
            <p key={`p${key++}`} className="chat-paragraph">
                {renderInline(para.join('\n'), `p${key}`)}
            </p>
        );
    }

    return out;
}
