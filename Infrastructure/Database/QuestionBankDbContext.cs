using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamApp.Infrastructure.Database;

// EF Core models
[Table("question_entity")]
public class QuestionEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("text")]
    public string Text { get; set; } = default!;

    [Column("category")]
    public string Category { get; set; } = default!;

    [Column("image_data_uri")]
    public string? ImageDataUri { get; set; }

    public IList<AnswerEntity> Answers { get; set; } = [];
}

[Table("answer_entity")]
public class AnswerEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("text")]
    public string Text { get; set; } = default!;

    [Column("is_correct")]
    public bool IsCorrect { get; set; }

    [Column("explanation")]
    public string? Explanation { get; set; }
}

public class QuestionBankDbContext(DbContextOptions<QuestionBankDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public IQueryable<QuestionEntity> Questions => Set<QuestionEntity>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure QuestionEntity as principal
        modelBuilder.Entity<QuestionEntity>(qb =>
        {
            qb.ToTable("question_entity");

            qb.HasKey(q => q.Id);

            qb.Property(q => q.Text)
                .IsRequired()
                .HasMaxLength(1000);

            qb.Property(q => q.Category)
                .IsRequired()
                .HasMaxLength(200);

            qb.Property(q => q.ImageDataUri)
                .HasMaxLength(2048);

            // Question OWNS Answers (cascade delete, no separate lifecycle)
            qb.OwnsMany(q => q.Answers, ab =>
            {
                ab.ToTable("answer_entity");

                // Composite key: QuestionId + Answer position/index
                ab.WithOwner()
                  .HasForeignKey("QuestionId");

                ab.HasKey("QuestionId", "Id"); // Composite key

                ab.Property<int>("QuestionId") // Shadow property
                  .HasColumnName("question_id")
                  .ValueGeneratedNever(); // Managed by owner

                ab.Property(a => a.Id)
                  .HasColumnName("id")
                  .ValueGeneratedOnAdd();

                ab.Property(a => a.Text)
                  .IsRequired()
                  .HasMaxLength(500);

                ab.Property(a => a.IsCorrect)
                  .IsRequired();

                ab.Property(a => a.Explanation)
                  .HasMaxLength(1000);
            });

            // Index for random selection by category
            qb.HasIndex(q => q.Category);
        });
    }
}
