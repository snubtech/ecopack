/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - memberProfile 유틸 (회원정보 자동 채움)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 회원가입 때 입력한 회사·담당자 정보를 세션에서 꺼내
 *      기술문서와 적합성 선언서의 제조사·제조국·회사명·담당자·직책·이메일·전화번호를 채웁니다.
 * 
 * 2. 채우는 규칙 (fillFromMember)
 *    - 비어 있는 항목만 채우고, 이미 입력된 값은 건드리지 않습니다.
 *    - 화면마다 다른 대응표(MEMBER_FIELDS)를 넘겨 쓰기 때문에
 *      같은 회원정보를 문서별로 다른 항목에 넣을 수 있습니다.
 * ==============================================================================
 */
/**
 * 로그인한 회원의 회사·담당자 정보를 꺼내온다.
 * 회원가입(customer 테이블) 때 입력한 값이며, 기술문서/적합성선언서 화면이
 * 제조사·제조국·회사명·담당자·직책·이메일·전화번호를 자동으로 채우는 데 쓴다.
 *
 * @returns {object|null} customer 프로필 (없으면 null)
 */
export function getMemberProfile() {
    try {
        const raw = sessionStorage.getItem('prjuserid');
        if (!raw) return null;
        const parsed = JSON.parse(raw);
        return parsed?.profile ?? null;
    } catch (e) {
        console.error('회원정보를 불러오는 중 오류:', e);
        return null;
    }
}

/**
 * 빈 항목만 회원정보로 채운다. 이미 값이 있는 항목은 건드리지 않는다.
 *
 * @param {object} form    현재 폼 상태
 * @param {object} mapping { 폼키: 프로필키 } 대응표
 * @returns {object} 채워진 새 폼
 */
export function fillFromMember(form, mapping) {
    const profile = getMemberProfile();
    if (!profile) return form;

    const next = { ...form };
    Object.entries(mapping).forEach(([formKey, profileKey]) => {
        if (!next[formKey]) {
            const v = profile[profileKey];
            if (v) next[formKey] = String(v);
        }
    });
    return next;
}
