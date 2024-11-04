using Microsoft.EntityFrameworkCore;
using VirtualizedGrid.Server;
using VirtualizedGrid.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS to allow any origin (for development purposes)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Add gRPC services to the container
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS with the defined policy
app.UseCors("AllowAll");

app.UseGrpcWeb(); // Enable gRPC-Web support

// Map gRPC services and enable gRPC-Web
app.MapGrpcService<PartService>().EnableGrpcWeb();

app.UseAuthorization();

app.MapControllers();

app.Run();
