-- ===================================================================
-- SPACECROP - SOLUÇÃO COMPLETA PARA GLOBAL SOLUTION
-- Banco de Dados Oracle com PL/SQL, IoT Espacial e Agronegócio
-- ===================================================================

-- ===================================================================
-- PARTE 3: TIPOS PARA O COMPOUND TRIGGER
-- ===================================================================

SET SERVEROUTPUT ON;

CREATE OR REPLACE TYPE TYP_ALERTA_TEMP AS OBJECT (
    id_leitura NUMBER,
    id_tipo_alerta NUMBER,
    id_usuario NUMBER
);
/

CREATE OR REPLACE TYPE TBL_ALERTA_TEMP AS TABLE OF TYP_ALERTA_TEMP;
/

-- ===================================================================
-- PARTE 4: COMPOUND TRIGGER (Alerta Automático - SEM MUTATION)
-- ===================================================================

CREATE OR REPLACE TRIGGER TRG_ALERTA_AUTOMATICO
FOR INSERT ON TB_LEITURA_SATELITE
COMPOUND TRIGGER
    
    -- Coleção para armazenar dados dos alertas
    v_alertas TBL_ALERTA_TEMP := TBL_ALERTA_TEMP();
    
    -- Variáveis locais
    v_valor_critico TB_TIPO_SENSOR.nr_valor_critico%TYPE;
    v_id_tipo_alerta TB_TIPO_ALERTA.id_tipo_alerta%TYPE;
    v_usuario_dono TB_USUARIO.id_usuario%TYPE;
    
    -- BEFORE EACH ROW: Coleta os dados das linhas inseridas
    BEFORE EACH ROW IS
    BEGIN
        -- Buscar valor crítico do tipo de sensor
        SELECT ts.nr_valor_critico
        INTO v_valor_critico
        FROM TB_TIPO_SENSOR ts
        INNER JOIN TB_SENSOR_ORBITAL so ON so.id_tipo_sensor = ts.id_tipo_sensor
        WHERE so.id_sensor_orbital = :NEW.id_sensor_orbital;
        
        -- Buscar dono da fazenda
        SELECT f.id_usuario
        INTO v_usuario_dono
        FROM TB_FAZENDA f
        WHERE f.id_fazenda = :NEW.id_fazenda;
        
        -- Verificar se valor ultrapassou crítico
        IF :NEW.nr_valor > v_valor_critico THEN
            -- Marcar a leitura como anomalia (permitido em BEFORE)
            :NEW.fl_anomalia := 'S';
            
            -- Determinar tipo de alerta baseado no sensor
            IF :NEW.id_sensor_orbital IN (1, 7) THEN
                v_id_tipo_alerta := 1;  -- Temperatura Critica
            ELSIF :NEW.id_sensor_orbital IN (2, 5) THEN
                v_id_tipo_alerta := 2;  -- Baixa Umidade
            ELSIF :NEW.id_sensor_orbital = 3 THEN
                v_id_tipo_alerta := 3;  -- Vegetacao Degradada
            ELSE
                v_id_tipo_alerta := 6;  -- Seca Severa
            END IF;
            
            -- Armazenar para inserção AFTER STATEMENT
            v_alertas.EXTEND;
            v_alertas(v_alertas.COUNT) := TYP_ALERTA_TEMP(:NEW.id_leitura, v_id_tipo_alerta, v_usuario_dono);
        END IF;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('ERRO: Dados nao encontrados para verificacao de alerta');
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('ERRO INESPERADO NO BEFORE EACH ROW: ' || SQLERRM);
    END BEFORE EACH ROW;
    
    -- AFTER STATEMENT: Insere todos os alertas de uma vez
    AFTER STATEMENT IS
    BEGIN
        FOR i IN 1..v_alertas.COUNT LOOP
            INSERT INTO TB_ALERTA (id_alerta, id_leitura, id_tipo_alerta, id_usuario, fl_resolvido, dt_alerta)
            VALUES (SEQ_ALERTA.NEXTVAL, 
                    v_alertas(i).id_leitura, 
                    v_alertas(i).id_tipo_alerta, 
                    v_alertas(i).id_usuario, 
                    'N', 
                    SYSDATE);
        END LOOP;
        
        -- Limpar a coleção
        v_alertas := TBL_ALERTA_TEMP();
        
        IF v_alertas.COUNT > 0 THEN
            DBMS_OUTPUT.PUT_LINE('Alertas gerados automaticamente: ' || v_alertas.COUNT);
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('ERRO AO INSERIR ALERTAS NO AFTER STATEMENT: ' || SQLERRM);
    END AFTER STATEMENT;
    
