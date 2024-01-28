using insurance_backend.Enums;
using insurance_backend.Helpers;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Db;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace insurance_backend.Services
{
	public class ProductService : IProductService<Product>
	{
		ILogger<PensionService> _logger;
		private readonly IMongoCollection<Product> _productsCollection;

		public ProductService(IOptions<DBModel> dbModel, ILogger<PensionService> logger)
		{
			_logger = logger;

			MongoClient client = new MongoClient(dbModel.Value.ConnectionURI);
			IMongoDatabase db = client.GetDatabase(dbModel.Value.DatabaseName);
			_productsCollection = db.GetCollection<Product>(dbModel.Value.ProductsCollectionName);
		}

		public async Task<BaseResponse<List<Product>>> GetAll()
		{
			BaseResponse<List<Product>> res = new();
			_logger.LogInformation($"{nameof(GetAll)} - Start");
			try
			{
				_logger.LogInformation($"{nameof(GetAll)} - attempting to get fetch products collection");
				List<Product>? products = await _productsCollection.Find(new BsonDocument()).ToListAsync();

				if (products == null || !products.Any())
				{
					_logger.LogError($"{nameof(GetAll)} - {Messages.CannotBeValueOf_Error(nameof(GetAll), products)}");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = "Could not find the products";
				}
				else
				{
					res.Data = products;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(GetAll)} - {ex.Message}");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetAll)} - End");
			return res;
		}

		public async Task<BaseResponse<Product>> GetOne(string id)
		{
			_logger.LogInformation($"{nameof(GetOne)} - Start");

			BaseResponse<Product> res = new();
			FilterDefinition<Product> filter = Builders<Product>.Filter.Eq("Id", id);

			try
			{
				_logger.LogInformation($"{nameof(GetOne)} - attempting to get fetch product with id: {id}");
				Product? product = await _productsCollection.Find(filter).FirstAsync();

				if (product == null)
				{
					_logger.LogError($"{nameof(GetOne)} - {Messages.CannotBeValueOf_Error(nameof(GetOne), product)}");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = "Could not find the product";
				}
				else
				{
					res.Data = product;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(GetOne)} - {ex.Message}");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetOne)} - End");
			return res;
		}

		public async Task<BaseResponse<bool>> Create(ProductCreateRequest product)
		{
			_logger.LogInformation($"{nameof(Create)} - Start");

			BaseResponse<bool> res = new();
			Product newProduct =
				new()
				{
					Name = product.Name,
					Description = product.Description,
					Price = product.Price,
					CompanyLogo = product.CompanyLogo,
					CompanyName = product.CompanyName,
					Category = product.Category,
				};

			try
			{
				_logger.LogInformation($"{nameof(Create)} - attempting to insert new product with id: {newProduct.Id}");
				await _productsCollection.InsertOneAsync(newProduct);
				res.Data = true;
				res.Status = HttpStatus.OK;
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(Create)} - {ex.Message}");
				res.Data = false;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(Create)} - End");
			return res;
		}

		public async Task<BaseResponse<bool>> Delete(string id)
		{
			_logger.LogInformation($"{nameof(Delete)} - Start");

			BaseResponse<bool> res = new();
			FilterDefinition<Product> filter = Builders<Product>.Filter.Eq("Id", id);

			try
			{
				_logger.LogInformation($"{nameof(Create)} - attempting to delete product with id: {id}");
				Product? product = await _productsCollection.FindOneAndDeleteAsync<Product>(filter);

				if (product == null)
				{
					_logger.LogError($"{nameof(GetOne)} - {Messages.CannotBeValueOf_Error(nameof(GetOne), product)}");
					res.Data = false;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = "Could not find the product";
				}
				else
				{
					res.Data = true;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(Delete)} - {ex.Message}");
				res.Data = false;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(Delete)} - End");
			return res;
		}
	}
}
