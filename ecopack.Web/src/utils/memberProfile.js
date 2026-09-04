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
