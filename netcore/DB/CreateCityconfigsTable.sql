-- Table: public.cityconfigs

-- DROP TABLE IF EXISTS public.cityconfigs;

CREATE TABLE IF NOT EXISTS public.cityconfigs
(
    city character varying(255) COLLATE pg_catalog."default" NOT NULL,
    year integer NOT NULL,
    config jsonb,
    CONSTRAINT cityconfigs_pkey PRIMARY KEY (city, year)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.cityconfigs
    OWNER to postgres;