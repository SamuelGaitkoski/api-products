namespace Products.Api.Models
{
    public class Product
    {
        // propriedades
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        // o enum é convertido para um inteiro, então category vai ter um inteiro que representa o valor do enum, mas podemos
        // converter o inteiro para texto para apresentar isso no banco.
        public ProductCategory Category { get; set; }
    }

    // enumerador que vai ter as categorias dos produtos
    public enum ProductCategory
    {
        Food = 0,
        Convenience = 1,
        Commodities = 2,
        Durables = 3,
        Digital = 4
    }
}
