using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ApexBackend.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        [StringLength(1000)]
        public String MessageContent { get; set; }

        [Required]
        public Boolean FromDoctor { get; set; }
        
        [Required]
        public long DateMillis { get; set; }

        // Foreign key 
        [Required]
        public int PatientId { get; set; }

        // Foreign key 
        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
    }
}