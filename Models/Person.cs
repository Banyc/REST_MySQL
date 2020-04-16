using System.ComponentModel.DataAnnotations;

namespace REST_MySQL.Models
{
    public class Person
    {
        [Key]
        public long Uid { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string IdentityCardNumber { get; set; }

    }
}
