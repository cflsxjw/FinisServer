using FinisServer.Configurations.Database;
using FinisServer.Services;
using FinisServer.Services.Impl;
using FinisServer.Configurations.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Markdig;
using System.Diagnostics.Tracing;
using Pgvector.EntityFrameworkCore;
using Pgvector;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

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
        IQwenService qwenService = new QwenService(options, getFinisDbContext());
        return qwenService;
    };

    private static readonly Func<FinisDbContext> getFinisDbContext = () =>
    {
        var postgresOptions = _configuration.GetSection("Postgres").Get<PostgresOptions>();
        var optionsBuilder = new DbContextOptionsBuilder<FinisDbContext>();
        optionsBuilder.UseNpgsql(postgresOptions!.ConnectionString, o => o.UseVector());
        return new FinisDbContext(optionsBuilder.Options);
    };

    private readonly HttpClient _httpClient = CreateConfiguredClient();

    private static HttpClient CreateConfiguredClient()
    {
        var qwenOptions = _configuration.GetSection("Qwen").Get<QwenOptions>();
        string apiKey = qwenOptions.ApiKey;
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://dashscope.aliyuncs.com/api/v1/services/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    [Fact]
    public void MdToPlainTextTest()
    {
        string str = Markdown.ToPlainText("# RAG 检索技术方案对比\n\n在构建知识库（如 Finis 平台）时，选择合适的检索策略至关重要。以下是三种主流方案的对比：\n\n## 核心技术对比表\n\n| 维度 | 稀疏检索 (BM25) | 稠密检索 (Embedding) | 混合检索 (Hybrid) |\n| :--- | :--- | :--- | :--- |\n| **实现原理** | 关键词频率统计 | 语义向量空间距离 | 多路召回 + 重排序 (RRF) |\n| **擅长场景** | 专有名词、代码、人名 | 模糊意图、近义词理解 | 工业级生产环境、全场景 |\n| **对 MD 标签** | 敏感（建议预处理清洗） | 不敏感（向量能忽略噪音） | 鲁棒性最高 |\n| **计算开销** | 极低（倒排索引） | 高（需要 GPU 推理） | 中等（双路计算） |\n\n## 技术建议\n1. **数据量较小 ( < 5000 篇)**：优先考虑 Lucene.Net 实现的 BM25，开发成本最低。\n2. **长文本处理**：建议将 Markdown 表格内容提取为纯文本，防止 `|` 和 `-` 符号干扰 N-gram 的权重计算。\n3. **检索增强**：召回阶段将原始 Markdown 喂给 LLM，生成阶段将纯文本喂给检索器。\n\n---\n*注：本表格由 FinisServer 自动化构建模块生成预览。*");
        outputHelper.WriteLine(str);
    }

    public async Task<IList<float[]>> EmbeddingTextsAsync(IList<string> chunks)
    {
        const int batchSize = 10;
        IList<float[]> result = [];
        for (int i = 0; i < chunks.Count; i += batchSize)
        {
            var batch = chunks.Skip(i).Take(batchSize).ToList();
            var requestData = new
            {
                model = "text-embedding-v4",
                input = new { texts = batch },
                parameters = new { dimension = 1024 }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "embeddings/text-embedding/text-embedding",
                requestData,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );
            if (!response.IsSuccessStatusCode)
            {
                // 读取服务器返回的详细错误 JSON
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"错误详情: {errorContent}");
            }
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<JsonObject>();
            var embeddings = json?["output"]?["embeddings"]?.AsArray() ?? []; ;
            foreach (var item in embeddings)
            {
                var embedding = item?["embedding"]?.Deserialize<float[]>();
                if (embedding != null)
                {
                    result.Add(embedding);
                }
            }
        }
        return result;
    }
    [Fact]
    public async Task RagQueryAsync()
    {
        string[] keywords = ["如何优化手机性能"];
        IList<float[]> vectorArray = await EmbeddingTextsAsync(keywords);
        var finisDbContext = getFinisDbContext();

        // 2. 修改临时列表的定义为 int
        var rawResults = new List<(string Content, int Id)>();

        for (int i = 0; i < keywords.Length; i++)
        {
            // 向量检索
            var vList = await finisDbContext.ArticleVectors
                .AsNoTracking()
                .OrderBy(d => d.Embedding.CosineDistance(new Vector(vectorArray[i])))
                .Take(30)
                .Select(d => new { d.Content, d.ParentArticleId }) // 这里的 ParentArticleId 是 int
                .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

            // 显式转为元组存入列表
            rawResults.AddRange(vList.Select(x => (x.Content, x.ParentArticleId)));

            // 关键词检索
            var kList = await finisDbContext.ArticleVectors
                .FromSqlInterpolated($"SELECT * , paradedb.score(\"Id\") AS score FROM article_vector WHERE \"Content\" ||| {keywords[i]} ORDER BY score DESC")
                .Include(d => d.ParentArticle)
                .AsNoTracking()
                .Where(d => !d.ParentArticle.IsDeletedByAdmin)
                .Take(30)
                .Select(d => new { d.Content, d.ParentArticleId })
                .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

            rawResults.AddRange(kList.Select(x => (x.Content, x.ParentArticleId)));
        }

        // 3. 统计结果（类型均为 int）
        int[] distinctIds = rawResults.Select(x => x.Id).Distinct().ToArray();
        string[] distinctContents = rawResults.Select(x => x.Content).Distinct().ToArray();

        outputHelper.WriteLine(string.Join(',', distinctContents));
    }
}