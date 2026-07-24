using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rovio.MatchMaking.Application;

namespace Rovio.MatchMaking.Net;

[ApiController]
[Route("match")]
public class MatchMakingController : ControllerBase
{
    private readonly MatchMakingService _matchMakingService;
    private readonly ILogger<MatchMakingController> _logger;

    public MatchMakingController(MatchMakingService matchMakingService, ILogger<MatchMakingController> logger)
    {
        _matchMakingService = matchMakingService;
        _logger = logger;
    }

    [HttpPost("queue")]
    public async Task<IActionResult> QueuePlayerAsync([FromBody] Player player)
    {
        var result = await _matchMakingService.QueuePlayerAsync(player);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Player queued successfully" });
    }

    [HttpPost("dequeue")]
    public async Task<IActionResult> DequeuePlayerAsync([FromBody] Player player)
    {
        var result = await _matchMakingService.DequeuePlayerAsync(player);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Player dequeued successfully" });
    }

    /// <summary>
    /// Returns session details for a matched player (does not mutate membership).
    /// </summary>
    [HttpPost("join")]
    public async Task<IActionResult> GetSession([FromBody] JoinSessionRequest requestBody)
    {
        var result = await _matchMakingService.GetSessionForPlayerAsync(requestBody);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }
}
