CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251011032747_InitialMigration') THEN
    CREATE TABLE "Boards" (
        "Id" uuid NOT NULL,
        "Grid" text NOT NULL,
        "LatestUpdateAt" timestamp with time zone NOT NULL,
        "Generation" integer NOT NULL,
        "IsRunning" boolean NOT NULL,
        CONSTRAINT "PK_Boards" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251011032747_InitialMigration') THEN
    CREATE UNIQUE INDEX "IX_Boards_Id_Generation" ON "Boards" ("Id", "Generation");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251011032747_InitialMigration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251011032747_InitialMigration', '7.0.20');
    END IF;
END $EF$;
COMMIT;