END TRG_ALERTA_AUTOMATICO;
/

-- ===================================================================
-- PARTE 5: PROCEDURES E FUNCTIONS
-- ===================================================================

CREATE OR REPLACE PROCEDURE SP_INSERIR_LEITURA(
    p_id_sensor_orbital IN NUMBER,
    p_id_fazenda IN NUMBER,
    p_nr_valor IN NUMBER,
    p_dt_leitura IN DATE
) IS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM TB_SENSOR_ORBITAL 
    WHERE id_sensor_orbital = p_id_sensor_orbital AND fl_ativo = 'S';
    
    IF v_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Sensor orbital inativo ou inexistente');
    END IF;
    
    SELECT COUNT(*) INTO v_count FROM TB_FAZENDA WHERE id_fazenda = p_id_fazenda;
    
    IF v_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20002, 'Fazenda nao cadastrada');
    END IF;
    
    INSERT INTO TB_LEITURA_SATELITE (id_leitura, id_sensor_orbital, id_fazenda, nr_valor, dt_leitura, fl_anomalia)
    VALUES (SEQ_LEITURA.NEXTVAL, p_id_sensor_orbital, p_id_fazenda, p_nr_valor, p_dt_leitura, 'N');
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Leitura inserida com sucesso! ID: ' || SEQ_LEITURA.CURRVAL);
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Erro ao inserir leitura: ' || SQLERRM);
END SP_INSERIR_LEITURA;
/

CREATE OR REPLACE PROCEDURE SP_RESOLVER_ALERTA(
    p_id_alerta IN NUMBER,
    p_id_usuario IN NUMBER,
    p_ds_acao IN VARCHAR2
) IS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM TB_ALERTA 
    WHERE id_alerta = p_id_alerta AND fl_resolvido = 'N';
    
    IF v_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20003, 'Alerta inexistente ou ja resolvido');
    END IF;
    
    UPDATE TB_ALERTA
    SET fl_resolvido = 'S'
    WHERE id_alerta = p_id_alerta;
    
    INSERT INTO TB_ACAO_ALERTA (id_acao, id_alerta, id_usuario, ds_acao_tomada, dt_acao)
    VALUES (SEQ_ACAO.NEXTVAL, p_id_alerta, p_id_usuario, p_ds_acao, SYSDATE);
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Alerta ' || p_id_alerta || ' resolvido com sucesso!');
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Erro ao resolver alerta: ' || SQLERRM);
END SP_RESOLVER_ALERTA;
/

CREATE OR REPLACE FUNCTION FN_MEDIA_LEITURAS_FAZENDA(
    p_id_fazenda IN NUMBER,
    p_dias_ultimos IN NUMBER DEFAULT 30
) RETURN NUMBER IS
    v_media NUMBER;
BEGIN
    SELECT AVG(nr_valor)
    INTO v_media
    FROM TB_LEITURA_SATELITE
    WHERE id_fazenda = p_id_fazenda
    AND dt_leitura >= SYSDATE - p_dias_ultimos;
    
    RETURN NVL(v_media, 0);
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 0;
    WHEN OTHERS THEN
        RETURN -1;
END FN_MEDIA_LEITURAS_FAZENDA;
/

CREATE OR REPLACE FUNCTION FN_ALERTAS_PENDENTES(
    p_id_fazenda IN NUMBER
) RETURN NUMBER IS
    v_total NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_total
    FROM TB_ALERTA a
    INNER JOIN TB_LEITURA_SATELITE l ON l.id_leitura = a.id_leitura
    WHERE l.id_fazenda = p_id_fazenda
    AND a.fl_resolvido = 'N';
    
    RETURN v_total;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 0;
    WHEN OTHERS THEN
        RETURN -1;
END FN_ALERTAS_PENDENTES;
/

-- ===================================================================
-- PARTE 6: PACKAGE
-- ===================================================================

