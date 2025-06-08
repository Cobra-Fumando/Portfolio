Este projeto faz parte do meu portfólio pessoal e implementa um sistema de autenticação utilizando ASP.NET Core, Entity Framework Core e PostgreSQL. Ele possui suporte a login com email/senha e autenticação via Google.

🚀 Tecnologias utilizadas
ASP.NET Core

Entity Framework Core

PostgreSQL

Google.Apis.Auth (Login com Google)

JWT (Tokens de autenticação)

Hash seguro de senhas

⚙️ Pré-requisitos
Antes de rodar o projeto, certifique-se de ter instalado:

.NET 8.0 ou superior

PostgreSQL

EF Core CLI (caso não tenha)

🛠️ Configuração do banco de dados
Crie um banco de dados PostgreSQL (ex: portfolio_db).

Atualize a string de conexão no arquivo appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=portfolio_db;Username=seu_usuario;Password=sua_senha"
}

🔧 Executando as migrations
Depois de configurar a string de conexão corretamente, execute os seguintes comandos para aplicar as migrations no banco:

# Adiciona uma nova migration
dotnet ef migrations add Inicial

# Aplica a migration ao banco de dados
dotnet ef database update

Obs.: Execute esses comandos na pasta do projeto onde está o arquivo .csproj.

▶️ Executando o projeto
Depois de configurar o banco e rodar as migrations, você pode iniciar o projeto com:
dotnet run
