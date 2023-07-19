using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Api.Data;
using Products.Api.Models;

namespace Products.Api.Controllers
{
    // Controller MVC Empty traz o Controller, vamos trocar para o ControllerBase.
    // Configurando as rotas, para ele mapear a rota sempre para api/nome do controller, nesse caso api/products
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // Criando um método
        // Para criar os produtos precisamos de uma base de dados para guardar os produtos, então vamos instalar
        // o Entity Framework e configurar ele para nossa base de dados, dai damos seguimento nisso.
        // Pacotes que instalamos:
        // Microsoft.EntityFrameworkCore.SqlServer na versão 6.0.4
        // Microsoft.EntityFrameworkCore na versão 6.0.4
        // Microsoft.EntityFrameworkCore.Tools na versão 6.0.4 - Nos permite usar as migrations e aplicar elas na BD.
        // Esses métodos de migrations chamam Code First, onde criamos o contexto, criamos a classe com as propriedades
        // e ele pega isso e mapeia tudo para uma base de dados que vai ser o SQL Server.
        // no appsettings.json vamos criar nossa connection string para o banco de dados.
        // O contexto é o que faz a ligação do Entity Framework com a nossa base de dados.
        // Ja injetamos no program o AddDbContext, que nos permite injetarmos nosso contexto no nosso controller, que seria
        // colocar isso abaixo:
        private readonly ProductsContext _context;

        // construtor, passamos o contexto sem precisarmos instanciar ele manualmente.
        public ProductsController(ProductsContext context)
        {
            _context = context;
        }

        // mapeando esse método para o /products
        [HttpGet]
        [Route("/products")]
        [Authorize]
        public async Task<ActionResult> GetProducts()
        {
            // O método Ok mapeia para o método http 200.
            // Como estamos usando um método assíncrono usamos o await para não ficarmos com nenhum warning no código.
            // ToListAsync para listar em formato de lista os produtos.
            return Ok(await _context.Products.ToListAsync());
        }

        // para conseguirmos visualizar isso no nosso banco de dados, vamos criar um método POST.
        // mapeando esse método para o /products
        [HttpPost]
        [Route("/products")]
        [Authorize]
        public async Task<ActionResult> CreateProduct(Product product)
        {
            await _context.Products.AddAsync(product);
            // salvar isso no banco
            await _context.SaveChangesAsync();

            // retornando isso em um 200, retornarndo o product dai, ele vai criar um product na base, com id e retornar
            // para nós o produto com o id do mesmo dai.
            return Ok(product);
        }

        // Ele ja traz para nós o que criamos aqui em um Swagger por default, então é só executarmos o projeto e testarmos.
        // Equipe do ASP.NET trouxe o swagger implementado nessa aplicação/projeto que criamos, então usamos para testar a API,
        // não precisamos de um postman para isso.
        // Isso acaba por ser muito útil como documentação da API.

        [HttpPut]
        [Route("/products")]
        [Authorize]
        public async Task<ActionResult> UpdateProduct(Product product)
        {
            // validando se o produto existe na nossa base de dados
            var produtoExistente = await _context.Products.FindAsync(product.Id);

            // se produto não existe na base de dados, retorna um NotFound(), sem texto mesmo, só para exemplificar.
            if (produtoExistente == null)
            {
                return NotFound();
            }

            // se produtoExistente não for nulo, vamos atualizar as propriedades do produtoExistente com o que veio do product
            produtoExistente.Name = product.Name;
            produtoExistente.Price = product.Price;
            produtoExistente.Category = product.Category;

            // salvando as alterações no banco de dados.
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpDelete]
        [Route("/products")]
        [Authorize]
        public async Task<ActionResult> DeleteProduct(Guid Id)
        {
            // validando se o produto existe na nossa base de dados
            var produtoExistente = await _context.Products.FindAsync(Id);

            // se produto não existe na base de dados, retorna um NotFound(), sem texto mesmo, só para exemplificar.
            if (produtoExistente == null)
            {
                return NotFound();
            }

            // se produtoExistente não for nulo, ele existe na nossa base de dados, então vamos deletar ele.
            // Só damos await com o método do contexto é assincrono, ex: FindAsync()
            _context.Products.Remove(produtoExistente);

            // salvando as alterações no banco de dados.
            await _context.SaveChangesAsync();

            // retornando um 200 com um objeto com success = true, message = "" e data = product (removido), idéia só
            //return Ok(new
            //{
            //    data = produtoExistente
            //    success = true,
            //    message = ""
            //})
            // vamos retornar um NoContent() - 204;
            return NoContent();
        }

        // Vamos criar o front-end também, para consumirmos essa api que acabamos de criar.
        // E também vamos fazer a parte de autenticação do Json Web Token (JWT), para vermos como trabalhamos com isso.
        // Temos 2 aplicações, o back-end e o front-end.
        // Com a anotação Authorize nos métodos aqui dos produtos e com as configurações que fizemos no Program.cs, quando
        // fazemos alguma requisição daqui da controller, para os produtos, ele retorna um 401, informando que não estamos
        // autorizados. Ai, se fizermos o login na rota /auth/login, obtemos o token e no Swagger podemos clicar no 
        // botão Authorize, utilizando o Bearer e passamos la no campo Value "Bearer (Token que obtemos)", exemplo: Bearer eygUTPS...
        // dai clicamos em Authorize e ele libera para nós fazermos requisições para os métodos dos produtos.
    }
}
