﻿CREATE TABLE `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `Users` (
    `UserId` char(36) NOT NULL,
    `AccountStatus` int NOT NULL,
    `AccountType` int NOT NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` longtext NOT NULL,
    `DateOfBirth` datetime(6),
    `EmailAddress` longtext NOT NULL,
    `FirstName` longtext NOT NULL,
    `Gender` tinyint unsigned NOT NULL,
    `LastName` longtext NOT NULL,
    `LastUpdate` datetime(6) NOT NULL,
    `PhoneNumber` longtext NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`UserId`)
);

CREATE TABLE `Doctors` (
    `DoctorId` int NOT NULL AUTO_INCREMENT,
    `ProfileSummary` longtext,
    `UserAccountUserId` char(36),
    `UserId` longtext,
    CONSTRAINT `PK_Doctors` PRIMARY KEY (`DoctorId`),
    CONSTRAINT `FK_Doctors_Users_UserAccountUserId` FOREIGN KEY (`UserAccountUserId`) REFERENCES `Users` (`UserId`) ON DELETE NO ACTION
);

CREATE TABLE `Patients` (
    `PatientId` int NOT NULL AUTO_INCREMENT,
    `BloodGroup` longtext,
    `Occupation` longtext,
    `PatientType` int NOT NULL,
    `UserAccountUserId` char(36),
    `UserId` int NOT NULL,
    CONSTRAINT `PK_Patients` PRIMARY KEY (`PatientId`),
    CONSTRAINT `FK_Patients_Users_UserAccountUserId` FOREIGN KEY (`UserAccountUserId`) REFERENCES `Users` (`UserId`) ON DELETE NO ACTION
);

CREATE TABLE `Appointments` (
    `PatientId` int NOT NULL,
    `DoctorId` int NOT NULL,
    `AilmentDescription` longtext,
    `Created` datetime(6) NOT NULL,
    `ScheduledDate` datetime(6) NOT NULL,
    `Status` int NOT NULL,
    `Symptoms` longtext,
    CONSTRAINT `PK_Appointments` PRIMARY KEY (`PatientId`, `DoctorId`),
    CONSTRAINT `FK_Appointments_Doctors_DoctorId` FOREIGN KEY (`DoctorId`) REFERENCES `Doctors` (`DoctorId`) ON DELETE CASCADE,
    CONSTRAINT `FK_Appointments_Patients_PatientId` FOREIGN KEY (`PatientId`) REFERENCES `Patients` (`PatientId`) ON DELETE CASCADE
);

CREATE INDEX `IX_Appointments_DoctorId` ON `Appointments` (`DoctorId`);

CREATE INDEX `IX_Doctors_UserAccountUserId` ON `Doctors` (`UserAccountUserId`);

CREATE INDEX `IX_Patients_UserAccountUserId` ON `Patients` (`UserAccountUserId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20180528023359_migv1-DbCreate', '2.0.1-rtm-125');

ALTER TABLE `Doctors` DROP FOREIGN KEY `FK_Doctors_Users_UserAccountUserId`;

ALTER TABLE `Patients` DROP FOREIGN KEY `FK_Patients_Users_UserAccountUserId`;

DROP TABLE `Users`;

ALTER TABLE `Patients` DROP INDEX `IX_Patients_UserAccountUserId`;

ALTER TABLE `Doctors` DROP INDEX `IX_Doctors_UserAccountUserId`;

ALTER TABLE `Patients` DROP COLUMN `UserAccountUserId`;
ALTER TABLE `Doctors` DROP COLUMN `UserAccountUserId`;
ALTER TABLE `Patients` ADD `UserAccountId` varchar(127);

ALTER TABLE `Doctors` ADD `UserAccountId` varchar(127);

CREATE TABLE `AspNetRoles` (
    `Id` varchar(127) NOT NULL,
    `ConcurrencyStamp` longtext,
    `Name` varchar(256),
    `NormalizedName` varchar(256),
    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetUsers` (
    `Id` varchar(127) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    `AccountStatus` int NOT NULL,
    `AccountType` int NOT NULL,
    `ConcurrencyStamp` longtext,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` longtext NOT NULL,
    `DateOfBirth` datetime(6),
    `Email` varchar(256),
    `EmailConfirmed` bit NOT NULL,
    `FirstName` longtext NOT NULL,
    `Gender` tinyint unsigned NOT NULL,
    `LastName` longtext NOT NULL,
    `LastUpdate` datetime(6) NOT NULL,
    `LockoutEnabled` bit NOT NULL,
    `LockoutEnd` datetime(6),
    `NormalizedEmail` varchar(256),
    `NormalizedUserName` varchar(256),
    `PasswordHash` longtext,
    `PhoneNumber` longtext,
    `PhoneNumberConfirmed` bit NOT NULL,
    `SecurityStamp` longtext,
    `TwoFactorEnabled` bit NOT NULL,
    `UserId` char(36) NOT NULL,
    `UserName` varchar(256),
    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`),
    CONSTRAINT `AK_AspNetUsers_UserId` UNIQUE (`UserId`)
);

CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ClaimType` longtext,
    `ClaimValue` longtext,
    `RoleId` varchar(127) NOT NULL,
    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ClaimType` longtext,
    `ClaimValue` longtext,
    `UserId` varchar(127) NOT NULL,
    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` varchar(127) NOT NULL,
    `ProviderKey` varchar(127) NOT NULL,
    `ProviderDisplayName` longtext,
    `UserId` varchar(127) NOT NULL,
    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserRoles` (
    `UserId` varchar(127) NOT NULL,
    `RoleId` varchar(127) NOT NULL,
    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserTokens` (
    `UserId` varchar(127) NOT NULL,
    `LoginProvider` varchar(127) NOT NULL,
    `Name` varchar(127) NOT NULL,
    `Value` longtext,
    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_Patients_UserAccountId` ON `Patients` (`UserAccountId`);

CREATE INDEX `IX_Doctors_UserAccountId` ON `Doctors` (`UserAccountId`);

CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

ALTER TABLE `Doctors` ADD CONSTRAINT `FK_Doctors_AspNetUsers_UserAccountId` FOREIGN KEY (`UserAccountId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE NO ACTION;

ALTER TABLE `Patients` ADD CONSTRAINT `FK_Patients_AspNetUsers_UserAccountId` FOREIGN KEY (`UserAccountId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE NO ACTION;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20180602085932_migv2-Added-Identity', '2.0.1-rtm-125');

ALTER TABLE `Patients` DROP COLUMN `UserAccountId`;
ALTER TABLE `Doctors` DROP COLUMN `UserAccountId`;
ALTER TABLE `AspNetUsers` DROP COLUMN `UserId`;
ALTER TABLE `Patients` MODIFY COLUMN `UserId` varchar(127) NOT NULL;
ALTER TABLE `Patients` ALTER COLUMN `UserId` DROP DEFAULT;
ALTER TABLE `Doctors` MODIFY COLUMN `UserId` varchar(127) NOT NULL;
ALTER TABLE `Doctors` ALTER COLUMN `UserId` DROP DEFAULT;
CREATE INDEX `IX_Patients_UserId` ON `Patients` (`UserId`);

CREATE INDEX `IX_Doctors_UserId` ON `Doctors` (`UserId`);

ALTER TABLE `Doctors` ADD CONSTRAINT `FK_Doctors_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

ALTER TABLE `Patients` ADD CONSTRAINT `FK_Patients_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20180616125327_UpdatedNavPrperties', '2.0.1-rtm-125');
