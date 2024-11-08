using Backend.Data;
using Backend.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Services
{
    public class TodoService
    {
        public static async Task<IResult> GetAllTodos(int dir, int take, int id, AppDbContext db, ClaimsPrincipal user)
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (dir == 0)
                {
                    return TypedResults.Ok(await db.TodoItems.Where(m => m.Id > id && m.UserId == userId).OrderBy(m => m.Id).Take(take).ToArrayAsync());
                }
                else if (dir == 1)
                {
                    return TypedResults.Ok(await db.TodoItems.Where(m => m.Id > id && m.UserId == userId).OrderByDescending(m => m.Id).Take(take).ToArrayAsync());
                }
            }
            return Results.Unauthorized();
        }

        public static async Task<IResult> GetCompleteTodos(AppDbContext db, ClaimsPrincipal user)
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                return TypedResults.Ok(await db.TodoItems.Where(m => m.UserId == userId && m.Mark == Mark.Solved).ToArrayAsync());
            }
            return Results.Unauthorized();
        }

        public static async Task<IResult> GetTodo(int id, AppDbContext db, ClaimsPrincipal user)
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                return await db.TodoItems.FirstOrDefaultAsync(m => m.Id ==id && m.UserId == userId) is TodoItem todo ? TypedResults.Ok(todo) : TypedResults.NotFound();
            }
            return Results.Unauthorized();
        }

        public static async Task<IResult> CreateTodo(List<TodoItem> todos, AppDbContext db, ClaimsPrincipal user)
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    Parallel.ForEach(todos, todo =>
                    {
                        todo.UserId = userId;
                    });

                    await db.TodoItems.AddRangeAsync(todos);
                    await db.SaveChangesAsync();
                    Parallel.ForEach(todos, todo =>
                    {
                        todo.ActivityNo = $"AC-{todo.Id:0000}";
                    });
                    await db.SaveChangesAsync();
                    return TypedResults.Created($"/todoitems/all/{todos.Count}/{todos[0].Id}", todos);
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                    return Results.BadRequest();
                }
            }
            return Results.Unauthorized();
        }

        public static async Task<IResult> UpdateTodo(int id, TodoItem inputTodo, AppDbContext db, ClaimsPrincipal user)
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try 
                {
                    string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    var todo = await db.TodoItems.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
                    if (todo is null)
                    {
                        return TypedResults.NotFound();
                    }

                    todo.Mark = inputTodo.Mark;
                    if (inputTodo.Mark != Mark.Unmarked)
                    {
                        todo.Subject = inputTodo.Subject;
                        todo.Description = inputTodo.Description;
                    }

                    await db.SaveChangesAsync();

                    return TypedResults.NoContent();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                    return Results.BadRequest();
                }
            }
            return Results.Unauthorized();
        }

        public static async Task<IResult> DeleteTodo(int id, AppDbContext db, ClaimsPrincipal user)
        {
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                { 
                    string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (await db.TodoItems.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId) is TodoItem todo)
                    {
                        if (todo.Mark != Mark.Unmarked)
                        {
                            return TypedResults.BadRequest();
                        }
                        db.TodoItems.Remove(todo);
                        await db.SaveChangesAsync();
                        return TypedResults.NoContent();
                    }

                    return TypedResults.NotFound();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                    return Results.BadRequest();
                }
            }
            return Results.Unauthorized();
        }
    }
}
