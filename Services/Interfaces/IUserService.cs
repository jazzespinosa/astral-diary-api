using AstralDiaryApi.Models.DTOs.Users;
using AstralDiaryApi.Models.Entities;
using FirebaseAdmin.Auth;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> LoginUser(string firebaseUid, LoginRequest loginRequest);
        Task<Guid> GetUserId(string firebaseUid);
    }
}
