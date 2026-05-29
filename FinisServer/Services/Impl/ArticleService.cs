using System.Xml.Schema;
using FinisServer.Configurations.Database;
using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Enums;
using FinisServer.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Pgvector;

namespace FinisServer.Services.Impl;

public class ArticleService(FinisDbContext finisDbContext, IFinisHttpContext finisHttpContext, IQwenService qwenService, IRankingService rankingService) : IArticleService
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
        var chunkedStrs = qwenService.ArticleChunking(articlePostDto.Content, articlePostDto.Title);
        var vectorArrays = await qwenService.EmbeddingTextsAsync(chunkedStrs);
        user.ArticleCount++;
        var newArticle = new Article
        {
            Title = articlePostDto.Title,
            Summary = articlePostDto.Summary,
            Category = articlePostDto.Category,
            AuthorId = finisHttpContext.GetRequestUserId() ?? throw new AuthenticationException(),
            CoverPath = articlePostDto.CoverPath,
            Keywords = articlePostDto.Keywords,
            ArticleVectors = []
        };
        for (int i = 0; i < vectorArrays.Count; i++)
        {
            newArticle.ArticleVectors.Add(new ArticleVector
            {
                Content = chunkedStrs[i],
                Embedding = new Vector(vectorArrays[i])
            });
        }
        var newArticleContent = new ArticleContent
        {
            Article = newArticle,
            Content = articlePostDto.Content
        };
        finisDbContext.Add(newArticleContent);
        await finisDbContext.SaveChangesAsync();
    }

    public async Task DeleteArticleAsync(int id)
    {
        var article = await finisDbContext.Articles.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new BusinessException("文章不存在");
        var userid = finisHttpContext.GetRequestUserId();
        var user = await finisDbContext.Users.FirstOrDefaultAsync(u => u.Id == userid)
            ?? throw new BusinessException("用户异常");
        if (article.AuthorId != userid)
        {
            throw new BusinessException("没有合适的权限");
        }
        user.ArticleCount--;
        user.ViewCount -= article.ViewCount;
        user.BookmarkCount -= article.BookmarkCount;
        finisDbContext.Articles.Remove(article);
        await rankingService.DeleteArticleRankAsync(article.Id, article.AuthorId);
        await finisDbContext.SaveChangesAsync();
    }
    public async Task ModifyArticleAsync(int id, ArticlePostDto articlePostDto)
    {
        var user = await finisDbContext.Users
                       .FirstOrDefaultAsync(u => u.Id == finisHttpContext.GetRequestUserId())
                   ?? throw new ResourceNotFoundException("找不到用户");
        await finisDbContext.ArticleVectors
            .Where(v => v.ParentArticleId == id)
            .ExecuteDeleteAsync();
        var articleToModify = await finisDbContext.Articles
            .Include(a => a.Content)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeletedByAdmin)
            ?? throw new BusinessException("文章不存在");
        var chunkedStrs = qwenService.ArticleChunking(articlePostDto.Content, articlePostDto.Title);
        var vectorArrays = await qwenService.EmbeddingTextsAsync(chunkedStrs);
        articleToModify.Title = articlePostDto.Title;
        articleToModify.Summary = articlePostDto.Summary;
        articleToModify.CoverPath = articlePostDto.CoverPath;
        articleToModify.Keywords = articlePostDto.Keywords;
        articleToModify.ArticleVectors ??= [];
        for (int i = 0; i < vectorArrays.Count; i++)
        {
            articleToModify.ArticleVectors.Add(new ArticleVector
            {
                Content = chunkedStrs[i],
                Embedding = new Vector(vectorArrays[i])
            });
        }
        articleToModify.Content.Content = articlePostDto.Content;
        await finisDbContext.SaveChangesAsync();
    }

    public async Task<ArticleDetailDto> GetArticleDetailByIdAsync(int id)
    {

        var article = await finisDbContext.Articles.Include(article => article.Content)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeletedByAdmin) ?? throw new ResourceNotFoundException("请求的文章不存在");
        var userid = finisHttpContext.GetRequestUserId();
        article.ViewCount++;
        finisDbContext.Articles.Update(article);
        var user = await finisDbContext.Users
            .FirstOrDefaultAsync(u => u.Id == article.AuthorId) ?? throw new InvalidOperationException("用户不存在");
        user.ViewCount++;
        await finisDbContext.SaveChangesAsync();
        await rankingService.RecordClickAsync(article.Id, article.AuthorId);
        var likeRecord = await finisDbContext.ArticleLikeRecords
            .FirstOrDefaultAsync(r => r.ArticleId == id && r.UserId == userid);
        var bookmarkRecord = await finisDbContext.ArticleBookmarkRecords
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
            IsBookmarked: bookmarkRecord != null);
    }
    public async Task<List<ArticleInfoDto>> QueryArticle(int count, int skip, string keyword)
    {
        // 1. 获取有序的 ID 列表（使用 List 替代 HashSet 以严格保留 score DESC 排序）
        List<int> orderedIds = await finisDbContext.ArticleContents
            .FromSqlInterpolated($"SELECT * FROM article_content WHERE \"Content\" ||| {keyword} ORDER BY paradedb.score(\"ArticleId\") DESC")
            .Select(s => s.ArticleId)
            .Skip(skip)
            .Take(count)
            .ToListAsync();

        // 判空，避免发起无意义的数据库请求
        if (orderedIds.Count == 0)
        {
            return new List<ArticleInfoDto>();
        }

        // 2. 根据 ID 批量查询文章详细信息
        var unsortedArticles = await finisDbContext.Articles
            .Where(a => orderedIds.Contains(a.Id) && !a.IsDeletedByAdmin)
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
                CoverPath: item.CoverPath))
            .ToListAsync();

        // 3. 在内存中按照全文检索产生的原始 ID 顺序进行映射重组
        var articleDict = unsortedArticles.ToDictionary(a => a.Id);

        var sortedResult = orderedIds
            .Where(articleDict.ContainsKey) // 过滤掉由于 IsDeletedByAdmin 为 true 而未查到的文章
            .Select(id => articleDict[id])
            .ToList();

        return sortedResult;
    }
    public async Task<string> GetArticleTitleByIdAsync(int id) =>
        await finisDbContext.Articles.Where(a => a.Id == id).Select(a => a.Title).FirstOrDefaultAsync() ?? throw new BusinessException("文章不存在");
    
    public async Task<IEnumerable<ArticleInfoDto>> GetArticleListAsync(int count, int lastId, ArticleCategory? category, string? keyword, int? authorId, bool? isBookmarks)
    {
        IQueryable<Article> query = finisDbContext.Articles;
        query = query.Where(p => !p.IsDeletedByAdmin);
        if (category != null)
        {
            query = query.Where(p => p.Category == category);
        }
        if (lastId > 0)
        {
            query = query.Where(p => p.Id < lastId);
        }
        if (authorId != null)
        {
            query = query.Where(p => p.AuthorId == authorId);
        }
        if (keyword != null)
        {
            var threshold = 0.3;
            query = query
                .Select(a => new
                {
                    Entity = a,
                    Similarity = EF.Functions.TrigramsWordSimilarity(keyword, a.Title)
                })
                .Where(x => x.Similarity > threshold)
                .OrderByDescending(x => x.Similarity)
                .Select(x => x.Entity);
        }
        if (isBookmarks == true)
        {
            var userId = finisHttpContext.GetRequestUserId() ?? throw new BusinessException("用户不存在");
            query = query
                .Where(p =>
                    finisDbContext.ArticleBookmarkRecords
                        .Any(r => r.UserId == userId && r.ArticleId == p.Id));
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

    public async Task BookMarkArticleAsync(int articleId)
    {
        var userId = finisHttpContext.GetRequestUserId();
        var record = await finisDbContext.ArticleBookmarkRecords
            .Include(r => r.Article)
            .ThenInclude(a => a.Author)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.ArticleId == articleId && r.UserId == userId);
        if (record != null)
        {
            // 取消
            finisDbContext.Remove(record);
            record.Article.BookmarkCount--;
            record.Article.Author.BookmarkCount--;
        }
        else
        {
            var article = await finisDbContext.Articles
                    .Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.Id == articleId) ?? throw new BusinessException("文章不存在");
            article.BookmarkCount++;
            article.Author.BookmarkCount++;
            var newRecord = new ArticleBookmarkRecord
            {
                UserId = userId ?? throw new AuthenticationException(),
                ArticleId = articleId
            };
            finisDbContext.Add(newRecord);
        }
        await finisDbContext.SaveChangesAsync();
    }
}