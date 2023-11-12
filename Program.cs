using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapPost("/products", (Product product) =>
{
  ProductRepository.Add(product);
  return Results.Created($"/products/{product.Code}", product);
});

app.MapGet("/products/{code}", ([FromRoute] string code) =>
{
  var product = ProductRepository.GetBy(code);

  return product != null ? Results.Ok(product) : Results.NotFound();
});

app.MapGet("/products", () =>
{
  var products = ProductRepository.GetAll();

  return products != null ? Results.Ok(products) : Results.NotFound();
});

app.MapPut("/products", (Product product) =>
{
  ProductRepository.Edit(product);

  return Results.Ok(product);
});

app.MapDelete("/products/{code}", ([FromRoute] string code) =>
{
  ProductRepository.Delete(code);

  return Results.Ok();
});
if (app.Environment.IsStaging())
{
  app.MapGet("/configuration/database", (IConfiguration configuration) => Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}"));
}

app.Run();
