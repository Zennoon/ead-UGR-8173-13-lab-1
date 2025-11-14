using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
  config.DocumentName = "TodoAPI";
  config.Title = "TodoAPI v1";
  config.Version = "v1";
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

todoItems.MapGet("/{id}", GetTodo);

todoItems.MapPost("/", CreateTodo);

todoItems.MapPut("/{id}", UpdateTodo);

todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db, Priority? priority, int page = 1, int pageSize = 10)
{
  var query = db.Todos.AsQueryable();

  if (priority.HasValue)
  {
    query = query.Where(t => t.Priority == priority);
  }

  return TypedResults.Ok(await query.Skip((page - 1) * pageSize).Take(pageSize).Select(t => new TodoItemDTO(t)).ToArrayAsync());
}


static async Task<IResult> GetCompleteTodos(TodoDb db, Priority? priority, int page = 1, int pageSize = 10)
{
  var query = db.Todos.AsQueryable();

  query = query.Where(t => t.IsComplete);

  if (priority.HasValue)
  {
    query = query.Where(t => t.Priority == priority);
  }

  return TypedResults.Ok(await query.Select(t => new TodoItemDTO(t)).ToListAsync());
}


static async Task<IResult> GetTodo(int id, TodoDb db)
{
  return await db.Todos.FindAsync(id)
    is Todo todo
      ? TypedResults.Ok(new TodoItemDTO(todo))
      : TypedResults.NotFound();
}


static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
  var todoItem = new Todo
  {
    IsComplete = todoItemDTO.IsComplete,
    Name = todoItemDTO.Name,
    Priority = todoItemDTO.Priority
  };

  db.Todos.Add(todoItem);
  await db.SaveChangesAsync();

  todoItemDTO = new TodoItemDTO(todoItem);

  return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
  var todo = await db.Todos.FindAsync(id);

  if (todo is null) return TypedResults.NotFound();

  todo.Name = todoItemDTO.Name;
  todo.IsComplete = todoItemDTO.IsComplete;
  todo.Priority = todoItemDTO.Priority;

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
