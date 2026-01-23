namespace MentalHealth.Repository.Entities;

 public class UserChatRequest
{
   public Guid Id { get; set; }

    public Guid FromUserId { get; set; }
    public User FromUser { get; set; }

    public Guid ToUserId { get; set; }
    public User ToUser { get; set; }

    public string Status { get; set; } // Pending, Accepted, Rejected, Cancelled

    public DateTime CreatedAt { get; set; }
}
