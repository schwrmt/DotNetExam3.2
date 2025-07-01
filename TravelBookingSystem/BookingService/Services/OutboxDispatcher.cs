using BookingService.Models;
using Grpc.Net.Client;
using GrpcContracts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookingService.Services;

public class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxDispatcher> logger,
    IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxDispatcher запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var pendingMessages = await db.OutboxMessages
                    .Where(m => !m.Processed)
                    .OrderBy(m => m.CreatedAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                if (pendingMessages.Count == 0)
                {
                    await Task.Delay(2000, stoppingToken);
                    continue;
                }

                using var channel = GrpcChannel.ForAddress(configuration["Grpc:HotelServiceUrl"]!);
                var client = new HotelBooking.HotelBookingClient(channel);

                foreach (var message in pendingMessages)
                {
                    if (message.Type == "HotelBookingRequest")
                    {
                        var hotelRequest = JsonSerializer.Deserialize<HotelBookingRequest>(message.Payload);

                        var response = await client.BookHotelAsync(hotelRequest!, cancellationToken: stoppingToken);

                        if (response.Success)
                        {
                            message.Processed = true;
                        }
                        else
                        {
                            logger.LogWarning("HotelService вернул false для BookingId={0}", hotelRequest!.BookingId);
                        }
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
