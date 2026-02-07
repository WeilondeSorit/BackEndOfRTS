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

-- Create statistics schema
CREATE SCHEMA IF NOT EXISTS statistics;

-- Player statistics table
CREATE TABLE IF NOT EXISTS statistics.player_stats (
    id SERIAL PRIMARY KEY,
    player_id UUID NOT NULL UNIQUE,
    username VARCHAR(100) NOT NULL,
    wins INTEGER DEFAULT 0,
    losses INTEGER DEFAULT 0,
    total_matches INTEGER DEFAULT 0,
    kills INTEGER DEFAULT 0,
    deaths INTEGER DEFAULT 0,
    win_streak INTEGER DEFAULT 0,
    max_win_streak INTEGER DEFAULT 0,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_player_stats_wins ON statistics.player_stats(wins DESC);
CREATE INDEX IF NOT EXISTS idx_player_stats_player_id ON statistics.player_stats(player_id);

-- Match results table
CREATE TABLE IF NOT EXISTS statistics.match_results (
    id SERIAL PRIMARY KEY,
    match_id VARCHAR(100) NOT NULL,
    player_id UUID NOT NULL,
    is_win BOOLEAN NOT NULL,
    match_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    duration_seconds INTEGER NOT NULL,
    units_killed INTEGER DEFAULT 0,
    units_lost INTEGER DEFAULT 0,
    base_destroyed BOOLEAN DEFAULT FALSE,
    opponent_id VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS idx_match_results_player_id ON statistics.match_results(player_id);
CREATE INDEX IF NOT EXISTS idx_match_results_match_date ON statistics.match_results(match_date DESC);

-- Server logs table
CREATE TABLE IF NOT EXISTS statistics.server_logs (
    id SERIAL PRIMARY KEY,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    level VARCHAR(20) NOT NULL,
    message TEXT NOT NULL,
    service_name VARCHAR(50),
    stack_trace TEXT
);

CREATE INDEX IF NOT EXISTS idx_server_logs_timestamp ON statistics.server_logs(timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_server_logs_level ON statistics.server_logs(level);

-- Error logs table
CREATE TABLE IF NOT EXISTS statistics.error_logs (
    id SERIAL PRIMARY KEY,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    error_message TEXT NOT NULL,
    stack_trace TEXT NOT NULL,
    service_name VARCHAR(50),
    endpoint VARCHAR(200),
    request_data TEXT
);

CREATE INDEX IF NOT EXISTS idx_error_logs_timestamp ON statistics.error_logs(timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_error_logs_service_name ON statistics.error_logs(service_name);

-- Insert sample data for testing
INSERT INTO statistics.player_stats (player_id, username, wins, losses, total_matches, kills, win_streak, max_win_streak)
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'PlayerOne', 45, 12, 57, 320, 5, 8),
    ('22222222-2222-2222-2222-222222222222', 'WarriorX', 38, 20, 58, 285, 3, 6),
    ('33333333-3333-3333-3333-333333333333', 'KnightMaster', 35, 15, 50, 267, 2, 7),
    ('44444444-4444-4444-4444-444444444444', 'CastleLord', 32, 18, 50, 245, 4, 5),
    ('55555555-5555-5555-5555-555555555555', 'StrategyKing', 30, 22, 52, 230, 1, 4)
ON CONFLICT (player_id) DO NOTHING;