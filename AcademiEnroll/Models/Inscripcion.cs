using AcademiEnroll.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiEnroll.Models
{
    public class Inscripcion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CodInscripcion { get; set; }

        [Required]
        public int IdEstudiante { get; set; } // Clave foránea

        [Required]
        [StringLength(50)]
        public string NombreMateria { get; set; }

        [Required]
        [StringLength(50)]
        public string Horario { get; set; }

        // Propiedad de navegación para la relación con Estudiante
        [ForeignKey("IdEstudiante")]
        public Estudiante Estudiantes { get; set; }
    }
}

