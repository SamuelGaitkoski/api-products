namespace Products.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        // Em um cenário que não seja de teste/aprendizado, autenticamos a password para não deixar ela no banco de dados
        // e termos problemas com segurança. Podemos usar uma biblioteca que ja faz toda essa parte de autenticação, não vai 
        // ser o caso aqui. Faremos aqui uma autenticação simples. Exemplo de biblioteca para autenticação é o ASP.NET Identity.
        public string Password { get; set; }
    }
}
