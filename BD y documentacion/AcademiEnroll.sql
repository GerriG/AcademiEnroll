-- Crear la base de datos si no existe
IF DB_ID('AcademiEnroll') IS NULL
	CREATE DATABASE AcademiEnroll;
GO

-- Usar la base de datos
USE AcademiEnroll;
GO

-- Crear la tabla Usuarios si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'Usuarios' AND type = 'U'
)
BEGIN
	CREATE TABLE Usuarios (
		IdUsuario INT PRIMARY KEY IDENTITY,
		Nombre NVARCHAR(100) NOT NULL,    
		Correo NVARCHAR(100) NOT NULL UNIQUE,
		Clave NVARCHAR(50) NOT NULL,
		Rol NVARCHAR(20) NOT NULL CHECK (Rol IN ('Estudiante', 'Docente', 'Administrador'))
	);

	-- Insertar un administrador por defecto
	INSERT INTO Usuarios (Correo, Nombre, Clave, Rol) 
	VALUES ('admin@academienroll.com', 'Juan', 'admin123', 'Administrador');
END;
GO

-- Crear la tabla Estudiantes si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'Estudiantes' AND type = 'U'
)
BEGIN
	-- Tabla Estudiantes con ON DELETE CASCADE
	CREATE TABLE Estudiantes (
		IdEstudiante INT IDENTITY(1,1) PRIMARY KEY,
		Nombre NVARCHAR(100) NOT NULL,    
		Correo NVARCHAR(100) NOT NULL UNIQUE,
		IdUsuario INT NOT NULL,
		FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario) ON DELETE CASCADE
	);
END;
GO
select * from materias
-- Crear la tabla Docentes si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'Docentes' AND type = 'U'
)
BEGIN
	-- Tabla Docentes con ON DELETE CASCADE
	CREATE TABLE Docentes (
		IdDocente INT IDENTITY(1,1) PRIMARY KEY,
		Nombre NVARCHAR(100) NOT NULL,
		Correo NVARCHAR(100) NOT NULL UNIQUE,
		IdUsuario INT NOT NULL,
		FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario) ON DELETE CASCADE
	);
END;
GO

-- Crear la tabla Administrador si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'Administrador' AND type = 'U'
)
BEGIN
	-- Tabla Administrador con ON DELETE CASCADE
	CREATE TABLE Administrador (
		IdAdministrador INT IDENTITY(1,1) PRIMARY KEY,
		Nombre NVARCHAR(100) NOT NULL,
		Correo NVARCHAR(100) NOT NULL UNIQUE,
		IdUsuario INT NOT NULL,
		FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario) ON DELETE CASCADE
	);
END;
GO

--Crear tabla de materias si no existe en la base de datos
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'Materias' AND type = 'U'
)
BEGIN
	CREATE TABLE Materias (
	   Id INT PRIMARY KEY Identity(1,1), -- Llave primaria con incremento automático
	   Nombre VARCHAR(255) NOT NULL,       -- Nombre de la materia
	   Codigo VARCHAR(100) NOT NULL,       -- Código único de la materia
	   IdDocente INT NOT NULL,             -- Llave foránea para la relación con Usuario

	-- Definición de la llave foránea
	CONSTRAINT FK_Materia_Docente FOREIGN KEY (IdDocente)
		REFERENCES Usuarios(IdUsuario)          -- Relación con la tabla Usuario (asumiendo que su llave primaria es 'Id')
		ON DELETE CASCADE               -- Borrado en cascada si el docente es eliminado
		ON UPDATE CASCADE               -- Actualización en cascada si el Id del docente cambia
	);
END;
GO

--Crear tabla de MateriasAprobadas si no existe en la base de datos
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'MateriasAprobadas' AND type = 'U'
)
BEGIN
CREATE TABLE MateriasAprobadas (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Llave primaria con autoincremento
    IdEstudiante INT NOT NULL,       -- ID del estudiante
    IdMateria INT NOT NULL,          -- ID de la materia
    Promedio DECIMAL(5,1) NOT NULL,  -- Promedio con una decimal
    FechaAprobacion DATETIME NOT NULL, -- Fecha de aprobación

    CONSTRAINT FK_MateriasAprobadas_Estudiantes FOREIGN KEY (IdEstudiante) 
        REFERENCES Estudiantes(IdEstudiante), -- Asegúrate de que la tabla Estudiantes exista
    CONSTRAINT FK_MateriasAprobadas_Materias FOREIGN KEY (IdMateria) 
        REFERENCES Materias(Id)     -- Asegúrate de que la tabla Materias exista
);
END;
GO

select * from MateriasAprobadas

update PeriodoGlobal set Periodo = 5

