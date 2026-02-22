# CursoDigio
API REST desenvolvida em **ASP.NET Core Web API (.NET 8)** para simular o backend de uma plataforma de aprendizado online, com **gerenciamento de cursos**, **perfis de estudantes integrados ao ASP.NET Core Identity** e **matrículas**, aplicando regras de negócio e segurança com **JWT**.

---

## 📌 Objetivo
Esta API foi desenvolvida como parte do Curso Digio, com o objetivo de simular um backend de uma plataforma de aprendizado online.
O sistema permite:
- Gerenciamento de **Cursos**
- Cadastro/gerenciamento de **Estudantes** (perfil vinculado ao **Identity**)
- Controle de **Matrículas**
- Regras de negócio (ex.: **impedir matrícula duplicada**)
- Persistência via **SQL Server LocalDB** usando **Entity Framework Core**

## 🛠️ Tecnologias Utilizadas
ASP.NET Core Web API (.NET 8)
Entity Framework Core 8 (ORM)
SQL Server / LocalDB
ASP.NET Core Identity
JWT Bearer Authentication
Swagger / OpenAPI
Git & GitHub

## ⚙️ Requisitos
Para executar o projeto localmente:
.NET 8 SDK
Visual Studio 2022 ou superior
SQL Server LocalDB (padrão do Visual Studio)

## ▶️ Executar Localmente
1) Clonar o repositório
```bash
git clone <url-do-repositorio>
```
2) Abrir a Solution no Visual Studio
3) Aplicar migrations (caso necessário)
```bash
Update-Database
```
4) Executar
F5 (Debug) ou Ctrl+F5 (sem Debug)


Executar o projeto
F5 ou Ctrl + F5

## 🗄️ Banco de Dados
A aplicação utiliza SQL Server LocalDB para desenvolvimento.
As tabelas são geradas via EF Core Migrations, incluindo:
. Domínio: Course, Student, Enrollment
. Identity: tabelas do ASP.NET Core Identity

## 🔐 Autenticação
A API utiliza ASP.NET Core Identity + JWT Bearer Tokens.

Login
POST /api/auth/login
Exemplo:
      JSON
      {
        "email": "admin@admin.com",
        "password": "Admin@123"
      }

Resposta:
      {
        "accessToken": "eyJhbGciOiJIUzI1NiIs..."
      }

## Como autenticar no Swagger
Faça login em POST /api/auth/login
Copie o valor de accessToken
No Swagger, clique em Authorize
Cole apenas o token (sem escrever “Bearer”)
Teste endpoints protegidos
O Swagger já adiciona automaticamente Authorization: Bearer {token}.

## Endpoints 
Courses
POST /api/courses — Admin, Instructor
GET /api/courses — Público (com paginação/filtro)
GET /api/courses/{id} — Público
PUT /api/courses/{id} — Admin, Instructor
DELETE /api/courses/{id} — Admin

Students
POST /api/students — Admin
GET /api/students — Admin
GET /api/students/{id} — Autenticado (recomendado: Admin ou proprietário)
PUT /api/students/{id} — Admin ou proprietário
DELETE /api/students/{id} — Admin (soft delete)
GET /api/students/me — Autenticado (perfil do usuário logado)

Enrollments
POST /api/enrollments — Autenticado
Student matricula a si (StudentId vem do token)
Admin pode informar studentId
GET /api/enrollments/{id}/enrollments — Autenticado
Admin vê qualquer estudante
Student vê apenas as próprias matrículas


## 🧩 Modelo de Domínio
. Course: cursos disponíveis na plataforma
. Student: perfil de estudante vinculado ao Identity (UsuarioId)
. Enrollment: matrícula (tabela de junção explícita entre Student e Course)

Regras aplicadas:
Relação N:N entre Student e Course via Enrollment
Proteção contra matrícula duplicada (validação + índice único)
StudentId não é aceito do cliente quando usuário é Student (segurança)

## 🌐 Endereços da Aplicação
Swagger UI:
https://localhost:7065/swagger

API Base URL:
https://localhost:7065

## 📚 Documentação
. Swagger / OpenAPI disponível via /swagger
. Documentação complementar: /docs

## 🚀 Status do Projeto
Projeto em evolução com foco em:
Boas práticas de API REST
Integração com Identity
Segurança com JWT
Regras de negócio e validações
Documentação (Swagger + README)
