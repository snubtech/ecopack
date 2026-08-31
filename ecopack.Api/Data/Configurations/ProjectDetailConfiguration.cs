using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
	public class ProjectDetailConfiguration : IEntityTypeConfiguration<ProjectDetail>
	{
		public void Configure(EntityTypeBuilder<ProjectDetail> builder)
		{
			builder.ToTable("project_detail");
			
			// 복합 기본키 설정 (PrjId + PackLevel)
			builder.HasKey(e => new { e.PrjId, e.PackLevel });

			builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjid");
			builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packlevel");
			builder.Property(e => e.PrjRevNo).HasMaxLength(10).HasColumnName("prjrevno");
			builder.Property(e => e.PackLevelNm).HasMaxLength(100).HasColumnName("packlevelnm");
			builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedmaterial");
			builder.Property(e => e.AppliedMaterialNm).HasMaxLength(100).HasColumnName("appliedmaterialnm");
			builder.Property(e => e.MatUse).HasMaxLength(50).HasColumnName("matuse");
			builder.Property(e => e.MatUseNm).HasMaxLength(100).HasColumnName("matusenm");
			builder.Property(e => e.MatType).HasMaxLength(50).HasColumnName("mattype");
			builder.Property(e => e.MatTypeNm).HasMaxLength(100).HasColumnName("mattypenm");
			builder.Property(e => e.MatForm).HasMaxLength(50).HasColumnName("matform");
			builder.Property(e => e.MatFormNm).HasMaxLength(100).HasColumnName("matformnm");
			builder.Property(e => e.PackDsgnTplId).HasMaxLength(50).HasColumnName("packdsgntplid");
			builder.Property(e => e.Projstatus).HasMaxLength(30).HasColumnName("projstatus");
			builder.Property(e => e.PrdExpCntry).HasMaxLength(50).HasColumnName("prdexpcntry");
			builder.Property(e => e.PrdExpCntryNm).HasMaxLength(50).HasColumnName("prdexpcntrynm");
            builder.Property(e => e.Prjuserid).HasMaxLength(20).HasColumnName("prjuserid");
            builder.Property(e => e.Updatedate).HasColumnType("datetime").HasColumnName("updatedate");
        }
	}
}