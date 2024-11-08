using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend.Data;

[PrimaryKey(nameof(Id))]
public sealed class TodoItem
{
    public int Id { get; set; }
    [Required]
    public string? UserId { get; set; }
    public string? ActivityNo { get; set; }
    [Required]
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public Mark Mark { get; set; }
    public AppUser AppUser { get; set; } = default!;
}

public enum Mark
{
    Unmarked,
    Solved,
    Canceled
}