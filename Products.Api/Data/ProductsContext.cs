using Microsoft.EntityFrameworkCore;
using Products.Api.Models;

namespace Products.Api.Data
{
    public class ProductsContext : DbContext
    {
        // construtor
        // chamando o método base  do DbContext, passando o options
        public ProductsContext(DbContextOptions<ProductsContext> options) : base(options) 
        { 
        }

        // precisamos reescrever um método também do DbContext, que é o OnConfiguring.
        // esse método passa o mapeamento do nosso appsettings.json, para utilizarmos o SQL Server buscando a connection string
        // que colocamos no nosso appsettings.json.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            optionsBuilder.UseSqlServer(configuration.GetConnectionString("ServerConnection"));
        }

        // precisamos adicionar um DbSet para o modelo/entidade Products que criamos, vai criar a tabela Products dai
        public DbSet<Product> Products { get; set; }

        // Adicionamos um novo modelo, que é o User, então temos que adicionar no nosso contexto para ele criar a tabela
        public DbSet<User> Users { get; set; }

        // como criamos o novo modelo/entidade User e a tabela, temos que adicionar e aplicar uma migração novamente.

        // Vamos em Tools -> NuGet Package Manager -> Package Manager Console
        // Rodando a migration para criar o BD:
        // Add-Migration 'InitialMigration' - builda nossa aplicação também, cria um protótipo do que vai fazer no BD
        // O Entity Framework tem muitas funcionalidades.
        // Remove-Migration para removermos a migration que criamos.
        // Update-Database - Aplicar a migration no banco de dados.
    }
}
