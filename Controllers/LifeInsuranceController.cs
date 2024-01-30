using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using insurance_backend.Models;
using insurance_backend.Services;
using Microsoft.AspNetCore.Mvc;
using insurance_backend.Interfaces;
using Microsoft.CodeAnalysis;

namespace insurance_backend.Controllers
{
	[Route("api/[controller]")]
	public class LifeInsuranceController : Controller
	{
		private readonly ILogger<LifeInsuranceController> _logger;
		private ILifeInsuranceService<LifeInsuranceProduct> _lifeInsuranceService;

		public LifeInsuranceController(ILogger<LifeInsuranceController> logger, ILifeInsuranceService<LifeInsuranceProduct> lifeInsuranceService)
		{
			_logger = logger;
			_lifeInsuranceService = lifeInsuranceService;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<List<LifeInsuranceProduct>>> GetLifeInsuraceProducts()
		{
			_logger.LogInformation($"{nameof(GetLifeInsuraceProducts)} - Attempting to fetch all life insurance product details");
			BaseResponse<List<LifeInsuranceProduct>> res = await _lifeInsuranceService.GetAll();
			return res;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<LifeInsuranceProduct>> GetLifeInsuraceProductById(string id)
		{
			_logger.LogInformation($"{nameof(GetLifeInsuraceProductById)} - Attempting to fetch life insurance product by id {id}");

			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException(nameof(id));

			BaseResponse<LifeInsuranceProduct> res = await _lifeInsuranceService.GetOne(id);
			return res;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<LifeInsuranceProduct>> GetLifeInsuraceProductByProductId(string productId)
		{
			_logger.LogInformation(
				$"{nameof(GetLifeInsuraceProductByProductId)} - Attempting to fetch life insurance product by product id {productId}"
			);

			if (string.IsNullOrEmpty(productId))
				throw new ArgumentNullException(nameof(productId));

			BaseResponse<LifeInsuranceProduct> res = await _lifeInsuranceService.GetOneByProductId(productId);
			return res;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<string>> GetLifeInsuraceProductId(string id)
		{
			_logger.LogInformation($"{nameof(GetLifeInsuraceProductId)} - Attempting to retrieve a life insurance productid using id {id}");

			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException(nameof(id));

			BaseResponse<string> res = await _lifeInsuranceService.GetProductIdFromId(id);
			return res;
		}

		[HttpPost]
		[Route("[action]")]
		public async Task<BaseResponse<LifeInsuranceCalcResponse>> CalculateLifeInsurance([FromBody] LifeInsuranceProductCalcRequest request)
		{
			_logger.LogInformation(
				$"{nameof(CalculateLifeInsurance)} - Attempting to retrieve a calculation for life insurance using product with id {request.ProductId}"
			);

			if (string.IsNullOrEmpty(request.ProductId))
				throw new ArgumentNullException(nameof(request.ProductId));

			if (request.HospitalizationInsurance > 0 && request.HospitalizationLength < 1)
				throw new ArgumentException(nameof(request.HospitalizationInsurance));

			BaseResponse<LifeInsuranceCalcResponse> res = await _lifeInsuranceService.CalculatePrice(request);
			return res;
		}
	}
}
