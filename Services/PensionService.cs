using insurance_backend.Enums;
using insurance_backend.Helpers;
using insurance_backend.Models;
using insurance_backend.Models.Db;
using insurance_backend.Models.Request;
using insurance_backend.Models.Requests;
using insurance_backend.Models.Response;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace insurance_backend.Services
{
	public class PensionService
	{
		ILogger<PensionService> _logger;
		private readonly IMongoCollection<StateContributionValue> _stateContributionValuesCollection;
		private readonly IMongoCollection<PensionProduct> _pensionSchemeCollection;

		public PensionService(IOptions<DBModel> dbModel, ILogger<PensionService> logger)
		{
			_logger = logger;

			MongoClient client = new MongoClient(dbModel.Value.ConnectionURI);
			IMongoDatabase db = client.GetDatabase(dbModel.Value.DatabaseName);

			_stateContributionValuesCollection = db.GetCollection<StateContributionValue>(dbModel.Value.StateContributionsCollectionName);
			_pensionSchemeCollection = db.GetCollection<PensionProduct>(dbModel.Value.PensionSchemeCollectionName);
		}

		public async Task<BaseResponse<List<PensionProduct>>> GetAll()
		{
			BaseResponse<List<PensionProduct>> res = new();

			try
			{
				List<PensionProduct>? products = await _pensionSchemeCollection.Find(new BsonDocument()).ToListAsync();

				if (products == null)
				{
					res.Data = null;
					res.Status = HttpStatus.NOT_FOUND;
					res.ResponseMessage = "Pension products were not found";
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
					_logger.LogError(
						$"{nameof(GetStateContributions)} - {Messages.CannotBeValueOf_Error(nameof(GetStateContributions), stateContrubionValues)}"
					);
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
				FilterDefinition<PensionProduct> filter = Builders<PensionProduct>.Filter.Eq("ProductId", data.ProductId);
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
					double totalSavings = data.CurrentSavings;
					int yearsToRetirement = 65 - data.UserAge;
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
					resData.StateContributionTotal = stateContribution * 12 * yearsToRetirement;

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
			if (pensionStrategy == "dynamická")
				return product.DynamicPercentage;
			if (pensionStrategy == "konzervativní")
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
			double currentTotalSavings = totalSavings;
			float totalContribution = userContribution + employerContribution + stateContribution;

			for (int i = 0; i < yearsToRetirement; i++)
			{
				float yearlyContribution = totalContribution * 12;
				currentTotalSavings += yearlyContribution;
				currentTotalSavings *= (1 + strategyInterestRate / 100);
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
			double currentTotalSavings = totalSavings;
			float totalContribution = userContribution + employerContribution + stateContribution;

			for (int i = 0; i < yearsToRetirement; i++)
			{
				float yearlyContribution = totalContribution * 12;
				currentTotalSavings += yearlyContribution;
			}

			return totalValueSaved - currentTotalSavings;
		}
	}
}
