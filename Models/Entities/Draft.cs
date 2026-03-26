using System.ComponentModel.DataAnnotations.Schema;
using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.Entities
{
    public class Draft : IAttachmentSource, IEntityIdSource
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("draft_id")]
        public string EntityId { get; set; } = $"draft-{GenerateDraftId()}";

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("created_at", TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; }

        [Column("modified_at", TypeName = "timestamp")]
        public DateTime ModifiedAt { get; set; }

        [Column("date", TypeName = "date")]
        public DateOnly Date { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("mood")]
        public int Mood { get; set; }

        public User User { get; set; } // Navigation property

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        [NotMapped]
        public string DraftId
        {
            get => EntityId;
            set => EntityId = value;
        }

        public void LinkAttachment(Attachment attachment)
        {
            attachment.DraftId = this.EntityId;
        }

        private static int _sequence = 0;
        private static readonly object _lock = new object();

        public static string GenerateDraftId()
        {
            lock (_lock)
            {
                _sequence = (_sequence + 1) % 100; // 3 digits
                return _sequence.ToString("D3") + DateTime.UtcNow.ToString("yyyyMMddHHmmssff");
            }
        }
    }
}
