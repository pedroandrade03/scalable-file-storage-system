using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StorageSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExternalIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalProvider",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSubject",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE users
                SET "ExternalProvider" = 'legacy',
                    "ExternalSubject" = "Id"::text
                WHERE "ExternalProvider" IS NULL
                   OR "ExternalSubject" IS NULL;
                """
            );

            migrationBuilder.AlterColumn<string>(
                name: "ExternalProvider",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalSubject",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_ExternalProvider_ExternalSubject",
                table: "users",
                columns: new[] { "ExternalProvider", "ExternalSubject" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_ExternalProvider_ExternalSubject",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ExternalProvider",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ExternalSubject",
                table: "users");
        }
    }
}
