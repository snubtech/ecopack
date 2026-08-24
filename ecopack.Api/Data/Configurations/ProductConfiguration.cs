using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
	public class ProductConfiguration : IEntityTypeConfiguration<Product>
	{
		public void Configure(EntityTypeBuilder<Product> builder)
		{
			builder.ToTable("product");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Id).HasColumnName("id");
			builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnName("name");
			builder.Property(e => e.CarbonEmission).HasColumnType("decimal(18,2)").HasColumnName("carbonemission");
		}
	}
}