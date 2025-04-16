using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public int TypeId { get; set; }

    }
}