CREATE OR REPLACE PACKAGE PKG_SPACECROP AS
    PROCEDURE INSERIR_LEITURA(p_id_sensor NUMBER, p_id_fazenda NUMBER, p_valor NUMBER, p_data DATE);
    PROCEDURE RESOLVER_ALERTA(p_id_alerta NUMBER, p_id_usuario NUMBER, p_acao VARCHAR2);
    PROCEDURE GERAR_RELATORIO_FAZENDA(p_id_fazenda NUMBER);
    FUNCTION MEDIA_LEITURAS(p_id_fazenda NUMBER, p_dias NUMBER DEFAULT 30) RETURN NUMBER;
    FUNCTION TOTAL_ALERTAS(p_id_fazenda NUMBER) RETURN NUMBER;
    FUNCTION ULTIMA_LEITURA(p_id_sensor NUMBER) RETURN NUMBER;
END PKG_SPACECROP;
/

CREATE OR REPLACE PACKAGE BODY PKG_SPACECROP AS

    PROCEDURE INSERIR_LEITURA(p_id_sensor NUMBER, p_id_fazenda NUMBER, p_valor NUMBER, p_data DATE) IS
    BEGIN
        SP_INSERIR_LEITURA(p_id_sensor, p_id_fazenda, p_valor, p_data);
    END INSERIR_LEITURA;

    PROCEDURE RESOLVER_ALERTA(p_id_alerta NUMBER, p_id_usuario NUMBER, p_acao VARCHAR2) IS
    BEGIN
        SP_RESOLVER_ALERTA(p_id_alerta, p_id_usuario, p_acao);
    END RESOLVER_ALERTA;
    
    PROCEDURE GERAR_RELATORIO_FAZENDA(p_id_fazenda NUMBER) IS
        CURSOR c_alertas IS
            SELECT a.id_alerta, ta.nm_tipo_alerta, ta.ds_severidade, a.dt_alerta
            FROM TB_ALERTA a
            INNER JOIN TB_TIPO_ALERTA ta ON ta.id_tipo_alerta = a.id_tipo_alerta
            INNER JOIN TB_LEITURA_SATELITE l ON l.id_leitura = a.id_leitura
            WHERE l.id_fazenda = p_id_fazenda AND a.fl_resolvido = 'N';
    BEGIN
        DBMS_OUTPUT.PUT_LINE('=== RELATORIO FAZENDA ' || p_id_fazenda || ' ===');
        DBMS_OUTPUT.PUT_LINE('Media leituras (30d): ' || MEDIA_LEITURAS(p_id_fazenda, 30));
        DBMS_OUTPUT.PUT_LINE('Alertas pendentes: ' || TOTAL_ALERTAS(p_id_fazenda));
        DBMS_OUTPUT.PUT_LINE('--- Alertos em aberto ---');
        
        FOR alerta IN c_alertas LOOP
            DBMS_OUTPUT.PUT_LINE('Alerta ' || alerta.id_alerta || ': ' || alerta.nm_tipo_alerta || 
                                 ' (' || alerta.ds_severidade || ') - ' || alerta.dt_alerta);
        END LOOP;
    END GERAR_RELATORIO_FAZENDA;

    FUNCTION MEDIA_LEITURAS(p_id_fazenda NUMBER, p_dias NUMBER DEFAULT 30) RETURN NUMBER IS
    BEGIN
        RETURN FN_MEDIA_LEITURAS_FAZENDA(p_id_fazenda, p_dias);
    END MEDIA_LEITURAS;
    
    FUNCTION TOTAL_ALERTAS(p_id_fazenda NUMBER) RETURN NUMBER IS
    BEGIN
        RETURN FN_ALERTAS_PENDENTES(p_id_fazenda);
    END TOTAL_ALERTAS;
    
    FUNCTION ULTIMA_LEITURA(p_id_sensor NUMBER) RETURN NUMBER IS
        v_valor NUMBER;
    BEGIN
        SELECT nr_valor INTO v_valor FROM TB_LEITURA_SATELITE
        WHERE id_sensor_orbital = p_id_sensor
        ORDER BY dt_leitura DESC
        FETCH FIRST 1 ROW ONLY;
        RETURN v_valor;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RETURN NULL;
    END ULTIMA_LEITURA;
    
END PKG_SPACECROP;
/

-- ===================================================================
-- PARTE 7: AÇÕES DE TESTE
-- ===================================================================

DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM TB_ALERTA WHERE fl_resolvido = 'N';
    
    IF v_count > 0 THEN
        FOR i IN 1..LEAST(5, v_count) LOOP
            SP_RESOLVER_ALERTA(i, 1, 'Irrigacao acionada via sistema automatico');
        END LOOP;
    END IF;
END;
/

