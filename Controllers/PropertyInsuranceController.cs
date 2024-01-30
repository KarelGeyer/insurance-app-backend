using insurance_backend.Enums;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using insurance_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace insurance_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PropertyInsuranceController : ControllerBase
	{
		private readonly ILogger<PropertyInsuranceController> _logger;
		private IPropertyInsuranceService<PropertytInsuranceProduct> _propertyInsuranceService;

		public PropertyInsuranceController(ILogger<PropertyInsuranceController> logger, IPropertyInsuranceService<PropertytInsuranceProduct> service)
		{
			_logger = logger;
			_propertyInsuranceService = service;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<List<PropertytInsuranceProduct>>> GetPropertyInsuraceProducts()
		{
			_logger.LogInformation($"{nameof(GetPropertyInsuraceProducts)} - Attempting to fetch all life insurance product details");
			BaseResponse<List<PropertytInsuranceProduct>> res = await _propertyInsuranceService.GetAll();
			return res;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<PropertytInsuranceProduct>> GetPropertyInsuranceProduct(string id)
		{
			_logger.LogInformation($"{nameof(GetPropertyInsuraceProducts)} - Attempting to fetch life insurance produc details");

			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException(id);

			BaseResponse<PropertytInsuranceProduct> res = await _propertyInsuranceService.GetOne(id);
			return res;
		}

		[HttpPost]
		[Route("action")]
		public async Task<BaseResponse<bool>> CreatePropertyInsuranceProduct(PropertyInsuranceProductCreateRequest request)
		{
			_logger.LogInformation($"{nameof(CreatePropertyInsuranceProduct)} - Attempting to create a life insurance product");

			if (string.IsNullOrEmpty(request.Name))
				throw new ArgumentNullException(nameof(request.Name));
			if (string.IsNullOrEmpty(request.Description))
				throw new ArgumentNullException(nameof(request.Description));
			if (string.IsNullOrEmpty(request.CompanyName))
				throw new ArgumentNullException(nameof(request.CompanyName));
			if (string.IsNullOrEmpty(request.CompanyLogo))
				throw new ArgumentNullException(nameof(request.CompanyLogo));
			if (
				request.Category != ProductCategory.Pension
				&& request.Category != ProductCategory.PropertyInsurance
				&& request.Category != ProductCategory.LifeInsurance
			)
				throw new ArgumentNullException(nameof(request.CompanyLogo));
			if (request.HousePerMeterSqaureCoefficient == 0)
				throw new ArgumentNullException(nameof(request.HousePerMeterSqaureCoefficient));
			if (request.FlatPerMeterSqaureCoefficient == 0)
				throw new ArgumentNullException(nameof(request.FlatPerMeterSqaureCoefficient));
			if (request.GaragePerMeterSqaureCoefficient == 0)
				throw new ArgumentNullException(nameof(request.GaragePerMeterSqaureCoefficient));
			if (request.EquipmentCoefficient == 0)
				throw new ArgumentNullException(nameof(request.EquipmentCoefficient));
			if (request.LiabilityCoefficient == 0)
				throw new ArgumentNullException(nameof(request.LiabilityCoefficient));

			BaseResponse<bool> response = await _propertyInsuranceService.Create(request);
			return response;
		}

		[HttpPost]
		[Route("[action]")]
		public async Task<BaseResponse<PropertyInsuranceCalcResponse>> CalculatePropertyInsurance(
			[FromBody] PropertyInsuranceProductCalcRequest request
		)
		{
			_logger.LogInformation(
				$"{nameof(CalculatePropertyInsurance)} - Attempting to retrieve a calculation for life insurance using product with id {request.ProductId}"
			);

			if (string.IsNullOrEmpty(request.ProductId))
				throw new ArgumentNullException(request.ProductId);

			if (request.SquareMeters < 1)
				throw new ArgumentNullException(request.SquareMeters.ToString());

			if (!request.ShouldCalculateProperty && !request.ShouldCalculateEquipment && !request.ShouldCalculateLiability)
				throw new ArgumentNullException("At least one of the product must be chosen");

			BaseResponse<PropertyInsuranceCalcResponse> res = await _propertyInsuranceService.CalculatePropertyInsurance(request);
			return res;
		}
	}
}
