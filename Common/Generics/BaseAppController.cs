using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Common.Generics
{
    public abstract class BaseAppController : ControllerBase
    {
        protected readonly IUserService _userService;

        protected BaseAppController(IUserService userService)
        {
            _userService = userService;
        }

        protected async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
