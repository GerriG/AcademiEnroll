using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiEnroll.Models
{
    public class Nota
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreEstudiante { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreAsignatura { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Calificacion { get; set; }
    }
}
