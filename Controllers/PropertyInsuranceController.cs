using Azure.Core;
using insurance_backend.Helpers;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using insurance_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace insurance_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PropertyInsuranceController : ControllerBase
	{
		private readonly ILogger<PropertyInsuranceController> _logger;
		private IPropertyInsuranceService<ProductInsuranceProduct> _propertyInsuranceService;

		public PropertyInsuranceController(ILogger<PropertyInsuranceController> logger, IPropertyInsuranceService<ProductInsuranceProduct> service)
		{
			_logger = logger;
			_propertyInsuranceService = service;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<List<ProductInsuranceProduct>>> GetPropertyInsuraceProducts()
		{
			_logger.LogInformation($"{nameof(GetPropertyInsuraceProducts)} - Attempting to fetch all life insurance product details");
			BaseResponse<List<ProductInsuranceProduct>> res = await _propertyInsuranceService.GetAll();
			return res;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<ProductInsuranceProduct>> GetPropertyInsuranceProduct(string id)
		{
			_logger.LogInformation($"{nameof(GetPropertyInsuraceProducts)} - Attempting to fetch life insurance produc details");

			if (string.IsNullOrEmpty(id))
			{
				_logger.LogError(Messages.CannotBeValueOf_Error(nameof(CalculatePropertyInsurance), id));
				throw new ArgumentException(Messages.Args_Exception);
			}

			BaseResponse<ProductInsuranceProduct> res = await _propertyInsuranceService.GetOne(id);
			return res;
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
			{
				_logger.LogError(Messages.CannotBeValueOf_Error(nameof(CalculatePropertyInsurance), request.ProductId));
				throw new ArgumentException(Messages.Args_Exception);
			}

			if (request.SquareMeters < 1)
			{
				_logger.LogError(Messages.CannotBeValueOf_Error(nameof(CalculatePropertyInsurance), request.SquareMeters));
				throw new ArgumentException(Messages.Args_Exception);
			}

			if (!request.ShouldCalculateProperty && !request.ShouldCalculateEquipment && !request.ShouldCalculateLiability)
			{
				_logger.LogError("Cannot calculate value without choosing at least one of the calculations");
				throw new ArgumentException(Messages.Args_Exception);
			}

			BaseResponse<PropertyInsuranceCalcResponse> res = await _propertyInsuranceService.CalculatePropertyInsurance(request);
			return res;
		}
	}
}
