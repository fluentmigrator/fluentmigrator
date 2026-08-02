-- Processor: MySql5
-- Generator: MySql5Generator

CREATE TABLE `VersionInfo` (`Version` BIGINT NOT NULL) ENGINE = INNODB;
CREATE UNIQUE INDEX `UC_Version` ON `VersionInfo` (`Version` ASC);

ALTER TABLE `VersionInfo` ADD COLUMN `AppliedOn` DATETIME;

ALTER TABLE `VersionInfo` ADD COLUMN `Description` NVARCHAR(1024);
CREATE TABLE `Users` (`Id` INTEGER NOT NULL, `Name` NVARCHAR(100) NOT NULL, CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)) ENGINE = INNODB;
INSERT INTO `VersionInfo` (`Version`, `AppliedOn`, `Description`) VALUES (1, (UTC_TIMESTAMP), 'CreateTableMigration');
