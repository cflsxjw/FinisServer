# Details

Date : 2026-04-07 01:42:15

Directory /workspace

Total : 84 files,  2536 codes, 232 comments, 350 blanks, all 3118 lines

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)

## Files
| filename | language | code | comment | blank | total |
| :--- | :--- | ---: | ---: | ---: | ---: |
| [.devcontainer/.env](/.devcontainer/.env) | Properties | 1 | 0 | 1 | 2 |
| [.devcontainer/devcontainer.json](/.devcontainer/devcontainer.json) | JSON with Comments | 6 | 0 | 1 | 7 |
| [.devcontainer/docker-compose.yml](/.devcontainer/docker-compose.yml) | YAML | 51 | 0 | 2 | 53 |
| [FinisServer/Configurations/Database/FinisDbContext.cs](/FinisServer/Configurations/Database/FinisDbContext.cs) | C# | 210 | 28 | 14 | 252 |
| [FinisServer/Configurations/Database/Interceptors/TimeInterceptor.cs](/FinisServer/Configurations/Database/Interceptors/TimeInterceptor.cs) | C# | 43 | 0 | 3 | 46 |
| [FinisServer/Configurations/Database/Redis/ClearExpiredTicks.lua](/FinisServer/Configurations/Database/Redis/ClearExpiredTicks.lua) | Lua | 28 | 4 | 3 | 35 |
| [FinisServer/Configurations/Database/TestDbContext.cs](/FinisServer/Configurations/Database/TestDbContext.cs) | C# | 38 | 8 | 5 | 51 |
| [FinisServer/Configurations/GlobalExceptionHandler.cs](/FinisServer/Configurations/GlobalExceptionHandler.cs) | C# | 18 | 0 | 2 | 20 |
| [FinisServer/Configurations/Options/JwtOptions.cs](/FinisServer/Configurations/Options/JwtOptions.cs) | C# | 15 | 0 | 6 | 21 |
| [FinisServer/Configurations/Options/PostgresOptions.cs](/FinisServer/Configurations/Options/PostgresOptions.cs) | C# | 8 | 0 | 3 | 11 |
| [FinisServer/Configurations/Options/QwenOptions.cs](/FinisServer/Configurations/Options/QwenOptions.cs) | C# | 10 | 0 | 4 | 14 |
| [FinisServer/Configurations/Options/RedisOptions.cs](/FinisServer/Configurations/Options/RedisOptions.cs) | C# | 10 | 0 | 4 | 14 |
| [FinisServer/Controllers/ArticleController.cs](/FinisServer/Controllers/ArticleController.cs) | C# | 84 | 0 | 12 | 96 |
| [FinisServer/Controllers/LargeModelController.cs](/FinisServer/Controllers/LargeModelController.cs) | C# | 25 | 0 | 3 | 28 |
| [FinisServer/Controllers/ResourceController.cs](/FinisServer/Controllers/ResourceController.cs) | C# | 43 | 0 | 4 | 47 |
| [FinisServer/Controllers/UserController.cs](/FinisServer/Controllers/UserController.cs) | C# | 64 | 0 | 8 | 72 |
| [FinisServer/FinisServer.csproj](/FinisServer/FinisServer.csproj) | XML | 23 | 0 | 5 | 28 |
| [FinisServer/Interfaces/IAuditEntity.cs](/FinisServer/Interfaces/IAuditEntity.cs) | C# | 6 | 0 | 2 | 8 |
| [FinisServer/Models/Constants.cs](/FinisServer/Models/Constants.cs) | C# | 5 | 0 | 1 | 6 |
| [FinisServer/Models/Dtos/ArticleCategoryDto.cs](/FinisServer/Models/Dtos/ArticleCategoryDto.cs) | C# | 4 | 0 | 2 | 6 |
| [FinisServer/Models/Dtos/ArticleDetailDto.cs](/FinisServer/Models/Dtos/ArticleDetailDto.cs) | C# | 16 | 0 | 2 | 18 |
| [FinisServer/Models/Dtos/ArticleInfoDto.cs](/FinisServer/Models/Dtos/ArticleInfoDto.cs) | C# | 16 | 0 | 2 | 18 |
| [FinisServer/Models/Dtos/ArticlePostDto.cs](/FinisServer/Models/Dtos/ArticlePostDto.cs) | C# | 14 | 0 | 2 | 16 |
| [FinisServer/Models/Dtos/ChatRequestDto.cs](/FinisServer/Models/Dtos/ChatRequestDto.cs) | C# | 2 | 0 | 2 | 4 |
| [FinisServer/Models/Dtos/ChatWithArticleRequestDto.cs](/FinisServer/Models/Dtos/ChatWithArticleRequestDto.cs) | C# | 2 | 0 | 2 | 4 |
| [FinisServer/Models/Dtos/CommentDetailDto.cs](/FinisServer/Models/Dtos/CommentDetailDto.cs) | C# | 11 | 0 | 1 | 12 |
| [FinisServer/Models/Dtos/CommentPostDto.cs](/FinisServer/Models/Dtos/CommentPostDto.cs) | C# | 8 | 0 | 2 | 10 |
| [FinisServer/Models/Dtos/LlmMessageDto.cs](/FinisServer/Models/Dtos/LlmMessageDto.cs) | C# | 2 | 0 | 0 | 2 |
| [FinisServer/Models/Dtos/PathDto.cs](/FinisServer/Models/Dtos/PathDto.cs) | C# | 2 | 0 | 1 | 3 |
| [FinisServer/Models/Dtos/TextEmbeddingPostDto.cs](/FinisServer/Models/Dtos/TextEmbeddingPostDto.cs) | C# | 7 | 0 | 4 | 11 |
| [FinisServer/Models/Dtos/TextEmbeddingResultDto.cs](/FinisServer/Models/Dtos/TextEmbeddingResultDto.cs) | C# | 4 | 0 | 2 | 6 |
| [FinisServer/Models/Dtos/TokenCreateDto.cs](/FinisServer/Models/Dtos/TokenCreateDto.cs) | C# | 4 | 0 | 2 | 6 |
| [FinisServer/Models/Dtos/TokenDto.cs](/FinisServer/Models/Dtos/TokenDto.cs) | C# | 2 | 0 | 1 | 3 |
| [FinisServer/Models/Dtos/UrlDto.cs](/FinisServer/Models/Dtos/UrlDto.cs) | C# | 2 | 0 | 1 | 3 |
| [FinisServer/Models/Dtos/UserInfoDto.cs](/FinisServer/Models/Dtos/UserInfoDto.cs) | C# | 2 | 0 | 1 | 3 |
| [FinisServer/Models/Dtos/UserLoginDto.cs](/FinisServer/Models/Dtos/UserLoginDto.cs) | C# | 8 | 0 | 2 | 10 |
| [FinisServer/Models/Dtos/UserRegisterDto.cs](/FinisServer/Models/Dtos/UserRegisterDto.cs) | C# | 9 | 0 | 2 | 11 |
| [FinisServer/Models/Dtos/UserSettingDto.cs](/FinisServer/Models/Dtos/UserSettingDto.cs) | C# | 10 | 0 | 2 | 12 |
| [FinisServer/Models/Entities/Article.cs](/FinisServer/Models/Entities/Article.cs) | C# | 22 | 40 | 10 | 72 |
| [FinisServer/Models/Entities/ArticleBookmarkRecord.cs](/FinisServer/Models/Entities/ArticleBookmarkRecord.cs) | C# | 8 | 0 | 2 | 10 |
| [FinisServer/Models/Entities/ArticleContent.cs](/FinisServer/Models/Entities/ArticleContent.cs) | C# | 8 | 0 | 2 | 10 |
| [FinisServer/Models/Entities/ArticleLikeRecord.cs](/FinisServer/Models/Entities/ArticleLikeRecord.cs) | C# | 9 | 0 | 3 | 12 |
| [FinisServer/Models/Entities/ArticleVector.cs](/FinisServer/Models/Entities/ArticleVector.cs) | C# | 10 | 0 | 3 | 13 |
| [FinisServer/Models/Entities/Comment.cs](/FinisServer/Models/Entities/Comment.cs) | C# | 17 | 0 | 2 | 19 |
| [FinisServer/Models/Entities/CommentLikeRecord.cs](/FinisServer/Models/Entities/CommentLikeRecord.cs) | C# | 10 | 0 | 4 | 14 |
| [FinisServer/Models/Entities/FinisJwtClaimTypes.cs](/FinisServer/Models/Entities/FinisJwtClaimTypes.cs) | C# | 6 | 0 | 1 | 7 |
| [FinisServer/Models/Entities/Tag.cs](/FinisServer/Models/Entities/Tag.cs) | C# | 6 | 0 | 1 | 7 |
| [FinisServer/Models/Entities/TestEntities/TestArticle.cs](/FinisServer/Models/Entities/TestEntities/TestArticle.cs) | C# | 35 | 0 | 10 | 45 |
| [FinisServer/Models/Entities/TestEntities/TestTag.cs](/FinisServer/Models/Entities/TestEntities/TestTag.cs) | C# | 15 | 0 | 3 | 18 |
| [FinisServer/Models/Entities/User.cs](/FinisServer/Models/Entities/User.cs) | C# | 20 | 57 | 3 | 80 |
| [FinisServer/Models/Entities/UserFollowRecord.cs](/FinisServer/Models/Entities/UserFollowRecord.cs) | C# | 7 | 0 | 1 | 8 |
| [FinisServer/Models/Enums/ArticleCategory.cs](/FinisServer/Models/Enums/ArticleCategory.cs) | C# | 21 | 0 | 3 | 24 |
| [FinisServer/Models/Enums/LikeRecordType.cs](/FinisServer/Models/Enums/LikeRecordType.cs) | C# | 6 | 0 | 2 | 8 |
| [FinisServer/Models/Enums/UserRole.cs](/FinisServer/Models/Enums/UserRole.cs) | C# | 6 | 0 | 1 | 7 |
| [FinisServer/Models/Exceptions/AuthenticationException.cs](/FinisServer/Models/Exceptions/AuthenticationException.cs) | C# | 6 | 0 | 2 | 8 |
| [FinisServer/Models/Exceptions/BusinessException.cs](/FinisServer/Models/Exceptions/BusinessException.cs) | C# | 5 | 0 | 1 | 6 |
| [FinisServer/Models/Exceptions/FinisException.cs](/FinisServer/Models/Exceptions/FinisException.cs) | C# | 5 | 0 | 1 | 6 |
| [FinisServer/Models/Exceptions/HttpContextException.cs](/FinisServer/Models/Exceptions/HttpContextException.cs) | C# | 5 | 0 | 1 | 6 |
| [FinisServer/Models/Exceptions/InvalidUploadFileException.cs](/FinisServer/Models/Exceptions/InvalidUploadFileException.cs) | C# | 5 | 0 | 1 | 6 |
| [FinisServer/Models/Exceptions/ResourceNotFoundException.cs](/FinisServer/Models/Exceptions/ResourceNotFoundException.cs) | C# | 5 | 0 | 1 | 6 |
| [FinisServer/Models/Extensions.cs](/FinisServer/Models/Extensions.cs) | C# | 13 | 0 | 2 | 15 |
| [FinisServer/Models/Result.cs](/FinisServer/Models/Result.cs) | C# | 14 | 0 | 3 | 17 |
| [FinisServer/Program.cs](/FinisServer/Program.cs) | C# | 192 | 24 | 20 | 236 |
| [FinisServer/Properties/launchSettings.json](/FinisServer/Properties/launchSettings.json) | JSON | 23 | 0 | 1 | 24 |
| [FinisServer/Services/IArticleService.cs](/FinisServer/Services/IArticleService.cs) | C# | 21 | 0 | 3 | 24 |
| [FinisServer/Services/IFinisHttpContext.cs](/FinisServer/Services/IFinisHttpContext.cs) | C# | 5 | 0 | 1 | 6 |
| [FinisServer/Services/IQwenService.cs](/FinisServer/Services/IQwenService.cs) | C# | 10 | 0 | 4 | 14 |
| [FinisServer/Services/IRankingService.cs](/FinisServer/Services/IRankingService.cs) | C# | 8 | 4 | 4 | 16 |
| [FinisServer/Services/IResourceService.cs](/FinisServer/Services/IResourceService.cs) | C# | 9 | 0 | 3 | 12 |
| [FinisServer/Services/ITokenService.cs](/FinisServer/Services/ITokenService.cs) | C# | 7 | 0 | 3 | 10 |
| [FinisServer/Services/IUserService.cs](/FinisServer/Services/IUserService.cs) | C# | 13 | 0 | 3 | 16 |
| [FinisServer/Services/Impl/ArticleService.cs](/FinisServer/Services/Impl/ArticleService.cs) | C# | 359 | 3 | 12 | 374 |
| [FinisServer/Services/Impl/FinisHttpContext.cs](/FinisServer/Services/Impl/FinisHttpContext.cs) | C# | 16 | 0 | 2 | 18 |
| [FinisServer/Services/Impl/QwenService.cs](/FinisServer/Services/Impl/QwenService.cs) | C# | 328 | 59 | 13 | 400 |
| [FinisServer/Services/Impl/RankingService.cs](/FinisServer/Services/Impl/RankingService.cs) | C# | 98 | 5 | 18 | 121 |
| [FinisServer/Services/Impl/ResourceService.cs](/FinisServer/Services/Impl/ResourceService.cs) | C# | 64 | 0 | 7 | 71 |
| [FinisServer/Services/Impl/TokenService.cs](/FinisServer/Services/Impl/TokenService.cs) | C# | 35 | 0 | 3 | 38 |
| [FinisServer/Services/Impl/UserService.cs](/FinisServer/Services/Impl/UserService.cs) | C# | 85 | 0 | 9 | 94 |
| [FinisServer/appsettings.Development.json](/FinisServer/appsettings.Development.json) | JSON | 9 | 0 | 1 | 10 |
| [FinisServer/appsettings.json](/FinisServer/appsettings.json) | JSON | 26 | 0 | 1 | 27 |
| [FinisServerTest/FinisServerTest.csproj](/FinisServerTest/FinisServerTest.csproj) | XML | 25 | 0 | 7 | 32 |
| [FinisServerTest/TestResources/Test.md](/FinisServerTest/TestResources/Test.md) | Markdown | 87 | 0 | 49 | 136 |
| [FinisServerTest/UnitTest1.cs](/FinisServerTest/UnitTest1.cs) | C# | 36 | 0 | 6 | 42 |
| [FinisServerTest/xunit.runner.json](/FinisServerTest/xunit.runner.json) | JSON | 3 | 0 | 1 | 4 |

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)