-- ===================================================================
-- SPACECROP - SOLUÇÃO COMPLETA PARA GLOBAL SOLUTION
-- Banco de Dados Oracle com PL/SQL, IoT Espacial e Agronegócio
-- ===================================================================

-- ===================================================================
-- PARTE 2: DML (Data Manipulation Language) - 90+ Registros
-- ===================================================================

SET SERVEROUTPUT ON;

-- USUARIOS (5)
INSERT INTO TB_USUARIO VALUES (SEQ_USUARIO.NEXTVAL, 'Joao Silva', 'joao@email.com', 'hash123');
INSERT INTO TB_USUARIO VALUES (SEQ_USUARIO.NEXTVAL, 'Maria Santos', 'maria@email.com', 'hash456');
INSERT INTO TB_USUARIO VALUES (SEQ_USUARIO.NEXTVAL, 'Carlos Oliveira', 'carlos@email.com', 'hash789');
INSERT INTO TB_USUARIO VALUES (SEQ_USUARIO.NEXTVAL, 'Ana Pereira', 'ana@email.com', 'hashabc');
INSERT INTO TB_USUARIO VALUES (SEQ_USUARIO.NEXTVAL, 'Lucas Costa', 'lucas@email.com', 'hashdef');

-- FAZENDAS (8)
INSERT INTO TB_FAZENDA VALUES (SEQ_FAZENDA.NEXTVAL, 1, 'Fazenda Boa Vista', 'Uberlandia', 'MG', 150.5);
INSERT INTO TB_FAZENDA VALUES (SEQ_FAZENDA.NEXTVAL, 1, 'Fazenda Santa Maria', 'Uberaba', 'MG', 230.0);
INSERT INTO TB_FAZENDA VALUES (SEQ_FAZENDA.NEXTVAL, 2, 'Fazenda Vale Verde', 'Cuiaba', 'MT', 500.0);
INSERT INTO TB_FAZENDA VALUES (SEQ_FAZENDA.NEXTVAL, 2, 'Fazenda Rio Claro', 'Rondonopolis', 'MT', 320.5);
INSERT INTO TB_FAZENDA VALUES (SEQ_FAZENDA.NEXTVAL, 3, 'Fazenda Horizonte', 'Luis Eduardo Magalhaes', 'BA', 1200.0);
INSERT INTO TB_FAZENDA VALUES (SEQ_FAZENDA.NEXTVAL, 3, 'Fazenda Eldorado', 'Barreiras', 'BA', 850.0);
INSERT INTO TB_FAZENDA VALUES (SEQ_FAZENDA.NEXTVAL, 4, 'Fazenda Boa Esperanca', 'Dourados', 'MS', 420.0);
INSERT INTO TB_FAZENDA VALUES (SEQ_FAZENDA.NEXTVAL, 5, 'Fazenda Recanto', 'Sorriso', 'MT', 680.0);

-- SETORES_PLANTIO (12)
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 1, 'Talhao Norte', 'Soja', 80.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 1, 'Talhao Sul', 'Milho', 70.5);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 2, 'Area Leste', 'Cafe', 120.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 2, 'Area Oeste', 'Milho', 110.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 3, 'Setor A', 'Soja', 250.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 3, 'Setor B', 'Algodao', 250.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 4, 'Quadrante 1', 'Milho', 160.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 4, 'Quadrante 2', 'Soja', 160.5);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 5, 'Talhao Central', 'Soja', 600.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 5, 'Talhao Oeste', 'Milho', 600.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 7, 'Area Irrigada', 'Soja', 210.0);
INSERT INTO TB_SETOR_PLANTIO VALUES (SEQ_SETOR.NEXTVAL, 7, 'Area Seca', 'Milho', 210.0);

-- SATELITES (4)
INSERT INTO TB_SATELITE VALUES (SEQ_SATELITE.NEXTVAL, 'Landsat-9', 'NASA', 'S');
INSERT INTO TB_SATELITE VALUES (SEQ_SATELITE.NEXTVAL, 'Sentinel-2', 'ESA', 'S');
INSERT INTO TB_SATELITE VALUES (SEQ_SATELITE.NEXTVAL, 'CBERS-4', 'INPE/China', 'S');
INSERT INTO TB_SATELITE VALUES (SEQ_SATELITE.NEXTVAL, 'GOES-16', 'NOAA', 'S');