-- ===================================================================
-- PARTE 8: MODELAGEM NoSQL (JSON) - MAIS REGISTROS
-- ===================================================================

-- Query NoSQL: Extrair dados do JSON
SELECT 
    j.id_json,
    JSON_VALUE(ds_conteudo, '$.alerta.tipo') AS tipo_alerta,
    JSON_VALUE(ds_conteudo, '$.alerta.severidade') AS severidade,
    JSON_VALUE(ds_conteudo, '$.alerta.recomendacao') AS recomendacao
FROM TB_ALERTAS_JSON j;

-- ===================================================================
-- PARTE 9: BLOCOS ANÔNIMOS (6 blocos com exceções)
-- ===================================================================

-- BLOCO 1: Calcular média de temperatura com IF/ELSIF/ELSE completo e SELECT INTO
DECLARE
    v_id_fazenda NUMBER := 1;
    v_media NUMBER;
    v_nome_fazenda TB_FAZENDA.nm_fazenda%TYPE;
BEGIN
    SELECT nm_fazenda INTO v_nome_fazenda FROM TB_FAZENDA WHERE id_fazenda = v_id_fazenda;
    
    SELECT AVG(l.nr_valor) INTO v_media
    FROM TB_LEITURA_SATELITE l
    INNER JOIN TB_SENSOR_ORBITAL s ON s.id_sensor_orbital = l.id_sensor_orbital
    INNER JOIN TB_TIPO_SENSOR t ON t.id_tipo_sensor = s.id_tipo_sensor
    WHERE l.id_fazenda = v_id_fazenda AND t.nm_tipo = 'Temperatura';
    
    DBMS_OUTPUT.PUT_LINE('Fazenda: ' || v_nome_fazenda);
    DBMS_OUTPUT.PUT_LINE('Media de Temperatura: ' || NVL(TO_CHAR(v_media, '999.99'), 'Sem dados') || '°C');
    
    IF v_media > 35 THEN
        DBMS_OUTPUT.PUT_LINE('ATENCAO: Temperatura media elevada!');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('ERRO: Fazenda ' || v_id_fazenda || ' nao encontrada');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO: ' || SQLERRM);
END;
/

-- BLOCO 2: Listar sensores com SELECT INTO e cursores
DECLARE
    CURSOR c_sensores IS
        SELECT nm_sensor, fl_ativo FROM TB_SENSOR_ORBITAL;
    v_contador NUMBER := 0;
    v_nm_sensor TB_SENSOR_ORBITAL.nm_sensor%TYPE;
    v_fl_ativo TB_SENSOR_ORBITAL.fl_ativo%TYPE;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== LISTAGEM DE SENSORES ===');
    
    FOR sensor IN c_sensores LOOP
        v_contador := v_contador + 1;
        v_nm_sensor := sensor.nm_sensor;
        v_fl_ativo := sensor.fl_ativo;
        
        IF v_fl_ativo = 'S' THEN
            DBMS_OUTPUT.PUT_LINE(v_contador || '. ' || v_nm_sensor || ' [ATIVO]');
        ELSIF v_fl_ativo = 'N' THEN
            DBMS_OUTPUT.PUT_LINE(v_contador || '. ' || v_nm_sensor || ' [INATIVO]');
        ELSE
            DBMS_OUTPUT.PUT_LINE(v_contador || '. ' || v_nm_sensor || ' [STATUS DESCONHECIDO]');
        END IF;
    END LOOP;
    
    IF v_contador = 0 THEN
        RAISE_APPLICATION_ERROR(-20010, 'Nenhum sensor encontrado');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO NA LISTAGEM: ' || SQLERRM);
END;
/

-- BLOCO 3: WHILE LOOP (NOVO - requisito obrigatório)
DECLARE
    v_counter NUMBER := 1;
    v_max NUMBER := 10;
    v_id_leitura NUMBER;
    v_soma_valores NUMBER := 0;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== INSERINDO 10 LEITURAS DE TESTE COM WHILE LOOP ===');
    
    WHILE v_counter <= v_max LOOP
        SELECT SEQ_LEITURA.NEXTVAL INTO v_id_leitura FROM DUAL;
        
        INSERT INTO TB_LEITURA_SATELITE (id_leitura, id_sensor_orbital, id_fazenda, nr_valor, dt_leitura, fl_anomalia)
        VALUES (v_id_leitura, 1, 1, 20 + v_counter, SYSDATE, 'N');
        
        v_soma_valores := v_soma_valores + (20 + v_counter);
        DBMS_OUTPUT.PUT_LINE('Leitura ' || v_counter || ' inserida - Valor: ' || (20 + v_counter) || '°C');
        
        v_counter := v_counter + 1;
    END LOOP;
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Total de leituras inseridas: ' || (v_counter - 1));
    DBMS_OUTPUT.PUT_LINE('Soma dos valores: ' || v_soma_valores);
EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
        DBMS_OUTPUT.PUT_LINE('ERRO: Chave duplicada ao inserir leitura');
        ROLLBACK;
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO: ' || SQLERRM);
        ROLLBACK;
END;
/

-- BLOCO 4: LOOP com EXIT WHEN
DECLARE
    v_counter NUMBER := 1;
    v_max NUMBER := 8;
    v_id_leitura NUMBER;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== INSERINDO LEITURAS COM LOOP EXIT WHEN ===');
    
    LOOP
        SELECT SEQ_LEITURA.NEXTVAL INTO v_id_leitura FROM DUAL;
        
        INSERT INTO TB_LEITURA_SATELITE (id_leitura, id_sensor_orbital, id_fazenda, nr_valor, dt_leitura, fl_anomalia)
        VALUES (v_id_leitura, 2, 2, 30 + v_counter, SYSDATE, 'N');
        
        DBMS_OUTPUT.PUT_LINE('Leitura ' || v_counter || ' inserida - Valor: ' || (30 + v_counter) || '°C');
        
        v_counter := v_counter + 1;
        EXIT WHEN v_counter > v_max;
    END LOOP;
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Total de leituras inseridas: ' || (v_counter - 1));
EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
        DBMS_OUTPUT.PUT_LINE('ERRO: Chave duplicada ao inserir leitura');
        ROLLBACK;
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO: ' || SQLERRM);
        ROLLBACK;
END;
/

-- BLOCO 5: Analisar criticidade com IF/ELSIF/ELSE completo
DECLARE
    v_media_temperatura NUMBER;
    v_classificacao VARCHAR2(30);
    v_total_fazendas NUMBER;
    v_processadas NUMBER := 0;
    v_media_formatada VARCHAR2(10);
BEGIN
    SELECT COUNT(*) INTO v_total_fazendas FROM TB_FAZENDA;
    DBMS_OUTPUT.PUT_LINE('=== ANALISE DE CRITICIDADE DAS FAZENDAS ===');
    
    FOR f IN (SELECT id_fazenda, nm_fazenda FROM TB_FAZENDA) LOOP
        BEGIN
            SELECT AVG(l.nr_valor) INTO v_media_temperatura
            FROM TB_LEITURA_SATELITE l
            INNER JOIN TB_SENSOR_ORBITAL s ON s.id_sensor_orbital = l.id_sensor_orbital
            INNER JOIN TB_TIPO_SENSOR t ON t.id_tipo_sensor = s.id_tipo_sensor
            WHERE l.id_fazenda = f.id_fazenda AND t.nm_tipo = 'Temperatura';
            
            -- Tratar valor NULL
            IF v_media_temperatura IS NULL THEN
                v_media_formatada := 'N/A';
                v_classificacao := 'SEM DADOS';
            ELSE
                v_media_formatada := TO_CHAR(ROUND(v_media_temperatura, 1), '999.9');
                
                IF v_media_temperatura < 18 THEN
                    v_classificacao := 'CRITICA (MUITO FRIA)';
                ELSIF v_media_temperatura < 22 THEN
                    v_classificacao := 'BAIXA (FRIA)';
                ELSIF v_media_temperatura < 28 THEN
                    v_classificacao := 'IDEAL';
                ELSIF v_media_temperatura < 35 THEN
                    v_classificacao := 'ATENCAO';
                ELSE
                    v_classificacao := 'CRITICA (MUITO QUENTE)';
                END IF;
            END IF;
            
            DBMS_OUTPUT.PUT_LINE('Fazenda: ' || RPAD(f.nm_fazenda, 25) || ' | Media: ' || 
                                 v_media_formatada || '°C | ' || v_classificacao);
            v_processadas := v_processadas + 1;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                DBMS_OUTPUT.PUT_LINE('Fazenda: ' || RPAD(f.nm_fazenda, 25) || ' | Sem dados de temperatura');
        END;
    END LOOP;
    
    DBMS_OUTPUT.PUT_LINE('Total de fazendas analisadas: ' || v_processadas || '/' || v_total_fazendas);
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO INESPERADO: ' || SQLERRM);
END;
/

