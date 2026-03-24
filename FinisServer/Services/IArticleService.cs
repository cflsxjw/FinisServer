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
    Task<IEnumerable<ArticleInfoDto>> GetArticleListAsync(int count, int lastId, ArticleCategory? category);
    Task PostCommentAsync([FromBody] CommentPostDto commentPostDto);
    Task<IList<CommentDetailDto>> GetCommentListAsync(int articleId);

    Task LikeCommentAsync(int commentId);
    Task LikeArticleAsync(int articleId);

}