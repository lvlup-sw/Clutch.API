
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StepNet.Providers.Servers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication
    public class ServersController : ControllerBase
    {
        private readonly IGameServerRepository _serverRepository;
        private readonly IJobQueue _jobQueue;

        public ServersController(IGameServerRepository serverRepository, IJobQueue jobQueue)
        {
            _serverRepository = serverRepository;
            _jobQueue = jobQueue;
        }

        [HttpPost]
        public async Task<IActionResult> CreateServer([FromBody] ServerConfiguration config)
        {
            // ... Input Validation ...

            var server = await _serverRepository.CreateServerAsync(config);

            // Enqueue a job to launch the server instance
            _jobQueue.Enqueue<ServerDeploymentJob>(job => job.LaunchServer(server.Id));

            return CreatedAtAction(nameof(GetServer), new { id = server.Id }, server);
        }

        // ... Other API endpoints ...
    }
}

