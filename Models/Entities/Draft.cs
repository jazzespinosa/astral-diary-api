using System.ComponentModel.DataAnnotations.Schema;
using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.Entities
{
    public class Draft : IEntityIdSource
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

        [Column("mood")]
        public int Mood { get; set; }

        [Column("encrypted_content")]
        public required string EncryptedContent { get; set; }

        [Column("content_iv")]
        public required string ContentIv { get; set; }

        [Column("content_salt")]
        public required string ContentSalt { get; set; }

        [Column("attachment_id")]
        public string? AttachmentId { get; set; }

        [Column("att_file_path")]
        public string? AttachmentPath { get; set; }

        [Column("att_thumbnail_path")]
        public string? ThumbnailPath { get; set; }

        [Column("attachment_hash")]
        public string? AttachmentHash { get; set; }

        public User User { get; set; } // Navigation property

        [NotMapped]
        public string DraftId
        {
            get => EntityId;
            set => EntityId = value;
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
