using HotelService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<HotelBookingService>();
app.MapGet("/", () => "This is the Hotel gRPC service");

app.Run();