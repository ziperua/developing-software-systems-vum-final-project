namespace Todo.Api.DTOs.Todos
{
    public class CreateTodoRequest
    {
        public string Title { get; set; }
        public string? Details { get; set; }
        public string Priority { get; set; }
        public DateOnly? DueDate { get; set; }
        public bool IsPublic { get; set; }
    }
}