-- BLOCO 6: Relatório consolidado com cursores e exceções nomeadas
DECLARE
    CURSOR c_relatorio IS
        SELECT 
            f.id_fazenda,
            f.nm_fazenda,
            COUNT(DISTINCT l.id_leitura) AS total_leituras,
            COUNT(DISTINCT a.id_alerta) AS total_alertas,
            COUNT(DISTINCT CASE WHEN a.fl_resolvido = 'N' THEN a.id_alerta END) AS alertas_pendentes
        FROM TB_FAZENDA f
        LEFT JOIN TB_LEITURA_SATELITE l ON l.id_fazenda = f.id_fazenda
        LEFT JOIN TB_ALERTA a ON a.id_leitura = l.id_leitura
        GROUP BY f.id_fazenda, f.nm_fazenda;
        
    v_total_leituras_geral NUMBER := 0;
    v_total_alertas_geral NUMBER := 0;
    v_total_pendentes NUMBER := 0;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== RELATORIO GERAL DE FAZENDAS ===');
    DBMS_OUTPUT.PUT_LINE('ID | NOME DA FAZENDA | LEITURAS | ALERTAS | PENDENTES');
    DBMS_OUTPUT.PUT_LINE('--------------------------------------------------------');
    
    FOR r IN c_relatorio LOOP
        v_total_leituras_geral := v_total_leituras_geral + NVL(r.total_leituras, 0);
        v_total_alertas_geral := v_total_alertas_geral + NVL(r.total_alertas, 0);
        v_total_pendentes := v_total_pendentes + NVL(r.alertas_pendentes, 0);
        
        DBMS_OUTPUT.PUT_LINE(
            LPAD(r.id_fazenda, 2) || ' | ' ||
            RPAD(SUBSTR(r.nm_fazenda, 1, 20), 20) || ' | ' ||
            LPAD(NVL(TO_CHAR(r.total_leituras), '0'), 8) || ' | ' ||
            LPAD(NVL(TO_CHAR(r.total_alertas), '0'), 7) || ' | ' ||
            LPAD(NVL(TO_CHAR(r.alertas_pendentes), '0'), 9)
        );
    END LOOP;
    
    DBMS_OUTPUT.PUT_LINE('--------------------------------------------------------');
    DBMS_OUTPUT.PUT_LINE('TOTAIS GERAIS -> Leituras: ' || v_total_leituras_geral || 
                         ' | Alertas: ' || v_total_alertas_geral ||
                         ' | Pendentes: ' || v_total_pendentes);
                         
    IF v_total_leituras_geral = 0 THEN
        RAISE_APPLICATION_ERROR(-20020, 'Nenhuma leitura registrada no sistema');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Nenhum dado encontrado');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO AO GERAR RELATORIO: ' || SQLERRM);
END;
/

-- ===================================================================
-- PARTE 11: CURSORES EXPLÍCITOS (4 cursores com EXCEPTION)
-- ===================================================================

-- CURSOR 1: Listar alertas críticos (com SELECT INTO)
DECLARE
    CURSOR c_alertas_criticos IS
        SELECT a.id_alerta, ta.nm_tipo_alerta, f.nm_fazenda, a.dt_alerta
        FROM TB_ALERTA a
        INNER JOIN TB_TIPO_ALERTA ta ON ta.id_tipo_alerta = a.id_tipo_alerta
        INNER JOIN TB_LEITURA_SATELITE l ON l.id_leitura = a.id_leitura
        INNER JOIN TB_FAZENDA f ON f.id_fazenda = l.id_fazenda
        WHERE ta.ds_severidade = 'CRITICA' AND a.fl_resolvido = 'N';
        
    v_contador NUMBER := 0;
    v_id_alerta NUMBER;
    v_nm_tipo VARCHAR2(80);
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== ALERTAS CRITICOS NAO RESOLVIDOS ===');
    
    FOR alerta IN c_alertas_criticos LOOP
        v_contador := v_contador + 1;
        v_id_alerta := alerta.id_alerta;
        v_nm_tipo := alerta.nm_tipo_alerta;
        
        DBMS_OUTPUT.PUT_LINE(v_contador || '. Alerta ' || v_id_alerta || ': ' || v_nm_tipo || 
                             ' - ' || alerta.nm_fazenda);
    END LOOP;
    
    IF v_contador = 0 THEN
        DBMS_OUTPUT.PUT_LINE('Nenhum alerta critico pendente');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Nenhum dado encontrado nos alertas criticos');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO AO LISTAR ALERTAS CRITICOS: ' || SQLERRM);
