using DocumentManagementBackend.Data.Interfaces;
using DocumentManagementBackend.Data;

using DocumentManagementBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using DocumentManagementBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Reg DB Context
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite("Data Source=users.db"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<EmailService>();

var app = builder.Build();


app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
