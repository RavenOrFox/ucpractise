using Microsoft.AspNetCore.Identity.Data;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
//string connection = "Server=(localdb)\\mssqllocaldb;Database=applicationdb;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

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

var err = 0;
/*app.UseWhen(context => context.Request.Path == "/api/register",
    appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            var UserData = await context.Request.ReadFromJsonAsync<User>(); 

        }
            );
    }
    );*/

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
