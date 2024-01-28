using insurance_backend.Enums;

namespace insurance_backend.Models.Response
{
	public class BaseResponse<T>
	{
		public T? Data { get; set; }

		public HttpStatus Status { get; set; } = HttpStatus.OK;

		public string ResponseMessage { get; set; } = string.Empty;
	}
}
