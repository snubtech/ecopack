using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
	public class ProjectConfiguration : IEntityTypeConfiguration<Project>
	{
		public void Configure(EntityTypeBuilder<Project> builder)
		{
			builder.ToTable("project");
			builder.HasKey(e => e.PrjId);

			builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjid");
			builder.Property(e => e.PrjNm).HasMaxLength(100).HasColumnName("prjnm");
			builder.Property(e => e.RepCustId).HasMaxLength(50).HasColumnName("repcustid");
			builder.Property(e => e.BizNo).HasMaxLength(30).HasColumnName("bizno");
			builder.Property(e => e.BizNm).HasMaxLength(100).HasColumnName("biznm");
			builder.Property(e => e.RepNm).HasMaxLength(50).HasColumnName("repnm");
			builder.Property(e => e.RoleNm).HasMaxLength(50).HasColumnName("rolenm");
			builder.Property(e => e.IndstNm).HasMaxLength(100).HasColumnName("indstnm");
			builder.Property(e => e.CntryNm).HasMaxLength(50).HasColumnName("cntrynm");
			builder.Property(e => e.AddrCd).HasMaxLength(20).HasColumnName("addrcd");
			builder.Property(e => e.DtlAddr1).HasMaxLength(200).HasColumnName("dtladdr1");
			builder.Property(e => e.DtlAddr2).HasMaxLength(200).HasColumnName("dtladdr2");
			builder.Property(e => e.EmlAddr).HasMaxLength(100).HasColumnName("emladdr");
			builder.Property(e => e.RepTelNo).HasMaxLength(30).HasColumnName("reptelno");
			builder.Property(e => e.MblTelNo).HasMaxLength(30).HasColumnName("mbltelno");

			// 제품 수출 국가 여부 (USA, EU, CHN, VNM, IDN, JPN, AUS, KOR)
			builder.Property(e => e.PrdExpCntryNm1).HasMaxLength(10).HasColumnName("prdexpcntrynm1");
			builder.Property(e => e.PrdExpCntryNm2).HasMaxLength(10).HasColumnName("prdexpcntrynm2");
			builder.Property(e => e.PrdExpCntryNm3).HasMaxLength(10).HasColumnName("prdexpcntrynm3");
			builder.Property(e => e.PrdExpCntryNm4).HasMaxLength(10).HasColumnName("prdexpcntrynm4");
			builder.Property(e => e.PrdExpCntryNm5).HasMaxLength(10).HasColumnName("prdexpcntrynm5");
			builder.Property(e => e.PrdExpCntryNm6).HasMaxLength(10).HasColumnName("prdexpcntrynm6");
			builder.Property(e => e.PrdExpCntryNm7).HasMaxLength(10).HasColumnName("prdexpcntrynm7");
			builder.Property(e => e.PrdExpCntryNm8).HasMaxLength(10).HasColumnName("prdexpcntrynm8");

            // 포장 차수 여부 (Primary, Secondary, Tertiary) 
            builder.Property(e => e.PrdPkgSeq1).HasMaxLength(10).HasColumnName("prdpkgseq1");
			builder.Property(e => e.PrdPkgSeq2).HasMaxLength(10).HasColumnName("prdpkgseq2");
			builder.Property(e => e.PrdPkgSeq3).HasMaxLength(10).HasColumnName("prdpkgseq3");

			builder.Property(e => e.PrjRevNo).HasMaxLength(20).HasColumnName("prjrevno");
            builder.Property(e => e.Prjuserid).HasMaxLength(20).HasColumnName("prjuserid");
            builder.Property(e => e.Prjmemo).HasMaxLength(20).HasColumnName("prjmemo");
            builder.Property(e => e.PackLevel).HasMaxLength(20).HasColumnName("packLevel");
            builder.Property(e => e.PrjFcrtDt).HasColumnName("prjfcrtcdt");
		}
	}
}