using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class GroupViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
