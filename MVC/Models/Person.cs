using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MVC.Models
{
    [Table("Person")]
    public class Person
    {
        [Key]
        public string? PersonId { get; set; }
        public string FullName { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string  Ages { get; set; } = default!;
    }
}