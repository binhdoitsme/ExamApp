using ExamApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExamApp.Infrastructure.Database;

public class ExamRepository(ExamDbContext dbContext) : IExamRepository
{
    public async Task<Exam?> GetById(
        ExamId id,
        CancellationToken cancellationToken = default
    ) => await dbContext.Exams
        .AsNoTracking()
        .Include(e => e.Questions.OrderBy(q => q.Index))
        .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Exam?> GetLatestUnstarted(
        UserId userId, CancellationToken cancellationToken = default
    ) => await dbContext.Exams
        .AsNoTracking()
        .Where(e => e.CreatedBy == userId && e.StartedAt == null)
        .OrderByDescending(e => e.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<Exam>> ListExams(
        UserId userId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default
    ) => await dbContext.Exams
        .AsNoTracking()
        .Include(e => e.Questions.OrderBy(q => q.Index))
        .Where(e => e.CreatedBy == userId)
        .Skip(offset)
        .Take(limit)
        .ToListAsync(cancellationToken);

    public async Task Save(Exam exam, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Exams.Where(e => e.Id == exam.Id).AnyAsync(cancellationToken))
        {
            dbContext.Update(exam);
        }
        else
        {
            await dbContext.Exams.AddAsync(exam, cancellationToken);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
