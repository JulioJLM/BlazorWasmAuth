using Microsoft.AspNetCore.Identity;

namespace BlazorWasmAuth.Data;

public class AppUser : IdentityUser
{
    ICollection<TodoItem> TodoItems { get; set; } = [];
}
