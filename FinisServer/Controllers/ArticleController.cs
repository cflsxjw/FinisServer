using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Enums;
using FinisServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ArticleController(IArticleService articleService) : ControllerBase
{
    [HttpGet("categories")]
    public Result<IEnumerable<string>> GetCategoryNames()
    {
        return Result<IEnumerable<string>>.Success(articleService.GetCategoryNames());
    }

    [Authorize]
    [HttpPost("post")]
    public async Task<Result> PostArticleAsync([FromBody] ArticlePostDto articlePostDto)
    {
        await articleService.PostArticleAsync(articlePostDto);
        return Result.Success("发布成功");
    }

    [HttpGet("fetch/{id:int}")]
    public async Task<Result<ArticleDetailDto>> GetArticleDetailById(int id)
    {
        var result = await articleService.GetArticleDetailByIdAsync(id);
        return Result<ArticleDetailDto>.Success(result);
    }

    [HttpGet("list")]
    public async Task<Result<IEnumerable<ArticleInfoDto>>> GetArticleList(int count, int lastId, ArticleCategory? category = null)
    {
        var result = await articleService.GetArticleListAsync(count, lastId, category);
        return Result<IEnumerable<ArticleInfoDto>>.Success(result);
    }

    [Authorize]
    [HttpPost("comment/post")]
    public async Task<Result> PostCommentAsync([FromBody] CommentPostDto commentPostDto)
    {
        await articleService.PostCommentAsync(commentPostDto);
        return Result.Success();
    }

    [HttpGet("comment/list/{articleId:int}")]
    public async Task<Result<IList<CommentDetailDto>>> GetCommentListById(int articleId)
    {
        var list = await articleService.GetCommentListAsync(articleId);
        return Result<IList<CommentDetailDto>>.Success(list);
    }

    [HttpPost("comment/like/{commentId:int}")]
    public async Task LikeCommentAsync(int commentId)
    {
        await articleService.LikeCommentAsync(commentId);
    }

    [HttpPost("like/{articleId:int}")]
    public async Task LikeArticleAsync(int articleId)
    {
        await articleService.LikeArticleAsync(articleId);
    }
}