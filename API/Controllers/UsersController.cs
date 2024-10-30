using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork,
                            IMapper mapper,
                            IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        var gender = await unitOfWork.UserRepository.GetUserGender(User.GetUserName());
        userParams.CurrentUsername = User.GetUserName();
        if (string.IsNullOrEmpty(userParams.Gender))
        {
            userParams.Gender = gender == "male" ? "female" : "male";
        }
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(users.CurrentPage, users.PageSize,users.TotalCount, users.TotalPages);
        return Ok(users);
    }

    [HttpGet("username")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await unitOfWork.UserRepository.GetMemberAsync(username);
        if(user == null) return NotFound("User Not Found!");
        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
        if(user == null) return NotFound("User Not Found!");
        mapper.Map(memberUpdateDto, user);
        unitOfWork.UserRepository.Update(user);
        if (await unitOfWork.Complete()) return NoContent();
        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
        if(user == null) return NotFound("User Not Found!");
        var result = await photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        if (user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }
        user.Photos.Add(photo);
        if (await unitOfWork.Complete())
        {
            return CreatedAtRoute("GetUser", new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
        }
        return BadRequest("Problem addding photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
        if(user == null) return NotFound("User Not Found!");
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if(photo == null) return NotFound("Photo Not Found!");
        if (photo.IsMain) return BadRequest("This is already your main photo");
        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;
        if (await unitOfWork.Complete()) return NoContent();
        return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
        if(user == null) return NotFound("User Not Found!");
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if(photo == null) return NotFound("Photo Not Found!");
        if (photo.IsMain) return BadRequest("You cannot delete your main photo");
        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }
        user.Photos.Remove(photo);
        if (await unitOfWork.Complete()) return Ok();
        return BadRequest("Failed to delete the photo");
    }
}