using Todo.Api.Models;
using Todo.Api.Data;
using Todo.Api.DTOs.Todos;
using Microsoft.EntityFrameworkCore;

namespace Todo.Api.Services
{
    public class TodoService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public TodoService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //creating
        public async Task<TodoResponse?> CreateTodo(CreateTodoRequest request, Guid userId)
        {
            //creating new todo
            var todo = new TodoItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                Details = request.Details,
                Priority = request.Priority,
                DueDate = request.DueDate,
                IsCompleted = false,
                IsPublic = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            //saving to database
            _context.TodoItems.Add(todo);
            await _context.SaveChangesAsync();

            //response
            return new TodoResponse
            {
                Id = todo.Id,
                Title = todo.Title,
                Details = todo.Details,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                IsCompleted = todo.IsCompleted,
                IsPublic = todo.IsPublic,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt
            };
        }
        //updating
        public async Task<TodoResponse?> UpdateTodo(UpdateTodoRequest request, Guid id, Guid userId)
        {
            var todo = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id);
            if (todo == null) return null;

            if (todo.UserId != userId) return null;

            todo.Title = request.Title;
            todo.Details = request.Details;
            todo.Priority = request.Priority;
            todo.DueDate = request.DueDate;
            todo.IsPublic = request.IsPublic;
            todo.IsCompleted = request.IsCompleted;
            todo.UpdatedAt = DateTime.UtcNow;

            _context.TodoItems.Update(todo);
            await _context.SaveChangesAsync();

            return new TodoResponse
            {
                Id = todo.Id,
                Title = todo.Title,
                Details = todo.Details,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                IsCompleted = todo.IsCompleted,
                IsPublic = todo.IsPublic,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt
            };
        }
        //search
        public async Task<TodoResponse?> GetById(Guid id, Guid userId)
        {
            var todo = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id);
            if (todo == null) return null;

            //security check
            if (todo.UserId != userId) return null;

            return new TodoResponse
            {
                Id = todo.Id,
                Title = todo.Title,
                Details = todo.Details,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                IsCompleted = todo.IsCompleted,
                IsPublic = todo.IsPublic,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt
            };
        }
        //deleting
        public async Task<bool> DeleteTodo(Guid id, Guid userId)
        {
            var todo = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id);
            if (todo == null) return false;

            if (todo.UserId != userId) return false;

            _context.TodoItems.Remove(todo);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<TodoResponse?> SetCompletion(Guid id, bool isCompleted, Guid userId)
        {
            var todo = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id);
            if (todo == null) return null;

            if (userId != todo.UserId) return null;

            todo.IsCompleted = isCompleted;
            todo.UpdatedAt = DateTime.UtcNow;

            _context.TodoItems.Update(todo);
            await _context.SaveChangesAsync();

            return new TodoResponse
            {
                Id = todo.Id,
                Title = todo.Title,
                Details = todo.Details,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                IsCompleted = todo.IsCompleted,
                IsPublic = todo.IsPublic,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt
            };
        }
        public async Task<PagedResponse> GetTodos(Guid userId, TodoQueryParams queryParams)
        {
            var query = _context.TodoItems.Where(t => t.UserId == userId);

            //status filter
            if (queryParams.Status == "active")
                query = query.Where(t => t.IsCompleted == false);
            else if (queryParams.Status == "completed")
                query = query.Where(t => t.IsCompleted == true);

            //priority filter
            if (queryParams.Priority != null)
                query = query.Where(t => t.Priority == queryParams.Priority);

            //search by key word
            if (queryParams.Search != null)
                query = query.Where(t =>
                t.Title.Contains(queryParams.Search) ||
                t.Details !=null && t.Details.Contains(queryParams.Search));

            //due from filter
            if (queryParams.DueFrom != null)
                query = query.Where(t => t.DueDate >= queryParams.DueFrom);

            //due to filter
            if (queryParams.DueTo != null)
                query = query.Where(t => t.DueDate <= queryParams.DueTo);

            //sorting
            query = queryParams.SortBy switch
            {
                "createdAt" => queryParams.SortDir == "asc"
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt),
                "dueDate" => queryParams.SortDir == "asc"
                ? query.OrderBy(t => t.DueDate)
                : query.OrderByDescending(t => t.DueDate),
                "priority" => queryParams.SortDir == "asc"
                ? query.OrderBy(t => t.Priority)
                : query.OrderByDescending(t => t.Priority),
                "title" => queryParams.SortDir == "asc"
                ? query.OrderBy(t => t.Title)
                : query.OrderByDescending(t => t.Title),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            //pagination
            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(t => new TodoResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Details = t.Details,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    IsPublic = t.IsPublic,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return new PagedResponse
            {
                Items = items,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
            };
        }
        public async Task<PagedResponse> GetPublicTodos(TodoQueryParams queryParams)
        {
            var query = _context.TodoItems.Where(t => t.IsPublic == true);

            //status filter
            if (queryParams.Status == "active")
                query = query.Where(t => t.IsCompleted == false);
            else if (queryParams.Status == "completed")
                query = query.Where(t => t.IsCompleted == true);

            //priority filter
            if (queryParams.Priority != null)
                query = query.Where(t => t.Priority == queryParams.Priority);

            //search by key word
            if (queryParams.Search != null)
                query = query.Where(t =>
                t.Title.Contains(queryParams.Search) ||
                t.Details != null && t.Details.Contains(queryParams.Search));

            //due from filter
            if (queryParams.DueFrom != null)
                query = query.Where(t => t.DueDate >= queryParams.DueFrom);

            //due to filter
            if (queryParams.DueTo != null)
                query = query.Where(t => t.DueDate <= queryParams.DueTo);

            //sorting
            query = queryParams.SortBy switch
            {
                "createdAt" => queryParams.SortDir == "asc"
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt),
                "dueDate" => queryParams.SortDir == "asc"
                ? query.OrderBy(t => t.DueDate)
                : query.OrderByDescending(t => t.DueDate),
                "priority" => queryParams.SortDir == "asc"
                ? query.OrderBy(t => t.Priority)
                : query.OrderByDescending(t => t.Priority),
                "title" => queryParams.SortDir == "asc"
                ? query.OrderBy(t => t.Title)
                : query.OrderByDescending(t => t.Title),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            //pagination
            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(t => new TodoResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Details = t.Details,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    IsPublic = t.IsPublic,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return new PagedResponse
            {
                Items = items,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
            };
        }
    }
}
