using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace API.Entities;

public class Group(string name)
{
    [Key]
    public string Name { get; set; } = name;
    public ICollection<Connection> Connections { get; set; } = new List<Connection>();
}
