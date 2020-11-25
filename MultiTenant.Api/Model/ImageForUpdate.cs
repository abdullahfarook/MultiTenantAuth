using System.ComponentModel.DataAnnotations;

namespace MultiTenant.Api.Model
{
    public class ImageForUpdate
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }      
    }
}
