using System.Net.Mail;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Models.Entities;
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
        private readonly IUserService _userService;

        public AttachmentController(
            IFileStorageService fileStorageService,
            IUserService userService
        )
        {
            _fileStorageService = fileStorageService;
            _userService = userService;
        }

        [HttpGet("get/{entityId}/{attachmentType}/{attachmentId}")]
        public async Task<IActionResult> GetThumbnail(
            string entityId,
            string attachmentType,
            string attachmentId
        )
        {
            var userId = await GetUserId();
            var fileDownloadResult = new FileDownloadResult();

            if (entityId.StartsWith("entry-"))
            {
                fileDownloadResult = await _fileStorageService.GetAttachmentFile<Entry>(
                    userId,
                    entityId,
                    attachmentType,
                    attachmentId
                );
            }
            else if (entityId.StartsWith("draft-"))
            {
                fileDownloadResult = await _fileStorageService.GetAttachmentFile<Draft>(
                    userId,
                    entityId,
                    attachmentType,
                    attachmentId
                );
            }
            else
                return BadRequest("Invalid entity type.");

            return File(
                fileDownloadResult.FileBytes,
                "application/octet-stream",
                fileDownloadResult.FileName
            );
        }

        private async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
