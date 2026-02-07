using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatisticsService.Models
{
    public class ErrorLog
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        [Required]
        public string ErrorMessage { get; set; } = string.Empty;
        
        [Required]
        public string StackTrace { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? ServiceName { get; set; }
        
        [MaxLength(200)]
        public string? Endpoint { get; set; }
        
        public string? RequestData { get; set; }
    }
}