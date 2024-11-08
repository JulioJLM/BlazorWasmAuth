using Microsoft.AspNetCore.Identity;

namespace Backend.Data;

public class AppUser : IdentityUser
{
    ICollection<TodoItem> TodoItems { get; set; } = [];
}
