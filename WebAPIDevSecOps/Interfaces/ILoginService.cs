using WebAPIDevSecOps.Dto;

namespace WebAPIDevSecOps.Interfaces
{
    public interface ILoginService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    }
}
