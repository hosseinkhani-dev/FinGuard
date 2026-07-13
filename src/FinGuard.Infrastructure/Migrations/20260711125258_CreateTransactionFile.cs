using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinGuard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateTransactionFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Format = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessingStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionFiles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionFiles");
        }
    }
}
