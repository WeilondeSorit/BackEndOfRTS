-- Create user and database (must be done by postgres superuser)
DO $$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles 
      WHERE rolname = 'game_user') THEN
      CREATE USER game_user WITH PASSWORD 'game_password';
   END IF;
END
$$;

-- Grant privileges to game_user
GRANT ALL PRIVILEGES ON DATABASE game_db TO game_user;

-- Create schemas
CREATE SCHEMA IF NOT EXISTS public;
CREATE SCHEMA IF NOT EXISTS statistics;

-- Grant usage and create privileges on schemas
GRANT USAGE ON SCHEMA public TO game_user;
GRANT USAGE ON SCHEMA statistics TO game_user;
GRANT CREATE ON SCHEMA public TO game_user;
GRANT CREATE ON SCHEMA statistics TO game_user;

-- Set default privileges for game_user
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO game_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO game_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA statistics GRANT ALL ON TABLES TO game_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA statistics GRANT ALL ON SEQUENCES TO game_user;

-- ============================================
-- PUBLIC SCHEMA (player-service)
-- ============================================

-- Users table
CREATE TABLE IF NOT EXISTS public.users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    username VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

-- User profiles table
CREATE TABLE IF NOT EXISTS public.user_profiles (
    user_id INTEGER PRIMARY KEY REFERENCES public.users(id) ON DELETE CASCADE,
    level INTEGER DEFAULT 1,
    experience INTEGER DEFAULT 0,
    gold INTEGER DEFAULT 1000,
    gems INTEGER DEFAULT 0,
    wins INTEGER DEFAULT 0,
    losses INTEGER DEFAULT 0,
    rank VARCHAR(50) DEFAULT 'Recruit',
    avatar_url VARCHAR(500),
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Units table (reference data)
CREATE TABLE IF NOT EXISTS public.units (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    base_hp INTEGER NOT NULL,
    base_damage INTEGER NOT NULL,
    cost INTEGER NOT NULL,
    unlock_level INTEGER DEFAULT 1,
    description TEXT
);

-- Weapons table (reference data)
CREATE TABLE IF NOT EXISTS public.weapons (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    base_damage INTEGER NOT NULL,
    rarity VARCHAR(20) NOT NULL,
    cost INTEGER NOT NULL,
    description TEXT
);

-- Upgrades table
CREATE TABLE IF NOT EXISTS public.upgrades (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    unit_id INTEGER NOT NULL REFERENCES public.units(id) ON DELETE CASCADE,
    level INTEGER DEFAULT 1,
    damage_bonus INTEGER DEFAULT 0,
    hp_bonus INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, unit_id)
);

-- Inventory table
CREATE TABLE IF NOT EXISTS public.inventory (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    item_type VARCHAR(50) NOT NULL,
    item_id INTEGER NOT NULL,
    quantity INTEGER DEFAULT 1,
    equipped BOOLEAN DEFAULT FALSE,
    acquired_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Game sessions table
CREATE TABLE IF NOT EXISTS public.game_sessions (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    session_id VARCHAR(100) NOT NULL,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP,
    result VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Transactions table
CREATE TABLE IF NOT EXISTS public.transactions (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL,
    amount INTEGER NOT NULL,
    currency VARCHAR(20) NOT NULL,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    description TEXT
);

-- Indexes for public schema
CREATE INDEX IF NOT EXISTS idx_users_email ON public.users(email);
CREATE INDEX IF NOT EXISTS idx_users_username ON public.users(username);
CREATE INDEX IF NOT EXISTS idx_user_profiles_user_id ON public.user_profiles(user_id);
CREATE INDEX IF NOT EXISTS idx_upgrades_user_id ON public.upgrades(user_id);
CREATE INDEX IF NOT EXISTS idx_inventory_user_id ON public.inventory(user_id);
CREATE INDEX IF NOT EXISTS idx_game_sessions_user_id ON public.game_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_game_sessions_session_id ON public.game_sessions(session_id);
CREATE INDEX IF NOT EXISTS idx_transactions_user_id ON public.transactions(user_id);
CREATE INDEX IF NOT EXISTS idx_transactions_timestamp ON public.transactions(timestamp DESC);

-- ============================================
-- STATISTICS SCHEMA (statistics-service)
-- ============================================

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

-- Server logs table
CREATE TABLE IF NOT EXISTS statistics.server_logs (
    id SERIAL PRIMARY KEY,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    level VARCHAR(20) NOT NULL,
    message TEXT NOT NULL,
    service_name VARCHAR(50),
    stack_trace TEXT
);

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

-- Indexes for statistics schema
CREATE INDEX IF NOT EXISTS idx_player_stats_wins ON statistics.player_stats(wins DESC);
CREATE INDEX IF NOT EXISTS idx_player_stats_player_id ON statistics.player_stats(player_id);
CREATE INDEX IF NOT EXISTS idx_match_results_player_id ON statistics.match_results(player_id);
CREATE INDEX IF NOT EXISTS idx_match_results_match_date ON statistics.match_results(match_date DESC);
CREATE INDEX IF NOT EXISTS idx_server_logs_timestamp ON statistics.server_logs(timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_server_logs_level ON statistics.server_logs(level);
CREATE INDEX IF NOT EXISTS idx_error_logs_timestamp ON statistics.error_logs(timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_error_logs_service_name ON statistics.error_logs(service_name);

-- Grant all privileges on tables to game_user
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO game_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA statistics TO game_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO game_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA statistics TO game_user;

-- Insert sample data for testing
INSERT INTO public.users (email, password_hash, username, created_at) 
VALUES 
    ('player1@example.com', '$2a$11$8sX7JZ5qY9KjN8LmP3Qr.eZxY1v2w3x4y5z6A7B8C9D0E1F2G', 'PlayerOne', NOW()),
    ('player2@example.com', '$2a$11$8sX7JZ5qY9KjN8LmP3Qr.eZxY1v2w3x4y5z6A7B8C9D0E1F2G', 'WarriorX', NOW()),
    ('player3@example.com', '$2a$11$8sX7JZ5qY9KjN8LmP3Qr.eZxY1v2w3x4y5z6A7B8C9D0E1F2G', 'KnightMaster', NOW())
ON CONFLICT (email) DO NOTHING;

INSERT INTO public.user_profiles (user_id, level, experience, gold, gems, wins, losses, rank) 
VALUES 
    (1, 5, 3450, 1250, 50, 45, 12, 'Captain'),
    (2, 4, 2800, 980, 30, 38, 20, 'Lieutenant'),
    (3, 4, 2500, 850, 25, 35, 15, 'Sergeant')
ON CONFLICT (user_id) DO NOTHING;

INSERT INTO statistics.player_stats (player_id, username, wins, losses, total_matches, kills, win_streak, max_win_streak, last_updated) 
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'PlayerOne', 45, 12, 57, 320, 5, 8, NOW()),
    ('22222222-2222-2222-2222-222222222222', 'WarriorX', 38, 20, 58, 285, 3, 6, NOW()),
    ('33333333-3333-3333-3333-333333333333', 'KnightMaster', 35, 15, 50, 267, 2, 7, NOW()),
    ('44444444-4444-4444-4444-444444444444', 'CastleLord', 32, 18, 50, 245, 4, 5, NOW()),
    ('55555555-5555-5555-5555-555555555555', 'StrategyKing', 30, 22, 52, 230, 1, 4, NOW())
ON CONFLICT (player_id) DO NOTHING;