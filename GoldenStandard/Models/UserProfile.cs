namespace GoldenStandard.Models;

public class UserProfile
{
    public int user_id { get; set; }
    public string username { get; set; }
    public string? display_name { get; set; }
    public string? email { get; set; }
    public string? description { get; set; }
}