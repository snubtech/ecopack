using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecopack.Api.Data.Configurations
{
	public class EcouserConfiguration : IEntityTypeConfiguration<Ecouser>
	{
		public void Configure(EntityTypeBuilder<Ecouser> builder)
		{
			builder.HasNoKey().ToTable("ecouser");

			builder.Property(e => e.Businessno).HasMaxLength(10).HasColumnName("businessno");
			builder.Property(e => e.Companynm).HasMaxLength(100).HasColumnName("companynm");
			builder.Property(e => e.Email).HasMaxLength(20).HasColumnName("email");
			builder.Property(e => e.Mobile).HasMaxLength(20).HasColumnName("mobile");
			builder.Property(e => e.Nation).HasMaxLength(20).HasColumnName("nation");
			builder.Property(e => e.Pass).HasMaxLength(20).HasColumnName("pass");
			builder.Property(e => e.Rule).HasMaxLength(3).HasDefaultValueSql("'0'").HasColumnName("rule");
			builder.Property(e => e.Usernm).HasMaxLength(30).HasColumnName("usernm");
			builder.Property(e => e.Userno).HasMaxLength(20).HasColumnName("userno");
		}
	}
}