using DotLearn.Assessment.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotLearn.Assessment.Data;

public class AssessmentDbContext : DbContext
{
    public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options)
        : base(options) { }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<AttemptAnswer> AttemptAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quiz>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Questions)
             .WithOne(x => x.Quiz)
             .HasForeignKey(x => x.QuizId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Options)
             .WithOne(x => x.Question)
             .HasForeignKey(x => x.QuestionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Answers)
             .WithOne(x => x.Attempt)
             .HasForeignKey(x => x.AttemptId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AttemptAnswer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SelectedOptionIdsJson).HasColumnName("SelectedOptionIds");
            e.Ignore(x => x.SelectedOptionIds);
        });
    }
}
