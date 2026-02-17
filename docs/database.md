📦 Modelagem de Dados

Este documento descreve a modelagem inicial deste banco de dados da aplicação CursoDigio.

🎓 Course

Tabela: Courses

Campos:

Id (PK)

Titulo : string

Descricao : string

Categoria : string

CargaHoraria : int

DataCriacao : datetime

👤 Student

Tabela: Students

Campos:

Id (PK)

NomeCompleto : string

Email : string (UNIQUE)

DataCadastro : datetime

📝 Enrollment

Tabela: Enrollments

Campos:

Id (PK)

StudentId (FK)

CourseId (FK)

Status : int

DataMatricula : datetime

Restrição:

UNIQUE (StudentId, CourseId)

🔗 Relacionamentos

Student 1..N Enrollment

Course 1..N Enrollment

Enrollment atua como tabela de junção explícita.

🖼 Diagrama

O diagrama da modelagem pode ser encontrado em:

/docs/images/diagram.png
