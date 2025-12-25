// Data/ApplicationDbContext.cs
using LTF_Library_V1.Data.Models;
using LTF_Library_V1.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LTF_Library_V1.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {

        // DbSets for all tables
        public DbSet<Publication> Publications
        {
            get; set;
        }
        public DbSet<Creator> Creators
        {
            get; set;
        }
        public DbSet<Publisher> Publishers
        {
            get; set;
        }
        public DbSet<Genre> Genres
        {
            get; set;
        }
        public DbSet<MediaType> MediaTypes
        {
            get; set;
        }
        public DbSet<MediaCondition> MediaConditions
        {
            get; set;
        }
        public DbSet<Bookcase> Bookcases
        {
            get; set;
        }
        public DbSet<Shelf> Shelves
        {
            get; set;
        }
        public DbSet<Participant> Participants
        {
            get; set;
        }
        public DbSet<PublicationTransfer> PublicationTransfers
        {
            get; set;
        }
        public DbSet<PublicationCreator> PublicationCreators
        {
            get; set;
        }
        public DbSet<PublicationGenre> PublicationGenres
        {
            get; set;
        }
        public DbSet<PublicationKeyWord> PublicationKeyWords
        {
            get; set;
        }
        public DbSet<PublicationRequest> PublicationRequests
        {
            get; set;
        }
        public DbSet<PendingPublicRequest> PendingPublicRequests
        {
            get; set;
        }
     
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Publisher entity (column name mapping)
            modelBuilder.Entity<Publisher>(entity =>
            {
                entity.Property(e => e.Publisher1).HasColumnName("Publisher");
            });

            // Configure Genre entity (column name mapping)
            modelBuilder.Entity<Genre>(entity =>
            {
                entity.Property(e => e.Genre1).HasColumnName("Genre");
            });

            // Configure MediaType entity (column name mapping)
            modelBuilder.Entity<MediaType>(entity =>
            {
                entity.Property(e => e.MediaType1).HasColumnName("MediaType");
            });

            // Configure MediaCondition entity (column name mapping)
            modelBuilder.Entity<MediaCondition>(entity =>
            {
                entity.Property(e => e.MediaCondition1).HasColumnName("MediaCondition");
            });

            // Configure Bookcase entity (column name mapping)
            modelBuilder.Entity<Bookcase>(entity =>
            {
                entity.Property(e => e.Bookcase1).HasColumnName("Bookcase");
            });

            // Configure Shelf entity (column name mapping)
            modelBuilder.Entity<Shelf>(entity =>
            {
                entity.Property(e => e.Shelf1).HasColumnName("Shelf");
            });
            // Configure ParticipantStatus entity (column name mapping)
            //Added ExtendedDescription mapping 2025-12-22
            //Added TransactionType and SortOrder mapping 2025-12-23
            modelBuilder.Entity<ParticipantStatus>(entity =>
            {
                entity.Property(e => e.ParticipantStatusID).HasColumnName("ParticipantStatusID");
                entity.Property(e => e.ParticipantStatus1).HasColumnName("ParticipantStatus");
                entity.Property(e => e.ExtendedDescription).HasColumnName("ExtendedDescription");
                entity.Property(e => e.TransactionType).HasColumnName("TransactionType");   
                entity.Property(e => e.SortOrder).HasColumnName("SortOrder");       
            });
            // Configure the view
            modelBuilder.Entity<PendingPublicRequest>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vwPendingPublicRequests");
            });

     
            // Configure relationships
            modelBuilder.Entity<Publication>(entity =>
            {
                entity.HasOne(p => p.MediaCondition)
                    .WithMany(mc => mc.Publications)
                    .HasForeignKey(p => p.MediaConditionID)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.MediaType)
                    .WithMany(mt => mt.Publications)
                    .HasForeignKey(p => p.MediaTypeID)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Publisher)
                    .WithMany(pub => pub.Publications)
                    .HasForeignKey(p => p.PublisherID)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Shelf)
                    .WithMany(s => s.Publications)
                    .HasForeignKey(p => p.ShelfID)
                    .OnDelete(DeleteBehavior.SetNull);
                // calculated PubsortTitle to ignore mapping
                entity.Property(p => p.PubSort).HasComputedColumnSql();  // Tells EF this is computed
                // Set default values
                entity.Property(p => p.MediaConditionID).HasDefaultValue(PublicationDefaults.MediaCondition) ;
                entity.Property(p => p.MediaTypeID).HasDefaultValue(PublicationDefaults.MediaType);
                entity.Property(p => p.Volume).HasDefaultValue(PublicationDefaults.Volume);
                entity.Property(p => p.NumberOfVolumes).HasDefaultValue(PublicationDefaults.NumberOfVolumes);
                entity.Property(p => p.ConfidenceLevel).HasDefaultValue(PublicationDefaults.ConfidenceLevel);
                entity.Property(p => p.DateCaptured).HasDefaultValueSql("GETDATE()");
                entity.Property(p => p.ShelfID).HasDefaultValue(PublicationDefaults.Shelf);
            });

            modelBuilder.Entity<PublicationCreator>(entity =>
            {
                entity.HasOne(pc => pc.Creator)
                    .WithMany(c => c.PublicationCreators)
                    .HasForeignKey(pc => pc.CreatorID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Publication)
                    .WithMany(p => p.PublicationCreators)
                    .HasForeignKey(pc => pc.PublicationID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(pc => new { pc.PublicationID, pc.CreatorID }).IsUnique();
            });

            modelBuilder.Entity<PublicationGenre>(entity =>
            {
                entity.HasOne(pg => pg.Genre)
                    .WithMany(g => g.PublicationGenres)
                    .HasForeignKey(pg => pg.GenreID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pg => pg.Publication)
                    .WithMany(p => p.PublicationGenres)
                    .HasForeignKey(pg => pg.PublicationID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(pg => new { pg.PublicationID, pg.GenreID }).IsUnique();
            });

            modelBuilder.Entity<PublicationKeyWord>(entity =>
            {
                entity.HasOne(pkw => pkw.Publication)
                    .WithMany(p => p.PublicationKeyWords)
                    .HasForeignKey(pkw => pkw.PublicationID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(pkw => new { pkw.PublicationID, pkw.KeyWord }).IsUnique();
            });

            modelBuilder.Entity<PublicationRequest>(entity =>
            {
                // Configure relationship to Publication
                entity.HasOne(pr => pr.Publication)
                    .WithMany() // or .WithMany(p => p.Requests) if you add that collection to Publication
                    .HasForeignKey(pr => pr.PublicationID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship to ApplicationUser (ProcessedBy)
                entity.HasOne(pr => pr.ProcessedByUser)
                    .WithMany(u => u.ProcessedRequests)
                    .HasForeignKey(pr => pr.ProcessedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                // Set default values
                entity.Property(pr => pr.RequestDate).HasDefaultValueSql("GETDATE()");
                entity.Property(pr => pr.Status).HasDefaultValue("Pending");
            });
            modelBuilder.Entity<Shelf>(entity =>
            {
                entity.HasOne(s => s.Bookcase)
                    .WithMany(bc => bc.Shelves)
                    .HasForeignKey(s => s.BookCaseID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure unique constraints for lookup tables
            modelBuilder.Entity<Creator>(entity =>
            {
                entity.HasIndex(c => new { c.CreatorFirstName, c.CreatorMiddleName, c.CreatorLastName })
                    .IsUnique()
                    .HasDatabaseName("IX_tblCreatorUnique");
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasIndex(g => g.Genre1)
                    .IsUnique()
                    .HasDatabaseName("IX_tblGenre_UniqueGenre");
            });

            modelBuilder.Entity<Publisher>(entity =>
            {
                entity.HasIndex(p => p.Publisher1)
                    .IsUnique()
                    .HasDatabaseName("IX_tblPublisher_Publication");
            });
        }
        //public override int SaveChanges(bool acceptAllChangesOnSuccess) to avoid errors on Null non-ZLS string fields
        public override int SaveChanges()
        {
            ConvertEmptyStringsToNull();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertEmptyStringsToNull();
            return base.SaveChangesAsync(cancellationToken);
        }

    

        // Helper method
        private void ConvertEmptyStringsToNull()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(string))
                    {
                        if (string.IsNullOrWhiteSpace(property.CurrentValue as string))
                        {
                            property.CurrentValue = null;
                        }
                    }
                }
            }
        }

    }
}