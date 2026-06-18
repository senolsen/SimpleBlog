using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFtpUpdateSettingsToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdateFtpHost",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdateFtpPasswordProtected",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdateFtpPort",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UpdateFtpRemotePath",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UpdateFtpUseSsl",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdateFtpUsername",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateFtpHost",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "UpdateFtpPasswordProtected",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "UpdateFtpPort",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "UpdateFtpRemotePath",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "UpdateFtpUseSsl",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "UpdateFtpUsername",
                table: "SiteSettings");
        }
    }
}
