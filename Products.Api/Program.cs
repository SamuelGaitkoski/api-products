using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Products.Api.Data;
using System.Security.Cryptography.Xml;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// também vamos utilizar essa outra configuração aqui, para dizer para o nosso controller que ele vai utilizar a autenticação,
// pelo Authorize que colocamos no ProductsController, precisamos configurar isso aqui para ele funcionar. Instalamos dai o pacote
// Microsoft.AspNetCore.Authentication.JwtBearer na versão 6.0.20.
var key = Encoding.ASCII.GetBytes("eyJhbGciOiJSUzI1NiIsImtpZCI6Ikt1M3BuYTU3NFppWjR4TFZsQTB");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        // essa key que ele está pedidno para passarmos como parametro para o SymmetricSecurityKey, é a mesma key que utilizamos
        // para gerar o token que está no AuthController, então passamos ela aqui. Ele ta dizendo que é a key(chave) que ele vai
        // usar para descriptografar. Criamos a key da mesma forma que fizemos no AuthController aqui, la em cima. Ele vai precisar
        // dessa chave para criptografar (gerar o token) e para descriptografar, e ver que o usuário está autenticado.
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configurações para a autenticação, alterando o builder.Services.AddSwaggerGen(), colamos o código aqui, dizendo que ele vai
// passar a utilizar o Bearer, que serve para utilizarmos a autenticação, então vamos passar isso também no nosso header, que tem
// esse formato "Bearer {passamos o token dai}", vamos falar para o Swagger utilizar essa configuração que colocamos aqui:
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Bearer {token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Precisamos passar a nossa base de dados, aqui no nosso Program, configuração do Entity Framework.
// No Program.cs fica toda as configurações que antes ficavam no arquivo Startup.cs, que não existe mais no .NET 6.0.
// No UseSqlServer vamos passar nossa connection string, acessando ela pelo builder.Configuration
builder.Services
    .AddDbContext<ProductsContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("ServerConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aqui ja temos o UseAuthorization(), vamos adicionar também o UseAuthentication(), que tem que vir antes do UseAuthorization(),
// se não a autenticação não funciona, não autoriza para autenticar, mesmo passando o token certo.
app.UseAuthentication();

app.UseAuthorization();

// Para que quando formos chamar as rotas para essa api, no front, não de o erro de CORS, informando nossa requisição a api foi
// bloqueada devido a não estarmos acessando a api de um mesmo domínio, pela mesma url da api. Erro que também informa que não temos
// nenhum header 'Access-Control-Allow-Origin' para o acesso a api, botamos a configuração com o UseCors abaixo, liberando o acesso
// para qualquer origem, com qualquer header e também de qualquer origem. Ele estava bloqueando nossa requisição do front para o
// back porque era de outra origem, obviamente é outra origem porque é até outra aplicação, ele pega nossa rota base do front que
// é localhost:7200 e ve que não é igual a rota base do back, que é localhost:7116, ele viu que a requisição tava vindo de outra
// porta e de outro domínio, por isso ele bloqueou.
app.UseCors(p => p
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();

app.Run();

// Criando 2 aplicações, back-end (ASP.NET Core 6.0) e front-end (Angular v13.0), base
// Trabalhando com base de dados SQL Server, utilizando o Entity Framework Core
// Autenticação com perfil de usuário e trabalhar com tokens (JWT)
// Criando projeto C#: ASP.NET Core Web API no .NET 6.0
// Tema da aplicação: Products
// Removendo model padrão WeatherForecast e WeatherForecastController


