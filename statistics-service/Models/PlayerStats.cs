using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatisticsService.Models
{
    public class PlayerStats
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public Guid PlayerId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int TotalMatches { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int WinStreak { get; set; }
        public int MaxWinStreak { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal WinRate => TotalMatches > 0 ? Math.Round((decimal)Wins / TotalMatches * 100, 2) : 0;
        
        public DateTime LastUpdated { get; set; }
    }
}