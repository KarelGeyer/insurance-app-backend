using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Request;
using insurance_backend.Models.Requests;
using insurance_backend.Models.Response;
using Microsoft.AspNetCore.Mvc;

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

			if (id == null)
				throw new ArgumentNullException(nameof(id));

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

			if (request.UserContribution == 0)
				throw new ArgumentNullException(request.UserContribution.ToString());

			BaseResponse<PensionCalcResponse> res = await _pensionService.CalculatePension(request);
			return res;
		}
	}
}
