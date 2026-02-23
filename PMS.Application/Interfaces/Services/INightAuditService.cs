using System.Threading.Tasks;
using PMS.Application.DTOs.Audit;
using PMS.Application.DTOs.Common;

namespace PMS.Application.Interfaces.Services
{
    public interface INightAuditService
    {
        /// <summary>
        /// Returns the current business day status (open/closed and since when).
        /// </summary>
        Task<ApiResponse<AuditStatusDto>> GetCurrentStatusAsync();

        /// <summary>
        /// Runs the night audit process for the specified user.
        /// </summary>
        /// <param name="userId">The user initiating the audit.</param>
        /// <param name="force">Whether to force closing even if validations fail.</param>
        Task<ApiResponse<AuditResponseDto>> RunNightAuditAsync(string userId, bool force);

        /// <summary>
        /// System-triggered wrapper: runs the night audit automatically at 2:00 AM.
        /// Calls RunNightAuditAsync with userId="SYSTEM_AUTO" and force=true.
        /// Throws on failure so Hangfire records the job as "Failed".
        /// </summary>
        Task RunAutoNightAuditAsync();
    }
}

