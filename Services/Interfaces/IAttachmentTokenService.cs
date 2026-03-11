using AstralDiaryApi.Models.DTOs.Attachments;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IAttachmentTokenService
    {
        string CreateToken(Guid userId, string entryId);
        AttachmentTokenObject ValidateToken(string token);
    }
}
