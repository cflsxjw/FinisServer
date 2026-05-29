using System.ComponentModel;
using System.Text;
using FinisServer.Configurations;
using FinisServer.Configurations.Database;
using FinisServer.Configurations.Options;
using FinisServer.Models.Entities;
using FinisServer.Models.Enums;
using FinisServer.Services;
using FinisServer.Services.Impl;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using BCrypt.Net;
using System.Text.RegularExpressions;
using StackExchange.Redis;


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
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IFinisHttpContext, FinisHttpContext>();
builder.Services.AddScoped<IQwenService, QwenService>();
builder.Services.AddScoped<IRankingService, RankingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
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

builder.Services.AddDbContext<TestDbContext>(options =>
{
    options.UseSqlite("Data Source=/workspace/FinisServer/JuejinArticles.db");
});


builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisOptions = builder.Configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ??
                       throw new InvalidOperationException($"{RedisOptions.SectionName} 配置缺失");
    return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
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
    static ArticleCategory displayNameToCategory(string cn)
    {
        return cn.Trim() switch
        {
            "前端" => ArticleCategory.Frontend,
            "后端" => ArticleCategory.Backend,
            "Android" => ArticleCategory.Android,
            "iOS" => ArticleCategory.iOS,
            "人工智能" => ArticleCategory.Ai,
            "开发工具" => ArticleCategory.Tools,
            "阅读" => ArticleCategory.Reading,
            "代码人生" => ArticleCategory.LifeNotes,
            _ => ArticleCategory.Reading
        };
    }
    string cleanMarkdown(string content)
    {
        // 1. (?im) 开启不区分大小写(i)和多行模式(m)
        // 2. ^\s*(?:```)?(arduino|csharp|python) 匹配可能带有 ``` 的语言标识
        // 3. [\s\S]*?复制代码 跨行匹配直到“复制代码”
        string pattern = @"\n(\w+)\n\n\n\n体验AI代码助手\n\n\n\n代码解读\n\n\n\n复制代码\n";
        string pattern2 = @"\n(\w+)\n\n\n\n体验AI代码助手\n\n\n\n复制代码\n";
        string pattern3 = @"\n体验AI代码助手\n\n\n\n代码解读\n\n\n\n复制代码\n";

        // 替换为 ```$1\n 
        // $1 是捕获到的语言名称（如 arduino）
        string s1 = Regex.Replace(content, pattern, "$1\n");
        string s2 = Regex.Replace(s1, pattern2, "$1\n");
        string s3 = Regex.Replace(s2, pattern3, "\n");
        return Regex.Replace(s3, @"!\[.*?\]\(.*?\)", "");
    }
    var dbContext = scope.ServiceProvider.GetService<FinisDbContext>() ?? throw new InvalidOperationException();
    var testDbContext = scope.ServiceProvider.GetService<TestDbContext>() ?? throw new InvalidOperationException();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();
    var services = scope.ServiceProvider;
    dbContext.Users.Add(new User
    {
        Name = "SuperAdmin",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("superadmin"),
        Email = "super.admin@gmail.com",
        Avatar = "DefaultAvatar.jpeg",
        SecurityQuestion = "用户名？",
        SecurityAnswerHash = BCrypt.Net.BCrypt.HashPassword("adminsuper"),
        Role = UserRole.SuperAdmin
    });
    dbContext.Users.Add(new User
    {
        Name = "cflsxjw",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
        Email = "cfl.sxjw@gmail.com",
        Avatar = "DefaultAvatar.jpeg",
        SecurityQuestion = "用户名？",
        SecurityAnswerHash = BCrypt.Net.BCrypt.HashPassword("sxjwcfl"),
        Role = UserRole.User
    });
    await dbContext.SaveChangesAsync();
    var rawTags = await testDbContext.TestTags.AsNoTracking().ToListAsync();
    var sqliteTags = rawTags.Select(s => new Tag
    {
        Name = s.TagName
    }).ToList();
    var rawSqliteArticles = await testDbContext.TestArticles
        .AsNoTracking()
        .ToListAsync();
    var sqliteArticles = rawSqliteArticles
        .Select(s => new Article
        {
            Title = s.Title,
            Summary = s.Summary,
            Category = displayNameToCategory(s.CategoryName),
            AuthorId = 1,
            Keywords = s.Keywords.Trim().Split(',') ?? [],
            ViewCount = s.ViewCount,
            Content = new ArticleContent
            {
                Content = cleanMarkdown(s.Content)
            },
            ArticleVectors = []
        }).ToList();
    using var transaction = await dbContext.Database.BeginTransactionAsync();
    // 4. 先插入 Tags (如果 Article 依赖 Tag，必须先插主表)
    if (sqliteTags.Any())
    {
        await dbContext.Tags.AddRangeAsync(sqliteTags);
        await dbContext.SaveChangesAsync();
    }
    // 5. 插入 Articles
    if (sqliteArticles.Any())
    {
        await dbContext.Articles.AddRangeAsync(sqliteArticles);
        await dbContext.SaveChangesAsync();
    }
    await transaction.CommitAsync();

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
