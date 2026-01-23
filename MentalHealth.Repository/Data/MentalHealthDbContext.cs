using Microsoft.EntityFrameworkCore;
using MentalHealth.Repository.Entities;

namespace MentalHealth.Repository.Data;

public class MentalHealthDbContext : DbContext
{
    public MentalHealthDbContext(DbContextOptions<MentalHealthDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ExpertProfile> ExpertProfiles => Set<ExpertProfile>();
    public DbSet<MoodEntry> MoodEntries => Set<MoodEntry>();
    public DbSet<MoodAnswer> MoodAnswers => Set<MoodAnswer>();
    public DbSet<MoodQuestion> MoodQuestions => Set<MoodQuestion>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<GroupTopic> GroupTopics => Set<GroupTopic>();
    public DbSet<GroupSession> GroupSessions => Set<GroupSession>();
    public DbSet<ChatParticipant> ChatParticipants => Set<ChatParticipant>();
    public DbSet<ChatReflection> ChatReflections => Set<ChatReflection>();
    public DbSet<ExpertInvitation> ExpertInvitations => Set<ExpertInvitation>();
    public DbSet<MoodTrend> MoodTrends => Set<MoodTrend>();
    public DbSet<UserChatRequest> UserChatRequests => Set<UserChatRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ExpertProfile (1–1)
        modelBuilder.Entity<ExpertProfile>()
            .HasOne(e => e.User)
            .WithOne(u => u.ExpertProfile)
            .HasForeignKey<ExpertProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // MoodEntry
        modelBuilder.Entity<MoodEntry>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId);

        // MoodAnswer
        modelBuilder.Entity<MoodAnswer>()
            .HasOne(a => a.MoodEntry)
            .WithMany(e => e.Answers)
            .HasForeignKey(a => a.MoodEntryId);

        modelBuilder.Entity<MoodAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message
        modelBuilder.Entity<Message>()
            .HasOne(m => m.ChatSession)
            .WithMany(cs => cs.Messages)
            .HasForeignKey(m => m.ChatSessionId);

        modelBuilder.Entity<Message>()
            .HasIndex(m => m.ChatSessionId);

        modelBuilder.Entity<Message>()
            .HasIndex(m => m.CreatedAt);

        // ChatParticipant
        modelBuilder.Entity<ChatParticipant>()
            .HasOne(cp => cp.ChatSession)
            .WithMany(cs => cs.Participants)
            .HasForeignKey(cp => cp.ChatSessionId);

        modelBuilder.Entity<ChatParticipant>()
            .HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChatParticipant>()
            .HasIndex(cp => new { cp.ChatSessionId, cp.UserId })
            .IsUnique();

        // ChatReflection (IMPORTANT FIX)
        modelBuilder.Entity<ChatReflection>()
            .HasOne(cr => cr.ChatSession)
            .WithMany(cs => cs.Reflections)
            .HasForeignKey(cr => cr.ChatSessionId);

            modelBuilder.Entity<UserChatRequest>()
    .HasOne(r => r.FromUser)
    .WithMany()
    .HasForeignKey(r => r.FromUserId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<UserChatRequest>()
    .HasOne(r => r.ToUser)
    .WithMany()
    .HasForeignKey(r => r.ToUserId)
    .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<ChatReflection>()
            .HasOne(cr => cr.User)
            .WithMany()
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // MoodTrend
        modelBuilder.Entity<MoodTrend>()
            .HasOne(mt => mt.User)
            .WithMany()
            .HasForeignKey(mt => mt.UserId);

        // Report
        modelBuilder.Entity<Report>()
            .HasOne(r => r.ChatSession)
            .WithMany()
            .HasForeignKey(r => r.ChatSessionId);

        // GroupSession
        modelBuilder.Entity<GroupSession>()
            .HasOne(gs => gs.Topic)
            .WithMany()
            .HasForeignKey(gs => gs.TopicId);

        modelBuilder.Entity<GroupSession>()
            .HasOne(gs => gs.ChatSession)
            .WithOne()
            .HasForeignKey<GroupSession>(gs => gs.ChatSessionId);

        // ExpertInvitation (EXPLICIT ROLES)
        modelBuilder.Entity<ExpertInvitation>()
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExpertInvitation>()
            .HasOne(i => i.Expert)
            .WithMany()
            .HasForeignKey(i => i.ExpertId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
