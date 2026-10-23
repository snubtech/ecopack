using System.ComponentModel.DataAnnotations;
namespace ecopack.Api.Dtos
{
    public class LoginDto
    {
        [StringLength(ecopack.Api.Support.InputRules.CustomerIdMax, ErrorMessage = "아이디는 50자를 넘을 수 없습니다.")]
        public string RepCustId { get; set; } = string.Empty;
        [StringLength(ecopack.Api.Support.InputRules.PasswordInputMax, ErrorMessage = "비밀번호가 너무 깁니다.")]
        public string RepCustPwd { get; set; } = string.Empty;
    }
}