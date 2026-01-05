-- Создаем таблицы если их нет
CREATE TABLE IF NOT EXISTS player_data (
    player_id VARCHAR(50) PRIMARY KEY,
    player_name VARCHAR(100),
    units INTEGER DEFAULT 0,
    food INTEGER DEFAULT 0,
    wood INTEGER DEFAULT 0,
    rock INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS buildings (
    id VARCHAR(50) PRIMARY KEY,
    player_id VARCHAR(50) REFERENCES player_data(player_id) ON DELETE CASCADE,
    building_type VARCHAR(50),
    coord_x INTEGER,
    coord_y INTEGER,
    current_health INTEGER DEFAULT 100,
    max_health INTEGER DEFAULT 100,
    level INTEGER DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_buildings_player ON buildings(player_id);