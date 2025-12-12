using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ExamApp.Domain;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ExamApp.Infrastructure.Database;

public class ExamDbContext(DbContextOptions<ExamDbContext> options) : DbContext(options)
{
    public DbSet<Exam> Exams => Set<Exam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Value converters for custom ID types
        var examIdConverter = new ValueConverter<ExamId, Guid>(
            v => v.Value,
            v => new ExamId(v)
        );
        var examQuestionIdConverter = new ValueConverter<ExamQuestionId, Guid>(
            v => v.Value,
            v => new ExamQuestionId(v)
        );
        var userIdConverter = new ValueConverter<UserId, string>(
            v => v.Value,
            v => new UserId(v)
        );
        var durationConverter = new ValueConverter<Duration, string>(
            v => v.ToStringFormat(),  // Duration → "1h"
            v => Duration.Parse(v)   // "1h" → Duration
        );
        // Dictionary/List ValueComparer for change tracking
        // Improved ValueComparer for List<int>
        var listComparer = new ValueComparer<List<int>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c != null ? c.Aggregate(0, (a, i) => HashCode.Combine(a, i.GetHashCode())) : 0,
            c => c != null ? c.ToList() : new List<int>());

        // Improved ValueComparer for List<int>?
        var nullableListComparer = new ValueComparer<List<int>?>(
            (c1, c2) => c1 == c2 || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c != null ? c.Aggregate(0, (a, i) => HashCode.Combine(a, i.GetHashCode())) : 0,
            c => c != null ? c.ToList() : null);

        var jsonSerializerOptions = new JsonSerializerOptions();

        // Configure Exam entity (aggregate root)
        modelBuilder.Entity<Exam>(eb =>
        {
            eb.HasKey(e => e.Id);
            eb.Property(e => e.Id).HasConversion(examIdConverter);
            eb.Property(e => e.CreatedBy).HasConversion(userIdConverter).IsRequired();
            eb.Property(e => e.Duration).HasConversion(durationConverter)
                .IsRequired()
                .HasMaxLength(10);
            eb.Property(e => e.StartedAt).IsRequired(false);
            eb.Property(e => e.SubmittedAt).IsRequired(false);
            eb.Property(e => e.CreatedAt).IsRequired();
        });
        // Configure ExamQuestion as OWNED entity
        modelBuilder.Entity<Exam>(eb =>
        {
            eb.OwnsMany(e => e.Questions, qb =>
            {
                qb.HasKey(q => q.Id);

                qb.Property(q => q.Id)
                    .HasConversion(examQuestionIdConverter)
                    .HasColumnName("id");

                qb.Property(q => q.ExamId)  // Owner foreign key
                    .HasConversion(examIdConverter)
                    .HasColumnName("exam_id");

                // Question properties
                qb.Property(q => q.Index).HasColumnName("index").IsRequired();
                qb.Property(q => q.SourceQuestionId).HasColumnName("source_question_id").IsRequired();
                qb.Property(q => q.UserSelection).HasColumnName("user_selection");
                qb.Property(q => q.CorrectAnswer).HasColumnName("correct_answer");

                // answer order
                qb.Property<List<int>?>("answerOrder")
                    .HasField("answerOrder")
                    .HasColumnName("answer_order")
                    .HasConversion(
                        v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                        v => v == null ? null : JsonSerializer.Deserialize<List<int>>(v, jsonSerializerOptions) ?? new())
                    .HasColumnType("jsonb")
                    .Metadata.SetValueComparer(nullableListComparer);

                qb.Property<Dictionary<int, string>?>("explanations")
                    .HasColumnName("explanations")
                    .HasConversion(
                        v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                        v => v == null ? null : JsonSerializer.Deserialize<Dictionary<int, string>>(v!, jsonSerializerOptions))
                    .HasColumnType("jsonb");
            });
        });
    }
}
