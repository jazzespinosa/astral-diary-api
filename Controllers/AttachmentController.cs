using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttachmentController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IAttachmentTokenService _attachmentTokenService;
        private readonly IUserService _userService;

        public AttachmentController(
            IFileStorageService fileStorageService,
            IAttachmentTokenService attachmentTokenService,
            IUserService userService
        )
        {
            _fileStorageService = fileStorageService;
            _attachmentTokenService = attachmentTokenService;
            _userService = userService;
        }

        [HttpGet("token/{entryId}")]
        [Authorize]
        public async Task<IActionResult> GetImageToken(string id)
        {
            var userId = await GetUserId();
            var token = _attachmentTokenService.CreateToken(userId, id);
            return Ok(new { token });
        }

        //[HttpGet("get/{fileName}")]
        //[Authorize]
        //public async Task<IActionResult> GetAttachment(string internalFileName)
        //{
        //    var userId = await GetUserId();
        //}

        [HttpGet("get-entry-thumbnail/{entryId}/{internalFileName}")]
        public async Task<IActionResult> GetEntryThumbnail(
            string entryId,
            string internalFileName,
            [FromQuery] string token
        )
        {
            var tokenData = _attachmentTokenService.ValidateToken(token);

            if (tokenData == null || tokenData.Id != entryId)
            {
                return Unauthorized();
            }

            var fileDownloadResult = await _fileStorageService.GetEntryThumbnail(
                tokenData.UserId,
                entryId,
                internalFileName
            );

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(fileDownloadResult.FileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(fileDownloadResult.FileBytes, contentType, fileDownloadResult.FileName);
        }

        private async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
