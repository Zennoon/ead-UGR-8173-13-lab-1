namespace TodoApi.Models;

public class TodoItemDTO
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public Priority Priority { get; set; } = Priority.Low;
  public bool IsComplete { get; set; }

  public TodoItemDTO() { }
  public TodoItemDTO(Todo todoItem) =>
    (Id, Name, Priority, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.Priority, todoItem.IsComplete);
}
