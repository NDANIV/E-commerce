using ECommerce.Application.Common;
using MediatR;
using System.Text;
using ECommerce.Application;
using ECommerce.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

     // Definir esquema JWT Bearer
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Ingrese 'Bearer {token}' para autenticarse",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// MediatR: escanea el assembly de Application
builder.Services.AddMediatR(typeof(ApplicationAssemblyMarker).Assembly);

// Application (MediatR, Validators, Pipeline)
builder.Services.AddApplication();

// Infrastructure (DbContext, Identity, JWT service)
builder.Services.AddInfrastructure(builder.Configuration);

// ====== JWT Authentication ======
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Config Jwt:Key es obligatoria.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ECommerce";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ECommerce";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // tokens expiran exactamente cuando deben
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS + AuthZ

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
