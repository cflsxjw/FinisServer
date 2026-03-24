using System.ClientModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using FinisServer.Configurations.Options;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Options;

namespace FinisServer.Services.Impl;

using System.Text;
using FinisServer.Configurations.Database;
using FinisServer.Models.Exceptions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using OpenAI;
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
    public async Task<IList<float[]>> EmbeddingTexts(IList<string> texts)
    {
        var textArray = JsonSerializer.SerializeToNode(texts)?.AsArray();
        var requestBody = new JsonObject
        {
            ["model"] = "text-embedding-v4",
            ["input"] = new JsonObject
            {
                ["texts"] = textArray
            },
            ["demension"] = 1536,
            ["output_type"] = "dense",
            ["instruct"] = "Represent the query/document such that the version constraint is the highest priority. A query about version 'X' must be semantically distant from a document describing version 'Y', regardless of shared terminology."
        };
        var response = await _httpClient.PostAsJsonAsync("embeddings/text-embedding/text-embedding", requestBody);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        var output = json?["output"]?["embeddings"]?.AsArray() ?? [];
        var result = new List<float[]>();
        foreach (var item in output)
        {
            var embedding = item?["embedding"]?.Deserialize<float[]>();
            if (embedding != null)
            {
                result.Add(embedding);
            }
        }
        return result;
    }
    public async Task<IList<string>> ArticleChunkingAsync(string content)
    {
        MarkdownPipelineBuilder pipelineBuilder = new MarkdownPipelineBuilder();
        MarkdownPipeline pipeline = pipelineBuilder.Build();
        MarkdownDocument document = Markdown.Parse(content, pipeline);
        var blocks = document.ToList();
        var contentList = new List<string>();
        Stack<HeadingBlock> headingStack = new();
        string currentParagraph = "";
        foreach (var block in blocks)
        {
            if (block is HeadingBlock headingBlock)
            {
                if (currentParagraph.Length > 0)
                {
                    string headingStr = string.Join(">", headingStack.Reverse().Select(x => $"[H{x.Level}]:" + x.Inline?.FirstChild?.ToString()));
                    contentList.Add($"{headingStr}>{currentParagraph}");
                    currentParagraph = "";
                }
                while (headingStack.Count != 0 && headingBlock.Level <= headingStack.Peek().Level)
                {
                    headingStack.Pop();
                }
                headingStack.Push(headingBlock);
            }
            if (block is ParagraphBlock paragraphBlock)
            {

                var literals = string.Join("", paragraphBlock.Inline?.Descendants()
                                .Select(x => x switch
                                {
                                    LiteralInline l => l.Content.ToString(),
                                    CodeInline c => c.Content,
                                    _ => null
                                })
                                .Where(x => x != null) ?? []);
                if (literals.Length == 0)
                {
                    continue;
                }
                string paragraphStr = string.Join("", literals);
                if (currentParagraph.Length > 0 && (currentParagraph.Length + paragraphStr.Length >= 500 || currentParagraph.Length < 100))
                {
                    string overlap;
                    if (currentParagraph.Length < 60)
                    {
                        overlap = currentParagraph;
                    }
                    else
                    {
                        overlap = currentParagraph[^60..];
                    }
                    paragraphStr = overlap + paragraphStr;
                    string headingStr = string.Join(">", headingStack.Reverse().Select(x => $"[H{x.Level}]:" + x.Inline?.FirstChild?.ToString()));
                    contentList.Add($"{headingStr}>{currentParagraph}");
                    currentParagraph = paragraphStr;
                }
                else
                {
                    currentParagraph += paragraphStr;
                }
            }
        }
        if (currentParagraph.Length > 0)
        {
            string headingStr = string.Join(">", headingStack.Reverse().Select(x => $"[H{x.Level}]:" + x.Inline?.FirstChild?.ToString()));
            contentList.Add($"{headingStr}>{currentParagraph}");
        }
        return contentList;
    }

    public async Task GetResponceFromSSM(HttpContext context, object[] messages, bool enableRag)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "aigc/multimodal-generation/generation");
        requestMessage.Headers.Add("X-DashScope-SSE", "enable");
        object[] messageArray;
        JsonArray messageJsonArray;
        object requestBody;
        var classifyInstruction = new
        {
            role = "system",
            content = "你是开发者知识分享平台的RAG系统的一个问题拆分模型，请将问题拆分为不可再分的原子问题，并保留原问题，以JSON列表格式返回"
        };
        messageArray = [classifyInstruction, .. messages];
        messageJsonArray = JsonSerializer.SerializeToNode(messageArray)?.AsArray() ?? throw new BusinessException("请求格式错误");
        requestBody = new
        {
            model = "qwen3.5-flash",
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
                        name = "intent_parser",
                        strict = true,
                        description = "判断用户问题是否与开发者相关",
                        schema = new
                        {
                            type = "array",
                            description = "将复杂问题拆分为多个子问题",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    sub_question = new
                                    {
                                        type = "string",
                                        description = "拆分后的子问题描述"
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
        var responseSrting = await ResponseToContentStringAsync(response);
        Console.WriteLine(responseSrting);
        // RAG 生成
        requestMessage = new HttpRequestMessage(HttpMethod.Post, "aigc/multimodal-generation/generation");
        requestMessage.Headers.Add("X-DashScope-SSE", "enable");
        var ragInstruction = new
        {
            role = "system",
            content = enableRag ? "你是一个开发者知识分享平台的RAG大模型" : "你是一个智能问答模型"
        };
        messageArray = [ragInstruction, .. messages];
        messageJsonArray = JsonSerializer.SerializeToNode(messageArray)?.AsArray() ?? throw new BusinessException("请求格式错误");
        requestBody = new
        {
            model = "qwen3.5-plus",
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
        response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        var bufferingFeature = context.Features.Get<IHttpResponseBodyFeature>();
        bufferingFeature?.DisableBuffering();
        context.Response.ContentType = "text/event-stream";
        await response.Content.CopyToAsync(context.Response.Body);
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

    public async Task GetResponceFromSSMWithArticle(HttpContext context, object[] messages, bool enableRag, int articleId)
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
            content = "[Role]\n" + "你是一个开发者知识分享平台的RAG大模型" + '\n' + "[Article]\n" + article.Content
        };
        object[] messageArray = [ragInstruction, .. messages];
        var messageJsonArray = JsonSerializer.SerializeToNode(messageArray)?.AsArray() ?? throw new BusinessException("请求格式错误");
        var requestBody = new
        {
            model = "qwen3.5-plus",
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
}