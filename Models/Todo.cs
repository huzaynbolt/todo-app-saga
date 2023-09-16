using Microsoft.EntityFrameworkCore;

namespace todo.api.Models
{
    public record Todo
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; } 
        
        public TaskState State { get; set; }

        public List<TodoOwner> Owners { get; } = new();
    }
    

    public enum TaskState
    {
        Completed = 2,
        Assigned = 1,
        Pending = 0,
        Archived = 3,
    }
}
