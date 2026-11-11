CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Categories` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Icon` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `IsDefault` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Categories` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `ExchangeRateCaches` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `BaseCurrency` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `TargetCurrency` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `Rate` decimal(18,4) NOT NULL,
    `LastUpdatedAt` datetime(6) NOT NULL,
    `Source` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ExchangeRateCaches` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Roles` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
    `RoleId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Users_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Accounts` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `CurrencyCode` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `CurrentBalance` decimal(18,4) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Accounts` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Accounts_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Debts` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Creditor` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `TotalAmount` decimal(18,4) NOT NULL,
    `RemainingAmount` decimal(18,4) NOT NULL,
    `InterestRate` decimal(18,4) NOT NULL,
    `RateType` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `DueDate` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Debts` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Debts_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `SavingsGoals` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `TargetAmount` decimal(18,4) NOT NULL,
    `CurrentAmount` decimal(18,4) NOT NULL,
    `Deadline` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_SavingsGoals` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_SavingsGoals_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Subscriptions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Amount` decimal(18,4) NOT NULL,
    `CurrencyCode` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `BillingCycle` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `NextPaymentDate` datetime(6) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Subscriptions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Subscriptions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Transactions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `AccountId` int NOT NULL,
    `CategoryId` int NULL,
    `Amount` decimal(18,4) NOT NULL,
    `Type` int NOT NULL,
    `Date` datetime(6) NOT NULL,
    `Description` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Transactions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Transactions_Accounts_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Transactions_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

INSERT INTO `Categories` (`Id`, `Icon`, `IsDefault`, `Name`)
VALUES (1, 'restaurant', TRUE, 'Alimentación'),
(2, 'directions_bus', TRUE, 'Transporte'),
(3, 'home', TRUE, 'Vivienda'),
(4, 'local_hospital', TRUE, 'Salud'),
(5, 'movie', TRUE, 'Entretenimiento'),
(6, 'school', TRUE, 'Educación'),
(7, 'bolt', TRUE, 'Servicios Básicos'),
(8, 'subscriptions', TRUE, 'Suscripciones'),
(9, 'payments', TRUE, 'Salario'),
(10, 'trending_up', TRUE, 'Inversiones'),
(11, 'more_horiz', TRUE, 'Otros');

INSERT INTO `ExchangeRateCaches` (`Id`, `BaseCurrency`, `LastUpdatedAt`, `Rate`, `Source`, `TargetCurrency`)
VALUES (1, 'USD', TIMESTAMP '2026-11-10 00:00:00', 4150.0, 'YahooFinance', 'COP');

INSERT INTO `Roles` (`Id`, `Name`)
VALUES (1, 'Usuario'),
(2, 'Administrador'),
(3, 'Soporte'),
(4, 'Auditor');

CREATE INDEX `IX_Accounts_UserId` ON `Accounts` (`UserId`);

CREATE INDEX `IX_Debts_UserId` ON `Debts` (`UserId`);

CREATE UNIQUE INDEX `IX_Roles_Name` ON `Roles` (`Name`);

CREATE INDEX `IX_SavingsGoals_UserId` ON `SavingsGoals` (`UserId`);

CREATE INDEX `IX_Subscriptions_UserId` ON `Subscriptions` (`UserId`);

CREATE INDEX `IX_Transactions_AccountId` ON `Transactions` (`AccountId`);

CREATE INDEX `IX_Transactions_CategoryId` ON `Transactions` (`CategoryId`);

CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

CREATE INDEX `IX_Users_RoleId` ON `Users` (`RoleId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261111130903_InitialCreate', '8.0.8');

COMMIT;

