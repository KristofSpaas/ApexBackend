using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ApexBackend.Models
{
    public class MoodRating
    {
        [Key]
        public int MoodRatingId { get; set; }

        [Required]
        public float Rating { get; set; }

        [Required]
        public long DateMillis { get; set; }

        // Foreign key 
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
    }
}