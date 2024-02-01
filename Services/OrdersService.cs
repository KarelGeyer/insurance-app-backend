using insurance_backend.Enums;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Db;
using insurance_backend.Models.Request.Order;
using insurance_backend.Models.Response;
using insurance_backend.Resources;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace insurance_backend.Services
{
	public class OrdersService : IOrderService<Order>
	{
		ILogger<OrdersService> _logger;
		private readonly IMongoCollection<Order> _ordersCollection;
		private readonly IEmailService _emailService;
		private readonly IProductService<Product> _productService;

		public OrdersService(
			IOptions<DBModel> dbModel,
			ILogger<OrdersService> logger,
			IEmailService emailService,
			IProductService<Product> productService
		)
		{
			_logger = logger;
			_emailService = emailService;
			_productService = productService;

			MongoClient client = new MongoClient(dbModel.Value.ConnectionURI);
			IMongoDatabase db = client.GetDatabase(dbModel.Value.DatabaseName);
			_ordersCollection = db.GetCollection<Order>(dbModel.Value.OrdersCollectionName);
		}

		public async Task<BaseResponse<List<Order>>> GetAll()
		{
			_logger.LogInformation($"{nameof(GetAll)} -  Start");
			BaseResponse<List<Order>> res = new();

			try
			{
				List<Order>? orders = await _ordersCollection.Find(new BsonDocument()).ToListAsync();

				if (orders == null || !orders.Any())
				{
					_logger.LogError($"{nameof(GetAll)} - failed to get all Orders");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = "Could not find the products";
				}
				else
				{
					res.Data = orders;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetAll)} -  End");
			return res;
		}

		public async Task<BaseResponse<Order>> GetOne(string id)
		{
			BaseResponse<Order> res = new();
			FilterDefinition<Order> filter = Builders<Order>.Filter.Eq("Id", id);

			try
			{
				Order? product = await _ordersCollection.Find(filter).FirstAsync();

				if (product == null)
				{
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {id}";
				}
				else
				{
					res.Data = product;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			return res;
		}

		public async Task<BaseResponse<bool>> Create(OrderCreateRequest orderReq)
		{
			BaseResponse<bool> res = new();

			try
			{
				BaseResponse<Product> productRes = await _productService.GetOne(orderReq.ProductId);
				Product? product = productRes.Data;

				if (productRes == null || product == null)
				{
					res.Data = false;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = "could not find a product that was supposedly ordered";
				}
				else
				{
					Order order = new Order()
					{
						ProductId = orderReq.ProductId,
						ProductName = product.Name,
						Category = product.Category,
						Name = orderReq.Name,
						Surname = orderReq.Surname,
						Date = orderReq.Date,
						YearlyPrice = orderReq.YearlyPrice,
					};

					await _ordersCollection.InsertOneAsync(order);

					string? emailBase = MailTemplates.ResourceManager.GetString("Mail_Order_Created_Body");
					string? subjectBase = MailTemplates.ResourceManager.GetString("Mail_Order_Created_Subject");

					if (!string.IsNullOrEmpty(emailBase) && !string.IsNullOrEmpty(subjectBase))
					{
						string customerName = $"{order.Name} {order.Surname}";
						string email = string.Format(emailBase, customerName, order.ProductName, order.Id);
						string subject = string.Format(subjectBase, order.Id);

						_emailService.SendEmail(email, subject, orderReq.EmailAddress);
					}

					res.Data = true;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				res.Data = false;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			return res;
		}

		public async Task<BaseResponse<bool>> Delete(OrderDeleteRequest request)
		{
			BaseResponse<bool> res = new();
			FilterDefinition<Order> filter = Builders<Order>.Filter.Eq("Id", request.OrderId);

			try
			{
				var order = await _ordersCollection.FindOneAndDeleteAsync(filter);

				if (order == null)
				{
					res.Data = false;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {request.OrderId}";
				}
				else
				{
					string? emailBase = MailTemplates.ResourceManager.GetString("Mail_Order_Deleted_Body");
					string? subjectBase = MailTemplates.ResourceManager.GetString("Mail_Order_Deleted_Subject");

					if (!string.IsNullOrEmpty(emailBase) && !string.IsNullOrEmpty(subjectBase))
					{
						string customerName = $"{order.Name} {order.Surname}";
						string email = string.Format(emailBase, customerName, request.OrderId);

						_emailService.SendEmail(email, subjectBase, request.Email);
					}

					res.Data = true;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				res.Data = false;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			return res;
		}
	}
}
