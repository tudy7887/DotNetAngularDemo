using System;

namespace API.Helpers;

public class UserParams : PaginationParams
{
    public required string CurrentUsername { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 150;
    public string OrderBy { get; set; } = "lastActive";
}
