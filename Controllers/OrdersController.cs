using insurance_backend.Enums;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Request.Order;
using insurance_backend.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace insurance_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly ILogger<OrdersController> _logger;
		private IOrderService<Order> _orderService;

		public OrdersController(ILogger<OrdersController> logger, IOrderService<Order> service)
		{
			_logger = logger;
			_orderService = service;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<List<Order>>> GetOrders()
		{
			_logger.LogInformation($"{nameof(GetOrders)} - Attempting to fetch all the products");
			return await _orderService.GetAll();
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<Order>> GetOrderById(string id)
		{
			_logger.LogInformation($"{nameof(GetOrderById)} - Start");

			if (id == null)
				throw new ArgumentNullException(nameof(id));

			_logger.LogInformation($"{nameof(GetOrderById)} - Attempting to fetch products with id {id}");
			return await _orderService.GetOne(id);
		}

		[HttpPost]
		[Route("[action]")]
		public async Task<BaseResponse<bool>> CreateOrder([FromBody] OrderCreateRequest orderReq)
		{
			_logger.LogInformation($"{nameof(CreateOrder)} - Start");

			if (string.IsNullOrEmpty(orderReq.EmailAddress))
				throw new ArgumentNullException(nameof(orderReq.EmailAddress));

			if (string.IsNullOrEmpty(orderReq.ProductId))
				throw new ArgumentNullException(nameof(orderReq.ProductId));

			if (string.IsNullOrEmpty(orderReq.Name))
				throw new ArgumentNullException(nameof(orderReq.Name));

			if (string.IsNullOrEmpty(orderReq.Surname))
				throw new ArgumentNullException(nameof(orderReq.Surname));

			if (orderReq.YearlyPrice.Equals(0))
				throw new ArgumentNullException(nameof(orderReq.YearlyPrice));

			if (orderReq.Date.Equals(DateTime.MinValue))
				throw new ArgumentNullException(nameof(orderReq.Date));

			_logger.LogInformation($"{nameof(CreateOrder)} - Attempting to create a new order");

			return await _orderService.Create(orderReq);
		}

		[HttpDelete]
		[Route("[action]")]
		public async Task<BaseResponse<bool>> DeleteOrder(OrderDeleteRequest request)
		{
			if (string.IsNullOrEmpty(request.OrderId))
				throw new ArgumentNullException(nameof(request.OrderId));

			_logger.LogInformation($"{nameof(DeleteOrder)} - Attempting to delete a product with id {request.OrderId}");
			return await _orderService.Delete(request);
		}
	}
}
