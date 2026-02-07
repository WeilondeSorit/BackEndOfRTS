namespace StatisticsService.Models
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public Guid PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int TotalMatches { get; set; }
        public decimal WinRate { get; set; }
        public int Kills { get; set; }
        public int WinStreak { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}