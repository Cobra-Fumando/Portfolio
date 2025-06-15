using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Classes;
using Portfolio.Conexao;
using Portfolio.Config;
using Portfolio.Interfaces;
using Portfolio.Tabelas;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProjeto, Projeto>();
builder.Services.AddScoped<IUsers, Users>();
builder.Services.AddScoped<IAdmin, Admin>();
builder.Services.AddScoped<Token>();
builder.Services.AddScoped<IPasswordHasher<Usuarios>, PasswordHasher<Usuarios>>();
builder.Services.AddTransient<Hash>();
builder.Services.AddMemoryCache();

var key = Encoding.UTF8.GetBytes(builder.Configuration["Token:Key"]);
var keyAdmin = Encoding.UTF8.GetBytes(builder.Configuration["TokenAdmin:Key"]);

builder.Services.AddAuthentication("Usuarios")
    .AddJwtBearer("Usuarios", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = builder.Configuration["Token:Audience"],
            ValidIssuer = builder.Configuration["Token:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    })
    .AddJwtBearer("Administrador", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters 
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = builder.Configuration["TokenAdmin:Audience"],
            ValidIssuer = builder.Configuration["TokenAdmin:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddCors(config =>
{
    config.AddPolicy("all", policy =>
    {
        policy.AllowAnyMethod()
        .AllowAnyOrigin()
        .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsync(
            "{\"message\": \"Calma lá! Você está fazendo requisições demais.\"}", token);
    };

    options.AddFixedWindowLimiter("fixed", config =>
    {
        config.Window = TimeSpan.FromSeconds(5);
        config.PermitLimit = 5;
        config.QueueLimit = 0;
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Adm", config =>
    {
        config.Window = TimeSpan.FromSeconds(5);
        config.PermitLimit = 20;
        config.QueueLimit = 0;
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("all");

//app.Urls.Add("http://0.0.0.0:5212");

app.Run();
