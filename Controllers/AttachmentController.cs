using AstralDiaryApi.Models.DTOs.Attachments;
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

        [HttpGet("token/{id}")]
        [Authorize]
        public async Task<IActionResult> GetImageToken(string id)
        {
            var userId = await GetUserId();
            var token = _attachmentTokenService.CreateToken(userId, id);
            return Ok(new { token });
        }

        [HttpGet("get-thumbnail/{entityId}/{internalFileName}")]
        public async Task<IActionResult> GetThumbnail(
            string entityId,
            string internalFileName,
            [FromQuery] string t
        )
        {
            var tokenData = _attachmentTokenService.ValidateToken(t);

            if (tokenData == null || tokenData.Id != entityId)
            {
                return Unauthorized();
            }

            var fileDownloadResult = await _fileStorageService.GetThumbnail(
                tokenData.UserId,
                entityId,
                internalFileName
            );

            var contentType = GetContentType(fileDownloadResult);

            return File(fileDownloadResult.FileBytes, contentType, fileDownloadResult.FileName);
        }

        [HttpGet("get-attachment/{entityId}/{internalFileName}")]
        [Authorize]
        public async Task<IActionResult> GetAttachment(string entityId, string internalFileName)
        {
            var userId = await GetUserId();

            var fileDownloadResult = await _fileStorageService.GetAttachment(
                userId,
                entityId,
                internalFileName
            );

            var contentType = GetContentType(fileDownloadResult);

            return File(fileDownloadResult.FileBytes, contentType, fileDownloadResult.FileName);
        }

        private string GetContentType(FileDownloadResult fileDownloadResult)
        {
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(fileDownloadResult.FileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        private async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
