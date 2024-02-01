using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;

namespace insurance_backend.Interfaces
{
	public interface IPropertyInsuranceService<T> : IBaseDBService<T>
	{
		Task<BaseResponse<PropertyInsuranceCalcResponse>> CalculatePropertyInsurance(PropertyInsuranceProductCalcRequest request);

		Task<BaseResponse<bool>> Create(PropertyInsuranceProductCreateRequest request);

		Task<BaseResponse<bool>> Delete(string id);
	}
}
