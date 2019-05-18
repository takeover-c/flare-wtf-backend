using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Flare.Backend.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    client_name = table.Column<string>(maxLength: 256, nullable: true),
                    client_secret_salt = table.Column<byte[]>(maxLength: 16, nullable: true),
                    client_secret_hash = table.Column<byte[]>(maxLength: 32, nullable: true),
                    trusted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 256, nullable: true),
                    size = table.Column<long>(nullable: false),
                    content_type = table.Column<string>(maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(nullable: false),
                    updated_at = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ip_addresses",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ip = table.Column<string>(maxLength: 256, nullable: true),
                    country_code = table.Column<string>(maxLength: 256, nullable: true),
                    country_name = table.Column<string>(maxLength: 256, nullable: true),
                    city_name = table.Column<string>(maxLength: 256, nullable: true),
                    isp = table.Column<string>(maxLength: 256, nullable: true),
                    organisation = table.Column<string>(maxLength: 256, nullable: true),
                    connection_type = table.Column<string>(maxLength: 256, nullable: true),
                    latitude = table.Column<double>(nullable: true),
                    longitude = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ip_addresses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servers",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 256, nullable: true),
                    proxy_active = table.Column<bool>(nullable: false),
                    proxy_block_requests = table.Column<bool>(nullable: false),
                    created_at = table.Column<DateTimeOffset>(nullable: false),
                    updated_at = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(maxLength: 256, nullable: true),
                    email = table.Column<string>(maxLength: 256, nullable: false),
                    password_salt = table.Column<byte[]>(maxLength: 16, nullable: true),
                    password_hash = table.Column<byte[]>(maxLength: 32, nullable: true),
                    password_set_at = table.Column<DateTimeOffset>(nullable: true),
                    type = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTimeOffset>(nullable: false),
                    updated_at = table.Column<DateTimeOffset>(nullable: true),
                    phone = table.Column<string>(maxLength: 256, nullable: true),
                    avatar_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_files_avatar_id",
                        column: x => x.avatar_id,
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "requests",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    server_id = table.Column<int>(nullable: false),
                    ip_id = table.Column<long>(nullable: false),
                    request_identity = table.Column<string>(maxLength: 256, nullable: true),
                    request_user_id = table.Column<string>(maxLength: 256, nullable: true),
                    request_date = table.Column<DateTimeOffset>(nullable: true),
                    request_method = table.Column<string>(maxLength: 256, nullable: true),
                    request_path = table.Column<string>(maxLength: 256, nullable: true),
                    request_query_string = table.Column<string>(maxLength: 256, nullable: true),
                    request_http_version = table.Column<int>(nullable: true),
                    response_code = table.Column<int>(nullable: true),
                    response_length = table.Column<int>(nullable: true),
                    flags = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_requests_ip_addresses_ip_id",
                        column: x => x.ip_id,
                        principalTable: "ip_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_requests_servers_server_id",
                        column: x => x.server_id,
                        principalTable: "servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "server_domain",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    server_id = table.Column<int>(nullable: false),
                    order = table.Column<int>(nullable: false),
                    domain = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_server_domain", x => x.id);
                    table.ForeignKey(
                        name: "FK_server_domain_servers_server_id",
                        column: x => x.server_id,
                        principalTable: "servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pacs",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    user_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 256, nullable: true),
                    password_salt = table.Column<byte[]>(maxLength: 16, nullable: true),
                    password_hash = table.Column<byte[]>(maxLength: 32, nullable: true),
                    created_at = table.Column<DateTimeOffset>(nullable: false),
                    updated_at = table.Column<DateTimeOffset>(nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pacs", x => x.id);
                    table.ForeignKey(
                        name: "FK_pacs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    exchange_code_salt = table.Column<byte[]>(maxLength: 16, nullable: true),
                    exchange_code_hash = table.Column<byte[]>(maxLength: 32, nullable: true),
                    refresh_token_salt = table.Column<byte[]>(maxLength: 16, nullable: true),
                    refresh_token_hash = table.Column<byte[]>(maxLength: 32, nullable: true),
                    user_id = table.Column<int>(nullable: false),
                    client_id = table.Column<Guid>(nullable: true),
                    clientid = table.Column<Guid>(nullable: true),
                    created_at = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_clients_clientid",
                        column: x => x.clientid,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pacs_user_id",
                table: "pacs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_client_id",
                table: "refresh_tokens",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_clientid",
                table: "refresh_tokens",
                column: "clientid");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_requests_ip_id",
                table: "requests",
                column: "ip_id");

            migrationBuilder.CreateIndex(
                name: "IX_requests_server_id",
                table: "requests",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "IX_server_domain_server_id",
                table: "server_domain",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_avatar_id",
                table: "users",
                column: "avatar_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
            
            migrationBuilder.Sql("INSERT INTO `clients` VALUES ('3DA4385D-5166-4C1F-9C4B-49AFE82BE47E', 'Flare Web Client', 0xED450E5BD910E3F1A0BEAE5311A0A33B, 0x215EA9F40CE560B25BDE66867E96E13407459216C676E73DEF1CEDD25AAC041C, b'1')");
            migrationBuilder.Sql("INSERT INTO `users` VALUES (1, 'Burak Tamturk', 'buraktamturk@gmail.com', 0x807B8F1E9BE5171F4F55910862A0C25A, 0xDF84153FEDD76AEE8409F0E74BF03D2FCB034052E8D1FEFEE33371A4AD283760, NULL, 512, '0001-01-01 00:00:00.000000', NULL, '+306949060117', NULL)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pacs");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "requests");

            migrationBuilder.DropTable(
                name: "server_domain");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "ip_addresses");

            migrationBuilder.DropTable(
                name: "servers");

            migrationBuilder.DropTable(
                name: "files");
        }
    }
}
