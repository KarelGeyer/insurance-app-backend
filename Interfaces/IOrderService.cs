using insurance_backend.Models.Request.Order;
using insurance_backend.Models.Response;

namespace insurance_backend.Interfaces
{
	public interface IOrderService<T> : IBaseDBService<T>
	{
		Task<BaseResponse<bool>> Create(OrderCreateRequest orderReq);
		Task<BaseResponse<bool>> Delete(OrderDeleteRequest request);
	}
}
