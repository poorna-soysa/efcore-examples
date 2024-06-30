using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreExample;

[Config(typeof(StyleConfig))]
[HideColumns(Column.RatioSD)]
public class CompliedQueriesBenchmarkExample
{
    private static readonly Func<TodoDbContext, string, IAsyncEnumerable<TodoItem>> GetTodosByName
        = EF.CompileAsyncQuery(
            (TodoDbContext context, string name) =>
            context.TodoItems.Where(b => b.Name.StartsWith(name)));

    private TodoDbContext _context;

    [Params(1000)]
    public int NoOfRecords { get; set; }

    public CompliedQueriesBenchmarkExample()
    {
        //NoOfRecords = 1000;
        using var context = new TodoDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.SeedData(NoOfRecords);

        _context = new TodoDbContext();

    }

    [Benchmark(Baseline = true)]
    public async ValueTask<int> WithoutCompiledQuery()
    {
        var idSum = 0;

        await foreach (var tofo in _context.TodoItems.Where(b => b.Name.StartsWith("Todo")).AsAsyncEnumerable())
        {
            idSum += tofo.Id;
        }

        return idSum;
    }

    [Benchmark]
    public async ValueTask<int> WithCompiledQuery()
    {
        var idSum = 0;

        await foreach (var todo in GetTodosByName(_context, "Todo"))
        {
            idSum += todo.Id;
        }

        return idSum;
    }

   

    [GlobalCleanup]
    public ValueTask Cleanup() => _context.DisposeAsync();


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
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
    }

    #endregion
}

public class StyleConfig : ManualConfig
{
    public StyleConfig()
    {
        SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
    }
}
