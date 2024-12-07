using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiEnroll.Models
{
    public class MateriasAprobadas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdEstudiante { get; set; }

        [Required]
        public int IdMateria { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,1)")]
        public decimal Promedio { get; set; }

        [Required]
        public DateTime FechaAprobacion { get; set; }
    }
}
