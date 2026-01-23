using MentalHealth.Shared.DTOs.Auth;

namespace MentalHealth.Service.Interfaces;

public interface IExpertService
{
    Task RegisterExpertAsync(ExpertRegisterDto dto);
    Task<List<ExpertApprovalResponseDto>> GetPendingExpertsAsync();
    Task ApproveExpertAsync(Guid expertProfileId, Guid adminId);
    Task RejectExpertAsync(Guid expertProfileId);
}
