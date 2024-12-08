namespace AcademiEnroll.Models
{
    public class ReporteDocente
    {
        // Nombre del docente
        public string NombreDocente { get; set; }

        // Cantidad de materias que imparte el docente
        public int MateriasImpartidas { get; set; }

        // Cantidad de estudiantes inscritos en las materias del docente
        public int TotalEstudiantes { get; set; }

        // Cantidad de estudiantes aprobados en las materias del docente
        public int TotalAprobados { get; set; }

        // Cantidad de estudiantes reprobados en las materias del docente
        public int TotalReprobados { get; set; }
    }
}
