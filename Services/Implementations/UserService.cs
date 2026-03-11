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

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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

            var loginResponse = new LoginResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
            };

            return loginResponse;
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

        public async Task<Guid> GetUserId(string firebaseUid)
        {
            var user = await _dbContext
                .Users.Where(u => u.FirebaseUid == firebaseUid)
                .FirstOrDefaultAsync();

            user = user ?? throw new UnauthorizedAccessException("User not found.");

            return user.UserId;
        }
    }
}
