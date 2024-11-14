//Modelo docente 
namespace AcademiEnroll.Models
{
    public class Docente
    {
        public int IdDocente { get; set; }
        public string Nombre { get; set; }        
        public string Correo { get; set; }
        public int IdUsuario { get; set; }               
    }
}
