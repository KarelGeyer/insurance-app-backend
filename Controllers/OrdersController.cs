using insurance_backend.Enums;
using insurance_backend.Helpers;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using insurance_backend.Models;
using insurance_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace insurance_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly ILogger<OrdersController> _logger;
		private OrdersService _orderService;

		public OrdersController(ILogger<OrdersController> logger, OrdersService service)
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
			{
				_logger.LogError($"{nameof(GetOrderById)} - {Messages.MissingProperty_Error(id)}");
				throw new ArgumentNullException(nameof(id));
			}

			_logger.LogInformation($"{nameof(GetOrderById)} - Attempting to fetch products with id {id}");
			return await _orderService.GetOne(id);
		}

		[HttpPost]
		[Route("[action]")]
		public async Task<BaseResponse<bool>> CreateOrder([FromBody] Order order)
		{
			_logger.LogInformation($"{nameof(CreateOrder)} - Start");

			if (string.IsNullOrEmpty(order.ProductId))
			{
				_logger.LogError($"{nameof(CreateOrder)} - {Messages.MissingProperty_Error(order.ProductId)}");
				throw new ArgumentNullException(nameof(order.ProductId));
			}
			if (string.IsNullOrEmpty(order.ProductName))
			{
				_logger.LogError($"{nameof(CreateOrder)} - {Messages.MissingProperty_Error(order.ProductName)}");
				throw new ArgumentNullException(nameof(order.ProductName));
			}
			if (string.IsNullOrEmpty(order.Name))
			{
				_logger.LogError($"{nameof(CreateOrder)} - {Messages.MissingProperty_Error(order.Name)}");
				throw new ArgumentNullException(nameof(order.Name));
			}
			if (string.IsNullOrEmpty(order.Surname))
			{
				_logger.LogError($"{nameof(CreateOrder)} - {Messages.MissingProperty_Error(order.Surname)}");
				throw new ArgumentNullException(nameof(order.Surname));
			}
			if (order.Category.Equals(0))
			{
				_logger.LogError($"{nameof(CreateOrder)} - {Messages.MissingProperty_Error(order.Category)}");
				throw new ArgumentNullException(nameof(order.Category));
			}

			if (Enum.IsDefined(typeof(ProductCategory), order.Category))
			{
				_logger.LogError($"{nameof(CreateOrder)} - {Messages.MissingProperty_Error(order.Category)}");
				throw new ArgumentNullException(nameof(order.Category));
			}

			if (order.YearlyPrice.Equals(0))
			{
				_logger.LogError($"{nameof(CreateOrder)} - {Messages.MissingProperty_Error(order.YearlyPrice)}");
				throw new ArgumentNullException(nameof(order.YearlyPrice));
			}

			if (order.Date.Equals(DateTime.MinValue))
			{
				_logger.LogError($"{nameof(CreateOrder)} - {Messages.MissingProperty_Error(order.Date)}");
				throw new ArgumentNullException(nameof(order.Date));
			}

			_logger.LogInformation($"{nameof(CreateOrder)} - Attempting to create a new order");

			return await _orderService.Create(order);
		}

		[HttpDelete]
		[Route("[action]")]
		public async Task<BaseResponse<bool>> DeleteOrder(string orderId)
		{
			if (orderId == null)
			{
				_logger.LogError($"{nameof(DeleteOrder)} - {Messages.MissingProperty_Error(orderId)}");
				throw new ArgumentNullException(nameof(orderId));
			}

			_logger.LogInformation($"{nameof(DeleteOrder)} - Attempting to delete a product with id {orderId}");
			return await _orderService.DeleteOne(orderId);
		}

		[HttpPost]
		[Route("[action]")]
		public void TestEmail(string body)
		{
			_orderService.TestEmail(body);
		}
	}
}
