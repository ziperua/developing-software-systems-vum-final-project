namespace Todo.Api.DTOs.Todos
{
    public class UpdateTodoRequest
    {
        public string Title { get; set; }
        public string? Details { get; set; }
        public string Priority { get; set; }
        public DateOnly? DueDate { get; set; }
        public bool IsPublic { get; set; }
        public bool IsCompleted { get; set; }
    }
}
