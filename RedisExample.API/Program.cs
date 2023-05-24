using Microsoft.EntityFrameworkCore;
using RedisExample.API.Models;
using RedisExample.API.Models.Repositories;
using RedisExample.API.Services;
using RedisExampleApp.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IProductRepository>(x =>
{
    var appDbContext = x.GetRequiredService<AppDbContext>();

    var productRepository = new ProductRepository(appDbContext);

    var redisService = x.GetRequiredService<RedisService>();

    return new ProductRepositoryWithCache(productRepository,redisService);
});

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseInMemoryDatabase("myDatabase");
});

builder.Services.AddSingleton<RedisService>(x =>
{
    return new RedisService(builder.Configuration["CacheOptions:Url"]);
});

builder.Services.AddSingleton<IDatabase>(x =>
{
    var redisService = x.GetRequiredService<RedisService>();

    return redisService.GetDatabase(0);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    //This procees made for In-Memory Database. If I Connect to Real Database. I dont need this process.
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    dbContext.Database.EnsureCreated();
}

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
