-- Создание таблицы Player
CREATE TABLE IF NOT EXISTS Player (
    id SERIAL PRIMARY KEY,
    login VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(100) NOT NULL,
    wins INTEGER DEFAULT 0 CHECK (wins >= 0),
    losses INTEGER DEFAULT 0 CHECK (losses >= 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы Settings
CREATE TABLE IF NOT EXISTS Settings (
    id SERIAL PRIMARY KEY,
    player_id INTEGER NOT NULL,
    sound_on BOOLEAN DEFAULT TRUE,
    volume DECIMAL(3,1) DEFAULT 100.0 CHECK (volume >= 0 AND volume <= 100),
    FOREIGN KEY (player_id) REFERENCES Player(id) ON DELETE CASCADE,
    UNIQUE(player_id)
);

-- Создание таблицы Achievement
CREATE TABLE IF NOT EXISTS Achievement (
    id SERIAL PRIMARY KEY,
    player_id INTEGER NOT NULL,
    achievement_name VARCHAR(100) NOT NULL,
    is_achieved BOOLEAN DEFAULT FALSE,
    achieved_at TIMESTAMP,
    FOREIGN KEY (player_id) REFERENCES Player(id) ON DELETE CASCADE,
    UNIQUE(player_id, achievement_name)
);

-- Создание таблицы player_data (ресурсы игрока)
CREATE TABLE IF NOT EXISTS player_data (
    id SERIAL PRIMARY KEY,
    player_id INTEGER NOT NULL,
    units INTEGER DEFAULT 0 CHECK (units >= 0),
    food INTEGER DEFAULT 0 CHECK (food >= 0),
    wood INTEGER DEFAULT 0 CHECK (wood >= 0),
    rock INTEGER DEFAULT 0 CHECK (rock >= 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (player_id) REFERENCES Player(id) ON DELETE CASCADE,
    UNIQUE(player_id)
);

-- Создание таблицы Unit (упрощенная для прогресса)
CREATE TABLE IF NOT EXISTS Unit (
    id SERIAL PRIMARY KEY,
    player_id INTEGER NOT NULL,
    unit_type VARCHAR(50) NOT NULL,
    coord_x INTEGER DEFAULT 0,
    coord_y INTEGER DEFAULT 0,
    current_health INTEGER DEFAULT 100,
    max_health INTEGER DEFAULT 100,
    level INTEGER DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (player_id) REFERENCES Player(id) ON DELETE CASCADE
);

-- Создание таблицы Building (упрощенная для прогресса)
CREATE TABLE IF NOT EXISTS Building (
    id SERIAL PRIMARY KEY,
    player_id INTEGER NOT NULL,
    building_type VARCHAR(50) NOT NULL,
    coord_x INTEGER DEFAULT 0,
    coord_y INTEGER DEFAULT 0,
    current_health INTEGER DEFAULT 100,
    max_health INTEGER DEFAULT 100,
    level INTEGER DEFAULT 1,
    built_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (player_id) REFERENCES Player(id) ON DELETE CASCADE
);

-- Индексы для ускорения запросов
CREATE INDEX IF NOT EXISTS idx_player_login ON Player(login);
CREATE INDEX IF NOT EXISTS idx_player_data_player ON player_data(player_id);
CREATE INDEX IF NOT EXISTS idx_unit_player ON Unit(player_id);
CREATE INDEX IF NOT EXISTS idx_building_player ON Building(player_id);