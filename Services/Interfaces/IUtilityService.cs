using AstralDiaryApi.Models.DTOs.Utility;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IUtilityService
    {
        Task<FeedbackResponse> TriggerEmailSend(FeedbackRequest feedbackRequestDto, Guid playerId);
    }
}
