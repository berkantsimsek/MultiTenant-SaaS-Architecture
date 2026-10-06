using System;

namespace SaaSProje.Services
{
    public interface ICurrentTenantService
    {
        Guid? FirmId { get; }
        void SetFirmId(Guid firmId);
    }
}