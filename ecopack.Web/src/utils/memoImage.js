/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - memoImage (공정도 설명 이미지 처리)
 * ==============================================================================
 *
 * 1. 쓰임새
 *    - 공정도의 memoImg 는 참조 시스템에서 받아온 HTML(<img src="data:image/..."> 포함)입니다.
 *    - 이 HTML 을 화면에 그대로 넣으면(dangerouslySetInnerHTML) 그 안의 스크립트나
 *      이벤트 속성까지 실행될 수 있습니다(HTML 주입).
 *    - 그래서 HTML 은 버리고 이미지 주소(data URI)만 골라내, 화면은 <img> 로만 그립니다.
 *
 * 2. getMemoImageUris(proc)
 *    - 서버가 골라 준 목록(memoImageUris)이 있으면 그것을 쓰고,
 *      없으면(예전에 저장된 리포트 등) memoImg 에서 직접 골라냅니다.
 *    - 서버의 ImageDataUri.ExtractAll 과 같은 규칙입니다.
 * ==============================================================================
 */

// 이미지 data URI 만 찾는다. 형식은 png/jpeg/gif/webp/bmp 로 한정한다(svg 는 스크립트를 담을 수 있어 제외).
const DATA_URI_PATTERN = /data:image\/(png|jpe?g|gif|webp|bmp);base64,([A-Za-z0-9+/=\s]+?)(?=["')])/gi;

/** HTML 문자열에서 이미지 data URI 만 골라낸다. */
export function extractImageUris(html) {
    if (typeof html !== 'string' || html.trim() === '') return [];

    const uris = [];
    for (const m of html.matchAll(DATA_URI_PATTERN)) {
        const data = m[2].replace(/\s+/g, '');
        if (!data) continue;
        const type = m[1].toLowerCase() === 'jpg' ? 'jpeg' : m[1].toLowerCase();
        uris.push(`data:image/${type};base64,${data}`);
    }
    return uris;
}

/** 공정도 한 건에서 화면에 그릴 이미지 주소 목록을 꺼낸다. */
export function getMemoImageUris(proc) {
    if (Array.isArray(proc?.memoImageUris) && proc.memoImageUris.length > 0) {
        // 서버가 준 값이라도 형식을 한 번 더 확인한다
        return proc.memoImageUris.flatMap((u) => extractImageUris(`"${u}"`));
    }
    return extractImageUris(proc?.memoImg);
}
