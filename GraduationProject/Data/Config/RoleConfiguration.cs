using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraduationProject.Data.Config;

public sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData([
            new IdentityRole
            {
                Id = "25801C14-CBA0-4E74-8F6A-9AA57BA5A57F",
                Name = "User",
                NormalizedName = "USER"
            },
            new IdentityRole
            {
                Id = "BE3B9D48-68F5-42E3-9371-E7964F96A25D",
                Name = "Admin",
                NormalizedName = "ADMIN"
            }
        ]);
    }
}