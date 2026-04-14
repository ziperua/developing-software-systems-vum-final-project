namespace Todo.Api.DTOs.Todos
{
    public class TodoResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Details { get; set; }
        public string Priority { get; set; }
        public DateOnly? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
