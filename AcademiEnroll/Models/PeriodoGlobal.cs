using System.ComponentModel.DataAnnotations;

namespace AcademiEnroll.Models
{
    public class PeriodoGlobal
    {
        public int Id { get; set; }  // Este es el identificador único de la tabla
        
        public int Periodo { get; set; }  // Valor de periodo (1, 2, 3, 4, 5)
    }
}
