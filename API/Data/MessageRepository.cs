using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddGroup(Group group) => context.Groups.Add(group);
    public void AddMessage(Message message) => context.Messages.Add(message);
    public void DeleteMessage(Message message) => context.Messages.Remove(message);
    public async Task<Connection?> GetConnection(string connectionId) => await context.Connections.FindAsync(connectionId);
    public async Task<Group?> GetGroupForConnection(string connectionId) => await context.Groups
                                                                            .Include(c => c.Connections)
                                                                            .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                                                                            .FirstOrDefaultAsync();
    public async Task<Message?> GetMessage(int id) => await context.Messages
                                                        .Include(u => u.Sender)
                                                        .Include(u => u.Recipient)
                                                        .SingleOrDefaultAsync(x => x.Id == id);
    public async Task<Group?> GetMessageGroup(string groupName) => await context.Groups
                                                                .Include(x => x.Connections)
                                                                .FirstOrDefaultAsync(x => x.Name == groupName);
    public async Task<PagedList<MessageDto>?> GetMessagesForUser(MessageParams messageParams)
    {
        var query = context.Messages
            .OrderByDescending(m => m.MessageSent)
            .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
            .AsQueryable();
        query = messageParams.Container switch
        {
            "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username
                && u.RecipientDeleted == false),
            "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
                && u.SenderDeleted == false),
            _ => query.Where(u => u.RecipientUsername ==
                messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
        };
        return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
    }
    public async Task<IEnumerable<MessageDto>?> GetMessageThread(string currentUsername, string recipientUsername)
    {
        var messages = await context.Messages
            .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                    && m.Sender.UserName == recipientUsername
                    || m.Recipient.UserName == recipientUsername
                    && m.Sender.UserName == currentUsername && m.SenderDeleted == false
            )
            .MarkUnreadAsRead(currentUsername)
            .OrderBy(m => m.MessageSent)
            .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return messages;
    }
    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }
}
