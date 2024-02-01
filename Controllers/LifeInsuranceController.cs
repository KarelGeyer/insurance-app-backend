using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using insurance_backend.Models;
using insurance_backend.Services;
using Microsoft.AspNetCore.Mvc;
using insurance_backend.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;
using insurance_backend.Enums;

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
		public async Task<BaseResponse<List<LifeInsuranceProduct>>> GetLifeInsuranceProducts()
		{
			_logger.LogInformation($"{nameof(GetLifeInsuranceProducts)} - Attempting to fetch all life insurance product details");
			BaseResponse<List<LifeInsuranceProduct>> res = await _lifeInsuranceService.GetAll();
			return res;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<LifeInsuranceProduct>> GetLifeInsuranceProductById(string id)
		{
			_logger.LogInformation($"{nameof(GetLifeInsuranceProductById)} - Attempting to fetch life insurance product by id {id}");

			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException(nameof(id));

			BaseResponse<LifeInsuranceProduct> res = await _lifeInsuranceService.GetOne(id);
			return res;
		}

		[HttpPost]
		[Route("[action]")]
		public async Task<BaseResponse<bool>> CreateLifeInsuranceProduct([FromBody] LifeInsuranceProductCreateRequest request)
		{
			_logger.LogInformation($"{nameof(CreateLifeInsuranceProduct)} - Attempting to create a life insurance product");

			if (string.IsNullOrEmpty(request.Name))
				throw new ArgumentNullException(nameof(request.Name));
			if (string.IsNullOrEmpty(request.Description))
				throw new ArgumentNullException(nameof(request.Description));
			if (string.IsNullOrEmpty(request.CompanyName))
				throw new ArgumentNullException(nameof(request.CompanyName));
			if (string.IsNullOrEmpty(request.CompanyLogo))
				throw new ArgumentNullException(nameof(request.CompanyLogo));
			if (request.DeathCoefficient == 0)
				throw new ArgumentNullException(nameof(request.DeathCoefficient));
			if (request.InjuriesCoefficient == 0)
				throw new ArgumentNullException(nameof(request.InjuriesCoefficient));
			if (request.DiseasesCoefficient == 0)
				throw new ArgumentNullException(nameof(request.DiseasesCoefficient));
			if (request.WorkIncapacityCoefficient == 0)
				throw new ArgumentNullException(nameof(request.WorkIncapacityCoefficient));
			if (request.HospitalizationCoefficient == 0)
				throw new ArgumentNullException(nameof(request.HospitalizationCoefficient));
			if (request.InvalidityCoefficient == 0)
				throw new ArgumentNullException(nameof(request.InvalidityCoefficient));
			if (request.SmokerCoefficient == 0)
				throw new ArgumentNullException(nameof(request.SmokerCoefficient));
			if (request.SportCoefficient == 0)
				throw new ArgumentNullException(nameof(request.SportCoefficient));
			if (request.SportCoefficientP == 0)
				throw new ArgumentNullException(nameof(request.SportCoefficientP));

			BaseResponse<bool> response = await _lifeInsuranceService.Create(request);
			return response;
		}

		[HttpDelete]
		[Route("[action]")]
		public async Task<BaseResponse<bool>> DeleteLifeInsuranceProduct(string id)
		{
			_logger.LogInformation(
				$"{nameof(DeleteLifeInsuranceProduct)} - Attempting to retrieve a calculation for life insurance using product with id {id}"
			);

			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException(nameof(id));

			BaseResponse<bool> res = await _lifeInsuranceService.Delete(id);
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
