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
        //public Usuario Usuario { get; set; }
        //public string Clave { get; set; }
        //public string Rol { get; set; }
        //ya no se usará
    }

}
