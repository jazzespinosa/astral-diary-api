using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AstralDiaryApi.Models.Entities
{
    public class Attachment : IValidatableObject
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("entry_id")]
        public string? EntryId { get; set; }

        [Column("draft_id")]
        public string? DraftId { get; set; }

        [Column("internal_name")]
        public required string InternalFileName { get; set; }

        [Column("original_name")]
        public required string OriginalFileName { get; set; }

        [Column("file_path")]
        public required string FilePath { get; set; }

        [Column("thumbnail_path")]
        public required string ThumbnailPath { get; set; }

        [Column("content_hash")]
        public required string ContentHash { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public Entry Entry { get; set; } // Navigation property
        public Draft Draft { get; set; } // Navigation property

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check if both are null
            if (EntryId == null && DraftId == null)
            {
                yield return new ValidationResult(
                    "An attachment must be linked to either an Entry or a Draft."
                );
            }

            // Check if both are filled
            if (EntryId != null && DraftId != null)
            {
                yield return new ValidationResult(
                    "An attachment cannot be linked to both an Entry and a Draft simultaneously."
                );
            }
        }
    }
}
