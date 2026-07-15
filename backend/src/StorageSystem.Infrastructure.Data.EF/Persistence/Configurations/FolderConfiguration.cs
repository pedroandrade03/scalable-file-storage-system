using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StorageSystem.Domain.Entities;

namespace StorageSystem.Infrastructure.Data.EF.Persistence.Configurations;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("folders");

        builder.HasKey(folder => folder.Id);

        builder.Property(folder => folder.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(folder => folder.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(folder => folder.UpdatedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(folder => folder.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Folder>()
            .WithMany()
            .HasForeignKey(folder => folder.ParentFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(folder => new { folder.UserId, folder.ParentFolderId, folder.Name })
            .IsUnique();
    }
}
