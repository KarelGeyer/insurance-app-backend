using insurance_backend.Models.Response;
using insurance_backend.Models;
using insurance_backend.Models.Request.Product;
using insurance_backend.Enums;

namespace insurance_backend.Interfaces
{
	public interface ILifeInsuranceService<T> : IBaseDBService<T>
	{
		Task<BaseResponse<T>> GetOneByProductId(string productId);

		Task<BaseResponse<bool>> Create(LifeInsuranceProductCreateRequest req);

		Task<BaseResponse<string>> GetProductIdFromId(string id);

		Task<BaseResponse<LifeInsuranceCalcResponse>> CalculatePrice(LifeInsuranceProductCalcRequest productData);
	}
}
