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
builder.Services.AddScoped<IPensionService<PensionProduct>, PensionService>();
builder.Services.AddScoped<IProductService<Product>, ProductService>();
builder.Services.AddScoped<ILifeInsuranceService<LifeInsuranceProduct>, LifeInsuranceService>();
builder.Services.AddScoped<IPropertyInsuranceService<PropertytInsuranceProduct>, PropertyInsuranceService>();
builder.Services.AddScoped<IOrderService<Order>, OrdersService>();

// Email service
builder.Services.Configure<MailConfig>(builder.Configuration.GetSection("MailConfig"));
builder.Services.AddScoped<IEmailService, EmailService>();

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
