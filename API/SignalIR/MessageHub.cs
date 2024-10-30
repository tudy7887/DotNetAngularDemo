using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalIR;

public class MessageHub : Hub
    {
        private readonly IMapper mapper;
        private readonly IHubContext<PresenceHub> presenceHub;
        private readonly PresenceTracker tracker;
        private readonly IUnitOfWork unitOfWork;
        public MessageHub(IMapper mapper, IUnitOfWork unitOfWork, IHubContext<PresenceHub> presenceHub,
            PresenceTracker tracker)
        {
            this.unitOfWork = unitOfWork;
            this.tracker = tracker;
            this.presenceHub = presenceHub;
            this.mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if(httpContext ==  null) return;
            var otherUser = httpContext.Request.Query["user"].ToString();
            if(Context.User == null) return;
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
            var messages = await unitOfWork.MessageRepository.
                GetMessageThread(Context.User.GetUserName(), otherUser);
            if (unitOfWork.HasChanges()) await unitOfWork.Complete();
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            if(Context.User == null)
                throw new HubException("Canot get username from token!");
            var username = Context.User.GetUserName();
            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yourself");
            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (sender ==  null) throw new HubException("Sender not found!");
            var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipient == null) throw new HubException("User not found!");
            if(sender.UserName == null) throw new HubException("Can't get sender username!"); 
            if(recipient.UserName == null) throw new HubException("Can't get user username!"); 
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            var groupName = GetGroupName(sender.UserName!, recipient.UserName!);
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if(group == null) throw new HubException("Group not found!");
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await tracker.GetConnectionsForUser(recipient.UserName!);
                if (connections != null)
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }
            unitOfWork.MessageRepository.AddMessage(message);
            if (await unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if(Context.User == null)
                throw new HubException("Canot get username from token!");
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());
            if (group == null)
            {
                group = new Group(groupName);
                unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            if (await unitOfWork.Complete()) return group;
            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            if(group == null) throw new HubException("Group not found!");
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if(connection == null)
                throw new HubException("Cannot find group connection!");
            unitOfWork.MessageRepository.RemoveConnection(connection);
            if (await unitOfWork.Complete()) return group;
            throw new HubException("Failed to remove from group");
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
