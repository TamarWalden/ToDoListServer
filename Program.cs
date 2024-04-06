
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddSingleton<ToDoDbContext>();
var app = builder.Build();

// builder.Services.AddDbContext<ToDoDbContext>(options =>
// options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection")).LogTo(Console.WriteLine));

app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.MapGet("/items", async (ToDoDbContext dbContext) =>
{
    try{
    var items = await dbContext.Items.ToListAsync();
    return Results.Ok(items);
    }
    catch(Exception ex)
    {
        throw ex;
    }
});

app.MapPost("", async (ToDoDbContext dbContext, Item item) =>
{
    dbContext.Items.Add(item);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/{id}", async (int id, ToDoDbContext dbContext, Item newItem) =>
{
    var existingItem = await dbContext.Items.FindAsync(id);
    if (existingItem == null)
        return Results.NotFound();

    existingItem.Name = newItem.Name;
    existingItem.IsComplete = newItem.IsComplete;

    await dbContext.SaveChangesAsync();
    return Results.Ok(existingItem);
});

app.MapDelete("/{id}", async (int id, ToDoDbContext dbContext) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound();
    dbContext.Items.Remove(item);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();