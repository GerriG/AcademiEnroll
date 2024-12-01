using System.ComponentModel.DataAnnotations;

namespace AcademiEnroll.Models
{
    public class Estudiante
    {
        [Key]
        public int IdEstudiante { get; set; }
        public string Nombre { get; set; }       
        public string Correo { get; set; }
        public int IdUsuario { get; set; }
    }

}
