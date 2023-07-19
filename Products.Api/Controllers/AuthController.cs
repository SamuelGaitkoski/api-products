using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Products.Api.Data;
using Products.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;

namespace Products.Api.Controllers
{
    // Parte de autenticação, para criarmos usuários e conseguir autenticar esses usuários, dar permissão para eles,
    //gerenciar essas permissões no front-end também, podemos fazer com que só o administrador consiga apagar ou editar um produto.

    //Para autenticar essa aplicação, vamos usar um método muito comum no desenvolvimento utilizando api's, tanto para autenticar
    //uma aplicação front-end web ou uma aplicação mobile, que é o JWT (Jason Web Token), que é um Token que passamos no header das
    //nossas requisições e conseguimos autenticar nossa aplicação, autenticar o usuário para ter acesso no nosso servidor/api.
    //É diferente daquela antiga autenticação que usava cookies do browser e ficava no próprio cliente.Dessa forma nosso back-end
    //não guarda mais estado, é uma autenticação que não guarda estado, quem guarda o token emitido pelo back-end é o front-end,
    //e em toda requisição que ele faz ele envia esse token via header e autentica no nosso back-end.

    //Criamos o controller para a autenticação e login.
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        // Injeção de dependência do nosso contexto.
        private readonly ProductsContext _context;

        // Aqui vamos precisar de um user, então vamos criar uma entidade User

        // criação do nosso construtor e iniciar nosso contexto
        public AuthController(ProductsContext context) { 
            _context = context;
        }

        // Precisamos criar um usuário para logarmos, então vamos criar um método para isso
        [HttpPost]
        [Route("/account")]
        public async Task<IActionResult> CreateAccount(User user)
        {
            if(_context.Users.Any(p => user.Name == p.Name || user.Email == p.Email))
            {
                return BadRequest();
            }

            // Vamos no nosso contexto e adicionamos o user.
            // await pois o método é assíncrono.
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Aqui vamos retornar nosso usuário e o token também, então vamos criar um método para criarmos esse token, esse JWT.
            // não vamos retornar o user direto, para ele não retornar a password, por questões de segurança, sempre que criamos um
            // usuário não devolvemos a senha para o front-end, ele não precisa conhecer a senha, porque sempre que ele vai
            // autenticar ele vai utilizar o token que vamos gerar.
            // vamos retornar um objeto que vai retornar um objeto user, que vai ter o id, nome e email, e o token, que vai ser
            // gerado quando o usuário for criado.
            // retornando um Ok (200) com o retorno que temos.
            return Ok(new
            {
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email
                }//,
                //token = GenerateToken()
            });
            // O token é o que vamos passar no header da nossa aplicação no front, quando formos chamar a api.
        }

        // Precisamos de um método para fazer login com o usuário.
        // Aqui, para não passarmos o user, vamos criar uma nova classe no model, que não vamos mapear para o banco de dados, 
        // vai ser como uma view model, ou dto, que são um objeto para manipular valores, que será a entidade Login.
        // colocamos também nossa rota, no Route, para o Swagger não se confundir e conseguir identificar nossas rotas e endpoints.
        [HttpPost]
        [Route("/auth/login")]
        public async Task<IActionResult> Login(Login login)
        {
            // Nosso Login vai ser bem simples, como não criptogramamos nossa password, vamos procurar um user no banco de dados,
            // onde o email e a password correspondam ao email e password passados no login, passado no body.
            // Where no contexto devolve um IQueryable, então por isso fazemos o FirstOrDefault.
            var user = await _context.Users.Where(p => p.Email == login.Email && p.Password == login.Password).FirstOrDefaultAsync();

            // se ele não encontrar o User (user == null)
            if (user == null) {
                // vamos informar que ele não encontrou. Não vamos colocar um NotFound pois estamos fazendo uma autenticação, vamos 
                // retornar um Unauthorized, dizendo que ele não tem autorização para fazer login, pois não foi encontrada a senha,
                // ou o email do usuário.
                return Unauthorized();
            }

            // se ele encontrou, vai retornar um Ok passando o User com o qual ele fez o login, passando o Token também para ele
            // autenticar futuramente.
            return Ok(new
            {
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email
                },
                token = GenerateToken()
            });
        }

        // Método para criarmos o Token JWT, que retorna uma string, que é o JWT.
        // botamos o método pronto aqui.
        // vamos adicionar os users aqui.
        // tiramos a parte dos subjects de dentro do tokenDescriptor, pois não vamos utilizar as claimns, então não vamos precisar
        // passar o user como paramêtro mais.
        // Quando formos ver sobre o Identity usamos as claims, pois ele vai ter isso implementado, ai quando formos criar um token
        // podemos usar as claimns, que servem, nesse de geração de token, para, por exemplo, darmos permissões para cada
        // autenticação, se formos um administrador podemos gerar um token, e ele vai conseguir acessar todas nossas url's
        // e endpoint's, se temos uma permissão menor, podemos permitir um token que só tenha leitura e cadastro, e assim por diante.
        private static string GenerateToken()
        {
            // criamos uma classe do tipo JwtSecurityTokenHandler, utilizando a bibliteca do JWT, que é interna aqui, não precisamos
            // instalar nada.
            var tokenHandler = new JwtSecurityTokenHandler();
            // aqui precisamos passar uma chave (key) que ele vai usar para criptografar esse token, a string grande que passamos
            // é a nossa chave.
            var key = Encoding.ASCII.GetBytes("eyJhbGciOiJSUzI1NiIsImtpZCI6Ikt1M3BuYTU3NFppWjR4TFZsQTB");
            // Aqui no tokenDescriptor, no Expires temos o tempo de expiração do token, o token que emitimos só é valido por 1 hora,
            // depois disso ele ja não consegue mais autenticar na aplicação. E no SigningCredentials temos alguns detalhes de
            // criptografia, qual o algoritmo, etc.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            };
            // Aqui é onde criamos o token.
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Vamos usar o swagger para criarmos esse novo User, Swagger cria uma outra rota para o api/auth

        // Precisamos configurar nossa aplicação para que ela seja autenticável, para que não tenha acesso quando não seja passado
        // o header com o token e que ele consiga autenticar (ter acesso) quando passar esse token. Isso vamos fazer aqui, 
        // no ProductsController. Para isso, em cima de cada método precisamos passar uma anotação [Authorize], fazendo isso ele
        // vai pedir a autorização nos endpoints da nossa api. Botando a anotação na controller em si, ele vai bloquear toda a nossa
        // controller em si dai. Vamos deixar o GetProducts liberado para testarmos no Swagger, pois la podemos autorizar ou não.
        // Vamos adicionar algumas outras configurações no nosso Program.cs.
        // Após realizar o restante das configurações no Program.cs, está tudo ok, então vamos testar isso via Swagger.
        // Testamos o login no Swagger, está tudo ok, retornando o usuário que fez o login e o token.
        // Na parte de api não vamos precisar fazer mais nada, vamos agora para o nosso front-end.
    }
}
