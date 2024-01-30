using insurance_backend.Enums;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Db;
using insurance_backend.Models.Request;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Requests;
using insurance_backend.Models.Response;
using insurance_backend.Resources;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Org.BouncyCastle.Ocsp;

namespace insurance_backend.Services
{
	public class PensionService : IPensionService<PensionProduct>
	{
		ILogger<PensionService> _logger;
		private readonly IMongoCollection<StateContributionValue> _stateContributionValuesCollection;
		private readonly IMongoCollection<PensionProduct> _pensionSchemeCollection;
		private readonly IEmailService _emailService;
		private readonly IProductService<Product> _productService;

		public PensionService(
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

			_stateContributionValuesCollection = db.GetCollection<StateContributionValue>(dbModel.Value.StateContributionsCollectionName);
			_pensionSchemeCollection = db.GetCollection<PensionProduct>(dbModel.Value.PensionSchemeCollectionName);
		}

		#region GET
		public async Task<BaseResponse<List<PensionProduct>>> GetAll()
		{
			_logger.LogInformation($"{nameof(GetAll)} - Start");
			BaseResponse<List<PensionProduct>> res = new();

			try
			{
				_logger.LogInformation($"{nameof(GetAll)} - Attempting to retrieve all the products");
				List<PensionProduct>? products = await _pensionSchemeCollection.Find(new BsonDocument()).ToListAsync();

				if (products == null)
				{
					_logger.LogError($"{nameof(GetAll)} - products could not have been found");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = "Pension products were not found";
				}
				else
				{
					_logger.LogInformation($"{nameof(GetAll)} - Products were found");
					res.Data = products;
					res.Status = HttpStatus.OK;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(GetAll)} - error apeared while trying to fetch all the products");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetAll)} - End");
			return res;
		}

