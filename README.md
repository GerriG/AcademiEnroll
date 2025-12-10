# 🎓 AcademiEnroll: Sistema de Gestión y Matricula Académica

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-512BD4?style=for-the-badge&logo=.net&logoColor=white)

---

## 📖 Descripción del Proyecto

**AcademiEnroll** es una plataforma web robusta de gestión escolar y matricula en línea desarrollada con la tecnología **ASP.NET Core MVC**. Su objetivo es digitalizar y simplificar los procesos administrativos y académicos de instituciones educativas, centralizando la información en un sistema seguro y escalable.

El sistema implementa una arquitectura basada en roles que permite al **Administrador** tener control total sobre los periodos académicos globales, la gestión de usuarios y la administración de materias. Por su parte, dota a los **Docentes** de herramientas para la gestión de sus cursos y el registro de calificaciones de manera eficiente. Finalmente, ofrece a los **Estudiantes** un portal de autogestión donde pueden realizar su inscripción de materias en línea, consultar su historial académico y visualizar sus notas en tiempo real, optimizando así la comunicación y el flujo de información entre todos los actores educativos.

---

## 🚀 Módulos y Funcionalidades

### 🛡️ Módulo de Administración (Dirección Académica)
* **Gestión de Usuarios:** Creación y administración de perfiles para Admin, Docentes y Estudiantes con seguridad basada en roles.
* **Control Académico:**
    * Gestión completa de **Materias** (Catálogo de cursos y horarios).
    * Configuración del **Periodo Global** (Apertura y cierre de ciclos escolares activos).
* **Reportería y Dashboard:** Visualización de métricas clave como cantidad de alumnos aprobados/reprobados y docentes activos.

### 👨‍🏫 Módulo Docente (Gestión de Clases)
* **Registro de Calificaciones:** Interfaz para ingresar y modificar notas de los estudiantes asignados, validando que correspondan a sus materias.
* **Reporte Docente:** Dashboard personalizado con estadísticas de sus cursos impartidos.
* **Listados de Alumnos:** Visualización de estudiantes matriculados por materia.

### 👨‍🎓 Módulo Estudiante (Portal del Alumno)
* **Matrícula en Línea:** Sistema de inscripción de materias con validación de horarios y cupos disponibles.
* **Consulta de Notas:** Visualización del récord académico y promedio global por materia.
* **Estado de Materias:** Seguimiento del estado de aprobación (Aprobado/Reprobado) según el promedio acumulado.

---

## 🛠️ Tecnologías Utilizadas

* **Lenguaje:** C# (C Sharp)
* **Framework:** ASP.NET Core MVC (Model-View-Controller)
* **ORM:** Entity Framework Core (Code-First / Database-First)
* **Base de Datos:** Microsoft SQL Server
* **Frontend:** Razor Views (.cshtml), Bootstrap 5, JavaScript, jQuery
* **Estilos:** CSS3 personalizado y diseño responsivo.

---

## 👥 Equipo de Desarrollo

Este proyecto fue desarrollado para la cátedra de **Programación II** en la **Universidad Tecnológica de El Salvador** (Ciclo 02-2024).

| Integrante | Carné |
| :--- | :--- |
| **Hernández Arévalo Osaki Vladimir** | 25-2627-2023 |
| **Hernández Molina Diego Enrique** | 25-2862-2023 |
| **López García José Adonay** | 25-1647-2023 |
| **Martínez Salmerón Gerardo David** | 25-2769-2023 |
| **Mejía Martinez Jonathan Alexander** | 25-3190-2023 |

---

## ⚙️ Instalación y Despliegue

1.  **Clonar el repositorio:**
    ```bash
    git clone [https://github.com/usuario/academienroll.git](https://github.com/usuario/academienroll.git)
    ```
2.  **Configurar Base de Datos:**
    * Asegúrate de tener **SQL Server** instalado.
    * Ejecuta el script `AcademiEnroll.sql` ubicado en la carpeta `BD y documentacion` para crear la estructura o restaura el archivo `.bak`.
    * Actualiza la cadena de conexión (`ConnectionString`) en el archivo `appsettings.json` para apuntar a tu instancia local.
3.  **Restaurar Paquetes:**
    ```bash
    dotnet restore
    ```
4.  **Ejecutar la aplicación:**
    ```bash
    dotnet run
    ```
5.  Acceder en el navegador (usualmente `https://localhost:7001` o similar).

---
© 2024 AcademiEnroll - Soluciones Educativas Digitales.
