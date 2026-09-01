using System.ComponentModel.DataAnnotations;

namespace Movies.Api.Models
{
    public class BaseModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime ModifiedAt { get; set; }
    }
}