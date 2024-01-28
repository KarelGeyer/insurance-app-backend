using insurance_backend.Helpers;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using insurance_backend.Models;
using insurance_backend.Services;
using Microsoft.AspNetCore.Mvc;
using insurance_backend.Models.Request;
using insurance_backend.Models.Requests;
using insurance_backend.Interfaces;

namespace insurance_backend.Controllers
{
	[Route("api/[controller]")]
	public class PensionController : Controller
	{
		private readonly ILogger<PensionController> _logger;
		private IPensionService<PensionProduct> _pensionService;

		public PensionController(ILogger<PensionController> logger, IPensionService<PensionProduct> service)
		{
			_logger = logger;
			_pensionService = service;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<List<PensionProduct>>> GetAllPensionProducts()
		{
			_logger.LogInformation($"{nameof(GetAllPensionProducts)} - Attempting to fetch all state pension products");

			BaseResponse<List<PensionProduct>> res = await _pensionService.GetAll();
			return res;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<PensionProduct>> GetPensionProductsById(string id)
		{
			_logger.LogInformation($"{nameof(GetAllPensionProducts)} - Attempting to fetch all state pension products");

			BaseResponse<PensionProduct> res = await _pensionService.GetOne(id);
			return res;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<List<StateContributionValue>>> GetStateContributions()
		{
			_logger.LogInformation($"{nameof(GetStateContributions)} - Attempting to fetch all state contribution values");

			BaseResponse<List<StateContributionValue>> res = await _pensionService.GetStateContributions();
			return res;
		}

		[HttpPost]
		[Route("[action]")]
		public async Task<BaseResponse<PensionCalcResponse>> GetPensionValue([FromBody] PensionCalcRequest request)
		{
			_logger.LogInformation($"{nameof(GetPensionValue)} - Attempting to fetch all pension values");

			if (
				request.PensionStrategy.ToLower() != Constants.BALANCED.ToLower()
				&& request.PensionStrategy.ToLower() != Constants.CONSERVATIVE.ToLower()
				&& request.PensionStrategy.ToLower() != Constants.DYNAMIC.ToLower()
			)
			{
				_logger.LogError(Messages.CannotBeValueOf_Error(nameof(GetPensionValue), request.PensionStrategy));
				throw new ArgumentException(Messages.Args_Exception);
			}

			if (request.UserContribution == 0)
			{
				_logger.LogError(Messages.CannotBeValueOf_Error(nameof(GetPensionValue), request.UserContribution));
				throw new ArgumentException(Messages.Args_Exception);
			}

			BaseResponse<PensionCalcResponse> res = await _pensionService.CalculatePension(request);
			return res;
		}
	}
}
