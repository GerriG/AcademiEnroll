using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AcademiEnroll.Models
{
    public class Inscripciones
    {
        [Key]
        public int CodInscripcion { get; set; }  // Identificador numérico automático

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Codigo { get; set; }  // Columna calculada: 'Inscripción' + código

        [ForeignKey("Estudiante")]
        public int IdEstudiante { get; set; }  // Llave foránea para la relación con Estudiantes

        [ForeignKey("Materia")]
        public int IdMateria { get; set; }  // Llave foránea para la relación con Materias

        public Estudiante Estudiante { get; set; }  // Relación con el modelo Estudiante
        public Materia Materia { get; set; }  // Relación con el modelo Materia

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Indica que se generará automáticamente en la base de datos
        public DateTime FechaInscripcion { get; set; }  // Columna que captura automáticamente la fecha y hora de inscripción
    }
}

