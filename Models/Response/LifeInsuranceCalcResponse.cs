namespace insurance_backend.Models.Response
{
    public class LifeInsuranceCalcResponse
    {
        public InsurancePrice YearlyLifeInsurance { get; set; } = null!;

        public InsurancePrice MonthlyLifeInsurance { get; set; } = null!;
    }

    public class InsurancePrice
    {
        public int DeathInsurancePrice { get; set; }

        public int InjuriesInsurancePrice { get; set; }

        public int DiseasesInsurancePrice { get; set; }

        public int WorkIncapacityInsurancePrice { get; set; }

        public int HospitalizationInsurancePrice { get; set; }

        public int InvalidityInsurancePrice { get; set; }

        public int TotalInsurancePrice { get; set; }
    }
}
