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

namespace insurance_backend.Services
{
	public class OrdersService
	{
		ILogger<OrdersService> _logger;
		private readonly IMongoCollection<Order> _ordersCollection;

		public OrdersService(IOptions<DBModel> dbModel, ILogger<OrdersService> logger)
		{
			_logger = logger;
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

		public async Task<BaseResponse<bool>> Create(Order order)
		{
			BaseResponse<bool> res = new();
			try
			{
				await _ordersCollection.InsertOneAsync(order);
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

		public async Task<BaseResponse<bool>> DeleteOne(string id)
		{
			BaseResponse<bool> res = new();
			FilterDefinition<Order> filter = Builders<Order>.Filter.Eq("Id", id);

			try
			{
				var order = await _ordersCollection.FindOneAndDeleteAsync(filter);

				if (order == null)
				{
					res.Data = false;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {id}";
				}
				else
				{
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

		public void TestEmail(string body)
		{
			MimeMessage email = new();
			email.From.Add(MailboxAddress.Parse("eudora.bins@ethereal.email"));
			email.To.Add(MailboxAddress.Parse("eudora.bins@ethereal.email"));
			email.Subject = "Test Subject";
			email.Body = new TextPart(TextFormat.Html) { Text = body };

			SmtpClient client = new();
			client.Connect("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);
			client.Authenticate("eudora.bins@ethereal.email", "chjTxQrmvNQc3Z3zSt");
			client.Send(email);
			client.Disconnect(true);
		}
	}
}
