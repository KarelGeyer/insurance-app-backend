using insurance_backend.Interfaces;
using insurance_backend.Models;
using insurance_backend.Models.Config;
using insurance_backend.Models.Db;
using insurance_backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// DI container
builder.Services.Configure<DBModel>(builder.Configuration.GetSection("MongoDB"));

// HTTP services
builder.Services.AddSingleton<IPensionService<PensionProduct>, PensionService>();
builder.Services.AddSingleton<IProductService<Product>, ProductService>();
builder.Services.AddSingleton<ILifeInsuranceService<LifeInsuranceProduct>, LifeInsuranceService>();
builder.Services.AddSingleton<IPropertyInsuranceService<ProductInsuranceProduct>, PropertyInsuranceService>();
builder.Services.AddSingleton<IOrderService<Order>, OrdersService>();

// Email service
builder.Services.Configure<MailConfig>(builder.Configuration.GetSection("MailConfig"));
builder.Services.AddSingleton<IEmailService, EmailService>();

// Add additional services
builder.Services.AddCors();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Config settings
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors(options => options.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.Run();
