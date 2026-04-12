using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttachmentController : BaseAppController
    {
        private readonly IFileStorageService _fileStorageService;

        public AttachmentController(
            IFileStorageService fileStorageService,
            IUserService userService
        )
            : base(userService)
        {
            _fileStorageService = fileStorageService;
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
    }
}
