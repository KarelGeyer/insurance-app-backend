using insurance_backend.Models.Request;
using insurance_backend.Models.Requests;
using insurance_backend.Models.Response;

namespace insurance_backend.Interfaces
{
	public interface IPensionService<T> : IBaseDBService<T>
	{
		Task<BaseResponse<List<StateContributionValue>>> GetStateContributions();
		Task<BaseResponse<PensionCalcResponse>> CalculatePension(PensionCalcRequest data);
	}
}
