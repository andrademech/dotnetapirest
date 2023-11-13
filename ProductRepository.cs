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
