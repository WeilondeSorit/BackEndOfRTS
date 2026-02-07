using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatisticsService.Models
{
    public class MatchResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string MatchId { get; set; } = string.Empty;
        
        [Required]
        public Guid PlayerId { get; set; }
        
        [Required]
        public bool IsWin { get; set; }
        
        public DateTime MatchDate { get; set; }
        
        [Required]
        public int DurationSeconds { get; set; }
        
        public int UnitsKilled { get; set; }
        public int UnitsLost { get; set; }
        public bool BaseDestroyed { get; set; }
        public string? OpponentId { get; set; }
    }
}