using insurance_backend.Enums;
using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Request.Product;
using insurance_backend.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace insurance_backend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductsController : ControllerBase
	{
		private readonly ILogger<ProductsController> _logger;
		private IProductService<Product> _productService;

		public ProductsController(ILogger<ProductsController> logger, IProductService<Product> service)
		{
			_logger = logger;
			_productService = service;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<List<Product>>> GetProducts()
		{
			_logger.LogInformation($"{nameof(GetProducts)} - Attempting to fetch all the products");
			return await _productService.GetAll();
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<BaseResponse<Product>> GetProductById(string productId)
		{
			_logger.LogInformation($"{nameof(GetProductById)} - Start");

			if (productId == null)
				throw new ArgumentNullException(nameof(productId));

			_logger.LogInformation($"{nameof(GetProductById)} - Attempting to fetch products with id {productId}");
			return await _productService.GetOne(productId);
		}

		[HttpPost]
		[Route("[action]")]
		public async Task<BaseResponse<bool>> CreateProduct(ProductCreateRequest product)
		{
			_logger.LogInformation($"{nameof(GetProductById)} - Start");

			if (string.IsNullOrEmpty(product.Name))
				throw new ArgumentNullException(nameof(product.Name));
			if (string.IsNullOrEmpty(product.Description))
				throw new ArgumentNullException(nameof(product.Description));
			if (string.IsNullOrEmpty(product.CompanyName))
				throw new ArgumentNullException(nameof(product.CompanyName));
			if (string.IsNullOrEmpty(product.CompanyLogo))
				throw new ArgumentNullException(nameof(product.CompanyLogo));
			if (Enum.IsDefined(typeof(ProductCategory), product.Category))
				throw new ArgumentNullException(nameof(product.Category));

			return await _productService.Create(product);
		}

		[HttpDelete]
		[Route("[action]")]
		public async Task<BaseResponse<bool>> DeleteProduct(string productId)
		{
			if (productId == null)
				throw new ArgumentNullException(nameof(productId));

			_logger.LogInformation($"{nameof(GetProductById)} - Attempting to delete a product with id {productId}");
			return await _productService.Delete(productId);
		}
	}
}
