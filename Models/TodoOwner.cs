namespace todo.api.Models
{
    public class TodoOwner
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid TodoId { get; set; }

        public Todo Todo { get; set; }
        public Owner Owner { get; set; }
    }
}
