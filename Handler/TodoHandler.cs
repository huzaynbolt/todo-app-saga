using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todo.api.Data;
using todo.api.Models;
using todo.common;
using Microsoft.AspNetCore.Http.HttpResults;
using MassTransit.Transports;


namespace todo.api.Handler
{
    public static class TodoHandler
    {
        public static async Task<Created<Todo>> AddTodo(Todo todo, [FromServices] TodoContext context, IPublishEndpoint publishEndpoint)
        {
            context.Todos.Add(todo);
            context.SaveChanges();

            await publishEndpoint.Publish(new PublishTodoEvent() {
                CreatedDate = DateTimeOffset.UtcNow,
                Notes = todo.Description,
                Title = todo.Title,
                Id = todo.Id
            });
            return TypedResults.Created($"/todo/{todo.Id}", todo);
        }


        public static async Task<Ok<Todo>> UpdateTodo(Todo updatedTodo, Guid id,  [FromServices] TodoContext context, IPublishEndpoint publishEndpoint)
        {
            var todo = await context.Todos.SingleOrDefaultAsync(c =>c.Id == id);
            todo.StartTime = updatedTodo.StartTime;
            todo.EndTime = updatedTodo.EndTime;
            todo.Title = updatedTodo.Title;
            var isStateChange = todo.State != updatedTodo.State;
            todo.State = updatedTodo.State;
            if (isStateChange)
            {
                HandleStateChanges(todo, publishEndpoint);
            }
            todo.Description = updatedTodo.Description;
            context.Entry(todo).State = EntityState.Modified;
            context.SaveChanges();
            
            return TypedResults.Ok(todo);
        }

        private static async void HandleStateChanges(Todo todo, IPublishEndpoint publishEndpoint)
        {
           
            if(todo.State == TaskState.Assigned)
            {
                await publishEndpoint.Publish(new TodoAssignedEvent() { Date = DateTimeOffset.Now, Title = todo.Title, TodoId = todo.Id });
            }
            if (todo.State == TaskState.Completed)
            {
                await publishEndpoint.Publish(new TodoCompletedEvent() { Date = DateTime.Now, Title = todo.Title, Id = todo.Id });
            }

        }

        public static async Task<Results<NoContent, NotFound>> DeleteTodo(Guid id, [FromServices] TodoContext context, IPublishEndpoint publishEndpoint)
        {
            var todo = await context.Todos.FindAsync(id);
            context.Entry(todo).State = EntityState.Deleted;
            context.SaveChanges();
            await publishEndpoint.Publish(new TodoDeletedEvent() { Date = DateTime.Now, Title = todo.Title, Id = todo.Id });
            return TypedResults.NoContent();
        }

        public static async Task<Results<Ok<Todo>, NotFound>> GetTodo(Guid id, [FromServices] TodoContext context)
        {
            context.Todos.Include(c => c.Owners);
            var todo = await context.Todos.FindAsync(id);
            return TypedResults.Ok(todo);
        }

        public static async Task<Ok<List<Todo>>> GetTodos([FromServices] TodoContext context)
        {
            var todos = await context.Todos.ToListAsync();
            return TypedResults.Ok(todos);
        }


        public static async Task<Results<Ok,NotFound>> AddOwner(Guid id, Owner owner, [FromServices] TodoContext context, IPublishEndpoint publishEndpoint)
        {
            var todo = await context.Todos.FindAsync(id);

            todo.State = TaskState.Assigned;
         
            context.Todos.Update(todo);
            context.Owners.Add(owner);
            context.TodoOwners.Add(new TodoOwner { OwnerId = owner.Id, TodoId = todo.Id });
            await context.SaveChangesAsync();
            await publishEndpoint.Publish(new TodoAssignedEvent()
            {
                Date = DateTimeOffset.Now,
                Title = todo.Title,
                TodoId = todo.Id,
                User = owner.FirstName
            });
            return TypedResults.Ok();
        }
         
        public static async Task<Results<Ok, NotFound>> AddOwnerViaId(Guid id, Guid ownerId, [FromServices] TodoContext context)
        {
            var todo = await context.Todos.FindAsync(id);
            context.TodoOwners.Add(new TodoOwner { OwnerId = ownerId, TodoId = todo.Id });
            await context.SaveChangesAsync();
            return TypedResults.Ok();
        }
    }
}
