using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public async Task<MemberDto?> GetMemberAsync(string username)
    {
        return await context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }
    public async Task<PagedList<MemberDto>?> GetMembersAsync(UserParams userParams)
    {
        var query = context.Users.AsQueryable();
        query = query.Where(u => u.UserName != userParams.CurrentUsername);
        query = query.Where(u => u.Gender == userParams.Gender);
        var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
        var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
        query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };
        return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider).AsNoTracking(), 
                                                                                userParams.PageNumber, userParams.PageSize);
    }
    public async Task<AppUser?> GetUserByIdAsync(int id) => await context.Users.FindAsync(id);
    public async Task<AppUser?> GetUserByUsernameAsync(string username) => await context.Users
                                                                            .Include(p => p.Photos)
                                                                            .SingleOrDefaultAsync(x => x.UserName == username);
    public async Task<string?> GetUserGender(string username) => await context.Users
                                                                .Where(x => x.UserName == username)
                                                                .Select(x => x.Gender).FirstOrDefaultAsync();
    public async Task<IEnumerable<AppUser>?> GetUsersAsync() => await context.Users
                                                                .Include(p => p.Photos)
                                                                .ToListAsync();
    public void Update(AppUser user) => context.Entry(user).State = EntityState.Modified;
}
