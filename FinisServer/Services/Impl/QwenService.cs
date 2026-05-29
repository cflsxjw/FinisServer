using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using FinisServer.Configurations.Options;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Options;

namespace FinisServer.Services.Impl;

using System.Reflection.Metadata.Ecma335;
using System.Text;
using FinisServer.Configurations.Database;
using FinisServer.Models.Dtos;
using FinisServer.Models.Exceptions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using StackExchange.Redis;

public class QwenService(IOptions<QwenOptions> options, FinisDbContext finisDbContext) : IQwenService
{
    private readonly QwenOptions _qwenOptions = options.Value;
    private readonly HttpClient _httpClient = CreateConfiguredClient(options.Value.ApiKey);

    private static HttpClient CreateConfiguredClient(string apiKey)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://dashscope.aliyuncs.com/api/v1/services/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
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
    public IList<string> ArticleChunking(string content, string title, bool attachTitle)
    {
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().Build();
        MarkdownDocument document = Markdown.Parse(content, pipeline);
        var blocks = document.Descendants<Block>().ToList();
        var contentList = new List<string>();
        Stack<HeadingBlock> headingStack = new();
        string currentChunk = "";
        string GetCurrentPath()
        {
            var headings = headingStack.Reverse().Select(x => $"[H{x.Level}]:{GetHeadingText(x)}");
            string path = string.Join(">", headings);
            return string.IsNullOrEmpty(path) ? $"[Title]:{title}" : $"[Title]:{title}>{path}";
        }

        void CommitChunk(string overlapText = "")
        {
            if (!string.IsNullOrWhiteSpace(currentChunk))
            {
                if (attachTitle)
                {
                    contentList.Add($"{GetCurrentPath()}>\n{currentChunk.Trim()}");
                }
                else
                {
                    contentList.Add($"{currentChunk.Trim()}");
                }
            }
            currentChunk = overlapText;
        }

        foreach (var block in blocks)
        {
            if (block is HeadingBlock headingBlock)
            {
                // 遇到新标题，强制切断当前分块
                CommitChunk();
                while (headingStack.Count > 0 && headingBlock.Level <= headingStack.Peek().Level)
                {
                    headingStack.Pop();
                }
                headingStack.Push(headingBlock);
                continue;
            }

            string blockText = "";
            if (block is ParagraphBlock paragraphBlock)
            {
                if (paragraphBlock.Inline == null) continue;
                var literals = paragraphBlock.Inline.Descendants()
                    .Select(x => x switch
                    {
                        LiteralInline l => l.Content.ToString(),
                        CodeInline c => c.Content,
                        _ => null
                    })
                    .Where(x => x != null);
                blockText = string.Join("", literals);
            }
            else if (block is FencedCodeBlock codeBlock)
            {
                blockText = $"```{codeBlock.Info}\n{codeBlock.Lines}\n```";
            }

            if (string.IsNullOrWhiteSpace(blockText)) continue;

            // 核心长度控制：合并判定（500为阈值，可视需求调整）
            if (currentChunk.Length + blockText.Length >= 800)
            {
                if (currentChunk.Length > 0)
                {
                    string overlap = currentChunk.Length < 60 ? currentChunk : currentChunk[^60..];
                    CommitChunk(overlap);
                }
            }
            currentChunk += (currentChunk.Length > 0 ? "\n\n" : "") + blockText;
        }

        CommitChunk();
        return contentList;
    }


