using System.ComponentModel.DataAnnotations.Schema;

namespace AstralDiaryApi.Models.Entities
{
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("email")]
        public required string Email { get; set; }

        [Column("name")]
        public required string Name { get; set; }

        [Column("avatar")]
        public string? Avatar { get; set; }

        [Column("firebase_uid")]
        public required string FirebaseUid { get; set; }

        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
        public ICollection<Draft> Drafts { get; set; } = new List<Draft>();
    }
}
