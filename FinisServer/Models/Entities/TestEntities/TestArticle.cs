using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("article")]
public class TestArticle
{
    [Key] // 假设 juejin_id 为主键
    [Column("juejin_id")]
    [MaxLength(255)]
    public string JuejinId { get; set; } = null!;

    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [Column("author")]
    [MaxLength(255)]
    public string Author { get; set; } = null!;

    [Column("category_name")]
    [MaxLength(255)]
    public string CategoryName { get; set; } = null!;

    [Column("view_count")]
    public int ViewCount { get; set; }

    [Column("read_time")]
    public int ReadTime { get; set; }

    [Column("publish_date")]
    [MaxLength(255)]
    public string PublishDate { get; set; } = null!;

    [Column("keywords")]
    [MaxLength(255)]
    public string Keywords { get; set; } = null!;

    [Column("summary")]
    [MaxLength(255)]
    public string Summary { get; set; } = null!;

    [Column("content")]
    [MaxLength(255)]
    public string Content { get; set; } = null!;
}