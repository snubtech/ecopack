namespace ecopack.Api.Support
{
    /// <summary>
    /// 비밀번호 규칙. 회원가입과 비밀번호 변경 때 검사한다(이미 쓰고 있는 비밀번호에는 적용하지 않는다).
    /// 화면(JoinPage / ProfilePage 의 utils/passwordPolicy.js)과 같은 규칙이어야 한다.
    ///
    /// 규칙: 영문 대문자·소문자·숫자·특수문자 중
    ///       3종류 이상이면 8자 이상, 2종류면 10자 이상. 최대 64자. 아이디를 포함할 수 없다.
    /// </summary>
    public static class PasswordPolicy
    {
        public const int MaxLength = 64;

        public const string RuleText =
            "비밀번호는 영문 대문자·소문자·숫자·특수문자 중 3종류 이상이면 8자 이상, 2종류면 10자 이상이어야 합니다.";

        /// <summary>규칙에 맞으면 null, 맞지 않으면 화면에 보여 줄 안내 문구.</summary>
        public static string? Check(string? password, string? repCustId)
        {
            if (string.IsNullOrEmpty(password))
            {
                return "비밀번호를 입력해 주세요.";
            }
            if (password.Length > MaxLength)
            {
                return $"비밀번호는 {MaxLength}자를 넘을 수 없습니다.";
            }
            if (password.Any(char.IsWhiteSpace))
            {
                return "비밀번호에는 공백을 넣을 수 없습니다.";
            }

            var kinds = 0;
            if (password.Any(char.IsAsciiLetterUpper)) kinds++;
            if (password.Any(char.IsAsciiLetterLower)) kinds++;
            if (password.Any(char.IsAsciiDigit)) kinds++;
            if (password.Any(c => !char.IsAsciiLetterOrDigit(c))) kinds++;

            var longEnough = (kinds >= 3 && password.Length >= 8) || (kinds >= 2 && password.Length >= 10);
            if (!longEnough)
            {
                return RuleText;
            }

            if (!string.IsNullOrWhiteSpace(repCustId) && repCustId.Length >= 4
                && password.Contains(repCustId, StringComparison.OrdinalIgnoreCase))
            {
                return "비밀번호에 아이디를 넣을 수 없습니다.";
            }

            return null;
        }
    }
}
