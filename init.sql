-- postgres/init.sql
CREATE TABLE IF NOT EXISTS "Players" (
    "Id" UUID PRIMARY KEY,
    "Login" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Experience" INTEGER NOT NULL DEFAULT 0,
    "Currency" INTEGER NOT NULL DEFAULT 100,
    "Wins" INTEGER NOT NULL DEFAULT 0,
    "Losses" INTEGER NOT NULL DEFAULT 0,
    "PurchasedItemsJson" TEXT NOT NULL DEFAULT '[]',
    "UnitUpgradesJson" TEXT NOT NULL DEFAULT '{}'
);

CREATE TABLE IF NOT EXISTS "ShopItems" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Price" INTEGER NOT NULL,
    "ImagePath" TEXT NOT NULL
);

-- Добавьте пару товаров в магазин
INSERT INTO "ShopItems" ("Name", "Price", "ImagePath") VALUES 
('Sword', 50, '/images/sword.png'),
('Shield', 80, '/images/shield.png')
ON CONFLICT DO NOTHING;