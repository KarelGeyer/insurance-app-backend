using insurance_backend.Models.Db;
using insurance_backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using insurance_backend.Models.Response;
using insurance_backend.Models.Request.Product;
using insurance_backend.Enums;
using insurance_backend.Interfaces;
using insurance_backend.Resources;
using MongoDB.Bson;

namespace insurance_backend.Services
{
	public class PropertyInsuranceService : IPropertyInsuranceService<PropertytInsuranceProduct>
	{
		ILogger<PensionService> _logger;
		private readonly IMongoCollection<PropertytInsuranceProduct> _propertyInsuranceProductsCollection;
		private readonly IEmailService _emailService;
		private readonly IProductService<Product> _productService;

		public PropertyInsuranceService(
			IOptions<DBModel> dbModel,
			ILogger<PensionService> logger,
			IEmailService emailService,
			IProductService<Product> productService
		)
		{
			_logger = logger;
			_emailService = emailService;
			_productService = productService;

			MongoClient client = new MongoClient(dbModel.Value.ConnectionURI);
			IMongoDatabase db = client.GetDatabase(dbModel.Value.DatabaseName);
			_propertyInsuranceProductsCollection = db.GetCollection<PropertytInsuranceProduct>(dbModel.Value.PropertyInsuranceCollectionName);
		}

