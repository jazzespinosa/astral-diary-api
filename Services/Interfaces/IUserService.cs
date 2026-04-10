using AstralDiaryApi.Models.DTOs.Users;
using AstralDiaryApi.Models.Entities;
using FirebaseAdmin.Auth;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> LoginUser(string firebaseUid, LoginRequest loginRequest);
        Task<Guid> GetUserId(string firebaseUid);
        Task<GetUserInfoResponse> GetUserInfoAsync(Guid userId, DateOnly currentDate);
        Task<string?> GetUserAvatar(Guid userId);
        Task<string?> UpdateUserAvatar(Guid userId, string? avatar);
    }
}
