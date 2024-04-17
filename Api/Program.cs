using JwtAuthentication.Authenticators;
using JwtAuthentication.Catalog.User;
using JwtAuthentication.Common.MongoDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;

const string AllowedOrigin = "*";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters() {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddMongo()
    .AddMongoRepository<User>("Users");
    

builder.Services.AddSingleton<JwtAuthenticationManager>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(swagger => {
    swagger.UseInlineDefinitionsForEnums();
    swagger.SwaggerDoc("v1",new OpenApiInfo { Title = "JwtAuthenticate Api",Version = "v1" });
    swagger.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme() {
        In = ParameterLocation.Header,
        Description = "Please Enter Your JWT Token Here",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
    });

    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(swagger => {
    swagger.SwaggerEndpoint("/swagger/v1/swagger.json","JwtAuthenticate Api v1");
});

if(app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();

    app.UseCors(builder => {
        builder.WithOrigins(AllowedOrigin)
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
