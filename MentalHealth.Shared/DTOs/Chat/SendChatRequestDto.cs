namespace MentalHealth.Shared.DTOs.Chat;


public class SendChatRequestDto
{
    public Guid ToUserId { get; set; }
}


public class SetChatAvailabilityDto
{
    public bool ReadyToChat { get; set; }
}

public class DiscoverUserDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; }
    public bool IsAnonymous { get; set; }
}

