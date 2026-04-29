-- =============================================
-- Таблица игроков
-- =============================================
CREATE TABLE IF NOT EXISTS "Players" (
    "Id" UUID PRIMARY KEY,
    "Login" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Experience" INTEGER NOT NULL DEFAULT 0,
    "Currency" INTEGER NOT NULL DEFAULT 100,
    "Wins" INTEGER NOT NULL DEFAULT 0,
    "Losses" INTEGER NOT NULL DEFAULT 0
);

-- =============================================
-- Товары магазина (скины/улучшения)
-- =============================================
CREATE TABLE IF NOT EXISTS "ShopItems" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Price" INTEGER NOT NULL,
    "ImagePath" TEXT NOT NULL
);

-- =============================================
-- Покупки игроков (многие-ко-многим)
-- =============================================
CREATE TABLE IF NOT EXISTS "PurchasedItems" (
    "PlayerId" UUID NOT NULL REFERENCES "Players"("Id") ON DELETE CASCADE,
    "ItemId" INTEGER NOT NULL REFERENCES "ShopItems"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("PlayerId", "ItemId")
);

-- =============================================
-- История матчей
-- =============================================
CREATE TABLE IF NOT EXISTS "Matches" (
    "Id" BIGSERIAL PRIMARY KEY,
    "PlayerId" UUID NOT NULL REFERENCES "Players"("Id") ON DELETE CASCADE,
    "IsWin" BOOLEAN NOT NULL,
    "ExperienceGained" INTEGER NOT NULL,
    "CurrencyGained" INTEGER NOT NULL DEFAULT 0,
    "Timestamp" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =============================================
-- Справочник юнитов
-- =============================================
CREATE TABLE IF NOT EXISTS "Units" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL UNIQUE,
    "BaseHealth" INTEGER NOT NULL,
    "BaseDamage" INTEGER NOT NULL
);

-- =============================================
-- Уровни прокачки юнитов для игрока
-- =============================================
CREATE TABLE IF NOT EXISTS "PlayerUnitUpgrades" (
    "PlayerId" UUID NOT NULL REFERENCES "Players"("Id") ON DELETE CASCADE,
    "UnitId" INTEGER NOT NULL REFERENCES "Units"("Id") ON DELETE CASCADE,
    "Level" INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY ("PlayerId", "UnitId")
);

-- =============================================
-- Справочник зданий
-- =============================================
CREATE TABLE IF NOT EXISTS "Buildings" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL UNIQUE,
    "BaseHp" INTEGER NOT NULL,
    "BaseProduction" INTEGER NOT NULL,
    "UpgradeCost" INTEGER NOT NULL
);

-- =============================================
-- Уровни улучшения зданий для игрока
-- =============================================
CREATE TABLE IF NOT EXISTS "PlayerBuildingUpgrades" (
    "PlayerId" UUID NOT NULL REFERENCES "Players"("Id") ON DELETE CASCADE,
    "BuildingId" INTEGER NOT NULL REFERENCES "Buildings"("Id") ON DELETE CASCADE,
    "Level" INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY ("PlayerId", "BuildingId")
);

-- =============================================
-- НОВОЕ: Справочник достижений
-- =============================================
CREATE TABLE IF NOT EXISTS "Achievements" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL UNIQUE,
    "Description" TEXT NOT NULL,
    "RequiredValue" INTEGER NOT NULL,     -- например, количество побед или опыта
    "RewardCurrency" INTEGER NOT NULL DEFAULT 0,
    "RewardExperience" INTEGER NOT NULL DEFAULT 0
);

-- =============================================
-- НОВОЕ: Выданные игрокам достижения (многие-ко-многим)
-- =============================================
CREATE TABLE IF NOT EXISTS "PlayerAchievements" (
    "PlayerId" UUID NOT NULL REFERENCES "Players"("Id") ON DELETE CASCADE,
    "AchievementId" INTEGER NOT NULL REFERENCES "Achievements"("Id") ON DELETE CASCADE,
    "UnlockedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    PRIMARY KEY ("PlayerId", "AchievementId")
);

-- Вставка тестовых игроков (если таблица уже есть)
INSERT INTO "Players" ("Id", "Login", "PasswordHash", "Experience", "Currency", "Wins", "Losses")
VALUES
  ('1a2b3c4d-0000-0000-0000-000000000001', 'PlayerOne', 'pass', 500, 200, 10, 2),
  ('1a2b3c4d-0000-0000-0000-000000000002', 'PlayerTwo', 'pass', 300, 150, 8, 3),
  ('1a2b3c4d-0000-0000-0000-000000000003', 'PlayerThree', 'pass', 700, 300, 12, 5),
  ('1a2b3c4d-0000-0000-0000-000000000004', 'PlayerFour', 'pass', 200, 100, 5, 1)
ON CONFLICT ("Id") DO NOTHING;
-- Индексы для ускорения
CREATE INDEX IF NOT EXISTS idx_player_achievements_player ON "PlayerAchievements" ("PlayerId");
CREATE INDEX IF NOT EXISTS idx_player_achievements_achievement ON "PlayerAchievements" ("AchievementId");