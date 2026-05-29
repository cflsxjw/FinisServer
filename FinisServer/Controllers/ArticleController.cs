using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Enums;
using FinisServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ArticleController(IArticleService articleService, IRankingService rankingService) : ControllerBase
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

    [Authorize]
    [HttpPost("modify/{id:int}")]
    public async Task<Result> PostArticleAsync(int id, [FromBody] ArticlePostDto articlePostDto)
    {
        await articleService.ModifyArticleAsync(id, articlePostDto);
        return Result.Success("修改成功");
    }

    [HttpGet("fetch/{id:int}")]
    public async Task<Result<ArticleDetailDto>> GetArticleDetailById(int id)
    {
        var result = await articleService.GetArticleDetailByIdAsync(id);
        return Result<ArticleDetailDto>.Success(result);
    }

    [HttpGet("title/{id:int}")]
    public async Task<Result<string>> GetArticleTitleById(int id)
    {
        var title = await articleService.GetArticleTitleByIdAsync(id);
        return Result<string>.Success(title);
    }

    [HttpGet("list")]
    public async Task<Result<IEnumerable<ArticleInfoDto>>> GetArticleList(int count, int lastId, ArticleCategory? category = null, string? keyword = null, int? authorId = null, bool? isBookmarks = null)
    {
        var result = await articleService.GetArticleListAsync(count, lastId, category, keyword, authorId, isBookmarks);
        return Result<IEnumerable<ArticleInfoDto>>.Success(result);
    }

    [Authorize]
    [HttpPost("delete/{id:int}")]
    public async Task<Result> DeleteArticleAsync(int id)
    {
        await articleService.DeleteArticleAsync(id);
        return Result.Success();
    }

    [HttpGet("query")]
    public async Task<Result<List<ArticleInfoDto>>> GetArticleList(int count, int skip, string keyword)
    {
        List<ArticleInfoDto> result = await articleService.QueryArticle(count, skip, keyword);
        return Result<List<ArticleInfoDto>>.Success(result);
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
    [HttpPost("bookmark/{articleId:int}")]
    public async Task BookmarkArticleAsync(int articleId)
    {
        await articleService.BookMarkArticleAsync(articleId);
    }

    [HttpGet("top_articles")]
    public async Task<Result<List<ArticleInfoDto>>> GetTopArticles()
    {
        var result = await rankingService.GetTopArticlesAsync(10);
        return Result<List<ArticleInfoDto>>.Success(result);
    }
    [HttpGet("top_authors")]
    public async Task<Result<List<UserInfoDto>>> GetTopAuthors()
    {
        var result = await rankingService.GetTopAuthorsAsync(10);
        return Result<List<UserInfoDto>>.Success(result);
    }
}