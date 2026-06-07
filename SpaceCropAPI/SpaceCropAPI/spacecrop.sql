-- ===================================================================
-- SPACECROP - SOLUÇÃO COMPLETA PARA GLOBAL SOLUTION
-- Banco de Dados Oracle com PL/SQL, IoT Espacial e Agronegócio
-- ===================================================================

-- ===================================================================
-- PARTE 1: DDL (Data Definition Language)
-- ===================================================================

-- Limpeza inicial
DROP TABLE TB_ACAO_ALERTA CASCADE CONSTRAINTS;
DROP TABLE TB_ALERTA CASCADE CONSTRAINTS;
DROP TABLE TB_TIPO_ALERTA CASCADE CONSTRAINTS;
DROP TABLE TB_LEITURA_SATELITE CASCADE CONSTRAINTS;
DROP TABLE TB_SENSOR_ORBITAL CASCADE CONSTRAINTS;
DROP TABLE TB_TIPO_SENSOR CASCADE CONSTRAINTS;
DROP TABLE TB_SATELITE CASCADE CONSTRAINTS;
DROP TABLE TB_SETOR_PLANTIO CASCADE CONSTRAINTS;
DROP TABLE TB_FAZENDA CASCADE CONSTRAINTS;
DROP TABLE TB_USUARIO CASCADE CONSTRAINTS;
DROP TABLE TB_ALERTAS_JSON CASCADE CONSTRAINTS;

-- Limpeza de tipos (se existirem)
BEGIN
    EXECUTE IMMEDIATE 'DROP TYPE TBL_ALERTA_TEMP FORCE';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TYPE TYP_ALERTA_TEMP FORCE';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

DROP SEQUENCE SEQ_USUARIO;
DROP SEQUENCE SEQ_FAZENDA;
DROP SEQUENCE SEQ_SETOR;
DROP SEQUENCE SEQ_SATELITE;
DROP SEQUENCE SEQ_TIPO_SENSOR;
DROP SEQUENCE SEQ_SENSOR_ORBITAL;
DROP SEQUENCE SEQ_LEITURA;
DROP SEQUENCE SEQ_TIPO_ALERTA;
DROP SEQUENCE SEQ_ALERTA;
DROP SEQUENCE SEQ_ACAO;
DROP SEQUENCE SEQ_JSON;
DROP SEQUENCE SEQ_LOG;

SET SERVEROUTPUT ON;

-- 01. TABELA USUARIO
CREATE TABLE TB_USUARIO (
    id_usuario      NUMBER PRIMARY KEY,
    nm_usuario      VARCHAR2(100) NOT NULL,
    ds_email        VARCHAR2(150) NOT NULL UNIQUE,
    ds_senha_hash   VARCHAR2(255) NOT NULL
);

-- 02. TABELA FAZENDA
CREATE TABLE TB_FAZENDA (
    id_fazenda          NUMBER PRIMARY KEY,
    id_usuario          NUMBER NOT NULL,
    nm_fazenda          VARCHAR2(150) NOT NULL,
    ds_cidade           VARCHAR2(100) NOT NULL,
    ds_estado           CHAR(2) NOT NULL,
    nr_area_hectares    NUMBER(10,2) NOT NULL,
    CONSTRAINT chk_area_hectares CHECK (nr_area_hectares > 0),
    CONSTRAINT fk_fazenda_usuario FOREIGN KEY (id_usuario) REFERENCES TB_USUARIO(id_usuario)
);

-- 03. TABELA SETOR_PLANTIO
CREATE TABLE TB_SETOR_PLANTIO (
    id_setor            NUMBER PRIMARY KEY,
    id_fazenda          NUMBER NOT NULL,
    nm_setor            VARCHAR2(100) NOT NULL,
    ds_cultura          VARCHAR2(100) NOT NULL,
    nr_area_hectares    NUMBER(10,2) NOT NULL,
    CONSTRAINT chk_setor_area CHECK (nr_area_hectares > 0),
    CONSTRAINT fk_setor_fazenda FOREIGN KEY (id_fazenda) REFERENCES TB_FAZENDA(id_fazenda)
);

-- 04. TABELA SATELITE
CREATE TABLE TB_SATELITE (
    id_satelite         NUMBER PRIMARY KEY,
    nm_satelite         VARCHAR2(100) NOT NULL,
    ds_operador         VARCHAR2(100) NOT NULL,
    fl_ativo            CHAR(1) DEFAULT 'S' NOT NULL,
    CONSTRAINT chk_satelite_ativo CHECK (fl_ativo IN ('S', 'N'))
);

-- 05. TABELA TIPO_SENSOR
CREATE TABLE TB_TIPO_SENSOR (
    id_tipo_sensor      NUMBER PRIMARY KEY,
    nm_tipo             VARCHAR2(80) NOT NULL,
    ds_unidade_medida   VARCHAR2(20) NOT NULL,
    nr_valor_critico    NUMBER(8,2) NOT NULL
);

