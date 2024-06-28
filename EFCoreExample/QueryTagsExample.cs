using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreExample;
public class QueryTagsExample
{

    public int NoOfRecords { get; set; }

    public QueryTagsExample()
    {
        NoOfRecords = 1000;
        using var context = new TodoDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.SeedData(NoOfRecords);
    }

    public List<TodoItem> GetTodos()
    {
        using var dbContext = new TodoDbContext();

        var todos = dbContext
            .TodoItems
            .Where(d => !d.IsCompleted)
            .TagWith($"Method: GetTodos {DateTime.UtcNow}")
            .TagWith(@$"Multiline tag: GetTodos 
Logged At: {{DateTime.UtcNow}}")
            .ToList();

        return todos;
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
