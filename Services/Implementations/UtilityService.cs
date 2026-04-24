using AstralDiaryApi.Data;
using AstralDiaryApi.env;
using AstralDiaryApi.Models.DTOs.Utility;
using AstralDiaryApi.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Services.Implementations
{
    public class UtilityService : IUtilityService
    {
        private readonly AppDbContext _dbContext;
        private readonly MyEnvironment _env;
        private readonly IConfiguration _configuration;

        public UtilityService(
            AppDbContext dbContext,
            MyEnvironment env,
            IConfiguration configuration
        )
        {
            _dbContext = dbContext;
            _env = env;
            _configuration = configuration;
        }

        public async Task<FeedbackResponse> TriggerEmailSend(
            FeedbackRequest feedbackRequestDto,
            Guid userId
        )
        {
            FirestoreDb db;

            if (!string.IsNullOrEmpty(_configuration["GoogleAdcJson"]))
            {
                var googleAdcJson = _configuration["GoogleAdcJson"];
                var googleCredential = CredentialFactory
                    .FromJson<ServiceAccountCredential>(googleAdcJson)
                    .ToGoogleCredential();

                db = new FirestoreDbBuilder
                {
                    ProjectId = "astral-diary",
                    DatabaseId = _env.FirestoreDatabaseId,
                    GoogleCredential = googleCredential,
                }.Build();
            }
            else
            {
                db = new FirestoreDbBuilder
                {
                    ProjectId = "astral-diary",
                    DatabaseId = _env.FirestoreDatabaseId,
                }.Build();
            }

            var feedbackEmailRecipient = _env.FeedbackEmailRecipient;
            var cloudFirestoreDbLink = _env.CloudFirestoreDbLink;

            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(utcNow, tz);

            var feedbackTimeStamp = localTime;
            var category = feedbackRequestDto.Category;
            var message = feedbackRequestDto.Message;
            var userDetails = await _dbContext
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
            var user = $"{userDetails?.Name} ({userDetails?.Email})";

            var response = new FeedbackResponse();

            var feedbackData = new Dictionary<string, object>
            {
                { "feedbackId", response.FeedbackId },
                { "feedbackTimeStamp", feedbackTimeStamp },
                { "user", user },
                { "category", category },
                { "message", message },
            };
            CollectionReference feedbackCollection = db.Collection("feedback");
            DocumentReference feedbackDocRef = feedbackCollection.Document(
                response.FeedbackId.ToString()
            );
            await feedbackDocRef.SetAsync(feedbackData);

            var emailBody =
                $"<p>Hi,</p><br><p>You have received new feedback. See details below.</p><br><p><strong>FeedbackId</strong> = {response.FeedbackId}</p><p><strong>FeedbackTimeStamp</strong> = {feedbackTimeStamp}</p><p><strong>User</strong> = {user}</p><p><strong>Category</strong> = {category}</p><p><strong>Message</strong> = {message}</p><br><p>For further information, check your Realtime Database via this&nbsp;<a href=\"{cloudFirestoreDbLink}\" target=\"_blank\">link</a>.</p>";

            var emailData = new Dictionary<string, object>
            {
                { "to", feedbackEmailRecipient },
                {
                    "message",
                    new Dictionary<string, object>
                    {
                        { "subject", "Astral Diary New Feedback" },
                        { "html", emailBody },
                    }
                },
            };

            CollectionReference emailCollection = db.Collection("mail");
            DocumentReference emailDocRef = emailCollection.Document(
                response.FeedbackId.ToString()
            );
            await emailDocRef.SetAsync(emailData);

            return response;
        }
    }
}
