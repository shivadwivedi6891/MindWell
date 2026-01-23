namespace MentalHealth.Repository.Entities;


public class ExpertInvitation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }      // invited user
 

    public Guid ExpertId { get; set; }  

     public string Status { get; set; } = "Pending";

     public string Message {get;set;} = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
      // expert (also a User)
  

    public User? User { get; set; }
    public User? Expert { get; set; }
}


