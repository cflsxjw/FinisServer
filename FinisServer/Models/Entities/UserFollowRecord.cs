namespace FinisServer.Models.Entities;

public class UserFollowRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FollowedUserId { get; set; }
}