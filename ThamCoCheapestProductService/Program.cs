using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ThamCoCheapestProductService;
using ThamCoCheapestProductService.Services;
using ThamCoCheapestProductService.Services.CompanyProduct;
using ThamCoCheapestProductService.Services.Product;
using ThamCoCheapestProductService.Services.Token;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:CheapestProductService:Authority"];
        options.Audience = builder.Configuration["Auth:CheapestProductService:Audience"];
    });
builder.Services.AddAuthorization();
var autoMapperConfig = new MapperConfiguration(c => c.AddProfile(new MapperProfile()));
IMapper mapper = autoMapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);


if (builder.Environment.IsDevelopment())
{
    // Use fake services in development
    builder.Services.AddSingleton<IProductService, ProductServiceFake>();
    builder.Services.AddSingleton<ICompanyProductService, CompanyProductServiceFake>();
}
else
{
    // // Use real services otherwise
    builder.Services.AddHttpClient<IProductService, ProductService>("product client", client =>
    {
        var uriString = builder.Configuration["WebServices:Products:BaseUrl"];
        if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
            client.BaseAddress = uri;
        }
        else
        {
            throw new UriFormatException($"Invalid URI: {uriString}");
        }
    });
        builder.Services.AddHttpClient<ICompanyProductService, CompanyProductService>("Company product client", client =>
    {
        var uriString = builder.Configuration["WebServices:Products:BaseUrl"];
        if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
            client.BaseAddress = uri;
        }
        else
        {
            throw new UriFormatException($"Invalid URI: {uriString}");
        }
    });
    builder.Services.AddSingleton<ITokenService, TokenService>();
}

builder.Services.AddMemoryCache();
builder.Services.AddLogging(logging =>
{
    //logging.ClearProviders();
//    logging.AddConsole();
    logging.AddAzureWebAppDiagnostics();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
 if (app.Environment.IsDevelopment())
 {
     app.UseSwagger();
     app.UseSwaggerUI();
 }

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

