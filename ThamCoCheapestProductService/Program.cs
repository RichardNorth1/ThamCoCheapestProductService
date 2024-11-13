using AutoMapper;
using Microsoft.Extensions.Logging.AzureAppServices;
using ThamCoCheapestProductService;
using ThamCoCheapestProductService.Services;
using ThamCoCheapestProductService.Services.CompanyProduct;
using ThamCoCheapestProductService.Services.Product;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var autoMapperConfig = new MapperConfiguration(c => c.AddProfile(new MapperProfile()));
IMapper mapper = autoMapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    // Use fake services in development
    builder.Services.AddSingleton<IProductService, ProductServiceFake>();
    builder.Services.AddSingleton<ICompanyProductService, CompanyProductServiceFake>();
}
else
{
    // Use real services otherwise
    var productServiceBaseUrl = builder.Configuration["WebServices:Products:BaseUrl"];
    if (Uri.TryCreate(productServiceBaseUrl, UriKind.Absolute, out var productServiceUri))
    {
        builder.Services.AddHttpClient<IProductService, ProductService>(client =>
        {
            client.BaseAddress = productServiceUri;
        });
    }
    else
    {
        throw new InvalidOperationException("Invalid ProductService Base URL");
    }

    var companyProductServiceBaseUrl = builder.Configuration["WebServices:Products:BaseUrl"];
    if (Uri.TryCreate(companyProductServiceBaseUrl, UriKind.Absolute, out var companyProductServiceUri))
    {
        builder.Services.AddHttpClient<ICompanyProductService, CompanyProductService>(client =>
        {
            client.BaseAddress = companyProductServiceUri;
        });
    }
    else
    {
        throw new InvalidOperationException("Invalid CompanyProductService Base URL");
    }
}

builder.Services.AddMemoryCache();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddAzureWebAppDiagnostics();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
