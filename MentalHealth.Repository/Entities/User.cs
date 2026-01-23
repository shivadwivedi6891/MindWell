namespace MentalHealth.Repository.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsAnonymous { get; set; }
    public bool IsActive { get; set; }
    public bool ReadyToChat { get; set; } = false;
    public bool Online {get; set;} = true;
      // user toggles this


    public ExpertProfile? ExpertProfile { get; set; }
}
