using System;

namespace API.Entities;

public class UserLike
{
    public required AppUser SourceUser { get; set; }
    public int SourceUserId { get; set; }

    public required AppUser LikedUser { get; set; }
    public int LikedUserId { get; set; }
}