		#region GET
		public async Task<BaseResponse<List<PropertytInsuranceProduct>>> GetAll()
		{
			_logger.LogInformation($"{nameof(GetAll)} -  Start");
			BaseResponse<List<PropertytInsuranceProduct>> res = new();

			try
			{
				List<PropertytInsuranceProduct>? products = await _propertyInsuranceProductsCollection.Find(new BsonDocument()).ToListAsync();

				if (products == null || !products.Any())
				{
					_logger.LogError($"{nameof(GetAll)} - could not fetch the products");
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

		public async Task<BaseResponse<PropertytInsuranceProduct>> GetOne(string id)
		{
			_logger.LogInformation($"{nameof(GetOne)} - Start");
			BaseResponse<PropertytInsuranceProduct> res = new();
			FilterDefinition<PropertytInsuranceProduct> filter = Builders<PropertytInsuranceProduct>.Filter.Eq("Id", id);

			try
			{
				_logger.LogInformation($"{nameof(GetOne)} - Attempting to find the product by id {id}");
				PropertytInsuranceProduct? product = await _propertyInsuranceProductsCollection.Find(filter).FirstAsync();

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

		public async Task<BaseResponse<bool>> Create(PropertyInsuranceProductCreateRequest req)
		{
			_logger.LogInformation($"{nameof(Create)} - Start");
			BaseResponse<bool> response = new();
			ObjectId id = ObjectId.GenerateNewId();

			ProductCreateRequest baseProduct = new ProductCreateRequest()
			{
				Id = id.ToString(),
				Name = req.Name,
				Description = req.Description,
				CompanyName = req.CompanyName,
				CompanyLogo = req.CompanyLogo,
				Category = ProductCategory.PropertyInsurance,
			};

			PropertytInsuranceProduct product = new PropertytInsuranceProduct()
			{
				ProductId = id.ToString(),
				Name = req.Name,
				HousePerMeterSqaureCoefficient = req.HousePerMeterSqaureCoefficient,
				FlatPerMeterSqaureCoefficient = req.FlatPerMeterSqaureCoefficient,
				GaragePerMeterSqaureCoefficient = req.GaragePerMeterSqaureCoefficient,
				EquipmentCoefficient = req.EquipmentCoefficient,
				LiabilityCoefficient = req.LiabilityCoefficient,
			};

			try
			{
				_logger.LogInformation($"{nameof(Create)} - Attempting to store both product and property insurance product");
				await _productService.Create(baseProduct);
				await _propertyInsuranceProductsCollection.InsertOneAsync(product);

				response.Data = true;
				response.Status = HttpStatus.OK;
				_logger.LogInformation($"{nameof(Create)} - sucesfully stored");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(Create)} - Something failed while trying to store property insurance product");
				response.Data = false;
				response.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				response.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(Create)} - End");
			return response;
		}

		public async Task<BaseResponse<bool>> Delete(string id)
		{
			_logger.LogInformation($"{nameof(Delete)} - Start");
			BaseResponse<bool> response = new();
			FilterDefinition<PropertytInsuranceProduct> filter = Builders<PropertytInsuranceProduct>.Filter.Eq("ProductId", id);

			try
			{
				_logger.LogInformation(
					$"{nameof(Delete)} - Product found, attempting to delete both product and the pension scheme product documents"
				);
				await _productService.Delete(id);
				await _propertyInsuranceProductsCollection.FindOneAndDeleteAsync<PropertytInsuranceProduct>(filter);

				response.Data = true;
				response.Status = HttpStatus.OK;
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(Delete)} - Something went wrong when trying to delete a product");
				response.Data = false;
				response.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				response.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(Delete)} - End");
			return response;
		}

		#region Calculations
		public async Task<BaseResponse<PropertyInsuranceCalcResponse>> CalculatePropertyInsurance(PropertyInsuranceProductCalcRequest request)
		{
			_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Start");
			BaseResponse<PropertyInsuranceCalcResponse> res = new();
			string? productIdString = Constants.ResourceManager.GetString("Constant_Product_Id");

			FilterDefinition<PropertytInsuranceProduct> filter = Builders<PropertytInsuranceProduct>.Filter.Eq(productIdString, request.ProductId);

			try
			{
				_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Attempting to fetch the product by product id");
				PropertytInsuranceProduct? product = await _propertyInsuranceProductsCollection.Find(filter).FirstAsync();

				if (product != null)
				{
					_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Attempting to calculate the property insurance");
					double perMeterPropertyPrice = GetCoefficient(request.PropertyType, product);

					string? propertyPriceString = Constants.ResourceManager.GetString("Constants_Property_Price");
					string? equipmentPriceString = Constants.ResourceManager.GetString("Constants_Equipment_Price");
					string? liabilityPriceString = Constants.ResourceManager.GetString("Constants_Liability_Price");
					string? totalPriceString = Constants.ResourceManager.GetString("Constants_Total");

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
							PropertyPrice = calcResult[propertyPriceString!],
							EquipmentPrice = calcResult[equipmentPriceString!],
							LiabilityPrice = calcResult[liabilityPriceString!],
							TotalPrice = calcResult[totalPriceString!],
						};

					if (!string.IsNullOrEmpty(request.Email))
					{
						_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Attempting to send an email");

						string? emailBase = MailTemplates.ResourceManager.GetString("Mail_Property_Insurance_Calculation_Body");
						string? subject = MailTemplates.ResourceManager.GetString("Mail_Property_Insurance_Calculation_Subject");

						if (!string.IsNullOrEmpty(emailBase) && !string.IsNullOrEmpty(subject))
						{
							string email = string.Format(
								emailBase,
								product.Name,
								totalCalc.PropertyPrice,
								totalCalc.EquipmentPrice,
								totalCalc.LiabilityPrice,
								totalCalc.TotalPrice
							);

							_emailService.SendEmail(email, subject, request.Email);
						}
					}
					else
						_logger.LogInformation($"{nameof(CalculatePropertyInsurance)} - Email address is missing, not sending an email");

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

			string? propertyPriceString = Constants.ResourceManager.GetString("Constants_Property_Price");
			string? equipmentPriceString = Constants.ResourceManager.GetString("Constants_Equipment_Price");
			string? liabilityPriceString = Constants.ResourceManager.GetString("Constants_Liability_Price");
			string? totalPriceString = Constants.ResourceManager.GetString("Constants_Total");

			Dictionary<string, double> result = new Dictionary<string, double>
			{
				{ propertyPriceString!, propertyPrice },
				{ equipmentPriceString!, equipmentPrice },
				{ liabilityPriceString!, liabilityPrice },
				{ totalPriceString!, totalPrice }
			};

			return result;
		}

		private double GetCoefficient(string propertyType, PropertytInsuranceProduct product)
		{
			string? propertyTypeHouse = Constants.ResourceManager.GetString("Constants_Property_Type_House");
			string? propertyTypeFlat = Constants.ResourceManager.GetString("Constants_Property_Type_Flat");

			if (propertyType.Equals(propertyTypeHouse))
				return product.HousePerMeterSqaureCoefficient;
			if (propertyType.Equals(propertyTypeFlat))
				return product.FlatPerMeterSqaureCoefficient;
			return product.GaragePerMeterSqaureCoefficient;
		}
		#endregion
	}
}
