using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // 테이블 이름 매핑 및 기본키(PK) 설정
            builder.ToTable("customer"); // DB에 실제 생성된 테이블명 (필요시 소문자/대문자 확인)
            builder.HasKey(e => e.RepCustId); // 대표자 ID를 기본키로 지정

            // 컬럼 매핑 및 제약조건 설정
            builder.Property(e => e.RepCustId).HasMaxLength(50).HasColumnName("repcustid");
            builder.Property(e => e.RepCustPwd).HasMaxLength(100).HasColumnName("repcustpwd");
            builder.Property(e => e.BizNo).HasMaxLength(20).HasColumnName("bizno");
            builder.Property(e => e.CustTypeNm).HasMaxLength(50).HasColumnName("custtypenm");
            builder.Property(e => e.BizNm).HasMaxLength(100).HasColumnName("biznm");
            builder.Property(e => e.RepNm).HasMaxLength(50).HasColumnName("repnm");
            builder.Property(e => e.EmlAddr).HasMaxLength(100).HasColumnName("emladdr");
            builder.Property(e => e.MblTelNo).HasMaxLength(20).HasColumnName("mbltelno");
            // 필요한 컬럼들만 추가로 매핑하거나, 기본 규칙을 따르게 둘 수 있습니다.
        }
    }
}