using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IUnitOfWork unitOfWork) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]
            MessageParams messageParams)
    {
        messageParams.Username = User.GetUserName();
        var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
        Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
        return messages;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUserName();
        var message = await unitOfWork.MessageRepository.GetMessage(id);
        if(message == null) return NotFound("Message Not Found!");
        if (message.Sender.UserName != username && message.Recipient.UserName != username)
            return Unauthorized();
        if (message.Sender.UserName == username) message.SenderDeleted = true;
        if (message.Recipient.UserName == username) message.RecipientDeleted = true;
        if (message.SenderDeleted && message.RecipientDeleted)
            unitOfWork.MessageRepository.DeleteMessage(message);
        if (await unitOfWork.Complete()) return Ok();
        return BadRequest("Problem deleting the message");
    }
}
