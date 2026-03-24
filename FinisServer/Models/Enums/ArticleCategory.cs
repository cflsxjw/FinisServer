namespace FinisServer.Models.Enums;
using System.ComponentModel.DataAnnotations;

public enum ArticleCategory
{
    [Display(Name = "前端")]
    Frontend,
    [Display(Name = "后端")]
    Backend,
    [Display(Name = "数据库")]
    Database,
    [Display(Name = "运维")]
    CloudDevOps,
    [Display(Name = "算法")]
    Algorithm,
    [Display(Name = "人工智能")]
    Ai,
    [Display(Name = "网络安全")]
    Security,
    [Display(Name = "开发工具")]
    Tools,
    [Display(Name = "码农职场")]
    Career,
    [Display(Name = "人生随笔")]
    LifeNotes
    
}
