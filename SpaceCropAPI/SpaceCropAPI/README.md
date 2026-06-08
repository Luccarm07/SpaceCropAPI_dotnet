# 🛰️ SpaceCrop API

**C# · .NET 8 · ASP.NET Core · Oracle Database · Entity Framework Core · Swagger/OpenAPI**

---

## 📋 Sumário

- [Sobre o Projeto](#sobre-o-projeto)
- [Equipe](#equipe)
- [Objetivo do Challenge](#objetivo-do-challenge)
- [Arquitetura do Projeto](#arquitetura-do-projeto)
- [Visão de Domínio](#visão-de-domínio)
- [Diagramas](#diagramas)
- [Documentação das Rotas](#documentação-das-rotas)
- [Banco de Dados](#banco-de-dados)
- [Instruções de Instalação e Execução](#instruções-de-instalação-e-execução)
- [Testes da API](#testes-da-api)
- [Observações Finais](#observações-finais)

---

## 📌 Sobre o Projeto

O **SpaceCrop** é uma API REST desenvolvida em **C# com ASP.NET Core (.NET 8)** para monitoramento inteligente de lavouras via dados de sensores orbitais e satélites. A proposta central é a **detecção automatizada de anomalias agrícolas**: toda vez que um sensor orbital registra uma leitura que ultrapassa o valor crítico configurado, um **Compound Trigger Oracle** dispara automaticamente, classifica o tipo de alerta e notifica o produtor rural responsável.

O sistema gerencia o ciclo completo: cadastro de usuários e fazendas, setores de plantio, satélites e seus sensores orbitais, ingestão de leituras de campo, geração automática de alertas via trigger Oracle, e registro das ações de resolução tomadas pelos agricultores.

O projeto demonstra uma aplicação backend robusta com:
- arquitetura em camadas bem definida
- persistência relacional via Oracle Database com sequences nativas
- mapeamento objeto-relacional com Entity Framework Core
- uso estratégico de **ADO.NET direto** para contornar limitações do EF Core com Oracle (`ORA-00904`, `ORA-01425`)
- transformação de dados com AutoMapper e DTOs
- documentação automática e interativa via Swagger
- lógica de negócio encapsulada no banco: **1 Compound Trigger, 3 Procedures, 2 Functions e 1 Package**

---

## 👥 Equipe

| Nome | RM |
|------|----|
| Lucas Grillo Alcântara | 561413 |
| Pietro Ferreira Gomes Abrahamian | 561469 |
| Pedro Peres Benitez | 561792 |
| Lucca Ramos Mussumecci | 562027 |

**Turma:** 2TDSPX

---

## 🎯 Objetivo do Challenge

Desenvolver uma solução utilizando C# e ASP.NET Core capaz de:

- persistir dados em banco relacional Oracle com sequences, triggers e procedures
- monitorar lavouras com sensores orbitais e gerar alertas automáticos via banco de dados
- aplicar Programação Orientada a Objetos com entidades bem definidas e relacionamentos N:N e 1:N
- utilizar Entity Framework Core com migrations e relacionamentos entre entidades
- garantir validações e tratamento de exceções específicas do Oracle
- respeitar os fundamentos de APIs REST (verbos HTTP, códigos de status, recursos aninhados)
- disponibilizar documentação interativa via Swagger/OpenAPI
- atender aos requisitos técnicos da disciplina Advanced Business Development with .NET — FIAP 2026

---

## 🧱 Arquitetura do Projeto

O projeto segue uma arquitetura em camadas com separação clara de responsabilidades:

```
SpaceCropAPI/
├── Controllers/                    # Camada de apresentação — endpoints REST
│   ├── UsuariosController.cs
│   ├── FazendasController.cs
│   ├── SetoresController.cs
│   ├── SatelitesController.cs
│   ├── SensoresController.cs
│   ├── LeiturasController.cs
│   └── AlertasController.cs
│
├── Data/
│   └── SpaceCropContext.cs         # DbContext do Entity Framework Core
│
├── DTOs/
│   └── SpaceCropDTOs.cs            # Objetos de entrada e saída (Request/Response/Paginação)
│
├── Mappings/
│   └── MappingProfile.cs           # Configurações do AutoMapper
│
├── Migrations/                     # Migrações geradas pelo EF Core
│
├── Models/
│   └── SpaceCropModels.cs          # Entidades mapeadas para as tabelas Oracle
│
├── Services/
│   └── SequenceHelper.cs           # Helper para obter NEXTVAL de sequences Oracle via ADO.NET
│
├── appsettings.json                # Configurações da aplicação (connection string)
└── Program.cs                      # Bootstrap, middlewares e injeção de dependências
```

---

## 🧠 Visão de Domínio

| Entidade | Descrição |
|----------|-----------|
| 👤 **Usuário** | Produtor rural cadastrado no sistema. Dono das fazendas e responsável por resolver alertas. |
| 🌾 **Fazenda** | Propriedade rural vinculada a um usuário, com área em hectares, cidade e estado. |
| 🟩 **Setor de Plantio** | Subdivisão de uma fazenda, com cultura específica (soja, milho, cana etc.) e sua área. |
| 🛰️ **Satélite** | Satélite orbital cadastrado no sistema com nome e operador. |
| 🔬 **Sensor Orbital** | Instrumento embarcado em um satélite com tipo e valor crítico configurado. |
| 📡 **Leitura de Satélite** | Coleta de dado de campo feita por um sensor orbital em uma fazenda/setor. |
| ⚠️ **Alerta** | Gerado automaticamente pelo banco quando uma leitura supera o valor crítico do sensor. |
| ✅ **Ação de Alerta** | Registro da resolução tomada pelo usuário para encerrar um alerta ativo. |

---

## 📊 Diagramas

### 🏗️ Diagrama de Arquitetura em Camadas

![Diagrama de Arquitetura em Camadas](./diagramas/Diagrama_de_Arquitetura_em_Camadas.png)

A API segue uma arquitetura em camadas: o **Cliente** acessa os **Controllers** (camada de Apresentação), que se comunicam com a camada de **Aplicação** (AutoMapper, DTOs, SequenceHelper) e com o domínio via **Models**. O acesso ao banco Oracle é feito através do **SpaceCropContext** com EF Core + ADO.NET direto para operações sensíveis.

---

### 🗂️ Diagrama de Classes — Modelo de Dados

![Diagrama de Classes](./diagramas/Diagrama_de_Classes_Modelo_de_Dados.png)

O modelo de dados contempla **11 tabelas** com relacionamentos 1:N e N:N. Destaque para a entidade `LeituraSatelite`, que conecta o mundo orbital (sensores/satélites) ao mundo agrícola (fazendas/setores), e para `Alerta` e `AcaoAlerta`, que representam o ciclo de detecção e resolução de problemas.

---

### 🔄 Fluxo de Criação de Leitura e Geração de Alerta

![Fluxo de Leitura e Alerta](./diagramas/Fluxo_de_Criacao_de_Leitura_e_Geracao_de_Alerta.png)

1. O cliente envia `POST /leituras` com sensor, fazenda e valor medido
2. O EF Core obtém o próximo ID via `SEQ_LEITURA.NEXTVAL` e insere em `TB_LEITURA_SATELITE`
3. O **Compound Trigger `TRG_ALERTA_AUTOMATICO`** dispara automaticamente após o INSERT
4. O trigger consulta o `NR_VALOR_CRITICO` do sensor, e se ultrapassado, armazena o alerta numa coleção temporária
5. Após o statement, todos os alertas são inseridos em `TB_ALERTA` de forma atômica
6. O cliente recebe `201 Created` e pode consultar alertas gerados via `GET /fazendas/{id}/alertas`

---

### ✅ Fluxo de Resolução de Alerta

![Fluxo de Resolução de Alerta](./diagramas/Diagrama_de_Resolucao_de_Alerta.png)

1. O usuário envia `PUT /alertas/{id}/resolver` com `usuarioId` e `acaoTomada`
2. A API valida o usuário e verifica se o alerta já foi resolvido via **ADO.NET direto**
3. Obtém o próximo ID de `SEQ_ACAO.NEXTVAL`
4. Em uma transação: atualiza `FL_RESOLVIDO = 'S'` em `TB_ALERTA` e insere em `TB_ACAO_ALERTA`
5. Retorna `200 OK` com o alerta atualizado

---

## 📦 Documentação das Rotas

A API roda em `http://localhost:5000`. Todos os recursos de listagem suportam paginação via `?page=0&size=10`.

> Acesse a documentação interativa completa em: **`http://localhost:5000/swagger`**

---

### 👤 Usuários — `/usuarios`

| Método | Rota | Descrição | Parâmetros de Query |
|--------|------|-----------|---------------------|
| `GET` | `/usuarios` | Lista usuários com paginação, busca por nome e ordenação | `nome`, `orderBy`, `orderDir`, `page`, `size` |
| `GET` | `/usuarios/{id}` | Busca usuário pelo ID | — |
| `POST` | `/usuarios` | Cadastra novo usuário (senha armazenada como SHA-256) | — |
| `PUT` | `/usuarios/{id}` | Atualiza dados do usuário | — |
| `DELETE` | `/usuarios/{id}` | Remove usuário e todos os dependentes em cascata | — |

**Request body (POST / PUT):**
```json
{
  "nome": "João Silva",
  "email": "joao@email.com",
  "senha": "senha123"
}
```

**Response (200 / 201):**
```json
{
  "id": 1,
  "nome": "João Silva",
  "email": "joao@email.com"
}
```

> ⚠️ A senha nunca é retornada nas respostas. É armazenada como hash SHA-256 em `DS_SENHA_HASH`.

> O DELETE remove em cascata: fazendas, setores, leituras, alertas e ações de alerta vinculados.

---

### 🌾 Fazendas — `/fazendas`

| Método | Rota | Descrição | Parâmetros de Query |
|--------|------|-----------|---------------------|
| `GET` | `/fazendas` | Lista fazendas com paginação. Filtra por `usuarioId` | `usuarioId`, `page`, `size` |
| `GET` | `/fazendas/{id}` | Busca fazenda pelo ID | — |
| `POST` | `/fazendas` | Cadastra nova fazenda vinculada a um usuário | — |
| `PUT` | `/fazendas/{id}` | Atualiza dados da fazenda | — |
| `DELETE` | `/fazendas/{id}` | Remove fazenda e todos os dependentes | — |

**Request body (POST / PUT):**
```json
{
  "nome": "Fazenda Santa Clara",
  "cidade": "Ribeirão Preto",
  "estado": "SP",
  "areaHectares": 1500.50,
  "usuarioId": 1
}
```

**Response (200 / 201):**
```json
{
  "id": 1,
  "usuarioId": 1,
  "nome": "Fazenda Santa Clara",
  "cidade": "Ribeirão Preto",
  "estado": "SP",
  "areaHectares": 1500.50
}
```

---

### 🟩 Setores de Plantio — `/fazendas/{fazendaId}/setores`

| Método | Rota | Descrição | Parâmetros de Query |
|--------|------|-----------|---------------------|
| `GET` | `/fazendas/{fazendaId}/setores` | Lista setores de uma fazenda com paginação | `page`, `size` |
| `POST` | `/fazendas/{fazendaId}/setores` | Cadastra setor em uma fazenda | — |
| `PUT` | `/setores/{id}` | Atualiza dados do setor | — |
| `DELETE` | `/setores/{id}` | Remove setor (desvincula leituras associadas) | — |

**Request body (POST / PUT):**
```json
{
  "nome": "Setor Norte",
  "cultura": "Soja",
  "areaHectares": 400.00
}
```

**Response (200 / 201):**
```json
{
  "id": 1,
  "fazendaId": 1,
  "nome": "Setor Norte",
  "cultura": "Soja",
  "areaHectares": 400.00
}
```

> O DELETE de setor **não remove** as leituras — apenas desvincula o setor (`IdSetor = null`) para preservar o histórico.

---

### 🛰️ Satélites — `/satelites`

| Método | Rota | Descrição | Parâmetros de Query |
|--------|------|-----------|---------------------|
| `GET` | `/satelites` | Lista todos os satélites com paginação | `page`, `size` |
| `GET` | `/satelites/{id}/sensores` | Lista sensores orbitais de um satélite | `page`, `size` |

**Response GET /satelites (200):**
```json
{
  "content": [
    {
      "id": 1,
      "nome": "Landsat-9",
      "operador": "NASA/USGS",
      "ativo": true
    }
  ],
  "page": 0,
  "size": 10,
  "totalElements": 3,
  "totalPages": 1,
  "last": true
}
```

> Satélites e sensores são dados de referência pré-cadastrados via script SQL. Não possuem endpoints de criação/atualização pela API.

---

### 🔬 Sensores Orbitais — `/sensores`

| Método | Rota | Descrição | Parâmetros de Query |
|--------|------|-----------|---------------------|
| `GET` | `/sensores` | Lista sensores com busca por nome ou tipo e filtro de ativo | `busca`, `ativo`, `page`, `size` |
| `GET` | `/sensores/{id}` | Busca sensor orbital pelo ID | — |

**Response GET /sensores (200):**
```json
{
  "content": [
    {
      "id": 1,
      "sateliteId": 1,
      "tipoSensorId": 2,
      "nome": "Sensor NDVI Landsat-9",
      "ativo": true,
      "tipoNome": "NDVI",
      "unidadeMedida": "índice",
      "valorCritico": 0.30
    }
  ],
  "page": 0,
  "size": 10,
  "totalElements": 5,
  "totalPages": 1,
  "last": true
}
```

---

### 📡 Leituras de Satélite — `/leituras`

| Método | Rota | Descrição | Parâmetros de Query |
|--------|------|-----------|---------------------|
| `POST` | `/leituras` | Insere nova leitura (simula coleta de satélite) | — |
| `GET` | `/leituras/{id}` | Busca leitura por ID | — |
| `GET` | `/fazendas/{fazendaId}/leituras` | Lista leituras da fazenda com filtros e ordenação | `orderBy`, `orderDir`, `anomalia`, `page`, `size` |
| `GET` | `/fazendas/{fazendaId}/leituras/ultimas` | Últimas leituras da fazenda, ordenadas da mais recente | `page`, `size` |

**Request body (POST):**
```json
{
  "sensorOrbitalId": 1,
  "fazendaId": 1,
  "setorId": 2,
  "valor": 150.5,
  "dataLeitura": "2026-06-07T10:00:00",
  "anomalia": false
}
```

**Response (201):**
```json
{
  "id": 150,
  "sensorOrbitalId": 1,
  "fazendaId": 1,
  "setorId": 2,
  "valor": 150.5,
  "dataLeitura": "2026-06-07T10:00:00",
  "anomalia": false,
  "sensorNome": "Sensor NDVI Landsat-9",
  "unidadeMedida": "índice"
}
```

> 🔥 **Destaque:** ao inserir uma leitura com valor acima do `NR_VALOR_CRITICO` do sensor, o **Compound Trigger Oracle `TRG_ALERTA_AUTOMATICO`** dispara automaticamente e cria o alerta correspondente em `TB_ALERTA`.

**Filtros disponíveis (GET /fazendas/{id}/leituras):**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `orderBy` | `data` \| `valor` | Campo de ordenação |
| `orderDir` | `asc` \| `desc` | Direção da ordenação |
| `anomalia` | `true` \| `false` | Filtra apenas leituras com/sem anomalia |

---

### ⚠️ Alertas — `/alertas`

| Método | Rota | Descrição | Parâmetros de Query |
|--------|------|-----------|---------------------|
| `GET` | `/alertas` | Lista todos os alertas com filtros de status e severidade | `resolvido`, `severidade`, `page`, `size` |
| `GET` | `/alertas/{id}` | Busca alerta por ID | — |
| `GET` | `/fazendas/{fazendaId}/alertas` | Lista alertas de uma fazenda, do mais recente | `page`, `size` |
| `PUT` | `/alertas/{id}/resolver` | Resolve um alerta registrando a ação tomada | — |

**Request body (PUT /alertas/{id}/resolver):**
```json
{
  "usuarioId": 5,
  "acaoTomada": "Irrigação acionada manualmente no setor norte."
}
```

**Response (200):**
```json
{
  "id": 15,
  "leituraId": 150,
  "tipoAlertaId": 2,
  "usuarioId": 5,
  "resolvido": true,
  "dataAlerta": "2026-06-07T10:00:00",
  "tipoNome": "Estresse Hídrico",
  "severidade": "ALTA"
}
```

**Filtros disponíveis (GET /alertas):**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `resolvido` | `true` \| `false` | Filtra por status de resolução |
| `severidade` | `BAIXA` \| `MEDIA` \| `ALTA` | Filtra por nível de severidade |

> ⚠️ **Importante:** A resolução usa **ADO.NET direto** (não EF Core) para evitar o erro `ORA-00904` causado pela geração de `TRUE`/`FALSE` pelo provider Oracle do EF Core em queries com booleanos.

---

### 🚨 Respostas de Erro

A API trata globalmente os erros Oracle com respostas padronizadas:

| Código HTTP | Situação |
|-------------|----------|
| `400` | Campo obrigatório ausente (`ORA-01400`) ou valor fora do limite (`ORA-01438`) |
| `400` | Alerta já resolvido (validação de negócio) |
| `404` | Recurso não encontrado (fazenda, sensor, usuário, alerta, etc.) |
| `409` | Registro duplicado — email já existente (`ORA-00001`) |
| `409` | Bloqueio por dependência de FK (`ORA-02292`) |
| `500` | Erro interno inesperado |

---

## 🗄️ Banco de Dados

### Estrutura Oracle

O banco de dados Oracle foi projetado com **11 tabelas**, **12 sequences**, **1 Compound Trigger**, **3 Procedures**, **2 Functions** e **1 Package**.

| Tabela | Descrição |
|--------|-----------|
| `TB_USUARIO` | Produtores rurais cadastrados no sistema |
| `TB_FAZENDA` | Fazendas vinculadas a usuários |
| `TB_SETOR_PLANTIO` | Setores de plantio dentro de cada fazenda |
| `TB_SATELITE` | Satélites orbitais operacionais |
| `TB_TIPO_SENSOR` | Tipos de sensores com unidade de medida e valor crítico |
| `TB_SENSOR_ORBITAL` | Sensores embarcados nos satélites |
| `TB_LEITURA_SATELITE` | Leituras de campo capturadas pelos sensores |
| `TB_TIPO_ALERTA` | Tipos de alerta com severidade e flag de ação requerida |
| `TB_ALERTA` | Alertas gerados automaticamente pelo trigger |
| `TB_ACAO_ALERTA` | Ações de resolução registradas pelos usuários |

### Diferenciais do Banco

- **`TRG_ALERTA_AUTOMATICO`** — Compound Trigger que avalia cada leitura inserida: busca o `NR_VALOR_CRITICO` do sensor, marca a leitura como anomalia (`FL_ANOMALIA = 'S'`), determina o tipo de alerta correto e insere em `TB_ALERTA` de forma atômica após o statement completo.
- **Sequences nativas** — Todos os IDs são gerados por sequences Oracle (`SEQ_USUARIO`, `SEQ_FAZENDA`, `SEQ_LEITURA`, `SEQ_ALERTA`, `SEQ_ACAO`, etc.) via `SequenceHelper` no .NET.
- **Procedures e Functions** — Lógica de negócio adicional encapsulada no banco para relatórios e consultas analíticas.

### Executar o script SQL no Oracle

```
spacecrop.sql          — DDL: tabelas e sequences
spacecrop_PLSQL.sql    — Trigger, procedures, functions e package
spacecrop_dml.sql      — Dados de referência (satélites, sensores, tipos de alerta)
```

Execute nesta ordem no **Oracle SQL Developer** ou via SQL*Plus:

```sql
@spacecrop.sql
@spacecrop_PLSQL.sql
@spacecrop_dml.sql
```

---

## 🚀 Instruções de Instalação e Execução

### Pré-requisitos

Antes de iniciar, certifique-se de ter instalado:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Oracle Database (XE, 19c ou superior) com os scripts executados

---

### Passo 1 — Clonar o repositório

```bash
git clone https://github.com/Luccarm07/SpaceCropAPI.git
cd SpaceCropAPI_dotnet-main/SpaceCropAPI
```

---

### Passo 2 — Configurar a string de conexão (OBRIGATÓRIO)

Edite o arquivo `SpaceCropAPI/appsettings.json` com as credenciais do seu Oracle:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=SEU_HOST:1521/XEPDB1;"
  }
}
```

> Para Oracle XE local, o `Data Source` geralmente é `localhost:1521/XEPDB1`.

---

### Passo 3 — Executar o banco de dados

Execute os scripts SQL **na ordem** no Oracle SQL Developer ou SQL*Plus:

```sql
@spacecrop.sql          -- cria tabelas e sequences
@spacecrop_PLSQL.sql    -- cria trigger, procedures, functions e package
@spacecrop_dml.sql      -- insere dados de referência (satélites, tipos de sensor, tipos de alerta)
```

---

### Passo 4 — Executar a aplicação

```bash
cd SpaceCropAPI
dotnet restore
dotnet run
```

A API estará disponível em:

```
http://localhost:5000
```

A documentação Swagger estará disponível em:

```
http://localhost:5000/swagger
```

---

### Resumo rápido

```bash
git clone https://github.com/Luccarm07/SpaceCropAPI.git
cd SpaceCropAPI_dotnet-main/SpaceCropAPI/SpaceCropAPI

# 1. Execute os scripts SQL no Oracle (spacecrop.sql → spacecrop_PLSQL.sql → spacecrop_dml.sql)
# 2. Edite appsettings.json com suas credenciais Oracle

dotnet restore
dotnet run

# Acesse: http://localhost:5000/swagger
```

---

## 🧪 Testes da API

Os endpoints podem ser testados via **Swagger UI**, **Postman** ou **Insomnia**.

Fluxo sugerido respeitando as dependências de FK:

1. `POST /usuarios` — cadastrar um produtor rural
2. `POST /fazendas` — criar uma fazenda vinculada ao usuário
3. `POST /fazendas/{id}/setores` — criar setores de plantio na fazenda
4. `GET /satelites` — consultar satélites disponíveis (pré-cadastrados)
5. `GET /sensores` — consultar sensores orbitais (pré-cadastrados com valor crítico)
6. `POST /leituras` — registrar leitura de satélite com valor **abaixo** do crítico → sem alerta
7. `POST /leituras` — registrar leitura com valor **acima** do crítico → alerta gerado automaticamente!
8. `GET /fazendas/{id}/alertas` — visualizar alertas gerados pelo trigger Oracle
9. `PUT /alertas/{id}/resolver` — resolver o alerta com a ação tomada
10. `GET /alertas?resolvido=false&severidade=ALTA` — filtrar alertas pendentes de alta severidade
11. `GET /fazendas/{id}/leituras?anomalia=true&orderBy=data&orderDir=desc` — leituras com anomalia

### Exemplos de teste de paginação e filtros

```
GET /usuarios?nome=João&orderBy=email&orderDir=asc&page=0&size=5
GET /sensores?busca=NDVI&ativo=true&page=0&size=10
GET /fazendas/{id}/leituras/ultimas?page=0&size=5
GET /alertas?resolvido=false&severidade=ALTA&page=0&size=10
```

---

## 🧭 Observações Finais

O SpaceCrop foi desenvolvido com foco em:

- **Arquitetura em camadas** clara: Controllers → DTOs → AutoMapper → Models → EF Core + ADO.NET → Oracle
- **Inteligência no banco de dados**: o Compound Trigger Oracle centraliza a lógica de detecção de anomalias, garantindo que nenhuma leitura crítica passe despercebida independentemente de quem insira o dado
- **Tratamento robusto de exceções Oracle** com mensagens amigáveis ao cliente (sem expor stack trace)
- **Uso híbrido de EF Core + ADO.NET direto** para contornar incompatibilidades do provider Oracle com booleanos e LIKE
- **Paginação padronizada** em todos os endpoints de listagem com `PagedResponseDTO<T>`
- **Padrão REST** com verbos HTTP semânticos, recursos aninhados (`/fazendas/{id}/setores`) e códigos de status corretos
- **Documentação automática** completa via Swagger/OpenAPI

Desenvolvido como parte do **Challenge 2TDSPX — Advanced Business Development with .NET — FIAP 2026**
