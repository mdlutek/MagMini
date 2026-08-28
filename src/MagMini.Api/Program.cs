using MagMini.Api.Services;
using MagMini.Application.Common.Interfaces;
using MagMini.Application.DTOs.Articles;
using MagMini.Application.DTOs.Categories;
using MagMini.Application.DTOs.Customers;
using MagMini.Application.DTOs.Orders;
using MagMini.Domain.Enums;
using MagMini.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Rejestracja warstwy danych i serwisów z projektu Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Obsługa JWT i tożsamości
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, ApiCurrentUserService>();

// 3. Konfiguracja uwierzytelniania JWT Bearer
var secretKey = builder.Configuration["JwtSettings:SecretKey"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// 4. Konfiguracja Swaggera z kłódką (Authorize z JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MagMini ERP API", Version = "v1" });    

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Wpisz token w formacie: Bearer {twoj_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });

    c.DocumentFilter<SwaggerTagOrderFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Włączenie uwierzytelniania i autoryzacji w potoku HTTP
app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// 1. ENDPOINT LOGOWANIA (PUBLICZNY)
// ==========================================
app.MapPost("/api/auth/login", async (IAuthService authService, JwtTokenService jwtService, LoginRequest request) =>
{
    var result = await authService.LoginAsync(request.Username, request.Password);
    if (!result.Success || result.User == null)
    {
        return Results.Unauthorized();
    }

    var token = jwtService.GenerateToken(result.User);
    return Results.Ok(new
    {
        token = token,
        user = result.User
    });
}).WithTags("Auth");

// ==========================================
// 2. ENDPOINTY ZABEZPIECZONE (.RequireAuthorization())
// ==========================================

// ARTYKUŁY
var articlesGroup = app.MapGroup("/api/articles").WithTags("Articles").RequireAuthorization();
articlesGroup.MapGet("/", async (IArticleService service, [AsParameters] ArticleFilterDto filter) => Results.Ok(await service.GetPagedAsync(filter)));
articlesGroup.MapGet("/{id:int}", async (IArticleService service, int id) => (await service.GetForEditAsync(id)) is { } a ? Results.Ok(a) : Results.NotFound());
articlesGroup.MapPost("/", async (IArticleService service, SaveArticleDto dto) => (await service.SaveAsync(dto)) is { Success: true } ? Results.Ok() : Results.BadRequest());
articlesGroup.MapDelete("/{id:int}", async (IArticleService service, int id) => (await service.DeleteAsync(id)) is { Success: true } ? Results.Ok() : Results.BadRequest());

// KONTRAHENCI
var customersGroup = app.MapGroup("/api/customers").WithTags("Customers").RequireAuthorization();
customersGroup.MapGet("/", async (ICustomerService service, [AsParameters] CustomerFilterDto filter) => Results.Ok(await service.GetPagedAsync(filter)));
customersGroup.MapGet("/lookup/{nip}", async (ICompanyLookupService service, string nip) => (await service.LookupByNipAsync(nip)) is { IsSuccess: true } res ? Results.Ok(res) : Results.BadRequest());
customersGroup.MapPost("/", async (ICustomerService service, SaveCustomerDto dto) => (await service.SaveAsync(dto)) is { Success: true } ? Results.Ok() : Results.BadRequest());
customersGroup.MapDelete("/{id:int}", async (ICustomerService service, int id) => (await service.DeleteAsync(id)) is { Success: true } ? Results.Ok() : Results.BadRequest());

// ZAMÓWIENIA
var ordersGroup = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();
ordersGroup.MapGet("/", async (IOrderService service, [AsParameters] OrderFilterDto filter) => Results.Ok(await service.GetPagedAsync(filter)));
ordersGroup.MapGet("/{id:int}", async (IOrderService service, int id) => (await service.GetForEditAsync(id)) is { } o ? Results.Ok(o) : Results.NotFound());
ordersGroup.MapPost("/", async (IOrderService service, SaveOrderDto dto) => (await service.SaveAsync(dto)) is { Success: true } ? Results.Ok() : Results.BadRequest());
ordersGroup.MapPut("/{id:int}/status", async (IOrderService service, int id, OrderStatus status) => (await service.ChangeStatusAsync(id, status)) is { Success: true } ? Results.Ok() : Results.BadRequest());

// KATEGORIE
var categoriesGroup = app.MapGroup("/api/categories").WithTags("Categories").RequireAuthorization();
categoriesGroup.MapGet("/", async (ICategoryService service, string? search) => Results.Ok(await service.GetAllAsync(search)));
categoriesGroup.MapPost("/", async (ICategoryService service, SaveCategoryDto dto) => (await service.SaveAsync(dto)) is { Success: true } ? Results.Ok() : Results.BadRequest());

app.Run();

public record LoginRequest(string Username, string Password);


public class SwaggerTagOrderFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new() { Name = "Auth", Description = "🔑 Uwierzytelnianie i pobieranie tokenu JWT" },
            new() { Name = "Articles", Description = "📦 Zarządzanie kartoteką towarów i stanami" },
            new() { Name = "Categories", Description = "🏷️ Słownik kategorii artykułów" },
            new() { Name = "Customers", Description = "👥 Kartoteka kontrahentów i weryfikacja GUS/MF" },
            new() { Name = "Orders", Description = "📝 Dokumenty zamówień od klientów (ZK)" }
        };
    }
}