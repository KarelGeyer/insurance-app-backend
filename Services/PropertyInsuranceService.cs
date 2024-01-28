using insurance_backend.Models.Db;
using insurance_backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using insurance_backend.Models.Response;
using insurance_backend.Models.Request.Product;
using insurance_backend.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;
using insurance_backend.Helpers;
using MongoDB.Bson;
using insurance_backend.Interfaces;

namespace insurance_backend.Services
{
	public class PropertyInsuranceService : IPropertyInsuranceService<ProductInsuranceProduct>
	{
		ILogger<PensionService> _logger;
		private readonly IMongoCollection<ProductInsuranceProduct> _propertyInsuranceProductsCollection;

		public PropertyInsuranceService(IOptions<DBModel> dbModel, ILogger<PensionService> logger)
		{
			_logger = logger;
			MongoClient client = new MongoClient(dbModel.Value.ConnectionURI);
			IMongoDatabase db = client.GetDatabase(dbModel.Value.DatabaseName);
			_propertyInsuranceProductsCollection = db.GetCollection<ProductInsuranceProduct>(dbModel.Value.PropertyInsuranceCollectionName);
		}

		#region GET
		public async Task<BaseResponse<List<ProductInsuranceProduct>>> GetAll()
		{
			_logger.LogInformation($"{nameof(GetAll)} -  Start");
			BaseResponse<List<ProductInsuranceProduct>> res = new();

			try
			{
				List<ProductInsuranceProduct>? products = await _propertyInsuranceProductsCollection.Find(new BsonDocument()).ToListAsync();

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
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetAll)} -  End");
			return res;
		}

		public async Task<BaseResponse<ProductInsuranceProduct>> GetOne(string id)
		{
			_logger.LogInformation($"{nameof(GetOne)} - Start");
			BaseResponse<ProductInsuranceProduct> res = new();
			FilterDefinition<ProductInsuranceProduct> filter = Builders<ProductInsuranceProduct>.Filter.Eq("Id", id);

			try
			{
				_logger.LogInformation($"{nameof(GetOne)} - Attempting to find the product by id {id}");
				ProductInsuranceProduct? product = await _propertyInsuranceProductsCollection.Find(filter).FirstAsync();

				if (product == null)
				{
					_logger.LogError($"{nameof(GetOne)} - Could not find a product by id {id}");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {id}";
				}
				else
				{
					_logger.LogInformation($"{nameof(GetOne)} - product by id {id} was found");
					res.Data = product;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(GetOne)} - Error apeared while trying to fetch a product");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetOne)} - End");
			return res;
		}
		#endregion

		#region Calculations
		public async Task<BaseResponse<PropertyInsuranceCalcResponse>> CalculatePropertyInsurance(PropertyInsuranceProductCalcRequest request)
		{
			_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Start");
			BaseResponse<PropertyInsuranceCalcResponse> res = new();
			FilterDefinition<ProductInsuranceProduct> filter = Builders<ProductInsuranceProduct>.Filter.Eq("productId", request.ProductId);

			try
			{
				_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Attempting to fetch the product by product id");
				ProductInsuranceProduct? product = await _propertyInsuranceProductsCollection.Find(filter).FirstAsync();

				if (product != null)
				{
					_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Attempting to calculate the property insurance");
					double perMeterPropertyPrice = GetCoefficient(request.PropertyType, product);
					Dictionary<string, double> calcResult = CalculatePropertyInsurance(
						request,
						product.EquipmentCoefficient,
						product.LiabilityCoefficient,
						perMeterPropertyPrice
					);

					PropertyInsuranceCalc perMeterCalc =
						new()
						{
							PropertyPrice = request.ShouldCalculateProperty ? perMeterPropertyPrice : 0,
							EquipmentPrice = request.ShouldCalculateEquipment ? product.EquipmentCoefficient : 0,
							LiabilityPrice = request.ShouldCalculateLiability ? product.LiabilityCoefficient : 0,
							TotalPrice = perMeterPropertyPrice + product.EquipmentCoefficient + product.LiabilityCoefficient
						};

					PropertyInsuranceCalc totalCalc =
						new()
						{
							PropertyPrice = calcResult["PropertyPrice"],
							EquipmentPrice = calcResult["EquipmentPrice"],
							LiabilityPrice = calcResult["LiabilityPrice"],
							TotalPrice = calcResult["Total"],
						};

					PropertyInsuranceCalcResponse response = new() { PerMeterSquareCalc = perMeterCalc, TotalCalc = totalCalc };
					_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Finished calculating the property insurance");
					res.Data = response;
					res.Status = HttpStatus.OK;
				}
				else
				{
					_logger.LogError($"{nameof(CalculatePropertyInsurance)} - Could not fetch the product by product id");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {request.ProductId}";
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(CalculatePropertyInsurance)} - Error apeared while trying to fetch a product");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - End");
			return res;
		}

		private Dictionary<string, double> CalculatePropertyInsurance(
			PropertyInsuranceProductCalcRequest data,
			double liabilityCoefficient,
			double equipmentyCoefficient,
			double coefficient
		)
		{
			double propertyPrice = 0;
			double equipmentPrice = 0;
			double liabilityPrice = 0;

			if (data.ShouldCalculateProperty)
				propertyPrice = coefficient * data.SquareMeters;
			if (data.ShouldCalculateEquipment)
				equipmentPrice = equipmentyCoefficient * data.SquareMeters;
			if (data.ShouldCalculateLiability)
				liabilityPrice = liabilityCoefficient * data.SquareMeters;

			double totalPrice = propertyPrice + equipmentPrice + liabilityPrice;

			Dictionary<string, double> result = new Dictionary<string, double>
			{
				{ "PropertyPrice", propertyPrice },
				{ "EquipmentPrice", equipmentPrice },
				{ "LiabilityPrice", liabilityPrice },
				{ "Total", totalPrice }
			};

			return result;
		}

		private double GetCoefficient(string propertyType, ProductInsuranceProduct product)
		{
			if (propertyType.Equals("house"))
				return product.HousePerMeterSqaureCoefficient;
			if (propertyType.Equals("flat"))
				return product.FlatPerMeterSqaureCoefficient;
			return product.GaragePerMeterSqaureCoefficient;
		}
		#endregion
	}
}
