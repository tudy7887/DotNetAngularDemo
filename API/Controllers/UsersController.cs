using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, 
                             IMapper mapper, 
                             IPhotoService photoService) : BaseApiController
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
        var user = await userRepository.GetUserByUsernameAsync(User.GetUserName());
        if(user == null) return BadRequest("User not found!");
        mapper.Map(memberUpdateDto, user);
        if(await userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Update failed!");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUserName());
        if(user == null) return BadRequest("Canot update user!");
        var result = await photoService.AddPhotoAsync(file);
        if(result.Error != null) return BadRequest(result.Error.Message);
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        user.Photos.Add(photo);
        if(await userRepository.SaveAllAsync())
        { 
            return CreatedAtAction(nameof(GetUser), 
                                   new { username = user.UserName}, 
                                   mapper.Map<PhotoDto>(photo));
        }
        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUserName());
        if(user == null) return BadRequest("Could not find the user!");
        var photo = user.Photos.FirstOrDefault( photo => photo.Id == photoId);
        if(photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo!");
        var currentMain = user.Photos.FirstOrDefault( photo => photo.IsMain);
        if(currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;
        if(await userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUserName());
        if(user == null) return BadRequest("Could not find the user!");
        var photo = user.Photos.FirstOrDefault( photo => photo.Id == photoId);
        if(photo == null || photo.IsMain) return BadRequest("Cannot delete this photo!");
        if(photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }
        user.Photos.Remove(photo);
        if(await userRepository.SaveAllAsync()) return Ok();
        return BadRequest("Problem deleting photo");
    }
}