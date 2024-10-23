using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (await UserExist(registerDto.Username))
            {
                return BadRequest("Username is taken!");
            }

            var user = CreateUser(registerDto);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return Ok(new UserDto{
              Username = user.UserName,
              Token = tokenService.CreateToken(user)
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            var user = await context.Users.FirstOrDefaultAsync(user => user.UserName.ToLower() == loginDto.Username.ToLower());
            if (!AuthorizeUser(user, loginDto))
            {
                return Unauthorized("Invalid username or password!");
            }
            return Ok(new UserDto{
              Username = user.UserName,
              Token = tokenService.CreateToken(user)
            });
        }



        private bool AuthorizeUser(AppUser user, LoginDto loginDto)
        {
            if (user == null)
            {
                return false;
            }
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (var i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return false;
                }
            }
            return true;
        }
        private AppUser CreateUser(RegisterDto registerDto)
        {
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDto.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            return user;
        }
        private async Task<bool> UserExist(string username)
        {
            return await context.Users.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
        }
    }
}
