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
	   Id INT PRIMARY KEY Identity(1,1), -- Llave primaria con incremento autom�tico
	   Nombre VARCHAR(255) NOT NULL,       -- Nombre de la materia
	   Codigo VARCHAR(100) NOT NULL,       -- C�digo �nico de la materia
	   IdDocente INT NOT NULL,             -- Llave for�nea para la relaci�n con Usuario

	-- Definici�n de la llave for�nea
	CONSTRAINT FK_Materia_Docente FOREIGN KEY (IdDocente)
		REFERENCES Usuarios(IdUsuario)          -- Relaci�n con la tabla Usuario (asumiendo que su llave primaria es 'Id')
		ON DELETE CASCADE               -- Borrado en cascada si el docente es eliminado
		ON UPDATE CASCADE               -- Actualizaci�n en cascada si el Id del docente cambia
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
    IdMateria INT NULL,              -- ID de la materia (ahora permite NULL)
    Promedio DECIMAL(5,1) NOT NULL,  -- Promedio con un decimal
    FechaAprobacion DATETIME,        -- Fecha de aprobaci�n
    Estado VARCHAR(10),              -- Estado de la materia aprobada

    -- Clave for�nea para IdEstudiante (sin cascada expl�cita aqu�)
    CONSTRAINT FK_MateriasAprobadas_Estudiantes FOREIGN KEY (IdEstudiante) 
        REFERENCES Estudiantes(IdEstudiante), 

    -- Clave for�nea para IdMateria con ON DELETE CASCADE
    CONSTRAINT FK_MateriasAprobadas_Materias FOREIGN KEY (IdMateria) 
        REFERENCES Materias(Id) 
        ON DELETE SET NULL
);
END
GO

-- Crear la tabla Inscripciones si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'Inscripciones' AND type = 'U'
)
BEGIN
	CREATE TABLE Inscripciones (
    CodInscripcion INT IDENTITY(1,1) PRIMARY KEY, -- Identificador num�rico autom�tico
    Codigo AS ('Inscripci�n' + RIGHT('000000000' + CAST(CodInscripcion AS NVARCHAR), 9)) PERSISTED, -- Columna calculada
    IdEstudiante INT NOT NULL,
    IdMateria INT NOT NULL,
    FechaInscripcion DATETIME DEFAULT GETDATE(), -- Columna de fecha y hora con valor por defecto

    -- Llave for�nea para la relaci�n con Estudiantes
    FOREIGN KEY (IdEstudiante) REFERENCES Estudiantes(IdEstudiante),

    -- Llave for�nea para la relaci�n con Materias
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
    CONSTRAINT FK_Notas_Docentes FOREIGN KEY (IdDocente) 
        REFERENCES Docentes(IdDocente) 
        ON DELETE CASCADE
);
END
GO

-- Crear la tabla PeriodoGlobal si no existe
IF NOT EXISTS (
	SELECT 1 
	FROM sys.tables 
	WHERE name = 'PeriodoGlobal' AND type = 'U'
)
BEGIN
	CREATE TABLE PeriodoGlobal (
		Id INT IDENTITY(1,1) PRIMARY KEY,  -- Un identificador �nico
		Periodo INT NOT NULL CHECK (Periodo BETWEEN 1 AND 5)  -- El periodo debe estar entre 1 y 5
	);
END;
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
		RAISERROR('La calificaci�n debe estar entre 0 y 10.', 16, 1);
		RETURN;
	END
	INSERT INTO Notas (NombreEstudiante, NombreAsignatura, Calificacion)
	VALUES (@NombreEstudiante, @NombreAsignatura, @Calificacion);
END;
GO

--SP para Panel de Docentes
CREATE OR ALTER PROCEDURE SPReporteDocente
    @IdDocente INT
AS
BEGIN
    SET NOCOUNT ON;

    -- CTE para identificar las materias impartidas por el docente
    WITH MateriasImpartidas AS (
        SELECT 
            m.Id AS IdMateria,
            m.Nombre AS NombreMateria,
            m.IdDocente
        FROM Materias m
        WHERE m.IdDocente = @IdDocente
    ),
    -- CTE para contar la cantidad total de estudiantes inscritos en todas las materias del docente
    TotalEstudiantes AS (
        SELECT 
            COUNT(DISTINCT i.IdEstudiante) AS Total
        FROM Inscripciones i
        INNER JOIN MateriasImpartidas mi ON i.IdMateria = mi.IdMateria
    ),
    -- CTE para contar la cantidad de estudiantes aprobados en todas las materias del docente
    TotalAprobados AS (
        SELECT 
            COUNT(ma.IdEstudiante) AS Total
        FROM MateriasAprobadas ma
        INNER JOIN MateriasImpartidas mi ON ma.IdMateria = mi.IdMateria
        WHERE ma.Estado = 'Aprobado' OR ma.Promedio >= 6 -- Ajusta el promedio seg�n tus reglas
    ),
    -- CTE para contar la cantidad de estudiantes reprobados en todas las materias del docente
    TotalReprobados AS (
        SELECT 
            COUNT(ma.IdEstudiante) AS Total
        FROM MateriasAprobadas ma
        INNER JOIN MateriasImpartidas mi ON ma.IdMateria = mi.IdMateria
        WHERE ma.Estado = 'Reprobado' OR ma.Promedio < 6 -- Ajusta el promedio seg�n tus reglas
    )
    -- Consulta final
    SELECT 
        d.Nombre AS Docente,
        (SELECT COUNT(*) FROM MateriasImpartidas) AS MateriasImpartidas,
        (SELECT Total FROM TotalEstudiantes) AS TotalEstudiantes,
        (SELECT Total FROM TotalAprobados) AS TotalAprobados,
        (SELECT Total FROM TotalReprobados) AS TotalReprobados
    FROM Docentes d
    WHERE d.IdDocente = @IdDocente;

