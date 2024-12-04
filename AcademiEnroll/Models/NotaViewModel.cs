namespace AcademiEnroll.Models
{
    public class NotaViewModel
    {
        public int Id { get; set; }
        public string NombreEstudiante { get; set; }
        public string NombreAsignatura { get; set; }
        public decimal Calificacion { get; set; }
        public string? NombreDocente { get; set; } // Información adicional que no está en el modelo Nota
    }
}
