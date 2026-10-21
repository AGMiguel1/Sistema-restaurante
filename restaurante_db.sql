-- ============================================================================
-- SCRIPT DE BASE DE DATOS: restaurante_db
-- Sistema de gestión de reservas, mesas y pedidos de restaurante
-- Generado directamente desde la migración de Entity Framework Core (Pomelo MySQL)
-- Listo para importar y ejecutar en MySQL Workbench
-- ============================================================================

-- 1. Creación y selección de la base de datos
CREATE DATABASE IF NOT EXISTS `restaurante_db`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `restaurante_db`;

-- 2. Tabla de historial de migraciones de Entity Framework Core
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4 ENGINE=InnoDB;

START TRANSACTION;

-- Ajuste de juego de caracteres para la base de datos
ALTER DATABASE `restaurante_db` CHARACTER SET utf8mb4;

-- 3. Tabla Clientes
CREATE TABLE IF NOT EXISTS `Clientes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `Telefono` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
    `Correo` varchar(150) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Clientes` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4 ENGINE=InnoDB;

-- 4. Tabla Mesas
CREATE TABLE IF NOT EXISTS `Mesas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Numero` int NOT NULL,
    `Capacidad` int NOT NULL,
    `Activa` tinyint(1) NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_Mesas` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4 ENGINE=InnoDB;

-- 5. Tabla Reservas
CREATE TABLE IF NOT EXISTS `Reservas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `ClienteId` int NOT NULL,
    `MesaId` int NOT NULL,
    `FechaHoraInicio` datetime(6) NOT NULL,
    `FechaHoraFin` datetime(6) NOT NULL,
    `CantidadPersonas` int NOT NULL,
    `Estado` int NOT NULL,
    `FechaRegistro` datetime(6) NOT NULL,
    CONSTRAINT `PK_Reservas` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Reservas_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservas_Mesas_MesaId` FOREIGN KEY (`MesaId`) REFERENCES `Mesas` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 ENGINE=InnoDB;

-- 6. Inserción de Mesas Iniciales (Seed Data)
-- Se insertan de forma idempotente para evitar duplicados si ya existen
INSERT IGNORE INTO `Mesas` (`Id`, `Activa`, `Capacidad`, `Numero`) VALUES
(1, TRUE, 2, 1),
(2, TRUE, 2, 2),
(3, TRUE, 4, 3),
(4, TRUE, 4, 4),
(5, TRUE, 6, 5),
(6, TRUE, 8, 6),
(7, FALSE, 4, 7);

-- 7. Creación de Índices
-- Índice único en Número de Mesa
CREATE UNIQUE INDEX IF NOT EXISTS `IX_Mesas_Numero` ON `Mesas` (`Numero`);

-- Índice de relación por Cliente
CREATE INDEX IF NOT EXISTS `IX_Reservas_ClienteId` ON `Reservas` (`ClienteId`);

-- Índice único en Código de Reserva
CREATE UNIQUE INDEX IF NOT EXISTS `IX_Reservas_Codigo` ON `Reservas` (`Codigo`);

-- Índice compuesto para acelerar la comprobación de cruces de intervalos
CREATE INDEX IF NOT EXISTS `IX_Reservas_MesaId_Estado_FechaHoraInicio_FechaHoraFin`
    ON `Reservas` (`MesaId`, `Estado`, `FechaHoraInicio`, `FechaHoraFin`);

-- 8. Registro de la migración inicial en el historial de Entity Framework Core
-- Esto asegura que EF Core detecte la migración como ya aplicada y no intente recrear las tablas
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261021043822_MigracionInicialMySql', '9.0.20');

COMMIT;
