namespace insurance_backend.Models.Response
{
	public class PropertyInsuranceCalcResponse
	{
		public PropertyInsuranceCalc PerMeterSquareCalc { get; set; }

		public PropertyInsuranceCalc TotalCalc { get; set; }
	}

	public class PropertyInsuranceCalc
	{
		public double PropertyPrice { get; set; }

		public double EquipmentPrice { get; set; }

		public double LiabilityPrice { get; set; }

		public double TotalPrice { get; set; }
	}
}
