CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Boards" (
    "Id" uuid NOT NULL,
    "Grid" text NOT NULL,
    "LatestUpdateAt" timestamp with time zone NOT NULL,
    "Generation" integer NOT NULL,
    "IsRunning" boolean NOT NULL,
    CONSTRAINT "PK_Boards" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Boards_Id_Generation" ON "Boards" ("Id", "Generation");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251011032747_InitialMigration', '7.0.20');

COMMIT;

