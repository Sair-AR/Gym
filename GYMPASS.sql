-- =======================================================================
-- SCRIPT DE CREACIÓN E INSERCIÓN DE BASE DE DATOS - PROYECTO GIMPASS (MEMBRESÍAS ELIMINADAS)
-- =======================================================================

-- 1. CREACIÓN DE LA BASE DE DATOS
CREATE DATABASE GYMPASS
GO
USE GYMPASS
GO

-- 2. CREACIÓN DE TABLAS
----------------------------------------------------------------------
CREATE TABLE Maquinas (
    IdMaquina INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(300)
);

CREATE TABLE Ejercicios (
    IdEjercicio INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Zona NVARCHAR(50) NOT NULL,
    Nivel NVARCHAR(20),
    RequiereMaquina bit NOT NULL,
    IdMaquina INT NULL,
    FOREIGN KEY (IdMaquina) REFERENCES Maquinas (IdMaquina)
);

CREATE TABLE Usuarios (
    IdUsuario INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    Edad INT,
    Peso FLOAT,
    Altura FLOAT,
    Nivel NVARCHAR(20),
    Objetivo NVARCHAR(50),
    ZonaObjetivo NVARCHAR(50),
    Lesiones NVARCHAR(200),
    AvatarUrl NVARCHAR(300),
    FechaRegistro DATETIME DEFAULT GETDATE(),
    TipoUsuario Nvarchar(50) -- Nuevo usuario con 'Admin'
);

CREATE TABLE HistorialProgreso (
    IdProgreso INT PRIMARY KEY IDENTITY(1,1),
    IdUsuario INT NOT NULL,
    Fecha DATE DEFAULT GETDATE(),
    Peso FLOAT,
    GrasaCorporal FLOAT,
    MedidaPecho FLOAT,
    MedidaCintura FLOAT,
    MedidaPierna FLOAT,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);

CREATE TABLE IA_Consultas (
    IdConsulta INT PRIMARY KEY IDENTITY(1,1),
    IdUsuario INT NOT NULL,
    Pregunta NVARCHAR(MAX) NOT NULL,
    Respuesta NVARCHAR(MAX) NOT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);

CREATE TABLE Rutinas (
    IdRutina INT PRIMARY KEY IDENTITY(1,1),
    IdUsuario INT NOT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    RutinaBase NVARCHAR(MAX),
    RutinaIA NVARCHAR(MAX),
    Notas NVARCHAR(MAX),
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);

CREATE TABLE RutinaDetalle (
    IdRutinaDetalle INT PRIMARY KEY IDENTITY(1,1),
    IdRutina INT NOT NULL,
    IdEjercicio INT NOT NULL,
    Series INT NOT NULL,
    Repeticiones INT NOT NULL,
    ParametrosIA NVARCHAR(200),
    FOREIGN KEY (IdRutina) REFERENCES Rutinas (IdRutina),
    FOREIGN KEY (IdEjercicio) REFERENCES Ejercicios (IdEjercicio)
);
GO

-- =======================================================================
-- 3. INSERCIÓN DE DATOS INICIALES (SEEDS)
-- =======================================================================

-- A. INSERCIÓN EN TABLA MAQUINAS
SET IDENTITY_INSERT Maquinas ON;
INSERT INTO Maquinas (IdMaquina, Nombre, Descripcion) VALUES
(1, N'Prensa de Piernas', N'Máquina ideal para trabajar cuádriceps, glúteos y femorales.'),
(2, N'Polea Alta (Lat Pulldown)', N'Máquina para ejercicios de jalón vertical.'),
(3, N'Máquina de Press de Pecho', N'Máquina guiada para press de pectoral.'),
(4, N'Banco de Pesas (Libre)', N'Banco móvil para realizar ejercicios con mancuernas.'),
(5, N'Máquina de Extensión de Cuádriceps', N'Máquina de aislamiento para el cuádriceps.');
SET IDENTITY_INSERT Maquinas OFF;

