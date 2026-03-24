using FinisServer.Configurations.Database;
using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Enums;
using FinisServer.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinisServer.Services.Impl;

public class ArticleService(FinisDbContext finisDbContext, IFinisHttpContext finisHttpContext) : IArticleService
{
    public IEnumerable<string> GetCategoryNames()
    {
        var values = Enum.GetValues<ArticleCategory>();
        foreach (var value in values)
        {
            yield return value.GetDisplayName();
        }
    }

    public async Task PostArticleAsync(ArticlePostDto articlePostDto)
    {
        var user = await finisDbContext.Users
                       .FirstOrDefaultAsync(u => u.Id == finisHttpContext.GetRequestUserId())
                   ?? throw new ResourceNotFoundException("找不到用户");
        user.ArticleCount++;
        var newArticle = new Article
        {
            Title = articlePostDto.Title,
            Summary = articlePostDto.Summary,
            Category = articlePostDto.Category,
            AuthorId = finisHttpContext.GetRequestUserId() ?? throw new AuthenticationException(),
            CoverPath = articlePostDto.CoverPath,
        };
        var newArticleContent = new ArticleContent
        {
            Article = newArticle,
            Content = articlePostDto.Content
        };
        finisDbContext.Add(newArticleContent);
        await finisDbContext.SaveChangesAsync();
    }

    public async Task<ArticleDetailDto> GetArticleDetailByIdAsync(int id)
    {
        var article = await finisDbContext.Articles.Include(article => article.Content)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id) ?? throw new ResourceNotFoundException("请求的文章不存在");
        var userid = finisHttpContext.GetRequestUserId();
        var likeRecord = await finisDbContext.ArticleLikeRecords
            .FirstOrDefaultAsync(r => r.ArticleId == id && r.UserId == userid);
        return new ArticleDetailDto(
            Content: article.Content.Content,
            Title: article.Title,
            CreatedOn: article.CreatedTimeOffset,
            LastModifiedOn: article.UpdatedTimeOffset,
            CategoryDisplay: article.Category.GetDisplayName(),
            AuthorId: article.AuthorId,
            ViewCount: article.ViewCount,
            LikeCount: article.LikeCount,
            BookmarkCount: article.BookmarkCount,
            CommentCount: article.CommentCount,
            IsLiked: likeRecord != null,
            IsBookmarked: true);
    }

    public async Task<IEnumerable<ArticleInfoDto>> GetArticleListAsync(int count, int lastId, ArticleCategory? category)
    {
        IQueryable<Article> query = finisDbContext.Articles;
        if (category != null)
        {
            query = query.Where(p => p.Category == category);
        }
        if (lastId > 0)
        {
            query = query.Where(p => p.Id < lastId);
        }
        var result = await query
            .OrderByDescending(p => p.Id)
            .Take(count)
            .Select(item => new ArticleInfoDto(
                Id: item.Id,
                Title: item.Title,
                Summary: item.Summary,
                ArticleCategory: item.Category,
                ViewCount: item.ViewCount,
                LikesCount: item.LikeCount,
                BookmarkCount: item.BookmarkCount,
                CommentCount: item.CommentCount,
                AuthorId: item.AuthorId,
                CreateOn: item.CreatedTimeOffset,
                LastModifiedOn: item.UpdatedTimeOffset,
                CoverPath: item.CoverPath
            ))
            .ToListAsync();
        return result;
    }

    public async Task PostCommentAsync([FromBody] CommentPostDto commentPostDto)
    {
        var article = await finisDbContext.Articles
            .FirstOrDefaultAsync(a => a.Id == commentPostDto.ArticleId) ?? throw new BusinessException("文章未找到");
        article.CommentCount++;
        var newComment = new Comment
        {
            Content = commentPostDto.Content,
            AuthorId = finisHttpContext.GetRequestUserId() ?? throw new AuthenticationException(),
            ArticleId = commentPostDto.ArticleId,
            RootCommentId = commentPostDto.RootCommentId,
            ReplyToUserId = commentPostDto.ReplyToUserId
        };
        finisDbContext.Add(newComment);
        await finisDbContext.SaveChangesAsync();
    }

    public async Task<IList<CommentDetailDto>> GetCommentListAsync(int articleId)
    {
        int count = 0;
        var comments = await finisDbContext.Comments
            .Where(c => c.ArticleId == articleId)
            .ToListAsync();
        var parentDictionary = new Dictionary<int, CommentDetailDto>();
        foreach (var item in comments)
        {
            if (item.RootCommentId != null) continue;
            parentDictionary.Add(item.Id, new CommentDetailDto(
                Id: item.Id,
                Content: item.Content,
                AuthorId: item.AuthorId,
                ArticleId: item.ArticleId,
                CreateOn: item.CreatedTimeOffset,
                LastModifiedOn: item.UpdatedTimeOffset,
                SubComments: new List<CommentDetailDto>()));
            count++;
        }
        foreach (var item in comments)
        {
            if (item.RootCommentId == null) continue;
            int index = item.RootCommentId.Value;
            parentDictionary[index].SubComments?.Add(new CommentDetailDto(
                Id: item.Id,
                Content: item.Content,
                AuthorId: item.AuthorId,
                ArticleId: item.ArticleId,
                RootCommentId: item.RootCommentId,
                ReplyToUserId: item.ReplyToUserId,
                CreateOn: item.CreatedTimeOffset,
                LastModifiedOn: item.UpdatedTimeOffset));
            count++;
        }
        Console.WriteLine(count);
        return parentDictionary.Values.ToList();
    }
    public async Task LikeCommentAsync(int commentId)
    {
        var userId = finisHttpContext.GetRequestUserId();
        var record = await finisDbContext.CommentLikeRecords
            .Include(r => r.Comment)
            .ThenInclude(a => a.Author)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);
        if (record != null)
        {
            // 取消
            finisDbContext.Remove(record);
            record.Comment.LikeCount--;
            record.Comment.Author.LikeCount--;
        }
        else
        {
            var comment = await finisDbContext.Comments
                   .Include(a => a.Author)
                   .FirstOrDefaultAsync(a => a.Id == commentId) ?? throw new BusinessException("文章不存在");
            comment.LikeCount++;
            comment.Author.LikeCount++;
            var newRecord = new CommentLikeRecord
            {
                UserId = userId ?? throw new AuthenticationException(),
                CommentId = commentId
            };
            finisDbContext.Add(newRecord);
        }
        await finisDbContext.SaveChangesAsync();
    }

    public async Task LikeArticleAsync(int articleId)
    {
        var userId = finisHttpContext.GetRequestUserId();
        var record = await finisDbContext.ArticleLikeRecords
            .Include(r => r.Article)
            .ThenInclude(a => a.Author)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.ArticleId == articleId && r.UserId == userId);
        if (record != null)
        {
            // 取消
            finisDbContext.Remove(record);
            record.Article.LikeCount--;
            record.Article.Author.LikeCount--;
        }
        else
        {
            var article = await finisDbContext.Articles
                    .Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.Id == articleId) ?? throw new BusinessException("文章不存在");
            article.LikeCount++;
            article.Author.LikeCount++;
            var newRecord = new ArticleLikeRecord
            {
                UserId = userId ?? throw new AuthenticationException(),
                ArticleId = articleId
            };
            finisDbContext.Add(newRecord);
        }
        await finisDbContext.SaveChangesAsync();
    }
}