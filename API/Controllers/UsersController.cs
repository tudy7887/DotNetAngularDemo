using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace API;

[Route("api/[controller]")]
[ApiController]
public class UsersController(DataContext context) : ControllerBase
{
    private readonly DataContext context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var users = await context.Users.ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        if(user == null)
        {
            return NotFound("User Not Found!");
        }
        return Ok(user);
    }
}