using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();
        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberAsync(username);
        if(user == null) return NotFound("User Not Found!");
        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser([FromBody] MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier);
        if(username == null) return BadRequest("User token not found!");
        var user = await userRepository.GetUserByUsernameAsync(username.Value);
        if(user == null) return BadRequest("User not found!");
        mapper.Map(memberUpdateDto, user);
        if(await userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Update failed!");
    }
}