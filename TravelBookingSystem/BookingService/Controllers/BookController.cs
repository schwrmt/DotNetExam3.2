using Microsoft.AspNetCore.Mvc;
using BookingService.Models;
using BookingService.Dtos;
using GrpcContracts;
using System.Text.Json;

namespace BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController(AppDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var booking = new Booking
        {
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var hotelRequest = new HotelBookingRequest
        {
            BookingId = booking.Id.ToString(),
            UserId = request.UserId
        };

        var outboxMessage = new OutboxMessage
        {
            Type = "HotelBookingRequest",
            Payload = JsonSerializer.Serialize(hotelRequest),
            CreatedAt = DateTime.UtcNow,
            Processed = false
        };

        context.OutboxMessages.Add(outboxMessage);
        await context.SaveChangesAsync();

        return Ok(new { booking.Id });
    }
}