namespace TodoApi.Models;

public class Todo
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public bool IsComplete { get; set; }
  public Priority Priority { get; set; } = Priority.Low;
  public string? Secret { get; set; }
}
