using FinisServer.Configurations.Database;
using FinisServer.Services;
using FinisServer.Services.Impl;
using FinisServer.Configurations.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FinisServer.Models.Entities;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace FinisServerTest;

public class UnitTest1(ITestOutputHelper outputHelper)
{
    private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

    private readonly Func<IQwenService> getQwenService = () =>
    {
        var qwenOptions = _configuration.GetSection("Qwen").Get<QwenOptions>();
        Assert.NotNull(qwenOptions);
        var options = Options.Create(qwenOptions);
        IQwenService qwenService = new QwenService(options);
        return qwenService;
    };

    private readonly Func<FinisDbContext> getFinisDbContext = () =>
    {
        var postgresOptions = _configuration.GetSection("Postgres").Get<PostgresOptions>();
        var optionsBuilder = new DbContextOptionsBuilder<FinisDbContext>();
        optionsBuilder.UseNpgsql(postgresOptions!.ConnectionString, o => o.UseVector());
        return new FinisDbContext(optionsBuilder.Options);
    };

    [Fact]
    public async Task TextEmbeddingTestAsync()
    {
        var qwenService = getQwenService();
        var array = await qwenService.EmbeddingTexts([".net11"]);
        Assert.NotNull(array);
        outputHelper.WriteLine($"向量维度: {array[0].Count()}");
        outputHelper.WriteLine($"向量内容: [{string.Join(',', array)}]");
    }

    [Fact]
    public async Task VectorDbTestAsync()
    {
        string[] content = [
            "[H3]:GitHub vs GitLab 简要对比>GitHub 和 GitLab 都是基于 Git 的托管平台，核心功能相似，但侧重点不同。GitHub 拥有全球最大的开源社区和更成熟的生态系统（如 Actions），通常是开源项目和个人开发者的首选。GitLab 则以提供完整的一站式 DevOps 平台著称，其内置的 CI/CD 功能极为强大，且支持私有化部署，更受中大型企业青睐。| 特性 | GitHub | GitLab || :--- | :--- | :--- || 核心定位 | 侧重社区、开源与代码托管 | 侧重一站式 DevOps 平台 || CI/CD | 通过 GitHub Actions 提供 | 内置 CI/CD，配置简单且强大 || 私有化部署 | 支持 (Enterprise 版) | 支持 (社区版/企业版)，更普遍 || 开源社区 | 极其庞大，开发者首选 | 相对较小 || 优势场景 | 开源项目、生态集成 | 企业内部开发、私有流水线 |"
        ];
        var context = getFinisDbContext();
        var qwenService = getQwenService();
        var vectorArrays = await qwenService.EmbeddingTexts(content);
        Assert.NotNull(vectorArrays);
        for (int i = 0; i < vectorArrays.Count(); i++)
        {
            context.ArticleVectors.Add(new ArticleVector
            {
                Content = content[i],
                Embedding = new Vector(vectorArrays[i])
            });
        }

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ArticleChunkingAsync()
    {
        var qwenService = getQwenService();
        string content = "### GitHub vs GitLab 简要对比\n\nGitHub 和 GitLab 都是基于 Git 的托管平台，核心功能相似，但侧重点不同。GitHub 拥有全球最大的开源社区和更成熟的生态系统（如 Actions），通常是开源项目和个人开发者的首选。GitLab 则以提供完整的一站式 DevOps 平台著称，其内置的 CI/CD 功能极为强大，且支持私有化部署，更受中大型企业青睐。\n\n| 特性 | GitHub | GitLab |\n| :--- | :--- | :--- |\n| **核心定位** | 侧重社区、开源与代码托管 | 侧重一站式 DevOps 平台 |\n| **CI/CD** | 通过 GitHub Actions 提供 | 内置 CI/CD，配置简单且强大 |\n| **私有化部署** | 支持 (Enterprise 版) | 支持 (社区版/企业版)，更普遍 |\n| **开源社区** | 极其庞大，开发者首选 | 相对较小 |\n| **优势场景** | 开源项目、生态集成 | 企业内部开发、私有流水线 |";
        var contentList = await qwenService.ArticleChunkingAsync(content);
        outputHelper.WriteLine(string.Join("\n", contentList));
    }
    [Fact]
    public async Task SearchQueryAsync()
    {
        var context = getFinisDbContext();
        IQwenService qwenService = getQwenService();
        string[] query = ["Github 的特点"];
        var vectorArrays = await qwenService.EmbeddingTexts(query);
        var result = await context.ArticleVectors
            .AsNoTracking()
            .OrderBy(d => d.Embedding.CosineDistance(new Vector(vectorArrays[0])))
            .Where(d => d.Embedding.CosineDistance(new Vector(vectorArrays[0])) <= 0.35)
            .Take(5)
            .Select(d => d.Content)
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        outputHelper.WriteLine(string.Join("\n", result));
    }
}