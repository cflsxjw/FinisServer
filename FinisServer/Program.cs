using System.Text;
using FinisServer.Configurations;
using FinisServer.Configurations.Database;
using FinisServer.Configurations.Options;
using FinisServer.Models.Entities;
using FinisServer.Services;
using FinisServer.Services.Impl;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// 注册错误处理中间件
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();


// 注册服务
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IFinisHttpContext, FinisHttpContext>();
builder.Services.AddScoped<IQwenService, QwenService>();
builder.Services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();


// 注册配置类
// Redis
builder.Services.AddOptions<RedisOptions>()
    .Bind(builder.Configuration.GetSection(RedisOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
// Jwt
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
// Mariadb
builder.Services.AddOptions<PostgresOptions>()
    .Bind(builder.Configuration.GetSection(PostgresOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
// Qwen
builder.Services.AddOptions<QwenOptions>()
    .Bind(builder.Configuration.GetSection(QwenOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// 数据库
builder.Services.AddDbContext<FinisDbContext>(options =>
{
    var postgresOptions = builder.Configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>() ??
                         throw new InvalidOperationException($"{PostgresOptions.SectionName} 配置缺失");
    options.UseNpgsql(connectionString: postgresOptions.ConnectionString,
        optionsBuilder =>
        {
            optionsBuilder.UseVector();
        });
});

// 注册 Redis 缓存
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisOptions = builder.Configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ??
                       throw new InvalidOperationException($"{RedisOptions.SectionName} 配置缺失");
    options.Configuration = redisOptions.ConnectionString;
    options.InstanceName = redisOptions.InstanceName;
});

// 注册 Jwt Bearer
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ??
                         throw new InvalidOperationException($"{JwtOptions.SectionName} 配置缺失");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = FinisJwtClaimTypes.Role,
            NameClaimType = FinisJwtClaimTypes.Username
        };
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();
// 创建数据库
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetService<FinisDbContext>() ?? throw new InvalidOperationException();
    // await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();
}

// 使用错误处理器
app.UseExceptionHandler();

app.UseRouting();
app.UseCors();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
// 身份验证
app.UseAuthentication();
// 准入验证
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
