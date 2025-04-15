using System;

namespace Rovio.MatchMaking
{
    public class SessionPlayer
    {
        public Guid Id { get; set; } // Unique identifier for each session-player relationship
        
        public Guid SessionId { get; set; } // ID of the session the player is attending
        
        public Guid PlayerId { get; set; } // ID of the player attending the session
        
        public string Status { get; set; } = "ATTENDED"; // Status of the player in the session (default: ATTENDED)
        
        public int Score { get; set; } = 0; // Score of the player in the contest (default: 0)
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when the record was created

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Timestamp when the record was last updated
    }
}
