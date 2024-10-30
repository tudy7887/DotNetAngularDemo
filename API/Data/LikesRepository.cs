using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Extensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(DataContext context) : ILikesRepository
{
    public async Task<UserLike?> GetUserLike(int sourceUserId, int likedUserId) => await context.Likes.FindAsync(sourceUserId, likedUserId);
    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
        var users = context.Users.OrderBy(u => u.UserName).AsQueryable();
        var likes = context.Likes.AsQueryable();
        if (likesParams.Predicate == "liked")
        {
            likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
            users = likes.Select(like => like.LikedUser)!;
        }
        if (likesParams.Predicate == "likedBy")
        {
            likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
            users = likes.Select(like => like.SourceUser)!;
        }
        var likedUsers = users.Select(user => new LikeDto
        {
            Username = user.UserName!,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)!.Url,
            City = user.City,
            Id = user.Id
        });
        return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<AppUser?> GetUserWithLikes(int userId) => await context.Users
                                                                .Include(x => x.LikedUsers)
                                                                .FirstOrDefaultAsync(x => x.Id == userId);
}