    private string GetHeadingText(HeadingBlock heading)
    {
        if (heading.Inline == null) return string.Empty;
        return string.Join("", heading.Inline.Descendants()
            .Select(x => x switch
            {
                LiteralInline l => l.Content.ToString(),
                CodeInline c => c.Content,
                _ => null
            })
            .Where(x => x != null));
    }
    public async Task GetResponseFromSSM(HttpContext context, object[] messages, bool enableRag)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "aigc/multimodal-generation/generation");
        requestMessage.Headers.Add("X-DashScope-SSE", "enable");
        object[] messageArray;
        JsonArray messageJsonArray;
        object requestBody;
        var classifyInstruction = new
        {
            role = "system",
            content = "你是开发者知识分享平台的RAG系统的一个主语还原模型。如果用户所说的话不是问题，请不要改写，否则假如用户问题的主语不明确，请以你对上下文的理解补全主语，返回一个改写后的问题，如果主语完整，请不要改写，以JSON列表格式返回"
        };
        Console.WriteLine(messages);
        messageArray = [classifyInstruction, .. messages];
        messageJsonArray = JsonSerializer.SerializeToNode(messageArray)?.AsArray() ?? throw new BusinessException("请求格式错误");
        requestBody = new
        {
            model = "qwen3.6-plus",
            input = new
            {
                messages = messageJsonArray,
            },
            parameters = new
            {
                enable_thinking = false,
                result_format = "message",
                incremental_output = true,
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "query_spliter",
                        strict = true,
                        description = "返回一个改写后的问题，使用陈述形式",
                        schema = new
                        {
                            type = "array",
                            description = "返回一个改写后的问题，使用陈述形式",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    sub_question = new
                                    {
                                        type = "string",
                                        description = "改写后的问题描述"
                                    },
                                },
                                required = new[] { "sub_question" },
                                additionalProperties = false
                            }
                        }
                    }
                }
            }
        };
        requestMessage.Content = JsonContent.Create(requestBody);
        var response = await _httpClient.SendAsync(requestMessage);
        var responseString = await ResponseToContentStringAsync(response);
        List<string> resultList = JsonSerializer.Deserialize<List<string>>(responseString) ?? throw new BusinessException("请求异常");
        string? query = null;
        if (resultList != null && resultList.Count > 0)
        {
            query = resultList[0];
        }
        Console.WriteLine(query);

        // RAG 生成
        requestMessage = new HttpRequestMessage(HttpMethod.Post, "aigc/multimodal-generation/generation");
        requestMessage.Headers.Add("X-DashScope-SSE", "enable");
        JsonObject? lastNode = JsonSerializer.SerializeToNode(messages[^1])?.AsObject();
        query = (query == null || query.Trim() == "") ? lastNode?["content"]?.ToString() ?? throw new BusinessException() : query;
        var (knowledge, Ids) = await RagQueryAsync([query]);
        var ragInstruction = new
        {
            role = "system",
            content = enableRag ? "[Role] " + "你是一个开发者知识分享平台的RAG大模型, 请根据所给的[knowledge]展开回答，若没有内容，请拒答" + '\n' + "[Knowledge]: " + string.Join('\n', knowledge) : "你是一个智能问答大模型"
        };
        messages[^1] = new
        {
            role = "user",
            content = query
        };
        messageArray = [ragInstruction, .. messages];
        messageJsonArray = JsonSerializer.SerializeToNode(messageArray)?.AsArray() ?? throw new BusinessException("请求格式错误");
        var idsString = string.Join(",", Ids);
        if (enableRag)
        {
            context.Response.Headers.Append("X-Article-Ids", idsString);
            context.Response.Headers.Append("Access-Control-Expose-Headers", "X-Article-Ids");
        }
        requestBody = new
        {
            model = "qwen3.6-plus",
            input = new
            {
                messages = messageJsonArray,
            },
            parameters = new
            {
                enable_thinking = false,
                result_format = "message",
                incremental_output = true
            }
        };
        requestMessage.Content = JsonContent.Create(requestBody);
        var response2 = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        var bufferingFeature = context.Features.Get<IHttpResponseBodyFeature>();
        bufferingFeature?.DisableBuffering();
        context.Response.ContentType = "text/event-stream";
        await response2.Content.CopyToAsync(context.Response.Body);
    }

    private static async Task<string> ResponseToContentStringAsync(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        var fullContentBuilder = new StringBuilder();
        string? line;
        while (true)
        {
            line = await reader.ReadLineAsync();
            if (line == null)
            {
                break;
            }
            if (line.StartsWith("data:"))
            {
                var data = line[5..].Trim(); // 删除 "data:" 5 是 data: 的长度
                var dataNode = JsonNode.Parse(data) ?? throw new BusinessException("请求失败");
                var contentArray = dataNode["output"]?["choices"]?[0]?["message"]?["content"]?.AsArray();
                if (contentArray != null && contentArray.Count > 0)
                {
                    string content = contentArray[0]?["text"]?.GetValue<string>() ?? "";
                    fullContentBuilder.Append(content);
                }
            }
        }

        return fullContentBuilder.ToString();
    }

    public async Task GetResponseFromSSMWithArticle(HttpContext context, object[] messages, bool enableRag, int articleId)
    {
        var article = await finisDbContext.Articles
            .AsNoTracking()
            .Select(a => a.Content)
            .FirstOrDefaultAsync(a => a.ArticleId == articleId)
            ?? throw new ResourceNotFoundException("文章不存在");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "aigc/multimodal-generation/generation");
        requestMessage.Headers.Add("X-DashScope-SSE", "enable");
        var ragInstruction = new
        {
            role = "system",
            content = "[Role]\n" + "你是一个开发者知识分享平台的文章内容分析RAG大模型" + '\n' + "[Article]\n" + article.Content
        };
        object[] messageArray = [ragInstruction, .. messages];
        var messageJsonArray = JsonSerializer.SerializeToNode(messageArray)?.AsArray() ?? throw new BusinessException("请求格式错误");
        var requestBody = new
        {
            model = "qwen3.6-plus",
            input = new
            {
                messages = messageJsonArray,
            },
            parameters = new
            {
                enable_thinking = false,
                result_format = "message",
                incremental_output = true
            }
        };
        requestMessage.Content = JsonContent.Create(requestBody);
        var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        var bufferingFeature = context.Features.Get<IHttpResponseBodyFeature>();
        bufferingFeature?.DisableBuffering();
        context.Response.ContentType = "text/event-stream";
        await response.Content.CopyToAsync(context.Response.Body);
    }

    // 1. 修改返回类型为 int[]
    public async Task<(string[] Contents, int[] ArticleIds)> RagQueryAsync(string[] keywords)
    {
        IList<float[]> vectorArray = await EmbeddingTextsAsync(keywords);

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
                .ToListAsync();

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
                .ToListAsync();

            rawResults.AddRange(kList.Select(x => (x.Content, x.ParentArticleId)));
        }

        // 3. 统计结果（类型均为 int）
        int[] distinctIds = rawResults.Select(x => x.Id).Distinct().ToArray();
        string[] distinctContents = rawResults.Select(x => x.Content).Distinct().ToArray();

        string[] rerankedContents = await RerankTextsAsync(keywords[0], distinctContents);

        return (rerankedContents, distinctIds);
    }
    private async Task<string[]> RerankTextsAsync(string originKeyword, string[] recalledStrs)
    {
        if (recalledStrs.Length == 0)
        {
            return [];
        }
        var requestBody = new
        {
            model = "qwen3-rerank",
            input = new
            {
                query = originKeyword,
                documents = recalledStrs
            },
            parameters = new
            {
                return_documents = true,
                top_n = 20
            }
        };
        var res = await _httpClient.PostAsync("rerank/text-rerank/text-rerank", JsonContent.Create(requestBody));
        var json = await res.Content.ReadFromJsonAsync<JsonObject>();
        var result = json?["output"]?["results"]?.AsArray() ?? throw new BusinessException("");
        IList<string> reranked = [];
        foreach (var item in result)
        {
            double score = (double?)(item?["relevance_score"]) ?? 0.0;
            if (score >= 0.4)
            {
                string? str = item?["document"]?["text"]?.ToString();
                if (str != null)
                {
                    reranked.Add(str);
                }
            }
        }
        return [.. reranked];
    }
}