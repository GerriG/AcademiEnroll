using System.ComponentModel;

namespace AcademiEnroll.Models
{
    public class NotaViewModel
    {
        public int Id { get; set; }
        [DisplayName("Nombre Estudiante")]
        public string? NombreEstudiante { get; set; }

        [DisplayName("Materia")]
        public string? Nombre { get; set; }
        public decimal Calificacion { get; set; }

        [DisplayName("Docente")]
        public string? NombreDocente { get; set; } // Información adicional que no está en el modelo Nota
        public int Periodo { get; set; }  // Columna Periodo
    }
}
