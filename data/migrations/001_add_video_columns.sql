-- Migration: Add missing columns to video table to match EF Core VideoEntity
-- Date: 2025-12-23

-- Add missing columns to the video table
ALTER TABLE public.video
    ADD COLUMN IF NOT EXISTS description_html TEXT,
    ADD COLUMN IF NOT EXISTS duration_seconds BIGINT NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS view_count BIGINT NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS dislikes_count BIGINT,
    ADD COLUMN IF NOT EXISTS published_at TIMESTAMP WITH TIME ZONE,
    ADD COLUMN IF NOT EXISTS genre VARCHAR(100),
    ADD COLUMN IF NOT EXISTS keywords TEXT,
    ADD COLUMN IF NOT EXISTS is_live BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS is_upcoming BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS is_short BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS is_watched BOOLEAN NOT NULL DEFAULT FALSE;

-- Change last_synced_at and created_at to timestamp with time zone for more precision
ALTER TABLE public.video
    ALTER COLUMN created_at TYPE TIMESTAMP WITH TIME ZONE USING created_at::TIMESTAMP WITH TIME ZONE,
    ALTER COLUMN last_synced_at TYPE TIMESTAMP WITH TIME ZONE USING last_synced_at::TIMESTAMP WITH TIME ZONE;
