public record ProductRequest(
  string Code, string Name, string Description, decimal Price, int CategoryId, string[] Tags
);
