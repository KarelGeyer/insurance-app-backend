using insurance_backend.Models.Response;
using insurance_backend.Models;

namespace insurance_backend.Interfaces
{
	public interface IBaseDBService<T>
	{
		Task<BaseResponse<List<T>>> GetAll();
		Task<BaseResponse<T>> GetOne(string id);
	}
}