-- TIPOS_SENSOR (6)
INSERT INTO TB_TIPO_SENSOR VALUES (SEQ_TIPO_SENSOR.NEXTVAL, 'Temperatura', '°C', 40.0);
INSERT INTO TB_TIPO_SENSOR VALUES (SEQ_TIPO_SENSOR.NEXTVAL, 'Umidade do Ar', '%', 20.0);
INSERT INTO TB_TIPO_SENSOR VALUES (SEQ_TIPO_SENSOR.NEXTVAL, 'Indice de Vegetacao', 'NDVI', 0.2);
INSERT INTO TB_TIPO_SENSOR VALUES (SEQ_TIPO_SENSOR.NEXTVAL, 'Umidade do Solo', '%', 15.0);
INSERT INTO TB_TIPO_SENSOR VALUES (SEQ_TIPO_SENSOR.NEXTVAL, 'Luminosidade', 'W/m²', 1000.0);
INSERT INTO TB_TIPO_SENSOR VALUES (SEQ_TIPO_SENSOR.NEXTVAL, 'Precipitacao', 'mm', 0.5);

-- SENSORES_ORBITAIS (8)
INSERT INTO TB_SENSOR_ORBITAL VALUES (SEQ_SENSOR_ORBITAL.NEXTVAL, 1, 1, 'Landsat TIRS Temp', 'S');
INSERT INTO TB_SENSOR_ORBITAL VALUES (SEQ_SENSOR_ORBITAL.NEXTVAL, 1, 2, 'Landsat TIRS Umidade', 'S');
INSERT INTO TB_SENSOR_ORBITAL VALUES (SEQ_SENSOR_ORBITAL.NEXTVAL, 2, 3, 'Sentinel MSI NDVI', 'S');
INSERT INTO TB_SENSOR_ORBITAL VALUES (SEQ_SENSOR_ORBITAL.NEXTVAL, 2, 5, 'Sentinel MSI Lum', 'S');
INSERT INTO TB_SENSOR_ORBITAL VALUES (SEQ_SENSOR_ORBITAL.NEXTVAL, 3, 2, 'CBERS IR Umidade', 'S');
INSERT INTO TB_SENSOR_ORBITAL VALUES (SEQ_SENSOR_ORBITAL.NEXTVAL, 3, 6, 'CBERS Precipitacao', 'S');
INSERT INTO TB_SENSOR_ORBITAL VALUES (SEQ_SENSOR_ORBITAL.NEXTVAL, 4, 1, 'GOES Temp', 'S');
INSERT INTO TB_SENSOR_ORBITAL VALUES (SEQ_SENSOR_ORBITAL.NEXTVAL, 4, 4, 'GOES Solo', 'S');

-- TIPOS_ALERTA (6)
INSERT INTO TB_TIPO_ALERTA VALUES (SEQ_TIPO_ALERTA.NEXTVAL, 'Temperatura Critica', 'ALTA', 'S');
INSERT INTO TB_TIPO_ALERTA VALUES (SEQ_TIPO_ALERTA.NEXTVAL, 'Baixa Umidade', 'ALTA', 'S');
INSERT INTO TB_TIPO_ALERTA VALUES (SEQ_TIPO_ALERTA.NEXTVAL, 'Vegetacao Degradada', 'MEDIA', 'S');
INSERT INTO TB_TIPO_ALERTA VALUES (SEQ_TIPO_ALERTA.NEXTVAL, 'Risco de Geada', 'CRITICA', 'S');
INSERT INTO TB_TIPO_ALERTA VALUES (SEQ_TIPO_ALERTA.NEXTVAL, 'Falta de Luminosidade', 'BAIXA', 'N');
INSERT INTO TB_TIPO_ALERTA VALUES (SEQ_TIPO_ALERTA.NEXTVAL, 'Seca Severa', 'CRITICA', 'S');

-- LEITURAS_SATELITE (50 registros - parte 1 com id_setor preenchido)
DECLARE
    v_id NUMBER;
    v_setor NUMBER;
BEGIN
    FOR i IN 1..50 LOOP
        SELECT SEQ_LEITURA.NEXTVAL INTO v_id FROM DUAL;
        IF MOD(i, 3) = 0 THEN
            v_setor := MOD(i, 12) + 1;
        ELSE
            v_setor := NULL;
        END IF;
        INSERT INTO TB_LEITURA_SATELITE (id_leitura, id_sensor_orbital, id_fazenda, id_setor, nr_valor, dt_leitura, fl_anomalia)
        VALUES (v_id, MOD(i, 8) + 1, MOD(i, 8) + 1, v_setor, 20 + (i * 0.3), SYSDATE - i, 'N');
    END LOOP;
END;
/

-- Mais 40 leituras com variação e algumas anomalias
DECLARE
    v_id NUMBER;
    v_valor NUMBER;
    v_anomalia CHAR(1);
    v_setor NUMBER;
