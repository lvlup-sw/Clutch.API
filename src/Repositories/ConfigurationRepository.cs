using Google;
using StepNet.Repositories.Interfaces;

namespace StepNet.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConfigurationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
