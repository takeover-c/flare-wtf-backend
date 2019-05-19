﻿// <auto-generated />
using System;
using Flare.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Flare.Backend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Flare.Backend.Models.client", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("client_name")
                        .HasMaxLength(256);

                    b.Property<byte[]>("client_secret_hash")
                        .HasMaxLength(32);

                    b.Property<byte[]>("client_secret_salt")
                        .HasMaxLength(16);

                    b.Property<bool>("trusted");

                    b.HasKey("id");

                    b.ToTable("clients");
                });

            modelBuilder.Entity("Flare.Backend.Models.file", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("content_type")
                        .HasMaxLength(256);

                    b.Property<DateTimeOffset>("created_at");

                    b.Property<string>("name")
                        .HasMaxLength(256);

                    b.Property<long>("size");

                    b.Property<DateTimeOffset?>("updated_at");

                    b.HasKey("id");

                    b.ToTable("files");
                });

            modelBuilder.Entity("Flare.Backend.Models.ip_address", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("city_name")
                        .HasMaxLength(256);

                    b.Property<string>("connection_type")
                        .HasMaxLength(256);

                    b.Property<string>("country_code")
                        .HasMaxLength(256);

                    b.Property<string>("country_name")
                        .HasMaxLength(256);

                    b.Property<string>("ip")
                        .HasMaxLength(256);

                    b.Property<string>("isp")
                        .HasMaxLength(256);

                    b.Property<double?>("latitude");

                    b.Property<double?>("longitude");

                    b.Property<string>("organisation")
                        .HasMaxLength(256);

                    b.HasKey("id");

                    b.ToTable("ip_addresses");
                });

            modelBuilder.Entity("Flare.Backend.Models.personal_access_token", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("created_at");

                    b.Property<DateTimeOffset?>("deleted_at");

                    b.Property<string>("name")
                        .HasMaxLength(256);

                    b.Property<byte[]>("password_hash")
                        .HasMaxLength(32);

                    b.Property<byte[]>("password_salt")
                        .HasMaxLength(16);

                    b.Property<DateTimeOffset?>("updated_at");

                    b.Property<int>("user_id");

                    b.HasKey("id");

                    b.HasIndex("user_id");

                    b.ToTable("pacs");
                });

            modelBuilder.Entity("Flare.Backend.Models.refresh_token", b =>
                {
                    b.Property<Guid>("id");

                    b.Property<Guid?>("client_id");

                    b.Property<Guid?>("clientid");

                    b.Property<DateTimeOffset>("created_at");

                    b.Property<byte[]>("exchange_code_hash")
                        .HasMaxLength(32);

                    b.Property<byte[]>("exchange_code_salt")
                        .HasMaxLength(16);

                    b.Property<byte[]>("refresh_token_hash")
                        .HasMaxLength(32);

                    b.Property<byte[]>("refresh_token_salt")
                        .HasMaxLength(16);

                    b.Property<int>("user_id");

                    b.HasKey("id");

                    b.HasIndex("client_id");

                    b.HasIndex("clientid");

                    b.HasIndex("user_id");

                    b.ToTable("refresh_tokens");
                });

            modelBuilder.Entity("Flare.Backend.Models.request", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("flags");

                    b.Property<long>("ip_id");

                    b.Property<DateTimeOffset?>("request_date");

                    b.Property<int?>("request_http_version");

                    b.Property<string>("request_identity")
                        .HasMaxLength(256);

                    b.Property<string>("request_method")
                        .HasMaxLength(256);

                    b.Property<string>("request_path")
                        .HasMaxLength(256);

                    b.Property<string>("request_query_string")
                        .HasMaxLength(256);

                    b.Property<string>("request_user_id")
                        .HasMaxLength(256);

                    b.Property<int?>("response_code");

                    b.Property<int?>("response_length");

                    b.Property<int>("server_id");

                    b.HasKey("id");

                    b.HasIndex("ip_id");

                    b.HasIndex("server_id");

                    b.ToTable("requests");
                });

            modelBuilder.Entity("Flare.Backend.Models.server", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("created_at");

                    b.Property<string>("name")
                        .HasMaxLength(256);

                    b.Property<string>("origin_ip")
                        .HasMaxLength(256);

                    b.Property<bool>("proxy_active");

                    b.Property<bool>("proxy_block_requests");

                    b.Property<DateTimeOffset?>("updated_at");

                    b.HasKey("id");

                    b.ToTable("servers");
                });

            modelBuilder.Entity("Flare.Backend.Models.server_domain", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("domain")
                        .HasMaxLength(256);

                    b.Property<int>("order");

                    b.Property<int>("server_id");

                    b.HasKey("id");

                    b.HasIndex("server_id");

                    b.ToTable("server_domain");
                });

            modelBuilder.Entity("Flare.Backend.Models.user", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("avatar_id");

                    b.Property<DateTimeOffset>("created_at");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("name")
                        .HasMaxLength(256);

                    b.Property<byte[]>("password_hash")
                        .HasMaxLength(32);

                    b.Property<byte[]>("password_salt")
                        .HasMaxLength(16);

                    b.Property<DateTimeOffset?>("password_set_at");

                    b.Property<string>("phone")
                        .HasMaxLength(256);

                    b.Property<int>("type");

                    b.Property<DateTimeOffset?>("updated_at");

                    b.HasKey("id");

                    b.HasIndex("avatar_id");

                    b.HasIndex("email")
                        .IsUnique();

                    b.ToTable("users");
                });

            modelBuilder.Entity("Flare.Backend.Models.personal_access_token", b =>
                {
                    b.HasOne("Flare.Backend.Models.user", "user")
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Flare.Backend.Models.refresh_token", b =>
                {
                    b.HasOne("Flare.Backend.Models.client")
                        .WithMany("refresh_tokens")
                        .HasForeignKey("client_id");

                    b.HasOne("Flare.Backend.Models.client", "client")
                        .WithMany()
                        .HasForeignKey("clientid");

                    b.HasOne("Flare.Backend.Models.user", "user")
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Flare.Backend.Models.request", b =>
                {
                    b.HasOne("Flare.Backend.Models.ip_address", "ip")
                        .WithMany()
                        .HasForeignKey("ip_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Flare.Backend.Models.server", "server")
                        .WithMany("requests")
                        .HasForeignKey("server_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Flare.Backend.Models.server_domain", b =>
                {
                    b.HasOne("Flare.Backend.Models.server", "server")
                        .WithMany("domains")
                        .HasForeignKey("server_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Flare.Backend.Models.user", b =>
                {
                    b.HasOne("Flare.Backend.Models.file", "avatar")
                        .WithMany()
                        .HasForeignKey("avatar_id");
                });
#pragma warning restore 612, 618
        }
    }
}
