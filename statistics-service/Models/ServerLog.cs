using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatisticsService.Models
{
    public class ServerLog
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Level { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? ServiceName { get; set; }
        
        public string? StackTrace { get; set; }
    }
}