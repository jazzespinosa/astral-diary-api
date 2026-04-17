using AstralDiaryApi.Data;
using AstralDiaryApi.Models.DTOs.Users;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEntryService _entryService;

        public UserService(AppDbContext dbContext, IEntryService entryService)
        {
            _dbContext = dbContext;
            _entryService = entryService;
        }

        public async Task<LoginResponse> LoginUser(string firebaseUid, LoginRequest loginRequest)
        {
            UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(
                loginRequest.Email
            );

            if (userRecord.Uid != firebaseUid)
            {
                throw new Exception("Firebase UID does not match email.");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                firebaseUid == u.FirebaseUid
            );

            if (user == null)
            {
                user = await CreateUserInDB(firebaseUid, loginRequest);
            }

            return new LoginResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Avatar = user.Avatar,
            };
        }

        private async Task<User> CreateUserInDB(string firebaseUid, LoginRequest loginRequest)
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = loginRequest.Email,
                Name = loginRequest.Name,
                FirebaseUid = firebaseUid,
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<GetUserInfoResponse> GetUserInfoAsync(Guid userId, DateOnly currentDate)
        {
            var userInitialDetails = await GetUserInitialDetailsAsync(userId);

            return await _entryService.GetUserStatsAsync(userId, userInitialDetails, currentDate);
        }

        private async Task<UserInitialDetailsDto> GetUserInitialDetailsAsync(Guid userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            return new UserInitialDetailsDto
            {
                Email = user.Email,
                DisplayName = user.Name,
                Avatar = user.Avatar,
            };
        }

        public async Task<Guid> GetUserId(string firebaseUid)
        {
            var user = await _dbContext
                .Users.Where(u => u.FirebaseUid == firebaseUid)
                .FirstOrDefaultAsync();

            user = user ?? throw new UnauthorizedAccessException("User not found.");

            return user.UserId;
        }

        public async Task<string?> UpdateUserAvatar(Guid userId, string? avatar)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            user = user ?? throw new UnauthorizedAccessException("User not found.");

            user.Avatar = avatar;
            await _dbContext.SaveChangesAsync();

            return user.Avatar;
        }
    }
}
