namespace TodoApi.Models
{
    public abstract class TodoItemDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public Priority Priority { get; set; } = Priority.Low;
        public bool IsComplete { get; set; }
        public DateTime? DueDate { get; set; }

        public TodoItemDTO() { }
        public TodoItemDTO(Todo todoItem)
        {
            (Id, Name, Priority, IsComplete, DueDate) = (todoItem.Id, todoItem.Name, todoItem.Priority, todoItem.IsComplete, todoItem.DueDate);
        }
    }

    public class TodoItemInputDTO : TodoItemDTO
    {
        public TodoItemInputDTO() { }
        public TodoItemInputDTO(Todo todoItem) : base(todoItem) { }
    }

    public class TodoItemOutputDTO : TodoItemDTO
    {
        public bool IsOverdue =>
            DueDate.HasValue &&
            !IsComplete &&
            DueDate.Value < DateTime.Now;

        public TodoItemOutputDTO() { }
        public TodoItemOutputDTO(Todo todoItem) : base(todoItem) { }
    }
}
