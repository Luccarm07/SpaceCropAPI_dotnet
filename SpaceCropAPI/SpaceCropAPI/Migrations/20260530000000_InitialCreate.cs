using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpaceCropAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NM_USUARIO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DS_EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    DS_SENHA_HASH = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIO", x => x.ID_USUARIO);
                });

            migrationBuilder.CreateTable(
                name: "TB_SATELITE",
                columns: table => new
                {
                    ID_SATELITE = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NM_SATELITE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DS_OPERADOR = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    FL_ATIVO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false, defaultValue: "S")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SATELITE", x => x.ID_SATELITE);
                });

            migrationBuilder.CreateTable(
                name: "TB_TIPO_SENSOR",
                columns: table => new
                {
                    ID_TIPO_SENSOR = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NM_TIPO = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    DS_UNIDADE_MEDIDA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    NR_VALOR_CRITICO = table.Column<decimal>(type: "NUMBER(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_TIPO_SENSOR", x => x.ID_TIPO_SENSOR);
                });

            migrationBuilder.CreateTable(
                name: "TB_TIPO_ALERTA",
                columns: table => new
                {
                    ID_TIPO_ALERTA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NM_TIPO_ALERTA = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    DS_SEVERIDADE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    FL_REQUER_ACAO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false, defaultValue: "N")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_TIPO_ALERTA", x => x.ID_TIPO_ALERTA);
                });

            migrationBuilder.CreateTable(
                name: "TB_FAZENDA",
                columns: table => new
                {
                    ID_FAZENDA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_USUARIO = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NM_FAZENDA = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    DS_CIDADE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DS_ESTADO = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    NR_AREA_HECTARES = table.Column<decimal>(type: "NUMBER(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_FAZENDA", x => x.ID_FAZENDA);
                    table.ForeignKey(
                        name: "FK_TB_FAZENDA_TB_USUARIO_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalTable: "TB_USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_SENSOR_ORBITAL",
                columns: table => new
                {
                    ID_SENSOR_ORBITAL = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_SATELITE = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_TIPO_SENSOR = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NM_SENSOR = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    FL_ATIVO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false, defaultValue: "S")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SENSOR_ORBITAL", x => x.ID_SENSOR_ORBITAL);
                    table.ForeignKey(
                        name: "FK_TB_SENSOR_ORBITAL_TB_SATELITE_ID_SATELITE",
                        column: x => x.ID_SATELITE,
                        principalTable: "TB_SATELITE",
                        principalColumn: "ID_SATELITE",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_SENSOR_ORBITAL_TB_TIPO_SENSOR_ID_TIPO_SENSOR",
                        column: x => x.ID_TIPO_SENSOR,
                        principalTable: "TB_TIPO_SENSOR",
                        principalColumn: "ID_TIPO_SENSOR",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_SETOR_PLANTIO",
                columns: table => new
                {
                    ID_SETOR = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_FAZENDA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NM_SETOR = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DS_CULTURA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    NR_AREA_HECTARES = table.Column<decimal>(type: "NUMBER(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SETOR_PLANTIO", x => x.ID_SETOR);
                    table.ForeignKey(
                        name: "FK_TB_SETOR_PLANTIO_TB_FAZENDA_ID_FAZENDA",
                        column: x => x.ID_FAZENDA,
                        principalTable: "TB_FAZENDA",
                        principalColumn: "ID_FAZENDA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_LEITURA_SATELITE",
                columns: table => new
                {
                    ID_LEITURA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_SENSOR_ORBITAL = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_FAZENDA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_SETOR = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    NR_VALOR = table.Column<decimal>(type: "NUMBER(8,2)", precision: 8, scale: 2, nullable: false),
                    DT_LEITURA = table.Column<DateTime>(type: "DATE", nullable: false),
                    FL_ANOMALIA = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false, defaultValue: "N")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_LEITURA_SATELITE", x => x.ID_LEITURA);
                    table.ForeignKey(
                        name: "FK_TB_LEITURA_SATELITE_TB_FAZENDA_ID_FAZENDA",
                        column: x => x.ID_FAZENDA,
                        principalTable: "TB_FAZENDA",
                        principalColumn: "ID_FAZENDA",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_LEITURA_SATELITE_TB_SETOR_PLANTIO_ID_SETOR",
                        column: x => x.ID_SETOR,
                        principalTable: "TB_SETOR_PLANTIO",
                        principalColumn: "ID_SETOR",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_LEITURA_SATELITE_TB_SENSOR_ORBITAL_ID_SENSOR_ORBITAL",
                        column: x => x.ID_SENSOR_ORBITAL,
                        principalTable: "TB_SENSOR_ORBITAL",
                        principalColumn: "ID_SENSOR_ORBITAL",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_ALERTA",
                columns: table => new
                {
                    ID_ALERTA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_LEITURA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_TIPO_ALERTA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_USUARIO = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    FL_RESOLVIDO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false, defaultValue: "N"),
                    DT_ALERTA = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ALERTA", x => x.ID_ALERTA);
                    table.ForeignKey(
                        name: "FK_TB_ALERTA_TB_LEITURA_SATELITE_ID_LEITURA",
                        column: x => x.ID_LEITURA,
                        principalTable: "TB_LEITURA_SATELITE",
                        principalColumn: "ID_LEITURA",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_ALERTA_TB_TIPO_ALERTA_ID_TIPO_ALERTA",
                        column: x => x.ID_TIPO_ALERTA,
                        principalTable: "TB_TIPO_ALERTA",
                        principalColumn: "ID_TIPO_ALERTA",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_ALERTA_TB_USUARIO_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalTable: "TB_USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_ACAO_ALERTA",
                columns: table => new
                {
                    ID_ACAO = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_ALERTA = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ID_USUARIO = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    DS_ACAO_TOMADA = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    DT_ACAO = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ACAO_ALERTA", x => x.ID_ACAO);
                    table.ForeignKey(
                        name: "FK_TB_ACAO_ALERTA_TB_ALERTA_ID_ALERTA",
                        column: x => x.ID_ALERTA,
                        principalTable: "TB_ALERTA",
                        principalColumn: "ID_ALERTA",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_ACAO_ALERTA_TB_USUARIO_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalTable: "TB_USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            // Índice único para e-mail
            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_DS_EMAIL",
                table: "TB_USUARIO",
                column: "DS_EMAIL",
                unique: true);

            // Índices de FK
            migrationBuilder.CreateIndex(
                name: "IX_TB_FAZENDA_ID_USUARIO",
                table: "TB_FAZENDA",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_TB_SETOR_PLANTIO_ID_FAZENDA",
                table: "TB_SETOR_PLANTIO",
                column: "ID_FAZENDA");

            migrationBuilder.CreateIndex(
                name: "IX_TB_SENSOR_ORBITAL_ID_SATELITE",
                table: "TB_SENSOR_ORBITAL",
                column: "ID_SATELITE");

            migrationBuilder.CreateIndex(
                name: "IX_TB_SENSOR_ORBITAL_ID_TIPO_SENSOR",
                table: "TB_SENSOR_ORBITAL",
                column: "ID_TIPO_SENSOR");

            migrationBuilder.CreateIndex(
                name: "IX_TB_LEITURA_SATELITE_ID_SENSOR_ORBITAL",
                table: "TB_LEITURA_SATELITE",
                column: "ID_SENSOR_ORBITAL");

            migrationBuilder.CreateIndex(
                name: "IX_TB_LEITURA_SATELITE_ID_FAZENDA",
                table: "TB_LEITURA_SATELITE",
                column: "ID_FAZENDA");

            migrationBuilder.CreateIndex(
                name: "IX_TB_LEITURA_SATELITE_ID_SETOR",
                table: "TB_LEITURA_SATELITE",
                column: "ID_SETOR");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ALERTA_ID_LEITURA",
                table: "TB_ALERTA",
                column: "ID_LEITURA");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ALERTA_ID_TIPO_ALERTA",
                table: "TB_ALERTA",
                column: "ID_TIPO_ALERTA");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ALERTA_ID_USUARIO",
                table: "TB_ALERTA",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ACAO_ALERTA_ID_ALERTA",
                table: "TB_ACAO_ALERTA",
                column: "ID_ALERTA");

            migrationBuilder.CreateIndex(
                name: "IX_TB_ACAO_ALERTA_ID_USUARIO",
                table: "TB_ACAO_ALERTA",
                column: "ID_USUARIO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TB_ACAO_ALERTA");
            migrationBuilder.DropTable(name: "TB_ALERTA");
            migrationBuilder.DropTable(name: "TB_LEITURA_SATELITE");
            migrationBuilder.DropTable(name: "TB_SETOR_PLANTIO");
            migrationBuilder.DropTable(name: "TB_SENSOR_ORBITAL");
            migrationBuilder.DropTable(name: "TB_FAZENDA");
            migrationBuilder.DropTable(name: "TB_TIPO_ALERTA");
            migrationBuilder.DropTable(name: "TB_TIPO_SENSOR");
            migrationBuilder.DropTable(name: "TB_SATELITE");
            migrationBuilder.DropTable(name: "TB_USUARIO");
        }
    }
}
