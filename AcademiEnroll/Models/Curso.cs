//Modelo Curso
namespace AcademiEnroll.Models
{
    public class Curso
    {
        public int CodInscripcion { get; set; }
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int DocenteId { get; set; }
        public Docente Docente { get; set; }       
    }
}