-- 06. TABELA SENSOR_ORBITAL
CREATE TABLE TB_SENSOR_ORBITAL (
    id_sensor_orbital   NUMBER PRIMARY KEY,
    id_satelite         NUMBER NOT NULL,
    id_tipo_sensor      NUMBER NOT NULL,
    nm_sensor           VARCHAR2(100) NOT NULL,
    fl_ativo            CHAR(1) DEFAULT 'S' NOT NULL,
    CONSTRAINT chk_sensor_ativo CHECK (fl_ativo IN ('S', 'N')),
    CONSTRAINT fk_sensor_satelite FOREIGN KEY (id_satelite) REFERENCES TB_SATELITE(id_satelite),
    CONSTRAINT fk_sensor_tipo FOREIGN KEY (id_tipo_sensor) REFERENCES TB_TIPO_SENSOR(id_tipo_sensor)
);

-- 07. TABELA LEITURA_SATELITE
CREATE TABLE TB_LEITURA_SATELITE (
    id_leitura          NUMBER PRIMARY KEY,
    id_sensor_orbital   NUMBER NOT NULL,
    id_fazenda          NUMBER NOT NULL,
    id_setor            NUMBER NULL,
    nr_valor            NUMBER(8,2) NOT NULL,
    dt_leitura          DATE DEFAULT SYSDATE NOT NULL,
    fl_anomalia         CHAR(1) DEFAULT 'N' NOT NULL,
    CONSTRAINT chk_anomalia CHECK (fl_anomalia IN ('S', 'N')),
    CONSTRAINT fk_leitura_sensor FOREIGN KEY (id_sensor_orbital) REFERENCES TB_SENSOR_ORBITAL(id_sensor_orbital),
    CONSTRAINT fk_leitura_fazenda FOREIGN KEY (id_fazenda) REFERENCES TB_FAZENDA(id_fazenda),
    CONSTRAINT fk_leitura_setor FOREIGN KEY (id_setor) REFERENCES TB_SETOR_PLANTIO(id_setor)
);

-- 08. TABELA TIPO_ALERTA
CREATE TABLE TB_TIPO_ALERTA (
    id_tipo_alerta      NUMBER PRIMARY KEY,
    nm_tipo_alerta      VARCHAR2(80) NOT NULL,
    ds_severidade       VARCHAR2(20) NOT NULL,
    fl_requer_acao      CHAR(1) DEFAULT 'N' NOT NULL,
    CONSTRAINT chk_severidade CHECK (ds_severidade IN ('BAIXA', 'MEDIA', 'ALTA', 'CRITICA')),
    CONSTRAINT chk_requer_acao CHECK (fl_requer_acao IN ('S', 'N'))
);

-- 09. TABELA ALERTA
CREATE TABLE TB_ALERTA (
    id_alerta           NUMBER PRIMARY KEY,
    id_leitura          NUMBER NOT NULL,
    id_tipo_alerta      NUMBER NOT NULL,
    id_usuario          NUMBER NOT NULL,
    fl_resolvido        CHAR(1) DEFAULT 'N' NOT NULL,
    dt_alerta           DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT chk_resolvido CHECK (fl_resolvido IN ('S', 'N')),
    CONSTRAINT fk_alerta_leitura FOREIGN KEY (id_leitura) REFERENCES TB_LEITURA_SATELITE(id_leitura),
    CONSTRAINT fk_alerta_tipo FOREIGN KEY (id_tipo_alerta) REFERENCES TB_TIPO_ALERTA(id_tipo_alerta),
    CONSTRAINT fk_alerta_usuario FOREIGN KEY (id_usuario) REFERENCES TB_USUARIO(id_usuario)
);

-- 10. TABELA ACAO_ALERTA
CREATE TABLE TB_ACAO_ALERTA (
    id_acao             NUMBER PRIMARY KEY,
    id_alerta           NUMBER NOT NULL,
    id_usuario          NUMBER NOT NULL,
    ds_acao_tomada      VARCHAR2(500) NOT NULL,
    dt_acao             DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT fk_acao_alerta FOREIGN KEY (id_alerta) REFERENCES TB_ALERTA(id_alerta),
    CONSTRAINT fk_acao_usuario FOREIGN KEY (id_usuario) REFERENCES TB_USUARIO(id_usuario)
);

-- 11. TABELA ALERTAS_JSON
CREATE TABLE TB_ALERTAS_JSON (
    id_json NUMBER PRIMARY KEY,
    ds_conteudo CLOB NOT NULL,
    dt_criacao DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT chk_json_valid CHECK (ds_conteudo IS JSON)
);

-- SEQUENCES
CREATE SEQUENCE SEQ_USUARIO START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_FAZENDA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_SETOR START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_SATELITE START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_TIPO_SENSOR START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_SENSOR_ORBITAL START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_LEITURA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_TIPO_ALERTA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_ALERTA START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_ACAO START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_JSON START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_LOG START WITH 1 INCREMENT BY 1;

-- ÍNDICES
CREATE INDEX IDX_LEITURA_FAZENDA ON TB_LEITURA_SATELITE (id_fazenda, dt_leitura);
CREATE INDEX IDX_LEITURA_ANOMALIA ON TB_LEITURA_SATELITE (fl_anomalia);
CREATE INDEX IDX_ALERTA_NAO_RESOLVIDO ON TB_ALERTA (fl_resolvido, dt_alerta);
CREATE INDEX IDX_LEITURA_SENSOR ON TB_LEITURA_SATELITE (id_sensor_orbital);

-- ===================================================================
-- COMMIT FINAL
-- ===================================================================
COMMIT;