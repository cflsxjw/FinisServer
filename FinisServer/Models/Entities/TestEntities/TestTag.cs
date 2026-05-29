using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("tag")]
public class TestTag
{
    [Key]
    [Column("tag_id")]
    [MaxLength(255)]
    public string TagId { get; set; } = null!;

    [Column("tag_name")]
    [MaxLength(255)]
    public string TagName { get; set; } = null!;

    [Column("fetched")]
    public bool Fetched { get; set; }
}