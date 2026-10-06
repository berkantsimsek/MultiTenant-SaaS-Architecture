using System;

namespace SaaSProje.Models
{
    public interface IMustHaveTenant
    {
        Guid FirmId { get; set; }
    }
}