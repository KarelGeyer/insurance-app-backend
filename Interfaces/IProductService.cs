using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;

namespace insurance_backend.Interfaces
{
	public interface IProductService<T> : IBaseDBService<T>
	{
		Task<BaseResponse<bool>> Create(ProductCreateRequest product);
		Task<BaseResponse<bool>> Delete(string id);
	}
}
