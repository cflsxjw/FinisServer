using FinisServer.Models.Dtos;
using FinisServer.Models.Exceptions;
using FinisServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LargeModelController(IQwenService qwenService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        [HttpPost("chat")]
        public async Task GetResponseFromSSM([FromBody] ChatRequestDto chatRequestDto)
        {
            var context = httpContextAccessor.HttpContext ?? throw new HttpContextException();
            await qwenService.GetResponseFromSSM(context, chatRequestDto.Messages, chatRequestDto.EnableRag);
        }

        [HttpPost("chat_with_article")]
        public async Task GetResponseFromSSM([FromBody] ChatWithArticleRequestDto chatWithArticleRequestDto)
        {
            var context = httpContextAccessor.HttpContext ?? throw new HttpContextException();
            await qwenService.GetResponseFromSSMWithArticle(context, chatWithArticleRequestDto.Messages, chatWithArticleRequestDto.EnableRag, chatWithArticleRequestDto.ArticleId);
        }
    }
}