BEGIN
    FOR i IN 1..40 LOOP
        SELECT SEQ_LEITURA.NEXTVAL INTO v_id FROM DUAL;
        v_valor := 20 + (DBMS_RANDOM.VALUE * 30);
        
        IF v_valor > 42 OR v_valor < 18 THEN
            v_anomalia := 'S';
        ELSE
            v_anomalia := 'N';
        END IF;
        
        IF MOD(i, 4) = 0 THEN
            v_setor := MOD(i, 12) + 1;
        ELSE
            v_setor := NULL;
        END IF;
        
        INSERT INTO TB_LEITURA_SATELITE (id_leitura, id_sensor_orbital, id_fazenda, id_setor, nr_valor, dt_leitura, fl_anomalia)
        VALUES (v_id, MOD(i, 8) + 1, MOD(i, 8) + 1, v_setor, v_valor, SYSDATE - (i/2), v_anomalia);
    END LOOP;
END;
/

INSERT INTO TB_ALERTAS_JSON (id_json, ds_conteudo, dt_criacao)
VALUES (SEQ_JSON.NEXTVAL, '{
    "alerta": {
        "tipo": "Temperatura Critica",
        "severidade": "ALTA",
        "fazenda": "Fazenda Boa Vista",
        "valor": 42.5,
        "unidade": "°C",
        "recomendacao": "Irrigar imediatamente",
        "timestamp": "2026-05-25T10:30:00"
    }
}', SYSDATE);

INSERT INTO TB_ALERTAS_JSON (id_json, ds_conteudo, dt_criacao)
VALUES (SEQ_JSON.NEXTVAL, '{
    "alerta": {
        "tipo": "Baixa Umidade",
        "severidade": "CRITICA",
        "fazenda": "Fazenda Horizonte",
        "valor": 18.5,
        "unidade": "%",
        "recomendacao": "Acionar sistema de nebulizacao",
        "timestamp": "2026-05-25T14:15:00"
    }
}', SYSDATE);

INSERT INTO TB_ALERTAS_JSON (id_json, ds_conteudo, dt_criacao)
VALUES (SEQ_JSON.NEXTVAL, '{
    "alerta": {
        "tipo": "Vegetacao Degradada",
        "severidade": "MEDIA",
        "fazenda": "Fazenda Vale Verde",
        "valor": 0.15,
        "unidade": "NDVI",
        "recomendacao": "Monitorar area para replantio",
        "timestamp": "2026-05-26T09:00:00"
    }
}', SYSDATE);

INSERT INTO TB_ALERTAS_JSON (id_json, ds_conteudo, dt_criacao)
VALUES (SEQ_JSON.NEXTVAL, '{
    "alerta": {
        "tipo": "Risco de Geada",
        "severidade": "CRITICA",
        "fazenda": "Fazenda Santa Maria",
        "valor": 2.0,
        "unidade": "°C",
        "recomendacao": "Proteger plantacoes com cobertura",
        "timestamp": "2026-05-26T06:30:00"
    }
}', SYSDATE);

INSERT INTO TB_ALERTAS_JSON (id_json, ds_conteudo, dt_criacao)
VALUES (SEQ_JSON.NEXTVAL, '{
    "alerta": {
        "tipo": "Seca Severa",
        "severidade": "CRITICA",
        "fazenda": "Fazenda Eldorado",
        "valor": 8.0,
        "unidade": "%",
        "recomendacao": "Iniciar irrigacao de emergencia",
        "timestamp": "2026-05-26T11:45:00"
    }
}', SYSDATE);

-- ===================================================================
-- COMMIT FINAL
-- ===================================================================
COMMIT;

-- ===================================================================
-- VERIFICAÇÃO DE REGISTROS
-- ===================================================================
SELECT 'USUARIOS' AS tabela, COUNT(*) AS total FROM TB_USUARIO UNION ALL
SELECT 'FAZENDAS', COUNT(*) FROM TB_FAZENDA UNION ALL
SELECT 'SETORES', COUNT(*) FROM TB_SETOR_PLANTIO UNION ALL
SELECT 'SATELITES', COUNT(*) FROM TB_SATELITE UNION ALL
SELECT 'TIPOS_SENSOR', COUNT(*) FROM TB_TIPO_SENSOR UNION ALL
SELECT 'SENSORES_ORBITAIS', COUNT(*) FROM TB_SENSOR_ORBITAL UNION ALL
SELECT 'LEITURAS', COUNT(*) FROM TB_LEITURA_SATELITE UNION ALL
SELECT 'TIPOS_ALERTA', COUNT(*) FROM TB_TIPO_ALERTA UNION ALL
SELECT 'ALERTAS', COUNT(*) FROM TB_ALERTA UNION ALL
SELECT 'ACOES', COUNT(*) FROM TB_ACAO_ALERTA UNION ALL
SELECT 'ALERTAS JSON', COUNT(*) FROM TB_ALERTAS_JSON;