-- Crear la tabla Inscripciones si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'Inscripciones' AND type = 'U'
)
BEGIN
	CREATE TABLE Inscripciones (
    CodInscripcion INT IDENTITY(1,1) PRIMARY KEY, -- Identificador numérico automático
    Codigo AS ('Inscripción' + RIGHT('000000000' + CAST(CodInscripcion AS NVARCHAR), 9)) PERSISTED, -- Columna calculada
    IdEstudiante INT NOT NULL,
    IdMateria INT NOT NULL,
    FechaInscripcion DATETIME DEFAULT GETDATE(), -- Columna de fecha y hora con valor por defecto

    -- Llave foránea para la relación con Estudiantes
    FOREIGN KEY (IdEstudiante) REFERENCES Estudiantes(IdEstudiante),

    -- Llave foránea para la relación con Materias
    CONSTRAINT FK_Inscripciones_Materias FOREIGN KEY (IdMateria)
        REFERENCES Materias(Id) 
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

END;

GO
-- Crear la tabla Notas si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'Notas' AND type = 'U'
)
BEGIN
	CREATE TABLE Notas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdDocente INT NOT NULL,
    NombreEstudiante NVARCHAR(100) NOT NULL,
    NombreAsignatura NVARCHAR(100) NOT NULL,
    Calificacion DECIMAL(5,2) NOT NULL,
    Periodo INT NOT NULL,  -- Columna para almacenar el periodo
    CONSTRAINT FK_Notas_Docentes FOREIGN KEY (IdDocente) REFERENCES Docentes(IdDocente)
);
END;
GO
select * from Materias
-- Crear la tabla PeriodoGlobal si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'PeriodoGlobal' AND type = 'U'
)
BEGIN
	CREATE TABLE PeriodoGlobal (
		Id INT IDENTITY(1,1) PRIMARY KEY,  -- Un identificador único
		Periodo INT NOT NULL CHECK (Periodo BETWEEN 1 AND 5)  -- El periodo debe estar entre 1 y 5
	);
END;
update PeriodoGlobal set Periodo = 2 WHERE Id = 1
GO

-- Crear o actualizar el procedimiento almacenado sp_AgregarNota
CREATE OR ALTER PROCEDURE sp_AgregarNota
	@NombreEstudiante NVARCHAR(100),
	@NombreAsignatura NVARCHAR(100),
	@Calificacion DECIMAL(5,2)
AS
BEGIN
	IF @Calificacion < 0 OR @Calificacion > 10
	BEGIN
		RAISERROR('La calificación debe estar entre 0 y 10.', 16, 1);
		RETURN;
	END
	INSERT INTO Notas (NombreEstudiante, NombreAsignatura, Calificacion)
	VALUES (@NombreEstudiante, @NombreAsignatura, @Calificacion);
END;
GO

-- Crear o actualizar el procedimiento almacenado SP_ActualizarNota
CREATE OR ALTER PROCEDURE SP_ActualizarNota
	@Id INT,
	@Calificacion DECIMAL(5,2)
AS
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;

		-- Validar calificación
		IF @Calificacion < 0 OR @Calificacion > 10
		BEGIN
			RAISERROR('La calificación debe estar entre 0 y 10.', 16, 1);
			ROLLBACK TRANSACTION;
			RETURN;
		END

		-- Verificar existencia del registro
		IF NOT EXISTS (SELECT 1 FROM Notas WHERE Id = @Id)
		BEGIN
			RAISERROR('No se encontró un registro con el Id proporcionado.', 16, 1);
			ROLLBACK TRANSACTION;
			RETURN;
		END

		-- Actualizar la nota
		UPDATE Notas
		SET Calificacion = @Calificacion
		WHERE Id = @Id;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO

--Store procedure
CREATE OR ALTER PROCEDURE sp_ReporteDashboard
AS
BEGIN
    -- Contar la cantidad de docentes
    DECLARE @TotalDocentes INT;
    SELECT @TotalDocentes = COUNT(*) FROM Usuarios WHERE Rol = 'Docente';

    -- Contar la cantidad de estudiantes
    DECLARE @TotalEstudiantes INT;
    SELECT @TotalEstudiantes = COUNT(*) FROM Usuarios WHERE Rol = 'Estudiante';

    -- Contar la cantidad de aprobados (consideramos que la calificación >= 6 es aprobada)
    DECLARE @TotalAprobados INT;
    SELECT @TotalAprobados = COUNT(*) 
    FROM Notas 
    WHERE Calificacion >= 6;

    -- Contar la cantidad de reprobados (consideramos que la calificación < 6 es reprobada)
    DECLARE @TotalReprobados INT;
    SELECT @TotalReprobados = COUNT(*) 
    FROM Notas 
    WHERE Calificacion < 6;

    -- Contar la cantidad de materias impartidas (debe haber un docente asignado a la materia)
    DECLARE @TotalMaterias INT;
    SELECT @TotalMaterias = COUNT(*) 
    FROM Materias;

    -- Contar la cantidad total de usuarios (todos los roles: Docentes, Estudiantes, Administradores)
    DECLARE @TotalUsuarios INT;
    SELECT @TotalUsuarios = COUNT(*) FROM Usuarios;

    -- Devolver los resultados
    SELECT 
        @TotalDocentes AS TotalDocentes,
        @TotalEstudiantes AS TotalEstudiantes,
        @TotalAprobados AS TotalAprobados,
        @TotalReprobados AS TotalReprobados,
        @TotalMaterias AS TotalMaterias,
        @TotalUsuarios AS TotalUsuarios;
END;
GO

exec sp_ReporteDashboard

-- Consultar registros de las tablas
SELECT * FROM Usuarios;
SELECT * FROM Estudiantes;
SELECT * FROM Docentes;
SELECT * FROM Administrador;
SELECT * FROM Notas;
SELECT * FROM Inscripciones;
GO
 