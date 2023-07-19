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

// tamb�m vamos utilizar essa outra configura��o aqui, para dizer para o nosso controller que ele vai utilizar a autentica��o,
// pelo Authorize que colocamos no ProductsController, precisamos configurar isso aqui para ele funcionar. Instalamos dai o pacote
// Microsoft.AspNetCore.Authentication.JwtBearer na vers�o 6.0.20.
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
        // essa key que ele est� pedidno para passarmos como parametro para o SymmetricSecurityKey, � a mesma key que utilizamos
        // para gerar o token que est� no AuthController, ent�o passamos ela aqui. Ele ta dizendo que � a key(chave) que ele vai
        // usar para descriptografar. Criamos a key da mesma forma que fizemos no AuthController aqui, la em cima. Ele vai precisar
        // dessa chave para criptografar (gerar o token) e para descriptografar, e ver que o usu�rio est� autenticado.
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configura��es para a autentica��o, alterando o builder.Services.AddSwaggerGen(), colamos o c�digo aqui, dizendo que ele vai
// passar a utilizar o Bearer, que serve para utilizarmos a autentica��o, ent�o vamos passar isso tamb�m no nosso header, que tem
// esse formato "Bearer {passamos o token dai}", vamos falar para o Swagger utilizar essa configura��o que colocamos aqui:
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

// Precisamos passar a nossa base de dados, aqui no nosso Program, configura��o do Entity Framework.
// No Program.cs fica toda as configura��es que antes ficavam no arquivo Startup.cs, que n�o existe mais no .NET 6.0.
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

// Aqui ja temos o UseAuthorization(), vamos adicionar tamb�m o UseAuthentication(), que tem que vir antes do UseAuthorization(),
// se n�o a autentica��o n�o funciona, n�o autoriza para autenticar, mesmo passando o token certo.
app.UseAuthentication();

app.UseAuthorization();

// Para que quando formos chamar as rotas para essa api, no front, n�o de o erro de CORS, informando nossa requisi��o a api foi
// bloqueada devido a n�o estarmos acessando a api de um mesmo dom�nio, pela mesma url da api. Erro que tamb�m informa que n�o temos
// nenhum header 'Access-Control-Allow-Origin' para o acesso a api, botamos a configura��o com o UseCors abaixo, liberando o acesso
// para qualquer origem, com qualquer header e tamb�m de qualquer origem. Ele estava bloqueando nossa requisi��o do front para o
// back porque era de outra origem, obviamente � outra origem porque � at� outra aplica��o, ele pega nossa rota base do front que
// � localhost:7200 e ve que n�o � igual a rota base do back, que � localhost:7116, ele viu que a requisi��o tava vindo de outra
// porta e de outro dom�nio, por isso ele bloqueou.
app.UseCors(p => p
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();

app.Run();

// Criando 2 aplica��es, back-end (ASP.NET Core 6.0) e front-end (Angular v13.0), base
// Trabalhando com base de dados SQL Server, utilizando o Entity Framework Core
// Autentica��o com perfil de usu�rio e trabalhar com tokens (JWT)
// Criando projeto C#: ASP.NET Core Web API no .NET 6.0
// Tema da aplica��o: Products
// Removendo model padr�o WeatherForecast e WeatherForecastController


