using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>();

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

public static class ProductRepository
{
  public static List<Product>? Products { get; set; } = Products = [];

  public static void Init(IConfiguration configuration)
  {
    var products = configuration.GetSection("Products").Get<List<Product>>();
    if (products != null) Products = products;
    Products = products;
  }

  public static void Add(Product product)
  {
    Products ??= [];

    Products.Add(product);
  }

  public static Product? GetBy(string code)
  {
    return Products?.FirstOrDefault(p => p.Code == code);
  }

  public static List<Product>? GetAll()
  {
    return Products?.FindAll(p => p.Code != null);
  }

  public static void Edit(Product product)
  {
    if (product.Code == null) return;

    var productToEdit = GetBy(product.Code);

    if (productToEdit == null) return;

    productToEdit.Name = product.Name;
    productToEdit.Price = product.Price;
  }

  public static void Delete(string code)
  {
    if (code == null) return;

    var productToDelete = GetBy(code);

    if (productToDelete == null) return;

    Products?.Remove(productToDelete);
  }
}

public class Product
{
  public string? Code { get; set; }

  public string? Name { get; set; }
  public decimal Price { get; set; }
}

public class ApplicationDbContext : DbContext
{
  public DbSet<Product> Products { get; set; }
  protected override void OnConfiguring(DbContextOptionsBuilder options) => options
    .UseSqlServer("Server=localhost;Database=Products;User Id=sa;Password=sql@2023;MultipleActiveResultSets=true;Encrypt=yes;TrustServerCertificate=yes;");
}
