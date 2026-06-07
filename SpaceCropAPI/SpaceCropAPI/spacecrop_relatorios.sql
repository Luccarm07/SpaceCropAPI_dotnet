-- ===================================================================
-- SPACECROP - SOLUÇÃO COMPLETA PARA GLOBAL SOLUTION
-- Banco de Dados Oracle com PL/SQL, IoT Espacial e Agronegócio
-- ===================================================================

-- ===================================================================
-- PARTE 10: RELATÓRIOS SQL (5 relatórios com JOIN)
-- ===================================================================

SET SERVEROUTPUT ON;

-- RELATÓRIO 1: Alertas por severidade e fazenda
SELECT 
    f.nm_fazenda,
    ta.ds_severidade,
    COUNT(a.id_alerta) AS total_alertas
FROM TB_ALERTA a
INNER JOIN TB_LEITURA_SATELITE l ON l.id_leitura = a.id_leitura
INNER JOIN TB_FAZENDA f ON f.id_fazenda = l.id_fazenda
INNER JOIN TB_TIPO_ALERTA ta ON ta.id_tipo_alerta = a.id_tipo_alerta
GROUP BY f.nm_fazenda, f.id_fazenda, ta.ds_severidade
ORDER BY f.nm_fazenda, ta.ds_severidade DESC;

-- RELATÓRIO 2: Eficiência dos sensores orbitais por satélite
SELECT 
    sat.nm_satelite,
    so.nm_sensor,
    COUNT(l.id_leitura) AS total_leituras,
    COUNT(CASE WHEN l.fl_anomalia = 'S' THEN 1 END) AS total_anomalias,
    CASE 
        WHEN COUNT(l.id_leitura) > 0 THEN 
            ROUND(COUNT(CASE WHEN l.fl_anomalia = 'S' THEN 1 END) * 100.0 / COUNT(l.id_leitura), 2)
        ELSE 0
    END AS taxa_anomalia
FROM TB_SATELITE sat
INNER JOIN TB_SENSOR_ORBITAL so ON so.id_satelite = sat.id_satelite
LEFT JOIN TB_LEITURA_SATELITE l ON l.id_sensor_orbital = so.id_sensor_orbital
GROUP BY sat.nm_satelite, so.nm_sensor, so.id_sensor_orbital
HAVING COUNT(l.id_leitura) > 0
ORDER BY taxa_anomalia DESC;

-- RELATÓRIO 3: Top 5 fazendas com maior número de alertas
SELECT 
    f.nm_fazenda,
    COUNT(a.id_alerta) AS total_alertas,
    ROUND(AVG(l.nr_valor), 2) AS media_leituras,
    MAX(l.dt_leitura) AS ultima_leitura
FROM TB_FAZENDA f
INNER JOIN TB_LEITURA_SATELITE l ON l.id_fazenda = f.id_fazenda
INNER JOIN TB_ALERTA a ON a.id_leitura = l.id_leitura
GROUP BY f.nm_fazenda, f.id_fazenda
ORDER BY total_alertas DESC
FETCH FIRST 5 ROWS ONLY;

-- RELATÓRIO 4: Resumo por tipo de cultura
SELECT 
    sp.ds_cultura,
    COUNT(DISTINCT sp.id_setor) AS total_setores,
    COUNT(DISTINCT f.id_fazenda) AS total_fazendas,
    ROUND(AVG(l.nr_valor), 2) AS media_valor_sensor,
    COUNT(CASE WHEN l.fl_anomalia = 'S' THEN 1 END) AS total_anomalias
FROM TB_SETOR_PLANTIO sp
INNER JOIN TB_FAZENDA f ON f.id_fazenda = sp.id_fazenda
LEFT JOIN TB_LEITURA_SATELITE l ON l.id_setor = sp.id_setor
GROUP BY sp.ds_cultura
ORDER BY total_anomalias DESC;

-- RELATÓRIO 5: Análise de tendência - Média móvel de temperatura
SELECT 
    f.nm_fazenda,
    l.dt_leitura,
    l.nr_valor AS temperatura_atual,
    ROUND(AVG(l.nr_valor) OVER (PARTITION BY f.id_fazenda ORDER BY l.dt_leitura ROWS BETWEEN 2 PRECEDING AND CURRENT ROW), 2) AS media_movel_3dias,
    CASE 
        WHEN l.nr_valor > 40 THEN 'CRITICA'
        WHEN l.nr_valor > 35 THEN 'ALTA'
        WHEN l.nr_valor > 30 THEN 'ATENCAO'
        ELSE 'NORMAL'
    END AS classificacao
FROM TB_FAZENDA f
INNER JOIN TB_LEITURA_SATELITE l ON l.id_fazenda = f.id_fazenda
INNER JOIN TB_SENSOR_ORBITAL so ON so.id_sensor_orbital = l.id_sensor_orbital
INNER JOIN TB_TIPO_SENSOR ts ON ts.id_tipo_sensor = so.id_tipo_sensor
WHERE ts.nm_tipo = 'Temperatura'
AND l.dt_leitura >= SYSDATE - 30
ORDER BY f.nm_fazenda, l.dt_leitura DESC;