END;
/

-- CURSOR 2: Atualizar alertas antigos (com FOR UPDATE)
DECLARE
    CURSOR c_alertas_antigos IS
        SELECT id_alerta
        FROM TB_ALERTA
        WHERE dt_alerta < SYSDATE - 30 AND fl_resolvido = 'N'
        FOR UPDATE;
        
    v_count NUMBER := 0;
BEGIN
    FOR alerta IN c_alertas_antigos LOOP
        UPDATE TB_ALERTA
        SET fl_resolvido = 'S'
        WHERE CURRENT OF c_alertas_antigos;
        
        v_count := v_count + 1;
    END LOOP;
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Alertas antigos resolvidos automaticamente: ' || v_count);
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Erro ao atualizar alertas: ' || SQLERRM);
END;
/

-- CURSOR 3: Cursor com parâmetro para análise de fazenda
DECLARE
    v_id_fazenda_param NUMBER := 1;
    
    CURSOR c_analise_fazenda(p_id_fazenda NUMBER) IS
        SELECT 
            ts.nm_tipo,
            AVG(l.nr_valor) AS media_valor,
            COUNT(CASE WHEN l.fl_anomalia = 'S' THEN 1 END) AS total_anomalias
        FROM TB_LEITURA_SATELITE l
        INNER JOIN TB_SENSOR_ORBITAL so ON so.id_sensor_orbital = l.id_sensor_orbital
        INNER JOIN TB_TIPO_SENSOR ts ON ts.id_tipo_sensor = so.id_tipo_sensor
        WHERE l.id_fazenda = p_id_fazenda
        GROUP BY ts.nm_tipo;
        
    v_nome_fazenda TB_FAZENDA.nm_fazenda%TYPE;
    v_total_registros NUMBER := 0;
BEGIN
    SELECT nm_fazenda INTO v_nome_fazenda FROM TB_FAZENDA WHERE id_fazenda = v_id_fazenda_param;
    
    DBMS_OUTPUT.PUT_LINE('=== ANALISE DA FAZENDA: ' || v_nome_fazenda || ' ===');
    
    FOR item IN c_analise_fazenda(v_id_fazenda_param) LOOP
        v_total_registros := v_total_registros + 1;
        DBMS_OUTPUT.PUT_LINE(RPAD(item.nm_tipo, 20) || ' | Media: ' || 
                             LPAD(TO_CHAR(item.media_valor, '999.99'), 10) || 
                             ' | Anomalias: ' || item.total_anomalias);
    END LOOP;
    
    IF v_total_registros = 0 THEN
        DBMS_OUTPUT.PUT_LINE('Nenhum dado de sensor encontrado para esta fazenda');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Fazenda nao encontrada: ' || v_id_fazenda_param);
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO NA ANALISE: ' || SQLERRM);
END;
/

