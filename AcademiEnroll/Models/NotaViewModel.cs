namespace AcademiEnroll.Models
{
    public class NotaViewModel
    {
        public int Id { get; set; }
        public string? NombreEstudiante { get; set; }
        public string? Nombre { get; set; }
        public decimal Calificacion { get; set; }
        public string? NombreDocente { get; set; } // Información adicional que no está en el modelo Nota
        public int Periodo { get; set; }  // Columna Periodo
    }
}
