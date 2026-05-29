namespace FinisServer.Models.Enums;
using System.ComponentModel.DataAnnotations;

public enum ArticleCategory
{
    [Display(Name = "前端")]
    Frontend,
    [Display(Name = "后端")]
    Backend,
    [Display(Name = "Android")]
    Android,
    [Display(Name = "iOS")]
    iOS,
    [Display(Name = "人工智能")]
    Ai,
    [Display(Name = "开发工具")]
    Tools,
    [Display(Name = "阅读")]
    Reading,
    [Display(Name = "代码人生")]
    LifeNotes
    
}
