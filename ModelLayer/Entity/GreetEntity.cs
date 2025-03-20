using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelLayer.Entity
{
    public class GreetEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public UserEntity User { get; set; }
    }
}