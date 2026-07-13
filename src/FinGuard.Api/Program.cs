using FinGuard.Api.Infrastructures;
using FinGuard.Application;
using FinGuard.Infrastructure;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("RazorPagesPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:1405") // The URL of the UI
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("RazorPagesPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

await app.RunAsync();
