namespace StepNet.Repositories.Interfaces
{
    public interface IConfigurationRepository
    {
        Task<ServerConfiguration> GetServerConfigurationByIdAsync(int id);
        Task<IEnumerable<ServerConfiguration>> GetAllServerConfigurationsAsync();
        Task AddServerConfigurationAsync(ServerConfiguration config);
        Task UpdateServerConfigurationAsync(ServerConfiguration config);
        Task DeleteServerConfigurationAsync(int id); Task<ServerConfiguration> GetServerConfigurationByIdAsync(int id);
        Task<IEnumerable<ServerConfiguration>> GetAllServerConfigurationsAsync();
        Task AddServerConfigurationAsync(ServerConfiguration config);
        Task UpdateServerConfigurationAsync(ServerConfiguration config);
        Task DeleteServerConfigurationAsync(int id);
    }
}