		public async Task<BaseResponse<PensionProduct>> GetOne(string id)
		{
			_logger.LogInformation($"{nameof(GetOne)} - Start");

			BaseResponse<PensionProduct> res = new();
			FilterDefinition<PensionProduct> filter = Builders<PensionProduct>.Filter.Eq("Id", id);

			try
			{
				_logger.LogInformation($"{nameof(GetOne)} - attempting to get fetch product with id: {id}");
				PensionProduct? product = await _pensionSchemeCollection.Find(filter).FirstAsync();

				if (product == null)
				{
					_logger.LogError($"{nameof(GetOne)} - failed to get all a product");
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
		#endregion

		public async Task<BaseResponse<bool>> Create(PensionProductCreateRequest req)
		{
			_logger.LogInformation($"{nameof(Create)} - Start");
			BaseResponse<bool> response = new();
			Guid id = new();

			ProductCreateRequest baseProduct = new ProductCreateRequest()
			{
				Id = id.ToString(),
				Name = req.Name,
				Description = req.Description,
				CompanyName = req.CompanyName,
				CompanyLogo = req.CompanyLogo,
				Category = req.Category,
			};

			PensionProduct pensionProduct = new PensionProduct()
			{
				ProductId = id.ToString(),
				Name = req.Name,
				DynamicPercentage = req.DynamicPercentage,
				ConservativePercentage = req.ConservativePercentage,
				BalancedPercentage = req.BalancedPercentage,
			};

			try
			{
				_logger.LogInformation($"{nameof(Create)} - Attempting to store both product and pension scheme product");
				await _productService.Create(baseProduct);
				await _pensionSchemeCollection.InsertOneAsync(pensionProduct);

				response.Data = true;
				response.Status = HttpStatus.OK;
				_logger.LogInformation($"{nameof(Create)} - sucesfully stored");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(Create)} - Something failed while trying to store pension scheme product");
				response.Data = false;
				response.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				response.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(Create)} - End");
			return response;
		}

		#region Calculations
		public async Task<BaseResponse<List<StateContributionValue>>> GetStateContributions()
		{
			_logger.LogInformation($"{nameof(GetStateContributions)} -  Start");
			BaseResponse<List<StateContributionValue>> res = new();
			try
			{
				List<StateContributionValue> stateContrubionValues = await _stateContributionValuesCollection.Find(new BsonDocument()).ToListAsync();

				if (stateContrubionValues != null)
				{
					res.Data = stateContrubionValues;
					res.Status = HttpStatus.OK;
				}
				else
				{
					_logger.LogError($"{nameof(GetStateContributions)} - Failed to get all contributions");
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = "State contribution values could not have been found";
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{nameof(GetStateContributions)} - {ex.Message}");
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(GetStateContributions)} -  End");
			return res;
		}

		public async Task<BaseResponse<PensionCalcResponse>> CalculatePension(PensionCalcRequest data)
		{
			_logger.LogInformation($"{nameof(CalculatePension)} -  Start");
			PensionCalcResponse resData = new();
			BaseResponse<PensionCalcResponse> res = new();

			try
			{
				string? productIdString = Constants.ResourceManager.GetString("Constant_Product_Id");

				FilterDefinition<PensionProduct> filter = Builders<PensionProduct>.Filter.Eq(productIdString, data.ProductId);
				List<StateContributionValue> state = _stateContributionValuesCollection.Find(new BsonDocument()).ToList();
				PensionProduct? product = await _pensionSchemeCollection.Find(filter).FirstAsync();

				if (product == null || state == null)
				{
					if (product == null)
						res.ResponseMessage = $"Could not find the product by Id {data.ProductId}";

					if (state == null)
						res.ResponseMessage = $"Could not retrieve the state contribution values";

					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
				}
				else
				{
					int retirementAge = int.Parse(Constants.ResourceManager.GetString("Constants_Retirement_Age")!);
					int monthsInYear = int.Parse(Constants.ResourceManager.GetString("Constant_Months_In_Year")!);

					double totalSavings = data.CurrentSavings;
					int yearsToRetirement = retirementAge - data.UserAge;
					int stateContribution = GetStateContribution(data.UserContribution);
					double strategyInterestRate = GetStrategyInterestrate(product, data.PensionStrategy);

					_logger.LogInformation($"{nameof(CalculatePension)} -  got all variables");

					_logger.LogInformation($"{nameof(CalculatePension)} -  calculating total savings");
					double totalValue = CalculateTotalPension(
						yearsToRetirement,
						totalSavings,
						data.UserContribution,
						data.EmployerContribution,
						stateContribution,
						strategyInterestRate
					);

					_logger.LogInformation($"{nameof(CalculatePension)} -  attempting to get the valorization value");
					double valorization = GetValoritization(
						yearsToRetirement,
						totalSavings,
						data.UserContribution,
						data.EmployerContribution,
						stateContribution,
						totalValue
					);

					_logger.LogInformation($"{nameof(CalculatePension)} -  got all the data");

					resData.StateContribution = stateContribution;
					resData.TotalSavings = totalValue;
					resData.Valorization = valorization;
					resData.StateContributionTotal = stateContribution * monthsInYear * yearsToRetirement;

					if (!string.IsNullOrEmpty(data.Email))
					{
						_logger.LogInformation($"{nameof(CalculatePension)} - Attempting to send an email");

						string? emailBase = MailTemplates.ResourceManager.GetString("Mail_Pension_Scheme_Calculation_Body");
						string? subject = MailTemplates.ResourceManager.GetString("Mail_Pension_Scheme_Calculation_Subject");

						if (!string.IsNullOrEmpty(emailBase) && !string.IsNullOrEmpty(subject))
						{
							string email = string.Format(emailBase, product.Name, stateContribution, totalValue, valorization);

							_emailService.SendEmail(email, subject, data.Email);
						}
					}
					else
						_logger.LogInformation($"{nameof(CalculatePension)} - email address is missing, not sending an email");

					res.Data = resData;
				}
			}
			catch (Exception ex)
			{
				res.Data = null;
				res.Status = HttpStatus.INTERNAL_SERVER_ERROR;
				res.ResponseMessage = ex.Message;
			}

			_logger.LogInformation($"{nameof(CalculatePension)} -  End");
			return res;
		}

		private int GetStateContribution(float userContribution)
		{
			_logger.LogInformation($"{nameof(GetStateContribution)} - Start");
			int nearestLower = 0;

			_logger.LogInformation($"{nameof(CalculatePension)} -  attempting to retrieve the state contribuiton values");
			List<StateContributionValue> stateContributionValues = _stateContributionValuesCollection.Find(new BsonDocument()).ToList();

			_logger.LogInformation($"{nameof(CalculatePension)} -  attempting to find the correct value based on the user contribution");
			for (int i = 0; i < stateContributionValues.Count; i++)
			{
				if (stateContributionValues[i].UserContribution <= userContribution && stateContributionValues[i].UserContribution > nearestLower)
				{
					nearestLower = stateContributionValues[i].StateContribution;
				}
			}

			_logger.LogInformation($"{nameof(GetStateContribution)} - End");
			return nearestLower;
		}

		private double GetStrategyInterestrate(PensionProduct product, string pensionStrategy)
		{
			_logger.LogInformation($"{nameof(GetStrategyInterestrate)} - Getting the interest rate");

			string? dynamicString = Constants.ResourceManager.GetString("Constant_Dynamic");
			string? conservativeString = Constants.ResourceManager.GetString("Constant_Consevative");

			if (pensionStrategy == dynamicString!)
				return product.DynamicPercentage;
			if (pensionStrategy == conservativeString!)
				return product.ConservativePercentage;
			return product.BalancedPercentage;
		}

		private double CalculateTotalPension(
			int yearsToRetirement,
			double totalSavings,
			float userContribution,
			float employerContribution,
			int stateContribution,
			double strategyInterestRate
		)
		{
			int year = int.Parse(Constants.ResourceManager.GetString("Constant_Months_In_Year")!);
			int pensionBaseCoef = int.Parse(Constants.ResourceManager.GetString("Constants_Base_Pension_Coef")!);

			double currentTotalSavings = totalSavings;
			float totalContribution = userContribution + employerContribution + stateContribution;

			for (int i = 0; i < yearsToRetirement; i++)
			{
				float yearlyContribution = totalContribution * year;
				currentTotalSavings += yearlyContribution;
				currentTotalSavings *= (1 + strategyInterestRate / pensionBaseCoef);
			}

			return currentTotalSavings;
		}

		private double GetValoritization(
			int yearsToRetirement,
			double totalSavings,
			float userContribution,
			float employerContribution,
			int stateContribution,
			double totalValueSaved
		)
		{
			int year = int.Parse(Constants.ResourceManager.GetString("Constant_Months_In_Year")!);

			double currentTotalSavings = totalSavings;
			float totalContribution = userContribution + employerContribution + stateContribution;

			for (int i = 0; i < yearsToRetirement; i++)
			{
				float yearlyContribution = totalContribution * year;
				currentTotalSavings += yearlyContribution;
			}

			return totalValueSaved - currentTotalSavings;
		}
		#endregion
	}
}