END;
GO

select * from Inscripciones
GO

-- Crear o actualizar el procedimiento almacenado SP_ActualizarNota
CREATE OR ALTER PROCEDURE SP_ActualizarNota
	@Id INT,
	@Calificacion DECIMAL(5,2)
AS
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;

		-- Validar calificaci�n
		IF @Calificacion < 0 OR @Calificacion > 10
		BEGIN
			RAISERROR('La calificaci�n debe estar entre 0 y 10.', 16, 1);
			ROLLBACK TRANSACTION;
			RETURN;
		END

		-- Verificar existencia del registro
		IF NOT EXISTS (SELECT 1 FROM Notas WHERE Id = @Id)
		BEGIN
			RAISERROR('No se encontr� un registro con el Id proporcionado.', 16, 1);
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

    -- Contar la cantidad de aprobados (consideramos que la calificaci�n >= 6 es aprobada)
    DECLARE @TotalAprobados INT;
    SELECT @TotalAprobados = COUNT(*) 
    FROM Notas 
    WHERE Calificacion >= 6;

    -- Contar la cantidad de reprobados (consideramos que la calificaci�n < 6 es reprobada)
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

Go

-- Insertar registros en la tabla Usuarios
INSERT INTO Usuarios (Nombre, Correo, Clave, Rol) 
VALUES 
('Ana Gonzalez', 'ana@academienroll.com', 'ana123', 'Estudiante'),
('Luis Fernandez', 'luis@academienroll.com', 'luis123', 'Estudiante'),
('Juan Martinez', 'juan@academienroll.com', 'juan123', 'Docente'),
('Laura Diaz', 'laura@academienroll.com', 'laura123', 'Docente'),
('Admin1', 'admin1@academienroll.com', 'admin123', 'Administrador'),
('Admin2', 'admin2@academienroll.com', 'admin456', 'Administrador');

-- Insertar registros en la tabla Estudiantes
INSERT INTO Estudiantes (Nombre, Correo, IdUsuario) 
VALUES 
('Ana Gonzalez', 'ana@academienroll.com', 1),
('Luis Fernandez', 'luis@academienroll.com', 2);

-- Insertar registros en la tabla Docentes
INSERT INTO Docentes (Nombre, Correo, IdUsuario) 
VALUES 
('Juan Martinez', 'juan@academienroll.com', 3),
('Laura Diaz', 'laura@academienroll.com', 4);

-- Insertar registros en la tabla Administrador
INSERT INTO Administrador (Nombre, Correo, IdUsuario) 
VALUES 
('Admin1', 'admin1@academienroll.com', 5),
('Admin2', 'admin2@academienroll.com', 6);

-- Insertar registros en la tabla Materias
INSERT INTO Materias (Nombre, Codigo, IdDocente) 
VALUES 
('Matem�ticas Avanzadas', 'MAT101', 3),
('Programaci�n B�sica', 'PRG102', 4);

-- Insertar registros en la tabla Inscripciones
INSERT INTO Inscripciones (IdMateria, IdEstudiante, FechaInscripcion) 
VALUES 
(1, 1, GETDATE()),
(2, 2, GETDATE());

-- Insertar registros en la tabla MateriasAprobadas
INSERT INTO MateriasAprobadas (IdEstudiante, IdMateria, Promedio, FechaAprobacion, Estado) 
VALUES 
(1, 1, 8.5, GETDATE(), 'Aprobado'),
(2, 2, 6.0, GETDATE(), 'Aprobado');

-- Insertar registros en la tabla Notas
INSERT INTO Notas (IdDocente, NombreEstudiante, NombreAsignatura, Calificacion, Periodo) 
VALUES 
(3, 'Ana Gonzalez', 'Matem�ticas Avanzadas', 8.5, 1),
(4, 'Luis Fernandez', 'Programaci�n B�sica', 6.0, 1);

-- Insertar registros en la tabla PeriodoGlobal
INSERT INTO PeriodoGlobal (Periodo) 
VALUES 
(2)

Go

-- Consultar registros de las tablas
SELECT * FROM Usuarios;
SELECT * FROM Estudiantes;
SELECT * FROM Docentes;
SELECT * FROM Administrador;
SELECT * FROM Notas;
SELECT * FROM Inscripciones;
GO
 