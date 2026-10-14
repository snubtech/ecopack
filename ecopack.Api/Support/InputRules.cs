namespace ecopack.Api.Support
{
    /// <summary>
    /// 요청 DTO 의 검증 속성([RegularExpression] 등)이 함께 쓰는 형식 규칙.
    /// 규칙을 바꿀 때 여기 한 곳만 고치면 된다.
    ///
    /// 길이는 여기서 다루지 않는다. 글자 수 제한은 실제 DB 컬럼 길이를 기준으로
    /// 저장 직전에 한꺼번에 검사한다(AppDbContext 의 "저장 전 길이 검사").
    /// 값이 비어 있는 경우는 모두 통과한다(필수 여부는 [Required] 로 따로 정한다).
    /// </summary>
    public static class InputRules
    {
        /// <summary>프로젝트 번호·대화 세션 ID: 영문·숫자·밑줄·하이픈, 최대 50자</summary>
        public const string Id = @"^[A-Za-z0-9_-]{1,50}$";
        public const string IdMessage = "ID 형식이 올바르지 않습니다.";

        /// <summary>포장차수: 1, 2, 3 중 하나</summary>
        public const string PackLevel = @"^[123]$";
        public const string PackLevelMessage = "포장차수는 1, 2, 3 중 하나여야 합니다.";

        /// <summary>여부 값: Y 또는 N</summary>
        public const string YesNo = @"^[YN]$";
        public const string YesNoMessage = "Y 또는 N 이어야 합니다.";

        /// <summary>이메일: 아이디@도메인.최상위 형태</summary>
        public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const string EmailMessage = "이메일 형식이 올바르지 않습니다.";

        /// <summary>전화번호: 숫자와 + - ( ) . 공백</summary>
        public const string Phone = @"^[0-9+\-(). ]+$";
        public const string PhoneMessage = "전화번호는 숫자와 + - ( ) . 만 입력할 수 있습니다.";

        /// <summary>고객 ID(아이디) 최대 길이. customer.repcustid 컬럼 길이와 같다.</summary>
        public const int CustomerIdMax = 50;

        /// <summary>
        /// 비밀번호 입력 최대 길이. 아주 긴 값을 보내 해시 계산으로 서버를 느리게 만드는 것을 막는다.
        /// (새 비밀번호의 규칙은 PasswordPolicy 가 따로 검사한다)
        /// </summary>
        public const int PasswordInputMax = 128;
    }
}
