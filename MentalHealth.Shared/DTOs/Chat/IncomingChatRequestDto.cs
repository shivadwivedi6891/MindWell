namespace MentalHealth.Shared.DTOs.Chat;

public class IncomingChatRequestDto
{
    public Guid RequestId { get; set; }
    public Guid FromUserId { get; set; }
    public string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}
