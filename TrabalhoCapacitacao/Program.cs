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

// 1. Configurar Servi�os (Inje��o de Depend�ncia)

// Adicionar DbContext para Entity Framework Core
// Certifique-se de que a string de conex�o "DefaultConnection" est� no seu appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Adicionar IPasswordHasher para hashing de senhas
// N�o � necess�rio AddIdentityCore completo se apenas IPasswordHasher for usado,
// mas registrar diretamente � mais leve.
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Configurar Autentica��o JWT Bearer (para tokens emitidos pela API)
var jwtSettings = builder.Configuration.GetSection("JWT");
var jwtSecret = jwtSettings["Secret"];

if (string.IsNullOrEmpty(jwtSecret))
{
    // Lan�ar uma exce��o ou logar um erro cr�tico se a chave secreta n�o estiver definida
    // Em produ��o, esta chave NUNCA deve ser hardcoded ou facilmente acess�vel.
    // Considere usar User Secrets para desenvolvimento e Azure Key Vault (ou similar) para produ��o.
    throw new InvalidOperationException("A chave secreta JWT (JWT:Secret) n�o est� configurada no appsettings.json.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true; // Opcional: guarda o token no HttpContext ap�s valida��o
    options.RequireHttpsMetadata = builder.Environment.IsProduction(); // Exigir HTTPS em produ��o
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Verifica se o token n�o expirou e se a data de in�cio � v�lida
        ValidateIssuerSigningKey = true, // Valida a assinatura do token
        ClockSkew = TimeSpan.Zero, // Remove a toler�ncia de tempo padr�o na valida��o de expira��o

        ValidAudience = jwtSettings["ValidAudience"],
        ValidIssuer = jwtSettings["ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Configurar Autoriza��o (opcional, mas geralmente usado com autentica��o)
builder.Services.AddAuthorization(options =>
{
    // Exemplo de pol�tica de autoriza��o baseada em roles (perfis)
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("admin"));
    options.AddPolicy("EmpresaPolicy", policy => policy.RequireRole("empresa", "admin")); // Empresa OU Admin
    // Adicione outras pol�ticas conforme necess�rio
});

// Adicionar servi�os de controllers
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
            // Em produ��o, seja mais restritivo com as origens, headers e m�todos.
        });
    // Adicione outras pol�ticas de CORS se necess�rio
});


// Configurar Swagger/OpenAPI para documenta��o e teste da API (�til em desenvolvimento)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API da Plataforma de Oportunidades", Version = "v1" });

    // Adicionar defini��o de seguran�a JWT para o Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, insira 'Bearer' seguido de um espa�o e o token JWT",
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


// 2. Configurar o Pipeline de Requisi��es HTTP

var app = builder.Build();

// Usar Swagger apenas em ambiente de desenvolvimento

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Plataforma V1");
    // c.RoutePrefix = string.Empty; // Para aceder ao Swagger UI na raiz da aplica��o (opcional)
});


// Redirecionar HTTP para HTTPS (recomendado em produ��o)
app.UseHttpsRedirection();

// Usar CORS - deve vir antes de UseAuthentication e UseAuthorization
// Use a pol�tica que voc� definiu. Em desenvolvimento, pode ser mais permissivo.
app.UseCors("AllowAngularDevClient"); // Ou uma pol�tica mais restritiva para produ��o

// Adicionar middleware de Autentica��o
app.UseAuthentication();

// Adicionar middleware de Autoriza��o
app.UseAuthorization();

// Mapear os controllers para as rotas
app.MapControllers();

app.Run();
