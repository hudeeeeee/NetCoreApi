using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MVC.Models
{
    public class Employee : Person
    {
        public string EmpoyeeId { get; set; } = default!;
        public int Age { get; set; } = default!;
    }
}