using System;

namespace StatsServer.Models
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public int Experience { get; set; }
        public int Currency { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }
}