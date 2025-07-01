using Grpc.Core;
using GrpcContracts;

namespace HotelService.Services;

public class HotelBookingService : HotelBooking.HotelBookingBase
{
    public override Task<HotelBookingResponse> BookHotel(HotelBookingRequest request, ServerCallContext context)
    {
        Console.WriteLine($"[HotelService] Booking hotel for user {request.UserId}, bookingId {request.BookingId}");
        return Task.FromResult(new HotelBookingResponse { Success = true });
    }
}
