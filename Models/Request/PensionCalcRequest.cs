using insurance_backend.Enums;
using Microsoft.Build.Framework;

namespace insurance_backend.Models.Requests
{
	public class PensionCalcRequest
	{
		public string ProductId { get; set; }

		public string? Email { get; set; }

		public float CurrentSavings { get; set; } = 0;

		public float UserContribution { get; set; } = 0;

		public float EmployerContribution { get; set; } = 0;

		public int UserAge { get; set; }

		public string PensionStrategy { get; set; }
	}
}
