using Microsoft.AspNetCore.Identity.Data;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
//string connection = "Server=(localdb)\\mssqllocaldb;Database=applicationdb;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
builder.Services.AddDbContext<NewsContext>(options => options.UseSqlServer(connection));
// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseDefaultFiles();

app.UseStaticFiles();

app.MapRazorPages();




app.MapGet("/api/register", async (ApplicationContext db) => await db.Users.ToListAsync());

List<News> news = new List<News>();

var contextOptions = new DbContextOptionsBuilder<NewsContext>()
    .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0")
    .Options;




using (var context = new NewsContext(contextOptions))
{
    news = context.news.ToList();
}

app.UseWhen(context => context.Request.Path == "/api/news",
    appBuilder =>
    {
        appBuilder.Run(async context =>
        {

            await context.Response.WriteAsJsonAsync(news);

        }
            );
    }
    );


app.MapPost("/api/register", async (User userData, ApplicationContext db) =>
{
    // получаем пользователя по login
    var user = await db.Users.FirstOrDefaultAsync(u => u.Login == userData.Login);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (user != null) return Results.NotFound(new { message = "Пользователь существует" }); 

    // если пользователь найден, изменяем его данные и отправляем обратно клиенту
    await db.Users.AddAsync(userData);
    await db.SaveChangesAsync();
    return Results.Json(userData);
});





app.Run();
public class User
{
    public int Id { get; set; }
   public String Login { get; set; } = "";
   public string Password { get; set; } = "";
}

public class News
{
    public int Id { get; set; }

    //public String img { get; set; } = "";
    public string Head { get; set; } = "";
    public String Info { get; set; } = "";
}

public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        Database.EnsureCreated();   // создаем базу данных при первом обращении
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Login = "bob", Password = "1234" },
                new User { Id = 2, Login = "bib", Password = "1234" },
                new User { Id = 3, Login = "bab", Password = "1234" }
        );
    }
}


public class NewsContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public NewsContext(DbContextOptions<NewsContext> options)
        : base(options)
    {
        Database.EnsureCreated();   // создаем базу данных при первом обращении
    }

    public DbSet<News> news { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<News>().HasData(
                new News { Id = 1, Head = "bob", Info = "1234" },
                new News { Id = 2, Head = "bib", Info = "1234" },
                new News { Id = 3, Head = "bab", Info = "1234" }
        );
    }
}

/*public class NewsContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0");
    }
}*/