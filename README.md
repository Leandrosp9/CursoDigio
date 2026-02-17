#CursoDigio

##📌 Objetivo
Esta API foi desenvolvida como parte do Curso Digio, com o objetivo de simular um backend de uma plataforma de aprendizado online.
O sistema permite:
Gerenciamento de Cursos
Cadastro de Estudantes (integrados ao ASP.NET Core Identity)
Controle de Matrículas
Aplicação de regras de negócio (ex.: impedir inscrições duplicadas)
Persistência de dados via SQL Server utilizando Entity Framework Core

##🛠️ Tecnologias Utilizadas
ASP.NET Core Web API (.NET 8)
Entity Framework Core 8 (ORM)
SQL Server / LocalDB
ASP.NET Core Identity
JWT Bearer Authentication
Swagger / OpenAPI
Git & GitHub

##⚙️ Requisitos
Para executar o projeto localmente:
.NET 8 SDK
Visual Studio 2022 ou superior
SQL Server LocalDB (padrão do Visual Studio)

##▶️ Executar Localmente
Clonar o repositório
git clone <url-do-repositorio>
Abrir a Solution no Visual Studio
Restaurar dependências (automático)
Aplicar migrations (caso necessário)
Update-Database


Executar o projeto
F5 ou Ctrl + F5

##🗄️ Banco de Dados
A aplicação utiliza SQL Server LocalDB para desenvolvimento.
As tabelas são geradas via Entity Framework Core Migrations, incluindo:
Estruturas do domínio (Course, Student, Enrollment)
Estruturas do ASP.NET Core Identity

##🔐 Autenticação
A API utiliza ASP.NET Core Identity + JWT Bearer Tokens.

Fluxos suportados:
Registro de usuário → POST /api/auth/register
Login → POST /api/auth/login
Refresh Token → POST /api/auth/refresh
Documentação detalhada disponível em:
/docs/authentication.md

##🧩 Modelo de Domínio
Principais entidades da aplicação:
Course → Cursos disponíveis na plataforma
Student → Estudantes vinculados ao Identity
Enrollment → Matrículas (tabela de junção explícita)

Regras aplicadas:
Relacionamento N:N entre Student e Course via Enrollment
Índice único para evitar matrículas duplicadas

##🌐 Endereços da Aplicação
Swagger UI:
https://localhost:7065/swagger

API Base URL:
https://localhost:7065

##📚 Documentação
A documentação técnica complementar pode ser encontrada em:
/docs

Exemplos:
authentication.md
authorization.md
endpoints.md

##🚀 Status do Projeto
Projeto em evolução, com foco em:
Boas práticas de API REST
Integração com Identity
Segurança com JWT
Organização de domínio e infraestrutura
