using insurance_backend.Enums;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Db;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using insurance_backend.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace insurance_backend.Services
{
	public class LifeInsuranceService : ILifeInsuranceService<LifeInsuranceProduct>
	{
		private readonly ILogger<PensionService> _logger;
		private readonly IMongoCollection<LifeInsuranceProduct> _lifeInsuranceProductsCollection;
		private readonly IEmailService _emailService;
		private readonly IProductService<Product> _productService;

		public LifeInsuranceService(
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
			_lifeInsuranceProductsCollection = db.GetCollection<LifeInsuranceProduct>(dbModel.Value.LifeInsuranceCollectionName);
		}

		#region GET
		public async Task<BaseResponse<List<LifeInsuranceProduct>>> GetAll()
		{
			_logger.LogInformation($"{nameof(GetAll)} -  Start");
			BaseResponse<List<LifeInsuranceProduct>> res = new();

			try
			{
				List<LifeInsuranceProduct>? products = await _lifeInsuranceProductsCollection.Find(new BsonDocument()).ToListAsync();

				if (products == null || !products.Any())
				{
					_logger.LogError($"{nameof(GetAll)} - Could not fetch the products");
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

		public async Task<BaseResponse<LifeInsuranceProduct>> GetOne(string id)
		{
			_logger.LogInformation($"{nameof(GetOne)} - Start");
			BaseResponse<LifeInsuranceProduct> res = new();
			FilterDefinition<LifeInsuranceProduct> filter = Builders<LifeInsuranceProduct>.Filter.Eq("Id", id);

			try
			{
				_logger.LogInformation($"{nameof(GetOne)} - Attempting to find a product by id: {id}");
				LifeInsuranceProduct? product = await _lifeInsuranceProductsCollection.Find(filter).FirstAsync();

				if (product == null)
				{
					_logger.LogError($"{nameof(GetOne)} - product by id: {id} was not found");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {id}";
				}
				else
				{
					_logger.LogError($"{nameof(GetOne)} - succesfully found a product by id: {id}");
					res.Data = product;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(GetOne)} - error apeared while trying to find product by id: {id}");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetOne)} - End");
			return res;
		}
		#endregion

		public async Task<BaseResponse<bool>> Create(LifeInsuranceProductCreateRequest req)
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
				Category = ProductCategory.LifeInsurance,
			};

			LifeInsuranceProduct lifeInsuranceProduct = new LifeInsuranceProduct()
			{
				ProductId = id.ToString(),
				Name = req.Name,
				DeathCoefficient = req.DeathCoefficient,
				InjuriesCoefficient = req.InjuriesCoefficient,
				DiseasesCoefficient = req.DiseasesCoefficient,
				WorkIncapacityCoefficient = req.WorkIncapacityCoefficient,
				HospitalizationCoefficient = req.HospitalizationCoefficient,
				InvalidityCoefficient = req.InvalidityCoefficient,
				SmokerCoefficient = req.SmokerCoefficient,
				SportCoefficient = req.SportCoefficient,
				SportCoefficientP = req.SportCoefficientP,
			};

			try
			{
				_logger.LogInformation($"{nameof(Create)} - Attempting to store both product and life insurance product");
				await _productService.Create(baseProduct);
				await _lifeInsuranceProductsCollection.InsertOneAsync(lifeInsuranceProduct);

				response.Data = true;
				response.Status = HttpStatus.OK;
				_logger.LogInformation($"{nameof(Create)} - sucesfully stored");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(Create)} - Something failed while trying to store life insurance product");
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
			FilterDefinition<LifeInsuranceProduct> filter = Builders<LifeInsuranceProduct>.Filter.Eq("ProductId", id);

			try
			{
				_logger.LogInformation(
					$"{nameof(Delete)} - Product found, attempting to delete both product and the pension scheme product documents"
				);
				await _productService.Delete(id);
				await _lifeInsuranceProductsCollection.FindOneAndDeleteAsync<LifeInsuranceProduct>(filter);

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

		#region Calculator
		public async Task<BaseResponse<LifeInsuranceCalcResponse>> CalculatePrice(LifeInsuranceProductCalcRequest productData)
		{
			_logger.LogInformation($"{nameof(CalculatePrice)} - Start");
			BaseResponse<LifeInsuranceCalcResponse> res = new();
			FilterDefinition<LifeInsuranceProduct> filter = Builders<LifeInsuranceProduct>.Filter.Eq("productId", productData.ProductId);

			try
			{
				_logger.LogInformation($"{nameof(CalculatePrice)} - Attempting to find a product by product id: {productData.ProductId}");
				LifeInsuranceProduct? product = await _lifeInsuranceProductsCollection.Find(filter).FirstAsync();
				string? yearlyAttr = Constants.ResourceManager.GetString("Constant_Yearly");
				string? monthyAttr = Constants.ResourceManager.GetString("Constant_Monthly");

				if (product != null)
				{
					_logger.LogInformation($"{nameof(CalculatePrice)} - Succesfulyy retrieved a product with product id: {productData.ProductId}");
					LifeInsuranceCalcResponse lifeInsuranceCalcResponse = new();
					bool isSmoker = productData.IsSmoker;
					bool doesSport = productData.DoesSport;
					double smokerAdditive = product.SmokerCoefficient;
					double sportAdditive = product.SportCoefficient;

					// REFACTOR THIS, THIS LOOKS LIKE SHIT, THIS CAN BE FOR SURE WRITEN AS A LOOP.
					_logger.LogInformation($"{nameof(CalculatePrice)} - Attempting to calcualte all the prices");
					Dictionary<string, int> deathCalcResult = CalculateItemPrice(
						true,
						productData.DeathInsurance,
						product.DeathCoefficient,
						isSmoker,
						smokerAdditive,
						doesSport,
						sportAdditive
					);

					Dictionary<string, int> injuryCalcResult = CalculateItemPrice(
						true,
						productData.InjuriesInsurance,
						product.InjuriesCoefficient,
						isSmoker,
						smokerAdditive,
						doesSport,
						sportAdditive
					);

					Dictionary<string, int> diseaseCalcResult = CalculateItemPrice(
						true,
						productData.DiseasesInsurance,
						product.DiseasesCoefficient,
						isSmoker,
						smokerAdditive,
						doesSport,
						sportAdditive
					);

					Dictionary<string, int> workIncapacityResult = CalculateItemPrice(
						false,
						productData.WorkIncapacityInsurance,
						product.WorkIncapacityCoefficient,
						isSmoker,
						smokerAdditive,
						doesSport,
						sportAdditive
					);

					Dictionary<string, int> invalidityCalcResult = CalculateItemPrice(
						true,
						productData.InvalidityInsurance,
						product.InvalidityCoefficient,
						isSmoker,
						smokerAdditive,
						doesSport,
						sportAdditive,
						productData.InvalidityLevel
					);

					Dictionary<string, int> hospitalizationCalcResult = CalculateItemPrice(
						false,
						productData.HospitalizationInsurance,
						product.HospitalizationCoefficient,
						isSmoker,
						smokerAdditive,
						doesSport,
						sportAdditive,
						false,
						productData.HospitalizationLength
					);

					InsurancePrice yearlyLifeInsurance = new();
					yearlyLifeInsurance.DeathInsurancePrice = deathCalcResult[yearlyAttr!];
					yearlyLifeInsurance.InjuriesInsurancePrice = injuryCalcResult[yearlyAttr!];
					yearlyLifeInsurance.DiseasesInsurancePrice = diseaseCalcResult[yearlyAttr!];
					yearlyLifeInsurance.WorkIncapacityInsurancePrice = workIncapacityResult[yearlyAttr!];
					yearlyLifeInsurance.HospitalizationInsurancePrice = hospitalizationCalcResult[yearlyAttr!];
					yearlyLifeInsurance.InvalidityInsurancePrice = invalidityCalcResult[yearlyAttr!];
					yearlyLifeInsurance.TotalInsurancePrice = new[]
					{
						deathCalcResult[yearlyAttr!],
						injuryCalcResult[yearlyAttr!],
						diseaseCalcResult[yearlyAttr!],
						workIncapacityResult[yearlyAttr!],
						hospitalizationCalcResult[yearlyAttr!],
						invalidityCalcResult[yearlyAttr!]
					}.Sum();

					InsurancePrice monthlyLifeInsurance = new();
					monthlyLifeInsurance.DeathInsurancePrice = deathCalcResult[monthyAttr!];
					monthlyLifeInsurance.InjuriesInsurancePrice = injuryCalcResult[monthyAttr!];
					monthlyLifeInsurance.DiseasesInsurancePrice = diseaseCalcResult[monthyAttr!];
					monthlyLifeInsurance.WorkIncapacityInsurancePrice = workIncapacityResult[monthyAttr!];
					monthlyLifeInsurance.HospitalizationInsurancePrice = hospitalizationCalcResult[monthyAttr!];
					monthlyLifeInsurance.InvalidityInsurancePrice = invalidityCalcResult[monthyAttr!];
					monthlyLifeInsurance.TotalInsurancePrice = new[]
					{
						deathCalcResult[monthyAttr!],
						injuryCalcResult[monthyAttr!],
						diseaseCalcResult[monthyAttr!],
						workIncapacityResult[monthyAttr!],
						hospitalizationCalcResult[monthyAttr!],
						invalidityCalcResult[monthyAttr!]
					}.Sum();

					LifeInsuranceCalcResponse result = new();
					result.YearlyLifeInsurance = yearlyLifeInsurance;
					result.MonthlyLifeInsurance = monthlyLifeInsurance;

					if (!string.IsNullOrEmpty(productData.Email))
					{
						_logger.LogInformation($"{nameof(CalculatePrice)} - Attempting to send an email");

						string? emailBase = MailTemplates.ResourceManager.GetString("Mail_Life_Insurance_Calculation_Body");
						string? subject = MailTemplates.ResourceManager.GetString("Mail_Life_Insurance_Calculation_Subject");

						if (!string.IsNullOrEmpty(emailBase) && !string.IsNullOrEmpty(subject))
						{
							string email = string.Format(
								emailBase,
								product.Name,
								monthlyLifeInsurance.TotalInsurancePrice,
								monthlyLifeInsurance.TotalInsurancePrice
							);

							_emailService.SendEmail(email, subject, productData.Email);
						}
					}
					else
						_logger.LogInformation($"{nameof(CalculatePrice)} - Email address is missing, not sending an email");

					_logger.LogInformation($"{nameof(CalculatePrice)} - Finished calculating all the prices");
					res.Data = result;
					res.Status = HttpStatus.OK;
				}
				else
				{
					_logger.LogError($"{nameof(CalculatePrice)} - a product could not have been found");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {productData.ProductId}";
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(CalculatePrice)} - error apeared while trying to find a product with id {productData.ProductId}");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(CalculatePrice)} - End");
			return res;
		}

		private Dictionary<string, int> CalculateItemPrice(
			bool isOnetimePayof,
			int amount,
			double coef,
			bool isSmoker,
			double smokeCoef,
			bool doesSport,
			double sportCoef
		)
		{
			int baseCoef = Int32.Parse(Constants.ResourceManager.GetString("Constants_Base_Life_Insurance_Coef")!);
			int additiveDivisionCoef = Int32.Parse(Constants.ResourceManager.GetString("Constanst_Base_Life_Insurance_Additive_Divison_Coef")!);
			int year = Int32.Parse(Constants.ResourceManager.GetString("Constant_Months_In_Year")!);

			int baseValue = isOnetimePayof ? baseCoef : 1;
			double yearlyBase = (amount / baseValue) * coef;
			double smokerAdditive = 0;
			double sportAdditive = 0;
			if (isSmoker)
				smokerAdditive = yearlyBase * (1 + smokeCoef / additiveDivisionCoef) - yearlyBase;
			if (doesSport)
				sportAdditive = yearlyBase * (1 + sportCoef / additiveDivisionCoef) - yearlyBase;

			yearlyBase += smokerAdditive + sportAdditive;

			int totalYearly = Convert.ToInt32(yearlyBase);

			Dictionary<string, int> res = new Dictionary<string, int> { { "yearly", totalYearly }, { "monthly", totalYearly / year } };

			return res;
		}

		private Dictionary<string, int> CalculateItemPrice(
			bool isOnetimePayof,
			int amount,
			double coef,
			bool isSmoker,
			double smokeCoef,
			bool doesSport,
			double sportCoef,
			InvalidityLevel invalidityLevel
		)
		{
			int year = Int32.Parse(Constants.ResourceManager.GetString("Constant_Months_In_Year")!);
			int invalidityCoef = Int32.Parse(Constants.ResourceManager.GetString("Constants_Base_Life_Insurance_Invalidity_Coef")!);

			Dictionary<string, int> baseRes = CalculateItemPrice(isOnetimePayof, amount, coef, isSmoker, smokeCoef, doesSport, sportCoef);
			int yearlyPrice = baseRes["yearly"];
			int additive = (1 + ((int)invalidityLevel * invalidityCoef / 10));
			int totalYearly = yearlyPrice * additive;

			Dictionary<string, int> res = new Dictionary<string, int> { { "yearly", totalYearly }, { "monthly", totalYearly / year } };

			return res;
		}

		private Dictionary<string, int> CalculateItemPrice(
			bool isOnetimePayof,
			int amount,
			double coef,
			bool isSmoker,
			double smokeCoef,
			bool doesSport,
			double sportCoef,
			bool isHospitalization,
			int HospitalizationLength
		)
		{
			string? yearlyAttr = Constants.ResourceManager.GetString("Constant_Yearly");
			int baseCoef = Int32.Parse(Constants.ResourceManager.GetString("Constants_Base_Life_Insurance_Coef")!);
			int year = Int32.Parse(Constants.ResourceManager.GetString("Constant_Months_In_Year")!);

			Dictionary<string, int> baseRes = CalculateItemPrice(isOnetimePayof, amount, coef, isSmoker, smokeCoef, doesSport, sportCoef);
			int yearlyPrice = baseRes[yearlyAttr!];
			int additive = (HospitalizationLength / baseCoef) + 1;
			int totalYearly = yearlyPrice * additive;

			Dictionary<string, int> res = new Dictionary<string, int> { { "yearly", totalYearly }, { "monthly", totalYearly / year } };

			return res;
		}
		#endregion
	}
}
