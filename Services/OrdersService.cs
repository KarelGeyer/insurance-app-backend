using insurance_backend.Models.Db;
using insurance_backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using insurance_backend.Services;
using insurance_backend.Enums;
using insurance_backend.Helpers;
using insurance_backend.Models.Response;
using MongoDB.Bson;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http.HttpResults;
using insurance_backend.Models.Request.Order;
using insurance_backend.Interfaces;

namespace insurance_backend.Services
{
	public class OrdersService : IOrderService<Order>
	{
		ILogger<OrdersService> _logger;
		private readonly IMongoCollection<Order> _ordersCollection;
		private readonly EmailService _emailService;

		public OrdersService(IOptions<DBModel> dbModel, ILogger<OrdersService> logger, EmailService emailService)
		{
			_logger = logger;
			_emailService = emailService;
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
					_logger.LogError($"{nameof(GetAll)} - {Messages.CannotBeValueOf_Error(nameof(GetAll), orders)}");
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
			Order order = orderReq.Order;
			try
			{
				await _ordersCollection.InsertOneAsync(order);

				string email = $"<p>Dear {order.Name}, Thank you for ordering {order.ProductName}. It was created under with id: {order.Id}</p>";
				string subject = $"<p>Order {order.Id} succesfully created</p>";
				_emailService.SendEmail(email, subject, orderReq.EmailAddress);

				res.Data = true;
				res.Status = HttpStatus.OK;
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
					string email = $"<p>Dear Customer, your order with number {request.OrderId} was succesfully canceled</p>";
					string subject = $"<p>Your order succesfully deleted</p>";
					_emailService.SendEmail(email, subject, request.Email);

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
