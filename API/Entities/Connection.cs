using System;

namespace API.Entities;

public class Connection(string connectionId, string username)
{
    public string ConnectionId { get; set; } = connectionId;
    public string Username { get; set; } = username;
}
