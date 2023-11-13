using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

  if (productRequest.Tags != null)
  {
    product.Tags = [];
    foreach (var item in productRequest.Tags)
    {
      product.Tags.Add(new Tag
      {
        Name = item
      });
    }
  }

  context.Products.Add(product);
  context.SaveChanges();

  return Results.Created($"/products/{product.Id}", product.Id);
});

app.MapGet("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
  var product = context.Products
    .Include(p => p.Category)
    .Include(p => p.Tags)
    .Where(p => p.Id == id).FirstOrDefault();

  return product != null
    ? Results.Ok(product)
    : Results.NotFound();
});

app.MapGet("/products", (ApplicationDbContext context) =>
{
  var products = context.Products
    .Include(p => p.Category)
    .Include(p => p.Tags)
    .ToList();

  return products != null
    ? Results.Ok(products)
    : Results.NotFound();
});

app.MapPut("/products/{id}", ([FromRoute] int id, ProductRequest productRequest, ApplicationDbContext context) =>
{
  var product = context.Products
    .Include(p => p.Tags)
    .Where(p => p.Id == id).FirstOrDefault();

  var category = context
    .Categories
    .Where(c => c.Id == productRequest.CategoryId)
    .FirstOrDefault();

  if (product == null) return Results.NotFound();

  product.Code = productRequest.Code;
  product.Name = productRequest.Name;
  product.Description = productRequest.Description;
  product.Price = productRequest.Price;
  product.Category = category;

  if (productRequest.Tags != null)
  {
    product.Tags = [];
    foreach (var item in productRequest.Tags)
    {
      product.Tags.Add(new Tag
      {
        Name = item
      });
    }
  }

  context.SaveChanges();
  return Results.Ok(product);
});

app.MapDelete("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
  var product = context.Products
    .Where(p => p.Id == id).FirstOrDefault();

  if (product == null) return Results.NotFound();

  context.Products.Remove(product);

  context.SaveChanges();

  return Results.Ok($"Product {id} deleted");
});

if (app.Environment.IsStaging())
{
  app.MapGet("/configuration/database", (IConfiguration configuration) => Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}"));
}

app.Run();
