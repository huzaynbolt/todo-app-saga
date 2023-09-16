namespace todo.api.Models
{
    public class Owner
    {

        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<TodoOwner> TodoOwners { get; } = new();

    }
}
