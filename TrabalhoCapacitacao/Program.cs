using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Para Swagger
using System.Text;
using TrabalhoCapacitacao.Data;
using TrabalhoCapacitacao.Models;
// using SeuProjeto.Data; // Substitua pelo namespace do seu AppDbContext
// using SeuProjeto.Models; // Substitua pelo namespace da sua entidade Usuario

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Serviços (Injeção de Dependência)

// Adicionar DbContext para Entity Framework Core
// Certifique-se de que a string de conexão "DefaultConnection" está no seu appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Adicionar IPasswordHasher para hashing de senhas
// Não é necessário AddIdentityCore completo se apenas IPasswordHasher for usado,
// mas registrar diretamente é mais leve.
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Configurar Autenticação JWT Bearer (para tokens emitidos pela API)
var jwtSettings = builder.Configuration.GetSection("JWT");
var jwtSecret = jwtSettings["Secret"];

if (string.IsNullOrEmpty(jwtSecret))
{
    // Lançar uma exceção ou logar um erro crítico se a chave secreta não estiver definida
    // Em produção, esta chave NUNCA deve ser hardcoded ou facilmente acessível.
    // Considere usar User Secrets para desenvolvimento e Azure Key Vault (ou similar) para produção.
    throw new InvalidOperationException("A chave secreta JWT (JWT:Secret) não está configurada no appsettings.json.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true; // Opcional: guarda o token no HttpContext após validação
    options.RequireHttpsMetadata = builder.Environment.IsProduction(); // Exigir HTTPS em produção
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Verifica se o token não expirou e se a data de início é válida
        ValidateIssuerSigningKey = true, // Valida a assinatura do token
        ClockSkew = TimeSpan.Zero, // Remove a tolerância de tempo padrão na validação de expiração

        ValidAudience = jwtSettings["ValidAudience"],
        ValidIssuer = jwtSettings["ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Configurar Autorização (opcional, mas geralmente usado com autenticação)
builder.Services.AddAuthorization(options =>
{
    // Exemplo de política de autorização baseada em roles (perfis)
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("admin"));
    options.AddPolicy("EmpresaPolicy", policy => policy.RequireRole("empresa", "admin")); // Empresa OU Admin
    // Adicione outras políticas conforme necessário
});

// Adicionar serviços de controllers
builder.Services.AddControllers();

// Configurar CORS (Cross-Origin Resource Sharing) - importante para frontend Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient",
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:4200") // URL do seu cliente Angular em desenvolvimento
                   .AllowAnyHeader()
                   .AllowAnyMethod();
            // Em produção, seja mais restritivo com as origens, headers e métodos.
        });
    // Adicione outras políticas de CORS se necessário
});


// Configurar Swagger/OpenAPI para documentação e teste da API (útil em desenvolvimento)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API da Plataforma de Oportunidades", Version = "v1" });

    // Adicionar definição de segurança JWT para o Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, insira 'Bearer' seguido de um espaço e o token JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey, // Ou Http com Scheme = "bearer" e BearerFormat = "JWT"
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }});
});


// 2. Configurar o Pipeline de Requisições HTTP

var app = builder.Build();

// Usar Swagger apenas em ambiente de desenvolvimento

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Plataforma V1");
    // c.RoutePrefix = string.Empty; // Para aceder ao Swagger UI na raiz da aplicação (opcional)
});


// Redirecionar HTTP para HTTPS (recomendado em produção)
app.UseHttpsRedirection();

// Usar CORS - deve vir antes de UseAuthentication e UseAuthorization
// Use a política que você definiu. Em desenvolvimento, pode ser mais permissivo.
app.UseCors("AllowAngularDevClient"); // Ou uma política mais restritiva para produção

// Adicionar middleware de Autenticação
app.UseAuthentication();

// Adicionar middleware de Autorização
app.UseAuthorization();

// Mapear os controllers para as rotas
app.MapControllers();

app.Run();
