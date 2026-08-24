namespace ecopack.Api.Dtos
{
	public class ProductDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public decimal CarbonEmission { get; set; }
	}
}