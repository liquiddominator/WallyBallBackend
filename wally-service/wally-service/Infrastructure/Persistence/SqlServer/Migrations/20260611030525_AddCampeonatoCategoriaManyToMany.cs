using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WallyBallBackend.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddCampeonatoCategoriaManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropProgrammability(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Campeonatos_IdCampeonato",
                table: "Categorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_Categorias_IdCategoria",
                table: "Equipos");

            migrationBuilder.DropForeignKey(
                name: "FK_Fases_Categorias_IdCategoria",
                table: "Fases");

            migrationBuilder.DropForeignKey(
                name: "FK_Partidos_Categorias_IdCategoria",
                table: "Partidos");

            migrationBuilder.DropForeignKey(
                name: "FK_TablaPosiciones_Categorias_IdCategoria",
                table: "TablaPosiciones");

            migrationBuilder.DropIndex(
                name: "UQ_Categorias_Campeonato_Nombre",
                table: "Categorias");

            migrationBuilder.CreateTable(
                name: "CampeonatosCategorias",
                columns: table => new
                {
                    IdCampeonatoCategoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCampeonato = table.Column<int>(type: "int", nullable: false),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVA"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampeonatosCategorias", x => x.IdCampeonatoCategoria);
                    table.CheckConstraint("CK_CampeonatosCategorias_Estado", "Estado IN ('ACTIVA', 'INACTIVA')");
                    table.ForeignKey(
                        name: "FK_CampeonatosCategorias_Campeonatos_IdCampeonato",
                        column: x => x.IdCampeonato,
                        principalTable: "Campeonatos",
                        principalColumn: "IdCampeonato");
                    table.ForeignKey(
                        name: "FK_CampeonatosCategorias_Categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria");
                });

            migrationBuilder.Sql("""
                INSERT INTO CampeonatosCategorias (IdCampeonato, IdCategoria, Estado, FechaCreacion, FechaActualizacion)
                SELECT
                    categoriaAnterior.IdCampeonato,
                    categoriaCanonica.IdCategoria,
                    categoriaAnterior.Estado,
                    categoriaAnterior.FechaCreacion,
                    categoriaAnterior.FechaActualizacion
                FROM Categorias categoriaAnterior
                INNER JOIN (
                    SELECT Nombre, MIN(IdCategoria) AS IdCategoria
                    FROM Categorias
                    GROUP BY Nombre
                ) categoriaCanonica ON categoriaCanonica.Nombre = categoriaAnterior.Nombre;
                """);

            migrationBuilder.RenameColumn(
                name: "IdCategoria",
                table: "TablaPosiciones",
                newName: "IdCampeonatoCategoria");

            migrationBuilder.RenameIndex(
                name: "UQ_TablaPosiciones_Categoria_Equipo",
                table: "TablaPosiciones",
                newName: "UQ_TablaPosiciones_CampeonatoCategoria_Equipo");

            migrationBuilder.RenameColumn(
                name: "IdCategoria",
                table: "Partidos",
                newName: "IdCampeonatoCategoria");

            migrationBuilder.RenameIndex(
                name: "IX_Partidos_IdCategoria",
                table: "Partidos",
                newName: "IX_Partidos_IdCampeonatoCategoria");

            migrationBuilder.RenameColumn(
                name: "IdCategoria",
                table: "Fases",
                newName: "IdCampeonatoCategoria");

            migrationBuilder.RenameIndex(
                name: "UQ_Fases_Categoria_Orden",
                table: "Fases",
                newName: "UQ_Fases_CampeonatoCategoria_Orden");

            migrationBuilder.RenameColumn(
                name: "IdCategoria",
                table: "Equipos",
                newName: "IdCampeonatoCategoria");

            migrationBuilder.RenameIndex(
                name: "UQ_Equipos_Categoria_Nombre",
                table: "Equipos",
                newName: "UQ_Equipos_CampeonatoCategoria_Nombre");

            RemapOperationalRows(migrationBuilder);

            migrationBuilder.Sql("""
                DELETE categoriaDuplicada
                FROM Categorias categoriaDuplicada
                INNER JOIN (
                    SELECT Nombre, MIN(IdCategoria) AS IdCategoria
                    FROM Categorias
                    GROUP BY Nombre
                ) categoriaCanonica ON categoriaCanonica.Nombre = categoriaDuplicada.Nombre
                WHERE categoriaDuplicada.IdCategoria <> categoriaCanonica.IdCategoria;
                """);

            migrationBuilder.DropColumn(
                name: "IdCampeonato",
                table: "Categorias");

            migrationBuilder.CreateIndex(
                name: "UQ_Categorias_Nombre",
                table: "Categorias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampeonatosCategorias_IdCategoria",
                table: "CampeonatosCategorias",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "UQ_CampeonatosCategorias_Campeonato_Categoria",
                table: "CampeonatosCategorias",
                columns: new[] { "IdCampeonato", "IdCategoria" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_CampeonatosCategorias_IdCampeonatoCategoria",
                table: "Equipos",
                column: "IdCampeonatoCategoria",
                principalTable: "CampeonatosCategorias",
                principalColumn: "IdCampeonatoCategoria");

            migrationBuilder.AddForeignKey(
                name: "FK_Fases_CampeonatosCategorias_IdCampeonatoCategoria",
                table: "Fases",
                column: "IdCampeonatoCategoria",
                principalTable: "CampeonatosCategorias",
                principalColumn: "IdCampeonatoCategoria");

            migrationBuilder.AddForeignKey(
                name: "FK_Partidos_CampeonatosCategorias_IdCampeonatoCategoria",
                table: "Partidos",
                column: "IdCampeonatoCategoria",
                principalTable: "CampeonatosCategorias",
                principalColumn: "IdCampeonatoCategoria",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TablaPosiciones_CampeonatosCategorias_IdCampeonatoCategoria",
                table: "TablaPosiciones",
                column: "IdCampeonatoCategoria",
                principalTable: "CampeonatosCategorias",
                principalColumn: "IdCampeonatoCategoria");

            CreateProgrammability(migrationBuilder);
        }

        private static void RemapOperationalRows(MigrationBuilder migrationBuilder)
        {
            var tables = new[] { "Equipos", "Fases", "Partidos", "TablaPosiciones" };

            foreach (var table in tables)
            {
                migrationBuilder.Sql($$"""
                    UPDATE item
                    SET IdCampeonatoCategoria = campeonatoCategoria.IdCampeonatoCategoria
                    FROM {{table}} item
                    INNER JOIN Categorias categoriaAnterior ON item.IdCampeonatoCategoria = categoriaAnterior.IdCategoria
                    INNER JOIN (
                        SELECT Nombre, MIN(IdCategoria) AS IdCategoria
                        FROM Categorias
                        GROUP BY Nombre
                    ) categoriaCanonica ON categoriaCanonica.Nombre = categoriaAnterior.Nombre
                    INNER JOIN CampeonatosCategorias campeonatoCategoria
                        ON campeonatoCategoria.IdCampeonato = categoriaAnterior.IdCampeonato
                       AND campeonatoCategoria.IdCategoria = categoriaCanonica.IdCategoria;
                    """);
            }
        }

        private static void DropProgrammability(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_ResultadosCompletos;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_FixtureCompleto;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_TablaPosicionesOrdenada;");

            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Resultados_Auditoria_Update;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Resultados_RecalcularPosiciones;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_ResultadoSets_RecalcularPosiciones;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Partidos_ValidarIntegridad;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Categorias_BloquearCampeonatoFinalizado;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_CampeonatosCategorias_BloquearCampeonatoFinalizado;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Equipos_BloquearCampeonatoFinalizado;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Resultados_BloquearCampeonatoFinalizado;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Maximo12JugadoresPorEquipo;");

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_RecalcularTablaPosicionesCategoria;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CrearTablaPosicionesCategoria;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_ReprogramarPartido;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_RegistrarResultado;");
        }

        private static void CreateProgrammability(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TRIGGER TRG_Maximo12JugadoresPorEquipo
                ON InscripcionesEquipoJugador
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT IdEquipo
                        FROM InscripcionesEquipoJugador
                        WHERE Estado = 'ACTIVO'
                        GROUP BY IdEquipo
                        HAVING COUNT(*) > 12
                    )
                    BEGIN
                        THROW 51000, 'Un equipo no puede tener mas de 12 jugadores activos.', 1;
                    END;
                END
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER TRG_CampeonatosCategorias_BloquearCampeonatoFinalizado
                ON CampeonatosCategorias
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        INNER JOIN Campeonatos c ON i.IdCampeonato = c.IdCampeonato
                        WHERE c.Estado = 'FINALIZADO'
                    )
                    BEGIN
                        THROW 51001, 'No se pueden asociar categorias a un campeonato finalizado.', 1;
                    END;
                END
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER TRG_Equipos_BloquearCampeonatoFinalizado
                ON Equipos
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        INNER JOIN CampeonatosCategorias cc ON i.IdCampeonatoCategoria = cc.IdCampeonatoCategoria
                        INNER JOIN Campeonatos cam ON cc.IdCampeonato = cam.IdCampeonato
                        WHERE cam.Estado = 'FINALIZADO'
                    )
                    BEGIN
                        THROW 51002, 'No se pueden crear o modificar equipos de un campeonato finalizado.', 1;
                    END;
                END
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER TRG_Partidos_ValidarIntegridad
                ON Partidos
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        INNER JOIN Equipos el ON i.IdEquipoLocal = el.IdEquipo
                        INNER JOIN Equipos ev ON i.IdEquipoVisitante = ev.IdEquipo
                        WHERE el.IdCampeonatoCategoria <> i.IdCampeonatoCategoria
                           OR ev.IdCampeonatoCategoria <> i.IdCampeonatoCategoria
                           OR el.IdCampeonatoCategoria <> ev.IdCampeonatoCategoria
                    )
                    BEGIN
                        THROW 51003, 'Los equipos del partido deben pertenecer a la categoria del campeonato del partido.', 1;
                    END;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        INNER JOIN Fases f ON i.IdFase = f.IdFase
                        INNER JOIN Jornadas j ON i.IdJornada = j.IdJornada
                        WHERE f.IdCampeonatoCategoria <> i.IdCampeonatoCategoria
                           OR j.IdFase <> i.IdFase
                    )
                    BEGIN
                        THROW 51004, 'La fase y la jornada del partido deben pertenecer a la categoria/fase indicada.', 1;
                    END;

                    IF EXISTS (
                        SELECT IdJornada, IdEquipo
                        FROM (
                            SELECT IdPartido, IdJornada, IdEquipoLocal AS IdEquipo FROM Partidos
                            UNION ALL
                            SELECT IdPartido, IdJornada, IdEquipoVisitante AS IdEquipo FROM Partidos
                        ) equipos_jornada
                        GROUP BY IdJornada, IdEquipo
                        HAVING COUNT(*) > 1
                    )
                    BEGIN
                        THROW 51005, 'Un equipo no puede jugar mas de una vez en la misma jornada.', 1;
                    END;
                END
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER TRG_Resultados_BloquearCampeonatoFinalizado
                ON Resultados
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        INNER JOIN Partidos p ON i.IdPartido = p.IdPartido
                        INNER JOIN CampeonatosCategorias cc ON p.IdCampeonatoCategoria = cc.IdCampeonatoCategoria
                        INNER JOIN Campeonatos cam ON cc.IdCampeonato = cam.IdCampeonato
                        WHERE cam.Estado = 'FINALIZADO'
                    )
                    BEGIN
                        THROW 51006, 'No se pueden registrar o modificar resultados de un campeonato finalizado.', 1;
                    END;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        INNER JOIN Partidos p ON i.IdPartido = p.IdPartido
                        WHERE i.IdEquipoGanador NOT IN (p.IdEquipoLocal, p.IdEquipoVisitante)
                    )
                    BEGIN
                        THROW 51007, 'El equipo ganador debe ser uno de los equipos del partido.', 1;
                    END;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted i
                        INNER JOIN Partidos p ON i.IdPartido = p.IdPartido
                        WHERE (i.SetsLocal > i.SetsVisitante AND i.IdEquipoGanador <> p.IdEquipoLocal)
                           OR (i.SetsVisitante > i.SetsLocal AND i.IdEquipoGanador <> p.IdEquipoVisitante)
                    )
                    BEGIN
                        THROW 51008, 'El equipo ganador debe coincidir con el mayor numero de sets.', 1;
                    END;

                    UPDATE p
                    SET Estado = 'FINALIZADO'
                    FROM Partidos p
                    INNER JOIN inserted i ON p.IdPartido = i.IdPartido;
                END
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER TRG_Resultados_Auditoria_Update
                ON Resultados
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    INSERT INTO AuditoriaResultados (
                        IdResultado,
                        IdPartido,
                        SetsLocalAnterior,
                        SetsVisitanteAnterior,
                        IdEquipoGanadorAnterior,
                        SetsLocalNuevo,
                        SetsVisitanteNuevo,
                        IdEquipoGanadorNuevo
                    )
                    SELECT
                        i.IdResultado,
                        i.IdPartido,
                        d.SetsLocal,
                        d.SetsVisitante,
                        d.IdEquipoGanador,
                        i.SetsLocal,
                        i.SetsVisitante,
                        i.IdEquipoGanador
                    FROM inserted i
                    INNER JOIN deleted d ON i.IdResultado = d.IdResultado
                    WHERE i.SetsLocal <> d.SetsLocal
                       OR i.SetsVisitante <> d.SetsVisitante
                       OR i.IdEquipoGanador <> d.IdEquipoGanador;
                END
                """);

            migrationBuilder.Sql("""
                CREATE PROCEDURE SP_CrearTablaPosicionesCategoria
                    @IdCategoria INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    INSERT INTO TablaPosiciones (IdCampeonatoCategoria, IdEquipo)
                    SELECT e.IdCampeonatoCategoria, e.IdEquipo
                    FROM Equipos e
                    WHERE e.IdCampeonatoCategoria = @IdCategoria
                      AND e.Activo = 1
                      AND NOT EXISTS (
                          SELECT 1
                          FROM TablaPosiciones tp
                          WHERE tp.IdCampeonatoCategoria = e.IdCampeonatoCategoria
                            AND tp.IdEquipo = e.IdEquipo
                      );
                END
                """);

            migrationBuilder.Sql("""
                CREATE PROCEDURE SP_RecalcularTablaPosicionesCategoria
                    @IdCategoria INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    EXEC SP_CrearTablaPosicionesCategoria @IdCategoria;

                    UPDATE TablaPosiciones
                    SET
                        PartidosJugados = 0,
                        Ganados = 0,
                        Perdidos = 0,
                        SetsFavor = 0,
                        SetsContra = 0,
                        PuntosFavor = 0,
                        PuntosContra = 0,
                        Puntos = 0,
                        FechaActualizacion = SYSUTCDATETIME()
                    WHERE IdCampeonatoCategoria = @IdCategoria;

                    WITH ResultadoBase AS (
                        SELECT
                            p.IdCampeonatoCategoria,
                            p.IdEquipoLocal,
                            p.IdEquipoVisitante,
                            r.IdEquipoGanador,
                            r.SetsLocal,
                            r.SetsVisitante,
                            ISNULL(SUM(rs.PuntosLocal), 0) AS PuntosLocal,
                            ISNULL(SUM(rs.PuntosVisitante), 0) AS PuntosVisitante
                        FROM Resultados r
                        INNER JOIN Partidos p ON r.IdPartido = p.IdPartido
                        LEFT JOIN ResultadoSets rs ON r.IdResultado = rs.IdResultado
                        WHERE p.IdCampeonatoCategoria = @IdCategoria
                        GROUP BY
                            p.IdCampeonatoCategoria,
                            p.IdEquipoLocal,
                            p.IdEquipoVisitante,
                            r.IdEquipoGanador,
                            r.SetsLocal,
                            r.SetsVisitante
                    ),
                    EquipoResultado AS (
                        SELECT
                            IdCampeonatoCategoria,
                            IdEquipoLocal AS IdEquipo,
                            1 AS PartidosJugados,
                            CASE WHEN IdEquipoGanador = IdEquipoLocal THEN 1 ELSE 0 END AS Ganados,
                            CASE WHEN IdEquipoGanador = IdEquipoLocal THEN 0 ELSE 1 END AS Perdidos,
                            SetsLocal AS SetsFavor,
                            SetsVisitante AS SetsContra,
                            PuntosLocal AS PuntosFavor,
                            PuntosVisitante AS PuntosContra,
                            CASE WHEN IdEquipoGanador = IdEquipoLocal THEN 3 ELSE 0 END AS Puntos
                        FROM ResultadoBase
                        UNION ALL
                        SELECT
                            IdCampeonatoCategoria,
                            IdEquipoVisitante AS IdEquipo,
                            1,
                            CASE WHEN IdEquipoGanador = IdEquipoVisitante THEN 1 ELSE 0 END,
                            CASE WHEN IdEquipoGanador = IdEquipoVisitante THEN 0 ELSE 1 END,
                            SetsVisitante,
                            SetsLocal,
                            PuntosVisitante,
                            PuntosLocal,
                            CASE WHEN IdEquipoGanador = IdEquipoVisitante THEN 3 ELSE 0 END
                        FROM ResultadoBase
                    ),
                    Acumulado AS (
                        SELECT
                            IdCampeonatoCategoria,
                            IdEquipo,
                            SUM(PartidosJugados) AS PartidosJugados,
                            SUM(Ganados) AS Ganados,
                            SUM(Perdidos) AS Perdidos,
                            SUM(SetsFavor) AS SetsFavor,
                            SUM(SetsContra) AS SetsContra,
                            SUM(PuntosFavor) AS PuntosFavor,
                            SUM(PuntosContra) AS PuntosContra,
                            SUM(Puntos) AS Puntos
                        FROM EquipoResultado
                        GROUP BY IdCampeonatoCategoria, IdEquipo
                    )
                    UPDATE tp
                    SET
                        PartidosJugados = a.PartidosJugados,
                        Ganados = a.Ganados,
                        Perdidos = a.Perdidos,
                        SetsFavor = a.SetsFavor,
                        SetsContra = a.SetsContra,
                        PuntosFavor = a.PuntosFavor,
                        PuntosContra = a.PuntosContra,
                        Puntos = a.Puntos,
                        FechaActualizacion = SYSUTCDATETIME()
                    FROM TablaPosiciones tp
                    INNER JOIN Acumulado a ON tp.IdCampeonatoCategoria = a.IdCampeonatoCategoria AND tp.IdEquipo = a.IdEquipo;
                END
                """);

            migrationBuilder.Sql("""
                CREATE PROCEDURE SP_ReprogramarPartido
                    @IdPartido INT,
                    @FechaHoraNueva DATETIME2,
                    @Motivo NVARCHAR(300) = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    BEGIN TRANSACTION;

                    DECLARE @FechaHoraAnterior DATETIME2;

                    SELECT @FechaHoraAnterior = FechaHoraProgramada
                    FROM Partidos
                    WHERE IdPartido = @IdPartido;

                    IF @FechaHoraAnterior IS NULL AND NOT EXISTS (SELECT 1 FROM Partidos WHERE IdPartido = @IdPartido)
                    BEGIN
                        THROW 51009, 'Partido no encontrado.', 1;
                    END;

                    INSERT INTO ReprogramacionesPartido (IdPartido, FechaHoraAnterior, FechaHoraNueva, Motivo)
                    VALUES (@IdPartido, @FechaHoraAnterior, @FechaHoraNueva, @Motivo);

                    UPDATE Partidos
                    SET FechaHoraProgramada = @FechaHoraNueva,
                        Estado = 'REPROGRAMADO'
                    WHERE IdPartido = @IdPartido;

                    COMMIT TRANSACTION;
                END
                """);

            migrationBuilder.Sql("""
                CREATE PROCEDURE SP_RegistrarResultado
                    @IdPartido INT,
                    @SetsLocal INT,
                    @SetsVisitante INT,
                    @IdEquipoGanador INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    BEGIN TRANSACTION;

                    INSERT INTO Resultados (IdPartido, SetsLocal, SetsVisitante, IdEquipoGanador)
                    VALUES (@IdPartido, @SetsLocal, @SetsVisitante, @IdEquipoGanador);

                    COMMIT TRANSACTION;
                END
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER TRG_Resultados_RecalcularPosiciones
                ON Resultados
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @Categorias TABLE (IdCampeonatoCategoria INT PRIMARY KEY);

                    INSERT INTO @Categorias (IdCampeonatoCategoria)
                    SELECT DISTINCT p.IdCampeonatoCategoria
                    FROM inserted i
                    INNER JOIN Partidos p ON i.IdPartido = p.IdPartido
                    UNION
                    SELECT DISTINCT p.IdCampeonatoCategoria
                    FROM deleted d
                    INNER JOIN Partidos p ON d.IdPartido = p.IdPartido;

                    DECLARE @IdCampeonatoCategoria INT;
                    DECLARE categoria_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT IdCampeonatoCategoria FROM @Categorias;

                    OPEN categoria_cursor;
                    FETCH NEXT FROM categoria_cursor INTO @IdCampeonatoCategoria;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        EXEC SP_RecalcularTablaPosicionesCategoria @IdCampeonatoCategoria;
                        FETCH NEXT FROM categoria_cursor INTO @IdCampeonatoCategoria;
                    END;

                    CLOSE categoria_cursor;
                    DEALLOCATE categoria_cursor;
                END
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER TRG_ResultadoSets_RecalcularPosiciones
                ON ResultadoSets
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @Categorias TABLE (IdCampeonatoCategoria INT PRIMARY KEY);

                    INSERT INTO @Categorias (IdCampeonatoCategoria)
                    SELECT DISTINCT p.IdCampeonatoCategoria
                    FROM inserted i
                    INNER JOIN Resultados r ON i.IdResultado = r.IdResultado
                    INNER JOIN Partidos p ON r.IdPartido = p.IdPartido
                    UNION
                    SELECT DISTINCT p.IdCampeonatoCategoria
                    FROM deleted d
                    INNER JOIN Resultados r ON d.IdResultado = r.IdResultado
                    INNER JOIN Partidos p ON r.IdPartido = p.IdPartido;

                    DECLARE @IdCampeonatoCategoria INT;
                    DECLARE categoria_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT IdCampeonatoCategoria FROM @Categorias;

                    OPEN categoria_cursor;
                    FETCH NEXT FROM categoria_cursor INTO @IdCampeonatoCategoria;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        EXEC SP_RecalcularTablaPosicionesCategoria @IdCampeonatoCategoria;
                        FETCH NEXT FROM categoria_cursor INTO @IdCampeonatoCategoria;
                    END;

                    CLOSE categoria_cursor;
                    DEALLOCATE categoria_cursor;
                END
                """);

            migrationBuilder.Sql("""
                CREATE VIEW VW_TablaPosicionesOrdenada
                AS
                SELECT
                    tp.IdCampeonatoCategoria,
                    cam.IdCampeonato,
                    cam.Nombre AS Campeonato,
                    c.IdCategoria,
                    c.Nombre AS Categoria,
                    tp.IdEquipo,
                    e.Nombre AS Equipo,
                    tp.PartidosJugados,
                    tp.Ganados,
                    tp.Perdidos,
                    tp.SetsFavor,
                    tp.SetsContra,
                    tp.SetsFavor - tp.SetsContra AS DiferenciaSets,
                    tp.PuntosFavor,
                    tp.PuntosContra,
                    tp.PuntosFavor - tp.PuntosContra AS DiferenciaPuntos,
                    tp.Puntos
                FROM TablaPosiciones tp
                INNER JOIN CampeonatosCategorias cc ON tp.IdCampeonatoCategoria = cc.IdCampeonatoCategoria
                INNER JOIN Campeonatos cam ON cc.IdCampeonato = cam.IdCampeonato
                INNER JOIN Categorias c ON cc.IdCategoria = c.IdCategoria
                INNER JOIN Equipos e ON tp.IdEquipo = e.IdEquipo
                """);

            migrationBuilder.Sql("""
                CREATE VIEW VW_FixtureCompleto
                AS
                SELECT
                    p.IdPartido,
                    cc.IdCampeonatoCategoria,
                    cam.IdCampeonato,
                    cam.Nombre AS Campeonato,
                    c.IdCategoria,
                    c.Nombre AS Categoria,
                    f.IdFase,
                    f.Nombre AS Fase,
                    f.Tipo AS TipoFase,
                    j.IdJornada,
                    j.NumeroJornada,
                    p.FechaHoraProgramada,
                    local.IdEquipo AS IdEquipoLocal,
                    local.Nombre AS EquipoLocal,
                    visitante.IdEquipo AS IdEquipoVisitante,
                    visitante.Nombre AS EquipoVisitante,
                    p.Estado
                FROM Partidos p
                INNER JOIN CampeonatosCategorias cc ON p.IdCampeonatoCategoria = cc.IdCampeonatoCategoria
                INNER JOIN Campeonatos cam ON cc.IdCampeonato = cam.IdCampeonato
                INNER JOIN Categorias c ON cc.IdCategoria = c.IdCategoria
                INNER JOIN Fases f ON p.IdFase = f.IdFase
                INNER JOIN Jornadas j ON p.IdJornada = j.IdJornada
                INNER JOIN Equipos local ON p.IdEquipoLocal = local.IdEquipo
                INNER JOIN Equipos visitante ON p.IdEquipoVisitante = visitante.IdEquipo
                """);

            migrationBuilder.Sql("""
                CREATE VIEW VW_ResultadosCompletos
                AS
                SELECT
                    r.IdResultado,
                    p.IdPartido,
                    cc.IdCampeonatoCategoria,
                    cam.IdCampeonato,
                    cam.Nombre AS Campeonato,
                    c.IdCategoria,
                    c.Nombre AS Categoria,
                    f.Nombre AS Fase,
                    j.NumeroJornada,
                    p.FechaHoraProgramada,
                    local.Nombre AS EquipoLocal,
                    visitante.Nombre AS EquipoVisitante,
                    r.SetsLocal,
                    r.SetsVisitante,
                    ganador.Nombre AS EquipoGanador,
                    r.FechaRegistro
                FROM Resultados r
                INNER JOIN Partidos p ON r.IdPartido = p.IdPartido
                INNER JOIN CampeonatosCategorias cc ON p.IdCampeonatoCategoria = cc.IdCampeonatoCategoria
                INNER JOIN Campeonatos cam ON cc.IdCampeonato = cam.IdCampeonato
                INNER JOIN Categorias c ON cc.IdCategoria = c.IdCategoria
                INNER JOIN Fases f ON p.IdFase = f.IdFase
                INNER JOIN Jornadas j ON p.IdJornada = j.IdJornada
                INNER JOIN Equipos local ON p.IdEquipoLocal = local.IdEquipo
                INNER JOIN Equipos visitante ON p.IdEquipoVisitante = visitante.IdEquipo
                INNER JOIN Equipos ganador ON r.IdEquipoGanador = ganador.IdEquipo
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropProgrammability(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "FK_Equipos_CampeonatosCategorias_IdCampeonatoCategoria",
                table: "Equipos");

            migrationBuilder.DropForeignKey(
                name: "FK_Fases_CampeonatosCategorias_IdCampeonatoCategoria",
                table: "Fases");

            migrationBuilder.DropForeignKey(
                name: "FK_Partidos_CampeonatosCategorias_IdCampeonatoCategoria",
                table: "Partidos");

            migrationBuilder.DropForeignKey(
                name: "FK_TablaPosiciones_CampeonatosCategorias_IdCampeonatoCategoria",
                table: "TablaPosiciones");

            migrationBuilder.DropTable(
                name: "CampeonatosCategorias");

            migrationBuilder.DropIndex(
                name: "UQ_Categorias_Nombre",
                table: "Categorias");

            migrationBuilder.RenameColumn(
                name: "IdCampeonatoCategoria",
                table: "TablaPosiciones",
                newName: "IdCategoria");

            migrationBuilder.RenameIndex(
                name: "UQ_TablaPosiciones_CampeonatoCategoria_Equipo",
                table: "TablaPosiciones",
                newName: "UQ_TablaPosiciones_Categoria_Equipo");

            migrationBuilder.RenameColumn(
                name: "IdCampeonatoCategoria",
                table: "Partidos",
                newName: "IdCategoria");

            migrationBuilder.RenameIndex(
                name: "IX_Partidos_IdCampeonatoCategoria",
                table: "Partidos",
                newName: "IX_Partidos_IdCategoria");

            migrationBuilder.RenameColumn(
                name: "IdCampeonatoCategoria",
                table: "Fases",
                newName: "IdCategoria");

            migrationBuilder.RenameIndex(
                name: "UQ_Fases_CampeonatoCategoria_Orden",
                table: "Fases",
                newName: "UQ_Fases_Categoria_Orden");

            migrationBuilder.RenameColumn(
                name: "IdCampeonatoCategoria",
                table: "Equipos",
                newName: "IdCategoria");

            migrationBuilder.RenameIndex(
                name: "UQ_Equipos_CampeonatoCategoria_Nombre",
                table: "Equipos",
                newName: "UQ_Equipos_Categoria_Nombre");

            migrationBuilder.AddColumn<int>(
                name: "IdCampeonato",
                table: "Categorias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "UQ_Categorias_Campeonato_Nombre",
                table: "Categorias",
                columns: new[] { "IdCampeonato", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Campeonatos_IdCampeonato",
                table: "Categorias",
                column: "IdCampeonato",
                principalTable: "Campeonatos",
                principalColumn: "IdCampeonato");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipos_Categorias_IdCategoria",
                table: "Equipos",
                column: "IdCategoria",
                principalTable: "Categorias",
                principalColumn: "IdCategoria");

            migrationBuilder.AddForeignKey(
                name: "FK_Fases_Categorias_IdCategoria",
                table: "Fases",
                column: "IdCategoria",
                principalTable: "Categorias",
                principalColumn: "IdCategoria");

            migrationBuilder.AddForeignKey(
                name: "FK_Partidos_Categorias_IdCategoria",
                table: "Partidos",
                column: "IdCategoria",
                principalTable: "Categorias",
                principalColumn: "IdCategoria",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TablaPosiciones_Categorias_IdCategoria",
                table: "TablaPosiciones",
                column: "IdCategoria",
                principalTable: "Categorias",
                principalColumn: "IdCategoria");
        }
    }
}
