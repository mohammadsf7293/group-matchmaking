using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Rovio.MatchMaking;
namespace Rovio.MatchMaking.Net;

[Route("match")]
public class MatchMakingController : Controller
{
    // Assuming there's already an injected service/repository to handle queuing
    private readonly IQueuedPlayerRepository _queuedPlayerRepository;
    private readonly ISessionRepository _sessionRepository;
    private IQueuedPlayerRepository @object;

    public MatchMakingController(IQueuedPlayerRepository queuedPlayerRepository, ISessionRepository sessionRepository)
    {
        _queuedPlayerRepository = queuedPlayerRepository;
        _sessionRepository = sessionRepository;
    }

    [HttpPost("queue")]
    public async Task<IActionResult> QueuePlayerAsync([FromBody] Player player)
    {
        // throw new NotImplementedException();
        if (player == null)
        {
            return BadRequest(new { error = "Player cannot be null" });
        }

        // Check if the player is already queued
        var existingQueuedPlayer = await _queuedPlayerRepository.GetQueuedPlayerByPlayerIdAsync(player.Id);
        if (existingQueuedPlayer != null)
        {
            return BadRequest(new { error = "Player is already queued" });
        }

        // Log Player properties to the console
        Console.WriteLine($"Player Queued: ID = {player.Id}, Name = {player.Name}, Latency = {player.LatencyMilliseconds}");
        
        var queuedPlayer = QueuedPlayer.CreateQueuedPlayerFromPlayer(player);
        var createdQueuedPlayer = await _queuedPlayerRepository.CreateQueuedPlayerAsync(queuedPlayer);
        // Assuming the sessionFactory handles the logic for queuing a player
        // await _sessionFactory.QueuePlayerAsync(player);
        // await _sessionFactory.Create();

        return Ok(new { message = "Player queued successfully" });
    }
    
    [HttpPost("dequeue")]
    public async Task<IActionResult> DequeuePlayerAsync([FromBody] Player player)
    {
        if (player == null)
        {
            return BadRequest(new { error = "Player cannot be null" });
        }

        // Check if the player is already queued
        var queuedPlayer = await _queuedPlayerRepository.GetQueuedPlayerByPlayerIdAsync(player.Id);
        if (queuedPlayer == null)
        {
            return BadRequest(new { error = "Player is not queued" });
        }

        await _queuedPlayerRepository.DeleteQueuedPlayerAsync(queuedPlayer.Id);

        return Ok(new { message = "Player dequeued successfully" });
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinSession([FromBody] JoinSessionRequest requestBody)
    {
        if (requestBody == null)
        {
            return BadRequest(new { error = "Request body cannot be null" });
        }

        var session = await _sessionRepository.GetSessionById(requestBody.SessionId);
        if (session == null) {
            return BadRequest(new { error = "Invalid Session" });
        }

        // if (session.StartsAt > DateTime.UtcNow) {
        //     return BadRequest(new { error = "Session hasn't been started yet!" });
        // }

        var sessionPlayers = await _sessionRepository.GetSessionPlayersBySessionId(requestBody.SessionId);
        List<Guid> playerIdsList = new List<Guid>();
        foreach (var sp in sessionPlayers) {
            playerIdsList.Add(sp.PlayerId);
        }
        if (!playerIdsList.Contains(requestBody.PlayerId)) {
            return BadRequest(new { error = "You don't have permission to join this session" });
        }

        return Ok(new { session = session, sessionPlayers = sessionPlayers });
    }
}