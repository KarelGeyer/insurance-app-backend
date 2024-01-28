using insurance_backend.Enums;
using insurance_backend.Helpers;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Db;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;

namespace insurance_backend.Services
{
	public class LifeInsuranceService : ILifeInsuranceService<LifeInsuranceProduct>
	{
		ILogger<PensionService> _logger;
		private readonly IMongoCollection<LifeInsuranceProduct> _lifeInsuranceProductsCollection;

		public LifeInsuranceService(IOptions<DBModel> dbModel, ILogger<PensionService> logger)
		{
			_logger = logger;
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

		public async Task<BaseResponse<LifeInsuranceProduct>> GetOneByProductId(string productId)
		{
			_logger.LogInformation($"{nameof(GetOneByProductId)} - Start");
			BaseResponse<LifeInsuranceProduct> res = new();
			FilterDefinition<LifeInsuranceProduct> filter = Builders<LifeInsuranceProduct>.Filter.Eq("ProductId", productId);

			try
			{
				_logger.LogInformation($"{nameof(GetOneByProductId)} - Attempting to find a product by product id: {productId}");
				LifeInsuranceProduct? product = await _lifeInsuranceProductsCollection.Find(filter).FirstAsync();

				if (product == null)
				{
					_logger.LogError($"{nameof(GetOneByProductId)} - product by product id: {productId} was not found");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {productId}";
				}
				else
				{
					_logger.LogError($"{nameof(GetOneByProductId)} - succesfully found a product by product id: {productId}");
					res.Data = product;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(GetOneByProductId)} - error apeared while trying to find product by product id: {productId}");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetOneByProductId)} - End");

			return res;
		}

		public async Task<BaseResponse<string>> GetProductIdFromId(string id)
		{
			_logger.LogInformation($"{nameof(GetProductIdFromId)} - Start");
			BaseResponse<string> res = new();
			FilterDefinition<LifeInsuranceProduct> filter = Builders<LifeInsuranceProduct>.Filter.Eq("Id", id);

			try
			{
				_logger.LogInformation($"{nameof(GetProductIdFromId)} - Attempting to find a product by id: {id}");
				LifeInsuranceProduct? product = await _lifeInsuranceProductsCollection.Find(filter).FirstAsync();

				if (product == null)
				{
					_logger.LogError($"{nameof(GetProductIdFromId)} - product by id: {id} was not found");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = $"Could not find the product by Id {id}";
				}
				else
				{
					_logger.LogInformation($"{nameof(GetProductIdFromId)} - Succesfully retrieved a product by id: {id}");
					res.Data = product.ProductId;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(GetProductIdFromId)} - error apeared while trying to find product by id: {id}");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetProductIdFromId)} - End");
			return res;
		}
		#endregion

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
					yearlyLifeInsurance.DeathInsurancePrice = deathCalcResult["yearly"];
					yearlyLifeInsurance.InjuriesInsurancePrice = injuryCalcResult["yearly"];
					yearlyLifeInsurance.DiseasesInsurancePrice = diseaseCalcResult["yearly"];
					yearlyLifeInsurance.WorkIncapacityInsurancePrice = workIncapacityResult["yearly"];
					yearlyLifeInsurance.HospitalizationInsurancePrice = hospitalizationCalcResult["yearly"];
					yearlyLifeInsurance.InvalidityInsurancePrice = invalidityCalcResult["yearly"];
					yearlyLifeInsurance.TotalInsurancePrice = new[]
					{
						deathCalcResult["yearly"],
						injuryCalcResult["yearly"],
						diseaseCalcResult["yearly"],
						workIncapacityResult["yearly"],
						hospitalizationCalcResult["yearly"],
						invalidityCalcResult["yearly"]
					}.Sum();

					InsurancePrice monthlyLifeInsurance = new();
					monthlyLifeInsurance.DeathInsurancePrice = deathCalcResult["monthly"];
					monthlyLifeInsurance.InjuriesInsurancePrice = injuryCalcResult["monthly"];
					monthlyLifeInsurance.DiseasesInsurancePrice = diseaseCalcResult["monthly"];
					monthlyLifeInsurance.WorkIncapacityInsurancePrice = workIncapacityResult["monthly"];
					monthlyLifeInsurance.HospitalizationInsurancePrice = hospitalizationCalcResult["monthly"];
					monthlyLifeInsurance.InvalidityInsurancePrice = invalidityCalcResult["monthly"];
					monthlyLifeInsurance.TotalInsurancePrice = new[]
					{
						deathCalcResult["monthly"],
						injuryCalcResult["monthly"],
						diseaseCalcResult["monthly"],
						workIncapacityResult["monthly"],
						hospitalizationCalcResult["monthly"],
						invalidityCalcResult["monthly"]
					}.Sum();

					LifeInsuranceCalcResponse result = new();
					result.YearlyLifeInsurance = yearlyLifeInsurance;
					result.MonthlyLifeInsurance = monthlyLifeInsurance;

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
			int baseValue = isOnetimePayof ? 1000 : 1;
			double yearlyBase = (amount / baseValue) * coef;
			double smokerAdditive = 0;
			double sportAdditive = 0;
			if (isSmoker)
				smokerAdditive = yearlyBase * (1 + smokeCoef / 100) - yearlyBase;
			if (doesSport)
				sportAdditive = yearlyBase * (1 + sportCoef / 100) - yearlyBase;

			yearlyBase += smokerAdditive + sportAdditive;

			int totalYearly = Convert.ToInt32(yearlyBase);

			Dictionary<string, int> res = new Dictionary<string, int> { { "yearly", totalYearly }, { "monthly", totalYearly / 12 } };

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
			Dictionary<string, int> baseRes = CalculateItemPrice(isOnetimePayof, amount, coef, isSmoker, smokeCoef, doesSport, sportCoef);
			int yearlyPrice = baseRes["yearly"];
			int additive = (1 + ((int)invalidityLevel * 2 / 10));
			int totalYearly = yearlyPrice * additive;

			Dictionary<string, int> res = new Dictionary<string, int> { { "yearly", totalYearly }, { "monthly", totalYearly / 12 } };

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
			Dictionary<string, int> baseRes = CalculateItemPrice(isOnetimePayof, amount, coef, isSmoker, smokeCoef, doesSport, sportCoef);
			int yearlyPrice = baseRes["yearly"];
			int additive = (HospitalizationLength / 1000) + 1;
			int totalYearly = yearlyPrice * additive;

			Dictionary<string, int> res = new Dictionary<string, int> { { "yearly", totalYearly }, { "monthly", totalYearly / 12 } };

			return res;
		}
		#endregion
	}
}
