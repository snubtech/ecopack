/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - passwordPolicy (입력값 규칙: 비밀번호·아이디·이메일·전화번호)
 * ==============================================================================
 *
 * 1. 쓰임새
 *    - 회원가입(JoinPage)과 회원정보 수정(ProfilePage)이 서버로 보내기 전에 입력값을 미리 확인합니다.
 *    - 서버도 같은 규칙으로 다시 검사합니다(서버: PasswordPolicy / InputRules / AuthController).
 *      화면 검사는 사용자가 바로 고칠 수 있게 돕는 용도이고, 실제로 막는 곳은 서버입니다.
 *      규칙을 바꿀 때는 양쪽을 함께 고쳐야 합니다.
 *
 * 2. 비밀번호 규칙
 *    - 영문 대문자·소문자·숫자·특수문자 중 3종류 이상이면 8자 이상, 2종류면 10자 이상
 *    - 최대 64자, 공백 불가, 아이디를 포함할 수 없음
 * ==============================================================================
 */

export const PASSWORD_MAX_LENGTH = 64;

export const PASSWORD_RULE_TEXT =
    '영문 대문자·소문자·숫자·특수문자 중 3종류 이상이면 8자 이상, 2종류면 10자 이상';

/** 규칙에 맞으면 빈 문자열, 아니면 안내 문구를 돌려준다. */
export function checkPassword(password, repCustId) {
    if (!password) return '비밀번호를 입력해 주세요.';
    if (password.length > PASSWORD_MAX_LENGTH) return `비밀번호는 ${PASSWORD_MAX_LENGTH}자를 넘을 수 없습니다.`;
    if (/\s/.test(password)) return '비밀번호에는 공백을 넣을 수 없습니다.';

    const kinds = [/[A-Z]/, /[a-z]/, /[0-9]/, /[^A-Za-z0-9]/].filter((re) => re.test(password)).length;
    const longEnough = (kinds >= 3 && password.length >= 8) || (kinds >= 2 && password.length >= 10);
    if (!longEnough) return `비밀번호는 ${PASSWORD_RULE_TEXT}이어야 합니다.`;

    const id = (repCustId || '').trim();
    if (id.length >= 4 && password.toLowerCase().includes(id.toLowerCase())) {
        return '비밀번호에 아이디를 넣을 수 없습니다.';
    }
    return '';
}

/** 새로 만드는 아이디: 영문·숫자와 . _ - @ 만, 4~20자 (프로젝트의 작성자 아이디 칸이 20자다) */
export function checkNewId(repCustId) {
    return /^[A-Za-z0-9._@-]{4,20}$/.test(repCustId || '')
        ? ''
        : '아이디는 영문·숫자와 . _ - @ 만 사용해 4~20자로 입력해 주세요.';
}

/** 이메일·전화번호 형식. 비어 있으면 통과한다(필수 항목이 아니다). */
export function checkContact(form) {
    if (form.emlAddr && !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(form.emlAddr.trim())) {
        return '이메일 형식이 올바르지 않습니다.';
    }
    for (const key of ['repTelNo', 'mblTelNo']) {
        if (form[key] && !/^[0-9+\-(). ]+$/.test(form[key].trim())) {
            return '전화번호는 숫자와 + - ( ) . 만 입력할 수 있습니다.';
        }
    }
    return '';
}
