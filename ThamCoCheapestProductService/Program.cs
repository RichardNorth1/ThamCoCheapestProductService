using AutoMapper;
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

     // Use fake services in development
    builder.Services.AddSingleton<IProductService, ProductServiceFake>();
    builder.Services.AddSingleton<ICompanyProductService, CompanyProductServiceFake>();

    // // Use real services otherwise
    // builder.Services.AddHttpClient<IProductService, ProductService>(client =>
    // {
    //     client.BaseAddress = new Uri(builder.Configuration["WebServices:Products:BaseUrl"]);
    // });

    // builder.Services.AddHttpClient<ICompanyProductService, CompanyProductService>(client =>
    // {
    //     client.BaseAddress = new Uri(builder.Configuration["WebServices:Products:BaseUrl"]);
    // });
}

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

