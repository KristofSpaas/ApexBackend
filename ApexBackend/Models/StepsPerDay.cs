using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ApexBackend.Models
{
    public class StepsPerDay
    {
        [Key]
        public int StepsPerDayId { get; set; }

        [Required]
        public int StepsPerDayInt { get; set; }

        [Required]
        public long DateMillis { get; set; }

        // Foreign key 
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
    }
}