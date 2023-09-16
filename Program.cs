using MassTransit;
using Microsoft.EntityFrameworkCore;
using todo.api.Data;
using todo.api.Handler;
using todo.common;
using todo.common.Consumer;
using todo.common.StateMachines;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TodoContext>(options =>
{
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    var dbPath = Path.Join(path, "todo.db");
    options.UseSqlite($"Data Source={dbPath}");
});


builder.Services.AddMassTransit(cfg => {
    //cfg.AddSagaStateMachine<TodoStateMachine, TodoState>().InMemoryRepository();
    cfg.UsingRabbitMq((cntxt, cfg) => {
        
        cfg.Host("localhost", "/", c => {
            c.Username("guest");
            c.Password("guest");
        });
        cfg.ConfigureEndpoints(cntxt);
    });
 
    cfg.AddConsumer<PublishTodoConsumer>();
    cfg.AddConsumer<UserAddedToTodoConsumer>();
    cfg.AddConsumer<TodoAssignedEventConsumer>();
    cfg.AddConsumer<TodoDeletedEventConsumer>();
    cfg.AddConsumer<TodoCompletedEventConsumer>();

});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetService<TodoContext>();
    db?.Database.MigrateAsync();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




app.MapPost("/todo", TodoHandler.AddTodo)
.WithName("Todo")
.WithOpenApi();

app.MapPut("/todo/{id}", TodoHandler.UpdateTodo)
.WithName("Update Todo")
.WithOpenApi();

app.MapDelete("/todo/{id}", TodoHandler.DeleteTodo)
.WithName("Delete Todo")
.WithOpenApi();

app.MapGet("/todo/{id}", TodoHandler.GetTodo)
.WithName("Get a Todo")
.WithOpenApi();


app.MapGet("/todo", TodoHandler.GetTodos)
.WithName("Get all Todos")
.WithOpenApi();

app.MapPost("/todo/{id}/owner", TodoHandler.AddOwner)
.WithName("Add Owner")
.WithOpenApi();

app.MapPost("/todo/{id}/owner/{ownerId}", TodoHandler.AddOwnerViaId)
.WithName("Add Owner Via Id")
.WithOpenApi();

app.Run();



namespace todo.api
{
    public partial class Program
    {

    }
}