using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Services;

public interface IArticleService
{
    public IEnumerable<string> GetCategoryNames();
    Task PostArticleAsync([FromBody] ArticlePostDto articlePostDto);
    Task<ArticleDetailDto> GetArticleDetailByIdAsync(int id);
    Task<IEnumerable<ArticleInfoDto>> GetArticleListAsync(int count, int lastId, ArticleCategory? category, string? keyword, int? authorId, bool? isBookmarks);
    Task PostCommentAsync([FromBody] CommentPostDto commentPostDto);
    Task ModifyArticleAsync(int id, ArticlePostDto articlePostDto);
    Task DeleteArticleAsync(int id);
    Task<IList<CommentDetailDto>> GetCommentListAsync(int articleId);

    Task LikeCommentAsync(int commentId);
    Task LikeArticleAsync(int articleId);
    Task BookMarkArticleAsync(int articleId);
    Task<List<ArticleInfoDto>> QueryArticle(int count, int skip, string keyword);
    Task<string> GetArticleTitleByIdAsync(int id);
}