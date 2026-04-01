using SharedKernel.Interfaces;
using SharedKernel.Ultilities;

namespace SupplyChain.WebApi.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId =>
             CF.GetInt(_httpContextAccessor.HttpContext?.User?
                 .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

        public string Username =>
            _httpContextAccessor.HttpContext?.User?
                 .FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    }
}
