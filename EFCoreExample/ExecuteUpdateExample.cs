using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreExample;
public class ExecuteUpdateExample
{
    public int NoOfRecords { get; set; }

    public ExecuteUpdateExample()
    {
        NoOfRecords = 1000;
        using var context = new TodoDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.SeedData(NoOfRecords);
    }

    public int UpdateTodo()
    {
        using var dbContext = new TodoDbContext();

        var todos = dbContext
            .TodoItems
            .Where(d => !d.IsCompleted)
            .ToList();

        foreach (var todo in todos)
        {
            todo.IsCompleted = true;
        }

        return dbContext.SaveChanges();
    }

    public int UpdateTodoWithExecuteUpdate()
    {
        using var dbContext = new TodoDbContext();

        return dbContext
              .TodoItems
              .Where(d => !d.IsCompleted)
              .ExecuteUpdate(s => s.SetProperty(
                  p => p.IsCompleted, true));
    }

    #region Database Configuration  
    public class TodoDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TodoDB;Trusted_Connection=True;ConnectRetryCount=0")
            .LogTo(Console.WriteLine, LogLevel.Information);

        public void SeedData(int numberOfRecords)
        {
            TodoItems.AddRange(
                Enumerable.Range(0, numberOfRecords).Select(
                    i => new TodoItem
                    {
                        Name = $"Todo {i}",
                        IsCompleted = false
                    }));
            SaveChanges();
        }
    }

    public class TodoItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
    }

    #endregion
}
