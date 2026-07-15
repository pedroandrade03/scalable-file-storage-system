using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StorageSystem.Domain.Entities;
using StorageSystem.Domain.Enums;

namespace StorageSystem.Infrastructure.Data.EF.Persistence.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<FileItem>
{
    public void Configure(EntityTypeBuilder<FileItem> builder)
    {
        builder.ToTable("files");

        builder.HasKey(file => file.Id);

        builder.Property(file => file.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(file => file.StorageKey)
            .HasMaxLength(1024)
            .IsRequired();

        builder.HasIndex(file => file.StorageKey)
            .IsUnique();

        builder.Property(file => file.ContentType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(file => file.SizeBytes)
            .IsRequired();

        builder.Property(file => file.Status)
            .HasConversion<int>()
            .HasDefaultValue(FileStatus.PendingUpload)
            .IsRequired();

        builder.Property(file => file.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(file => file.UpdatedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(file => file.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Folder>()
            .WithMany()
            .HasForeignKey(file => file.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
