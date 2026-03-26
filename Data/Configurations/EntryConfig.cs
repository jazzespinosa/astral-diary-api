using AstralDiaryApi.Models.Entities;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstralDiaryApi.Data.Configurations
{
    public class EntryConfig : IEntityTypeConfiguration<Entry>
    {
        public void Configure(EntityTypeBuilder<Entry> builder)
        {
            builder.HasKey(e => e.Id);
            builder
                .HasOne(e => e.User)
                .WithMany(u => u.Entries)
                .HasForeignKey(e => e.UserId)
                .HasPrincipalKey(u => u.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Property(e => e.EntityId).HasMaxLength(25).IsRequired();
            builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
            builder.Property(e => e.Content).HasColumnType("TEXT").IsRequired();
            builder.ToTable(t =>
                t.HasCheckConstraint(
                    "CK_Entry_DeletedAt_OnlyIf_IsDeleted",
                    "(is_deleted = 1 AND deleted_at IS NOT NULL) OR (is_deleted = 0 AND deleted_at IS NULL)"
                )
            );
            builder.HasIndex(e => e.EntityId);
            builder.HasIndex(e => new { e.Title, e.Content }).IsFullText();
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Date);
            builder.HasIndex(e => e.Mood);

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
