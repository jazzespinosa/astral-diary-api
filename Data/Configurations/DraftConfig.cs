using AstralDiaryApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstralDiaryApi.Data.Configurations
{
    public class DraftConfig : IEntityTypeConfiguration<Draft>
    {
        public void Configure(EntityTypeBuilder<Draft> builder)
        {
            builder.HasKey(d => d.Id);
            builder
                .HasOne(d => d.User)
                .WithMany(u => u.Drafts)
                .HasForeignKey(d => d.UserId)
                .HasPrincipalKey(u => u.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Property(d => d.EntityId).HasMaxLength(25).IsRequired();
            builder
                .Property(d => d.CreatedAt)
                .HasColumnType("timestamp")
                .IsRequired()
                .ValueGeneratedNever();
            builder
                .Property(d => d.ModifiedAt)
                .HasColumnType("timestamp")
                .IsRequired()
                .ValueGeneratedNever();
            builder.HasIndex(d => d.EntityId);
            builder.HasIndex(d => d.UserId);
            builder.HasIndex(d => d.ModifiedAt);
        }
    }
}
