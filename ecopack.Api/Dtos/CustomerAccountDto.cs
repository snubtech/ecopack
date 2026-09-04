/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - CustomerAccountDto (회원가입 / 회원정보 자료 묶음)
 * ==============================================================================
 * 
 * 1. 구성
 *    - CustomerJoinDto          : 회원가입 입력값. customer 테이블 컬럼과 1:1로 맞췄습니다.
 *    - CustomerProfileUpdateDto : 회원정보 수정 입력값. 비밀번호를 바꿀 때만 현재 비밀번호를 함께 받습니다.
 *    - CustomerProfileDto       : 로그인·회원정보 조회 응답.
 * 
 * 2. 알아둘 점
 *    - 가입일·정보변경일·최종로그인일시는 화면에서 받지 않고 서버가 기록합니다.
 *    - 여기 담긴 회사·담당자 정보가 기술문서와 적합성 선언서의
 *      제조사·제조국·회사명·담당자·직책·이메일·전화번호를 자동으로 채우는 데 쓰입니다.
 * ==============================================================================
 */
namespace ecopack.Api.Dtos
{
    /// <summary>
    /// 회원가입 입력값. customer 테이블 컬럼과 1:1로 대응한다.
    /// (joinDt / prvcChgDt / lastLgnDtm 은 서버가 기록한다)
    /// </summary>
    public class CustomerJoinDto
    {
        public string RepCustId { get; set; } = string.Empty;   // 아이디
        public string RepCustPwd { get; set; } = string.Empty;  // 비밀번호

        public string? BizNo { get; set; }        // 법인등록번호/개인사업자번호
        public string? CustTypeNm { get; set; }   // 법인/개인사업 구분
        public string? BizNm { get; set; }        // 법인/개인사업 명 (회사명·제조사)
        public string? RepNm { get; set; }        // 대표자명 (담당자)
        public string? RoleNm { get; set; }       // 직책
        public string? IndstNm { get; set; }      // 업종
        public string? CntryNm { get; set; }      // 국가 (제조국)
        public string? AddrCd { get; set; }       // 우편번호
        public string? DtlAddr1 { get; set; }     // 상세주소(도로명)
        public string? DtlAddr2 { get; set; }     // 상세주소(동/호수)
        public string? EmlAddr { get; set; }      // 이메일
        public string? RepTelNo { get; set; }     // 대표번호
        public string? MblTelNo { get; set; }     // 휴대폰번호
    }

    /// <summary>회원정보 수정 입력값 (비밀번호는 바꿀 때만 채운다)</summary>
    public class CustomerProfileUpdateDto : CustomerJoinDto
    {
        /// <summary>비밀번호를 바꾸려면 현재 비밀번호를 함께 보낸다</summary>
        public string? CurrentPwd { get; set; }
    }

    /// <summary>
    /// 로그인 사용자 / 회원정보 조회 응답.
    /// 기술문서·적합성선언서 화면이 제조사·담당자 정보를 자동으로 채우는 데 사용한다.
    /// </summary>
    public class CustomerProfileDto
    {
        public string? RepCustId { get; set; }
        public string? BizNo { get; set; }
        public string? CustTypeNm { get; set; }
        public string? BizNm { get; set; }
        public string? RepNm { get; set; }
        public string? RoleNm { get; set; }
        public string? IndstNm { get; set; }
        public string? CntryNm { get; set; }
        public string? AddrCd { get; set; }
        public string? DtlAddr1 { get; set; }
        public string? DtlAddr2 { get; set; }
        public string? EmlAddr { get; set; }
        public string? RepTelNo { get; set; }
        public string? MblTelNo { get; set; }
        public DateTime? JoinDt { get; set; }
        public DateTime? LastLgnDtm { get; set; }
    }
}
