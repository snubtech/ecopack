using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
	public class If003Configuration : IEntityTypeConfiguration<If003>
	{
		public void Configure(EntityTypeBuilder<If003> builder)
		{
			builder.ToTable("if003");
			builder.HasKey(e => e.Idx);

			builder.Property(e => e.Idx).HasColumnName("idx");
			builder.Property(e => e.PackMmftProcId).HasMaxLength(50).HasColumnName("packmmftprocid");
			builder.Property(e => e.AppliedMaterialNm).HasMaxLength(100).HasColumnName("appliedmaterialnm");
			builder.Property(e => e.MatTypeNm).HasMaxLength(100).HasColumnName("mattypenm");
			builder.Property(e => e.MatCompNm).HasMaxLength(100).HasColumnName("matcompnm");
			builder.Property(e => e.MatFormNm).HasMaxLength(100).HasColumnName("matformnm");
			builder.Property(e => e.Subject).HasMaxLength(255).HasColumnName("subject");
			builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedmaterial");
			builder.Property(e => e.MatType).HasMaxLength(50).HasColumnName("mattype");
			builder.Property(e => e.MatComp).HasMaxLength(50).HasColumnName("matcomp");
			builder.Property(e => e.MatForm).HasMaxLength(50).HasColumnName("matform");
			builder.Property(e => e.CreatedAt).HasColumnName("createdat");
		}
	}
}