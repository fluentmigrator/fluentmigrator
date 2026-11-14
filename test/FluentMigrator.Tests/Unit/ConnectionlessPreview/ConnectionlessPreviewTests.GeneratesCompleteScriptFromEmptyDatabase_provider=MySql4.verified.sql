-- Processor: MySql4
-- Generator: MySql4Generator

CREATE TABLE `VersionInfo` (`Version` BIGINT NOT NULL) ENGINE = INNODB;
CREATE UNIQUE INDEX `UC_Version` ON `VersionInfo` (`Version` ASC);

ALTER TABLE `VersionInfo` ADD COLUMN `AppliedOn` DATETIME;

ALTER TABLE `VersionInfo` ADD COLUMN `Description` VARCHAR(1024);
CREATE TABLE `Users` (`Id` INTEGER NOT NULL, `Name` VARCHAR(100) NOT NULL, CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)) ENGINE = INNODB;
INSERT INTO `VersionInfo` (`Version`, `AppliedOn`, `Description`) VALUES (1, (UTC_TIMESTAMP), 'CreateTableMigration');
