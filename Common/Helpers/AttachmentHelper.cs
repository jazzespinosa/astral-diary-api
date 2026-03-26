using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Common.Helpers
{
    public static class AttachmentHelper
    {
        public static async Task<List<AttachmentObjRequest>> ProcessAttachmentsAsync(
            IList<IFormFile>? attachments
        )
        {
            var result = new List<AttachmentObjRequest>();

            if (attachments == null || !attachments.Any())
                return result;

            foreach (var file in attachments)
            {
                string hash = await file.GenerateContentHashAsync();
                result.Add(new AttachmentObjRequest { ContentHash = hash, File = file });
            }

            return result;
        }
    }
}
