namespace insurance_backend.Models.Db
{
	public class DBModel
	{
		public string ConnectionURI { get; set; } = null!;
		public string DatabaseName { get; set; } = null!;
		public string ProductsCollectionName { get; set; } = null!;
		public string StrategyInterestCollectionNames { get; set; } = null!;
		public string StateContributionsCollectionName { get; set; } = null!;
		public string LifeInsuranceCollectionName { get; set; } = null!;
		public string PropertyInsuranceCollectionName { get; set; } = null!;
		public string PensionSchemeCollectionName { get; set; } = null!;
		public string OrdersCollectionName { get; set; } = null!;
	}
}
