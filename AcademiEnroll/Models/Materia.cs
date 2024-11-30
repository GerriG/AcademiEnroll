using AcademiEnroll.Models;
using System.ComponentModel.DataAnnotations;

namespace AcademiEnroll.Models
{
    public class Materia
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Materia y Horario")]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Codigo Materia")]
        public string Codigo { get; set; }

        [Required]
        [Display(Name = "Docente de Materia")]
        public int IdDocente { get; set; }

        public Docente? Docente { get; set; }
    }
}
