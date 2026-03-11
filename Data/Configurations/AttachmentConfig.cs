using AstralDiaryApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstralDiaryApi.Data.Configurations
{
    public class AttachmentConfig : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.HasKey(a => a.Id);
            builder.ToTable(t =>
                t.HasCheckConstraint(
                    "CK_Attachment_SingleSource",
                    "(entry_id IS NULL AND draft_id IS NOT NULL) OR (entry_id IS NOT NULL AND draft_id IS NULL)"
                )
            );
            builder
                .HasOne(a => a.Entry)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EntryId)
                .HasPrincipalKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasOne(a => a.Draft)
                .WithMany(d => d.Attachments)
                .HasForeignKey(a => a.DraftId)
                .HasPrincipalKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(a => a.InternalFileName);
        }
    }
}
