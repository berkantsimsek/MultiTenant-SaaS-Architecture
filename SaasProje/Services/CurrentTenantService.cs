using Microsoft.AspNetCore.Http;
using System;

namespace SaaSProje.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Guid? _firmId;

        public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? FirmId
        {
            get
            {
              
                if (_firmId.HasValue) return _firmId;

                
                var firmIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("FirmId")?.Value;
                if (Guid.TryParse(firmIdClaim, out Guid id))
                {
                    _firmId = id;
                }
                return _firmId;
            }
        }

        public void SetFirmId(Guid firmId) => _firmId = firmId;
    }
}