# 🔐 Autenticação e Autorização (JWT)

A API utiliza **ASP.NET Core Identity + JWT Bearer Token** para autenticação e controle de acesso.

---

# 📌 Fluxo de Autenticação

1. Usuário realiza login enviando email e senha.
2. A API valida no ASP.NET Core Identity.
3. Um **JWT (JSON Web Token)** é gerado.
4. O token deve ser enviado nas requisições protegidas.

---

# ▶️ Login
### Endpoint
POST `/api/auth/login`

### Exemplo de requisição
```json
    {
      "email": "admin@admin.com",
      "password": "Admin@123"
    }
```

Resposta
```json
    {
      "accessToken": "eyJhbGciOiJIUzI1NiIs..."
    }
```

## 🪪 Como usar o token
Após o login:
Copie o accessToken
Envie no header Authorization:
```Código
Authorization: Bearer SEU_TOKEN
```

## 🧪 Usando no Swagger
Execute o endpoint /api/auth/login
Copie o token retornado
Clique em Authorize no Swagger
Cole somente o token
Clique em Authorize → Close
Agora você pode testar endpoints protegidos
Não escreva "Bearer" manualmente no Swagger
Ele já adiciona automaticamente.

## 👤 Roles (Papéis de usuário)
A API utiliza controle por roles:
. Role	Permissões
. Admin	Acesso total
. Instructor	Gerenciar cursos
. Student	Acessar apenas próprios dados

Exemplo:
Criar curso → Admin ou Instructor
Deletar curso → apenas Admin
Ver próprio perfil → Student autenticado

## 🚫Erros comuns
401 Unauthorized
Token não enviado ou inválido.

Verifique:
Se colou o token no Swagger
Se não digitou "Bearer Bearer"
403 Forbidden
Usuário autenticado sem permissão.

Exemplo:
Student tentando deletar curso
Student tentando ver outro estudante
404 Not Found

O recurso não existe:
Curso inexistente
Student não encontrado

## 🔒Segurança aplicada
JWT assinado com chave secreta
Roles incluídas no token
StudentId nunca confiado ao cliente
Validação contra duplicidade
Soft delete de estudante
Transações em matrículas

## 📚Observação
Este projeto foi desenvolvido com foco em boas práticas de APIs REST, segurança e organização de domínio, simulando um backend real de plataforma educacional.

# 🧾 Agora o commit perfeito
## Commit message (curto)
docs: adiciona documentação de autenticação JWT e uso no Swagger

## Extended description
Documenta fluxo de login e uso de JWT
Explica autenticação via Swagger
Detalha roles e permissões
Lista erros comuns (401, 403, 404)
Adiciona arquivo docs/authentication.md para portfólio

