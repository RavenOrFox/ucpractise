using Microsoft.AspNetCore.Identity.Data;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
//string connection = "Server=(localdb)\\mssqllocaldb;Database=applicationdb;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
/*builder.Services.AddDbContext<NewsContext>(options => options.UseSqlServer(connection));*/
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

var secs = "";

app.UseWhen(
    context => context.Request.Path == "/login",
    appBuilder =>
    {

        // отправляем ответ
        appBuilder.Run(async context =>
        {
            await context.Response.SendFileAsync("pages/au.html");
        });
    });


app.UseWhen(
    context => context.Request.Path == "/admin",
    appBuilder =>
    {

        // отправляем ответ
        appBuilder.Run(async context =>
        {
            await context.Response.SendFileAsync("pages/ads.html");
        });
    });



var contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
    .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0")
    .Options;

var context1 = new ApplicationContext(contextOptions);

app.MapGet("/api/news", async (ApplicationContext db) =>
{
    var news = context1.news.ToList();
    
    return Results.Json(news);
});

app.MapPost("/api/login", async ( HttpContext context, ApplicationContext db) =>
{
    var userData = await context.Request.ReadFromJsonAsync<User>();
    // получаем пользователя по login
    var user = await db.Users.FirstOrDefaultAsync(u => u.Login == userData.Login);

    
    if (user == null) return Results.NotFound(new { message = "неверный логин" });
    
    if (user.Password != userData.Password) return Results.NotFound(new { message = "неверный пароль" });
    

    secs = Guid.NewGuid().ToString();
    //context.Response.SendFileAsync("Pages/ads.html");
    return Results.Json(secs);
    
});


app.MapPost("/api/admin", async (HttpContext context, News nws, ApplicationContext db) =>
{   
   // nws = await context.Request.ReadFromJsonAsync<News>();
        string ssid = context.Request.Cookies["ssid"];
    if (ssid != secs) return Results.Conflict("не верный ssid");
    //context.Response.SendFileAsync("Pages/ads.html");

    context1.news.Add(nws);

    await context1.SaveChangesAsync();
    var nes = context1.news.ToList();
    return Results.Json(nes);

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
    public DbSet<News> news { get; set; } = null!;
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        /*Database.EnsureCreated();   // создаем базу данных при первом обращении*/
    }

    /*protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Login = "admin", Password = "admin" },
                new User { Id = 2, Login = "bib", Password = "1234" },
                new User { Id = 3, Login = "bab", Password = "1234" }
        );*/

        /*modelBuilder.Entity<News>().HasData(
                new News { Id = 1, Head = "bob", Info = "1234" },
                new News { Id = 2, Head = "bib", Info = "1234" },
                new News { Id = 3, Head = "bab", Info = "1234" }
        );
    }*/
}
