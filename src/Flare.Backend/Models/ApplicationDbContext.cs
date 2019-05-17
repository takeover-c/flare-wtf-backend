using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Flare.Backend.Models {
    public class file {
        [Key]
        public int id { get; set; }

        public string name { get; set; }

        public long size { get; set; }

        public string content_type { get; set; }

        public DateTimeOffset created_at { get; set; }

        public DateTimeOffset? updated_at { get; set; }
    }
    
    public class user {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        
        public string email { get; set; }

        public byte[] password_salt { get; set; }

        public byte[] password_hash { get; set; }
		
	    public DateTimeOffset? password_set_at { get; set; }

		public int type { get; set; }
        
        public DateTimeOffset created_at { get; set; }

        public DateTimeOffset? updated_at { get; set; }

        public string phone { get; set; }
        
        public int? avatar_id { get; set; }
        
        public virtual file avatar { get; set; }
    }

    public class client {
        [Key]
        public Guid id { get; set; }

        public string client_name { get; set; }

        public byte[] client_secret_salt { get; set; }

        public byte[] client_secret_hash { get; set; }

        public bool trusted { get; set; }

		public string product_name { get; set; }

		public string copyright_text { get; set; }

		public string sender { get; set; }

        public virtual List<refresh_token> refresh_tokens { get; set; }
    }
    
    public class refresh_token {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public Guid id { get; set; }

        public byte[] exchange_code_salt { get; set; }

        public byte[] exchange_code_hash { get; set; }

        public byte[] refresh_token_salt { get; set; }

        public byte[] refresh_token_hash { get; set; }

        public int user_id { get; set; }
        
        public Guid? client_id { get; set; }

        public virtual client client { get; set; }

        public DateTimeOffset created_at { get; set; }

        public virtual user user { get; set; }
    }

    public class personal_access_token {
        [Key]
        public Guid id { get; set; }

        public int user_id { get; set; }

        public virtual user user { get; set; }

        public string name { get; set; }

        public byte[] password_salt { get; set; }

        public byte[] password_hash { get; set; }

        public DateTimeOffset created_at { get; set; }

        public DateTimeOffset? updated_at { get; set; }

        public DateTimeOffset? deleted_at { get; set; }
    }
    
    public class ApplicationDbContext : DbContext {

        public virtual DbSet<client> clients { get; set; }

        public virtual DbSet<refresh_token> refresh_tokens { get; set; }

        public virtual DbSet<user> users { get; set; }

        public virtual DbSet<personal_access_token> pacs { get; set; }
        
        public virtual DbSet<file> files { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {

        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string)))
            {
                property.AsProperty().Builder
                    .HasMaxLength(256, ConfigurationSource.Convention);
            }
            
            modelBuilder.Entity<user>()
                .HasIndex(a => a.email)
                .IsUnique();

            modelBuilder.Entity<user>()
                .Property(x => x.email)
                .IsRequired();

            modelBuilder.Entity<user>()
                .Property(x => x.password_hash)
                .HasMaxLength(32);

            modelBuilder.Entity<user>()
                .Property(x => x.password_salt)
                .HasMaxLength(16);

            modelBuilder.Entity<client>()
                .Property(x => x.client_secret_hash)
                .HasMaxLength(32);

            modelBuilder.Entity<client>()
                .Property(x => x.client_secret_salt)
                .HasMaxLength(16);

            modelBuilder.Entity<personal_access_token>()
                .Property(x => x.password_hash)
                .HasMaxLength(32);

            modelBuilder.Entity<personal_access_token>()
                .Property(x => x.password_salt)
                .HasMaxLength(16);

            modelBuilder.Entity<personal_access_token>()
                .HasOne(x => x.user)
                .WithMany()
                .HasForeignKey(a => a.user_id);

            modelBuilder.Entity<refresh_token>()
                .Property(x => x.exchange_code_hash)
                .HasMaxLength(32);

            modelBuilder.Entity<refresh_token>()
                .Property(x => x.exchange_code_salt)
                .HasMaxLength(16);

            modelBuilder.Entity<refresh_token>()
                .Property(x => x.refresh_token_hash)
                .HasMaxLength(32);

            modelBuilder.Entity<refresh_token>()
                .Property(x => x.refresh_token_salt)
                .HasMaxLength(16);

            modelBuilder.Entity<refresh_token>()
                .HasOne(a => a.user)
                .WithMany()
                .HasForeignKey(a => a.user_id);

            modelBuilder.Entity<refresh_token>()
                .HasOne(a => a.client)
                .WithMany()
                .HasForeignKey(a => a.client_id);

            modelBuilder.Entity<client>()
                .HasMany(a => a.refresh_tokens)
                .WithOne()
                .HasForeignKey(a => a.client_id);
            
            modelBuilder.Entity<user>()
                .HasOne(a => a.avatar)
                .WithMany()
                .HasForeignKey(a => a.avatar_id);
        }
    }
}
