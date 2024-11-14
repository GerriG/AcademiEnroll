using System.ComponentModel.DataAnnotations;

namespace AcademiEnroll.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }

        public string Correo { get; set; }

        public string Clave { get; set; }
        public string Rol { get; set; }
    }

}
