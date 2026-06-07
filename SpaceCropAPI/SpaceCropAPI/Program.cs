using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SpaceCropAPI.Data;
using SpaceCropAPI.Mappings;
using SpaceCropAPI.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// Controllers + JSON
// ======================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ======================================================
// Swagger
// ======================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SpaceCrop API",
        Version = "v1",
        Description = "API de monitoramento agrícola via satélite - FIAP Global Solution 2026 | SpaceCrop"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ======================================================
// Oracle / EF Core
// ======================================================
var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
builder.Services.AddDbContext<SpaceCropContext>(options =>
    options.UseOracle(connectionString)
           .EnableDetailedErrors()
           .EnableSensitiveDataLogging()
);

// ======================================================
// AutoMapper + Serviços customizados
// ======================================================
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddScoped<SequenceHelper>();

// ======================================================
// CORS
// ======================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ======================================================
// Build
// ======================================================
var app = builder.Build();

// ======================================================
// Swagger UI
// ======================================================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpaceCrop API v1");
    c.RoutePrefix = "swagger";
});

// ======================================================
// Global Exception Handler — expõe mensagem real em dev
// ======================================================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exceptionFeature = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (exceptionFeature == null) return;

        var ex = exceptionFeature.Error;
        var innerMsg = ex.InnerException?.Message ?? ex.Message;
        var fullMsg  = ex.ToString(); // stack trace completo para debug

        if (innerMsg.Contains("ORA-00001") || innerMsg.Contains("unique constraint"))
        {
            context.Response.StatusCode = 409;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 409,
                erro = "Registro duplicado.",
                mensagem = "Já existe um registro com esses dados."
            });
            return;
        }

        if (innerMsg.Contains("ORA-02292") || innerMsg.Contains("integrity constraint"))
        {
            context.Response.StatusCode = 409;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 409,
                erro = "Operação bloqueada por dependência.",
                mensagem = "Este registro possui dados relacionados e não pode ser removido diretamente."
            });
            return;
        }

        if (innerMsg.Contains("ORA-01400") || innerMsg.Contains("cannot insert NULL"))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 400,
                erro = "Campo obrigatório ausente.",
                mensagem = "Um ou mais campos obrigatórios não foram informados."
            });
            return;
        }

        if (innerMsg.Contains("ORA-01438"))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 400,
                erro = "Valor fora do limite.",
                mensagem = "Um dos campos enviados excede o tamanho máximo permitido."
            });
            return;
        }

        // Em desenvolvimento, expõe o erro real para facilitar debug
        context.Response.StatusCode = 500;
        var isDev = app.Environment.IsDevelopment();
        await context.Response.WriteAsJsonAsync(new
        {
            statusCode = 500,
            erro = "Erro interno no servidor.",
            mensagem = isDev ? fullMsg : "Ocorreu um erro inesperado. Tente novamente em instantes."
        });
    });
});

// Aplica migrations pendentes automaticamente ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SpaceCropContext>();
    db.Database.Migrate();
}

app.UseCors("AllowAll");
app.MapControllers();

app.Run("http://localhost:5000");