-- CURSOR 4: Cursor para relatório de desempenho de satélites
DECLARE
    CURSOR c_desempenho_satelite IS
        SELECT 
            sat.nm_satelite,
            COUNT(DISTINCT so.id_sensor_orbital) AS total_sensores,
            COUNT(l.id_leitura) AS total_leituras,
            COUNT(CASE WHEN l.fl_anomalia = 'S' THEN 1 END) AS anomalias_detectadas
        FROM TB_SATELITE sat
        LEFT JOIN TB_SENSOR_ORBITAL so ON so.id_satelite = sat.id_satelite
        LEFT JOIN TB_LEITURA_SATELITE l ON l.id_sensor_orbital = so.id_sensor_orbital
        GROUP BY sat.nm_satelite
        ORDER BY total_leituras DESC;
        
    v_total_leituras NUMBER := 0;
    v_total_anomalias NUMBER := 0;
    v_contador NUMBER := 0;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== DESEMPENHO DOS SATELITES ===');
    DBMS_OUTPUT.PUT_LINE('Satelite | Sensores | Leituras | Anomalias | Eficiencia');
    DBMS_OUTPUT.PUT_LINE('--------------------------------------------------------');
    
    FOR sat IN c_desempenho_satelite LOOP
        v_contador := v_contador + 1;
        v_total_leituras := v_total_leituras + NVL(sat.total_leituras, 0);
        v_total_anomalias := v_total_anomalias + NVL(sat.anomalias_detectadas, 0);
        
        DBMS_OUTPUT.PUT_LINE(
            RPAD(SUBSTR(sat.nm_satelite, 1, 15), 15) || ' | ' ||
            LPAD(NVL(TO_CHAR(sat.total_sensores), '0'), 8) || ' | ' ||
            LPAD(NVL(TO_CHAR(sat.total_leituras), '0'), 8) || ' | ' ||
            LPAD(NVL(TO_CHAR(sat.anomalias_detectadas), '0'), 9) || ' | ' ||
            CASE 
                WHEN NVL(sat.total_leituras, 0) > 0 THEN 
                    ROUND(NVL(sat.anomalias_detectadas, 0) * 100.0 / sat.total_leituras, 2) || '%'
                ELSE '0%'
            END
        );
    END LOOP;
    
    DBMS_OUTPUT.PUT_LINE('--------------------------------------------------------');
    DBMS_OUTPUT.PUT_LINE('TOTAIS GERAIS | Leituras: ' || v_total_leituras || 
                         ' | Anomalias: ' || v_total_anomalias ||
                         ' | Media: ' || ROUND(v_total_anomalias / GREATEST(v_total_leituras, 1) * 100, 2) || '%');
    
    IF v_contador = 0 THEN
        RAISE_APPLICATION_ERROR(-20030, 'Nenhum satelite encontrado');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('ERRO NO RELATORIO DE SATELITES: ' || SQLERRM);
END;
/

-- ===================================================================
-- PARTE 12: LEITURAS QUE DISPARAM O TRIGGER (DEMONSTRAÇÃO)
-- ===================================================================

DECLARE
    v_id NUMBER;
BEGIN
    -- Temperatura crítica (acima de 40°C) - Sensor 1
    SELECT SEQ_LEITURA.NEXTVAL INTO v_id FROM DUAL;
    INSERT INTO TB_LEITURA_SATELITE (id_leitura, id_sensor_orbital, id_fazenda, nr_valor, dt_leitura, fl_anomalia)
    VALUES (v_id, 1, 1, 45.5, SYSDATE, 'N');
    DBMS_OUTPUT.PUT_LINE('1. Leitura temperatura crítica inserida - ID: ' || v_id);
    
    -- Umidade do Solo crítica (abaixo de 15%) - Sensor 8
    SELECT SEQ_LEITURA.NEXTVAL INTO v_id FROM DUAL;
    INSERT INTO TB_LEITURA_SATELITE (id_leitura, id_sensor_orbital, id_fazenda, nr_valor, dt_leitura, fl_anomalia)
    VALUES (v_id, 8, 1, 10.0, SYSDATE, 'N');
    DBMS_OUTPUT.PUT_LINE('2. Leitura umidade solo crítica inserida - ID: ' || v_id);
    
    -- Precipitação crítica (abaixo de 0.5mm) - Sensor 6
    SELECT SEQ_LEITURA.NEXTVAL INTO v_id FROM DUAL;
    INSERT INTO TB_LEITURA_SATELITE (id_leitura, id_sensor_orbital, id_fazenda, nr_valor, dt_leitura, fl_anomalia)
    VALUES (v_id, 6, 1, 0.0, SYSDATE, 'N');
    DBMS_OUTPUT.PUT_LINE('3. Leitura precipitação crítica inserida - ID: ' || v_id);
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('=== LEITURAS INSERIDAS COM SUCESSO ===');
END;
/

-- ===================================================================
-- PARTE 13: VERIFICAÇÃO DOS ALERTAS GERADOS
-- ===================================================================

SELECT COUNT(*) AS total_alertas_gerados FROM TB_ALERTA;

SELECT 
    a.id_alerta,
    ta.nm_tipo_alerta,
    ta.ds_severidade,
    l.nr_valor,
    l.dt_leitura,
    l.fl_anomalia
FROM TB_ALERTA a
INNER JOIN TB_TIPO_ALERTA ta ON ta.id_tipo_alerta = a.id_tipo_alerta
INNER JOIN TB_LEITURA_SATELITE l ON l.id_leitura = a.id_leitura
ORDER BY a.id_alerta;

-- ===================================================================
-- COMMIT FINAL
-- ===================================================================
COMMIT;
