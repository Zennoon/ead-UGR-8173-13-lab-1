using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.Converters.Add(new FriendlyDateConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
      {
          config.DocumentTitle = "TodoAPI";
          config.Path = "/swagger";
          config.DocumentPath = "/swagger/{documentName}/swagger.json";
          config.DocExpansion = "list";
      }
    );
}

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos);

todoItems.MapGet("/complete", GetCompleteTodos);

todoItems.MapGet("/overdue", GetOverdueTodos);

todoItems.MapGet("/{id}", GetTodo);

todoItems.MapPost("/", CreateTodo);

todoItems.MapPut("/{id}", UpdateTodo);

todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db, Priority? priority, bool? overdue, int page = 1, int pageSize = 10)
{
    var query = db.Todos.AsQueryable();

    if (priority.HasValue)
    {
        query = query.Where(t => t.Priority == priority);
    }

    if (overdue.HasValue)
    {
        query = query.Where(t =>
            overdue.Value
                ? t.DueDate < DateTime.Now && !t.IsComplete
                : t.DueDate >= DateTime.Now || t.IsComplete
        );
    }

    return TypedResults.Ok(await query.Skip((page - 1) * pageSize).Take(pageSize).Select(t => new TodoItemOutputDTO(t)).ToArrayAsync());
}


static async Task<IResult> GetCompleteTodos(TodoDb db, Priority? priority, bool? overdue, int page = 1, int pageSize = 10)
{
    var query = db.Todos.AsQueryable();

    query = query.Where(t => t.IsComplete);

    if (priority.HasValue)
    {
        query = query.Where(t => t.Priority == priority);
    }

    if (overdue.HasValue)
    {
        query = query.Where(t =>
            overdue.Value
                ? t.DueDate < DateTime.Now && !t.IsComplete
                : t.DueDate >= DateTime.Now || t.IsComplete
        );
    }

    return TypedResults.Ok(await query.Select(t => new TodoItemOutputDTO(t)).ToListAsync());
}

static async Task<IResult> GetOverdueTodos(TodoDb db, Priority? priority, bool? overdue, int page = 1, int pageSize = 10)
{
    var query = db.Todos.AsQueryable();

    query = query.Where(t => t.DueDate < DateTime.Now);

    if (priority.HasValue)
    {
        query = query.Where(t => t.Priority == priority);
    }

    if (overdue.HasValue)
    {
        query = query.Where(t =>
            overdue.Value
                ? t.DueDate < DateTime.Now && !t.IsComplete
                : t.DueDate >= DateTime.Now || t.IsComplete
        );
    }

    return TypedResults.Ok(await query.Select(t => new TodoItemOutputDTO(t)).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
      is Todo todo
        ? TypedResults.Ok(new TodoItemOutputDTO(todo))
        : TypedResults.NotFound();
}


static async Task<IResult> CreateTodo(TodoItemInputDTO todoItemInputDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        IsComplete = todoItemInputDTO.IsComplete,
        Name = todoItemInputDTO.Name,
        Priority = todoItemInputDTO.Priority,
        DueDate = todoItemInputDTO.DueDate
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    var todoItemOutputDTO = new TodoItemOutputDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemOutputDTO);
}

static async Task<IResult> UpdateTodo(int id, TodoItemInputDTO todoItemDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;
    todo.Priority = todoItemDTO.Priority;
    todo.DueDate = todoItemDTO.DueDate;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}