-- B. INSERCIÓN EN TABLA EJERCICIOS (Depende de Maquinas)
SET IDENTITY_INSERT Ejercicios ON;
INSERT INTO Ejercicios (IdEjercicio, Nombre, Zona, Nivel, RequiereMaquina, IdMaquina) VALUES
(101, N'Sentadilla con Barra', N'Pierna', N'Intermedio', 0, NULL),
(102, N'Prensa de Piernas', N'Pierna', N'Principiante', 1, 1),
(202, N'Press de Banca con Mancuernas', N'Pecho', N'Intermedio', 1, 4), 
(302, N'Jalón al Pecho (Lat Pulldown)', N'Espalda', N'Principiante', 1, 2),
(401, N'Curl de Bíceps con Mancuernas', N'Brazos', N'Principiante', 0, NULL),
(502, N'Elevaciones Laterales con Mancuernas', N'Hombros', N'Principiante', 0, NULL);
SET IDENTITY_INSERT Ejercicios OFF;


-- C. INSERCIÓN EN TABLA USUARIOS (Incluye el nuevo Administrador)
SET IDENTITY_INSERT Usuarios ON;
INSERT INTO Usuarios (IdUsuario, Nombre, Email, PasswordHash, Edad, Peso, Altura, Nivel, Objetivo, ZonaObjetivo, Lesiones, FechaRegistro, TipoUsuario) 
VALUES 
(1, N'Juan Pérez', N'juan.perez@gympass.com', N'$2a$10$XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX', 30, 75.5, 1.78, N'Intermedio', N'Ganar Músculo', N'Cuerpo Completo', N'Rodilla derecha sensible', GETDATE(), N'Cliente'),
(2, N'Maria Gómez', N'maria.gomez@gympass.com', N'$2a$10$YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY', 25, 60.2, 1.65, N'Principiante', N'Pérdida de Peso', N'Abdomen', N'Ninguna', GETDATE(), N'Cliente'),
(3, N'Carlos Admin', N'carlos.admin@gympass.com', N'$2a$10$ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ', 45, 85.0, 1.80, N'Avanzado', N'Mantenimiento', N'Ninguna', N'Ninguna', GETDATE(), N'Admin'); -- 👈 Nuevo Administrador
SET IDENTITY_INSERT Usuarios OFF;

-- D. INSERCIÓN EN TABLA HISTORIALPROGRESO (Depende de Usuarios)
SET IDENTITY_INSERT HistorialProgreso ON;
INSERT INTO HistorialProgreso (IdProgreso, IdUsuario, Fecha, Peso, GrasaCorporal, MedidaPecho, MedidaCintura, MedidaPierna) VALUES
(1, 1, '2025-11-01', 76.0, 18.5, 100.0, 85.0, 58.0),
(2, 1, GETDATE(), 75.5, 18.0, 101.0, 84.5, 58.5);
SET IDENTITY_INSERT HistorialProgreso OFF;

-- E. INSERCIÓN EN TABLA RUTINAS (Depende de Usuarios)
SET IDENTITY_INSERT Rutinas ON;
INSERT INTO Rutinas (IdRutina, IdUsuario, Fecha, RutinaBase, RutinaIA, Notas) VALUES
(1, 1, GETDATE(), N'Rutina de Fuerza estándar', N'Rutina optimizada para la rodilla sensible, enfocada en la progresión de sentadilla con bajo peso.', N'Generada por la IA el 20/11/2025');
SET IDENTITY_INSERT Rutinas OFF;

-- F. INSERCIÓN EN TABLA RUTINADETALLE (Depende de Rutinas y Ejercicios)
SET IDENTITY_INSERT RutinaDetalle ON;
INSERT INTO RutinaDetalle (IdRutinaDetalle, IdRutina, IdEjercicio, Series, Repeticiones, ParametrosIA) VALUES
(1, 1, 101, 4, 8, N'Peso sugerido: 60kg, Descanso: 90s'),
(2, 1, 202, 3, 10, N'Peso sugerido: 20kg por mancuerna'),
(3, 1, 302, 3, 12, N'Descanso: 60s, Agarre: Neutro');
SET IDENTITY_INSERT RutinaDetalle OFF;

-- G. INSERCIÓN EN TABLA IA_CONSULTAS (Depende de Usuarios)
SET IDENTITY_INSERT IA_Consultas ON;
INSERT INTO IA_Consultas (IdConsulta, IdUsuario, Pregunta, Respuesta, Fecha) VALUES
(1, 1, N'¿Qué ejercicios puedo hacer con la rodilla sensible?', N'La IA sugirió opciones de bajo impacto como la prensa de piernas (Id 102).', GETDATE());
SET IDENTITY_INSERT IA_CONSULTAS OFF;