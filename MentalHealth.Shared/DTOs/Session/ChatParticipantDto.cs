namespace MentalHealth.Shared.DTOs.Session;
public class ChatParticipantDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } =null!;
    public bool IsAnonymous { get; set; }
    public string Role { get; set; } =null!  ; // User / Expert
}
