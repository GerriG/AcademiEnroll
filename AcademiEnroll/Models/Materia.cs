using AcademiEnroll.Models;
using System.ComponentModel.DataAnnotations;

namespace AcademiEnroll.Models
{
    public class Materia
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Codigo { get; set; }

        [Required]
        [Display(Name = "Docente")]
        public int IdDocente { get; set; }

        public Docente? Docente { get; set; }
    }
}
