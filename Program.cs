using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapPost("/products", (ProductRequest productRequest, ApplicationDbContext context) =>
{
  var category = context.Categories.Where(c => c.Id == productRequest.CategoryId).FirstOrDefault();
  var product = new Product
  {
    Code = productRequest.Code,
    Name = productRequest.Name,
    Description = productRequest.Description,
    Price = productRequest.Price,
    Category = category
  };
  if(productRequest.Tags != null) {
    product.Tags = [];
    foreach(var item in productRequest.Tags) {
      product.Tags.Add(new Tag { 
        Name = item
       });
    }
  }
  
  context.Products.Add(product);
  context.SaveChanges();

  return Results.Created($"/products/{product.Id}", product.Id);
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
