namespace ecopack.Api.Support
{
    /// <summary>요청 횟수 제한 정책 이름. Program.cs 의 AddRateLimiter 와 컨트롤러의 [EnableRateLimiting] 이 함께 쓴다.</summary>
    public static class RateLimitPolicies
    {
        /// <summary>로그인·회원가입·아이디 중복확인</summary>
        public const string Auth = "auth";

        /// <summary>AI 채팅 질문</summary>
        public const string AiChat = "ai-chat";

        /// <summary>AI 이미지 생성·3D 변환 요청</summary>
        public const string AiImage = "ai-image";
    }
}
