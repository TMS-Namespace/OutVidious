--
-- PostgreSQL database dump
--

\restrict CMvutOo27kh2NKFSkA9GpiOmDg7KFyWtbwDiH6KBqKVfUF6Vgcdgs0F2GflOM5z

-- Dumped from database version 16.11 (Debian 16.11-1.pgdg13+1)
-- Dumped by pg_dump version 16.11

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: channel; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.channel (
    id integer NOT NULL,
    created_at date DEFAULT now() NOT NULL,
    remote_id character varying(20) NOT NULL,
    title character varying(255) NOT NULL,
    last_synced_at date NOT NULL,
    alias character varying(255),
    default_playback_speed real
);


ALTER TABLE public.channel OWNER TO root;

--
-- Name: channel_avatar_map; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.channel_avatar_map (
    id integer NOT NULL,
    channel_id integer NOT NULL,
    image_id integer NOT NULL
);


ALTER TABLE public.channel_avatar_map OWNER TO root;

--
-- Name: channel_avatars_map_channel_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

ALTER TABLE public.channel_avatar_map ALTER COLUMN channel_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.channel_avatars_map_channel_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: channel_avatars_map_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.channel_avatars_map_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.channel_avatars_map_id_seq OWNER TO root;

--
-- Name: channel_avatars_map_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.channel_avatars_map_id_seq OWNED BY public.channel_avatar_map.id;


--
-- Name: channel_avatars_map_image_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

ALTER TABLE public.channel_avatar_map ALTER COLUMN image_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.channel_avatars_map_image_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: channel_banner_map; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.channel_banner_map (
    id integer NOT NULL,
    channel_id integer NOT NULL,
    image_id integer NOT NULL
);


ALTER TABLE public.channel_banner_map OWNER TO root;

--
-- Name: channel_banner_map_channel_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

ALTER TABLE public.channel_banner_map ALTER COLUMN channel_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.channel_banner_map_channel_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: channel_banner_map_image_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

ALTER TABLE public.channel_banner_map ALTER COLUMN image_id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.channel_banner_map_image_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: channel_banners_map_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.channel_banners_map_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.channel_banners_map_id_seq OWNER TO root;

--
-- Name: channel_banners_map_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.channel_banners_map_id_seq OWNED BY public.channel_banner_map.id;


--
-- Name: channel_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.channel_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.channel_id_seq OWNER TO root;

--
-- Name: channel_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.channel_id_seq OWNED BY public.channel.id;


--
-- Name: image; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.image (
    id integer NOT NULL,
    last_synced_at date,
    data bytea NOT NULL,
    created_at date,
    width integer,
    height integer,
    remote_id character varying(20)
);


ALTER TABLE public.image OWNER TO root;

--
-- Name: image_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.image_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.image_id_seq OWNER TO root;

--
-- Name: image_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.image_id_seq OWNED BY public.image.id;


--
-- Name: local_playlist; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.local_playlist (
    id integer NOT NULL,
    created_at date DEFAULT now() NOT NULL,
    alias character varying(255) NOT NULL,
    is_built_in boolean NOT NULL
);


ALTER TABLE public.local_playlist OWNER TO root;

--
-- Name: local_playlist_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.local_playlist_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.local_playlist_id_seq OWNER TO root;

--
-- Name: local_playlist_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.local_playlist_id_seq OWNED BY public.local_playlist.id;


--
-- Name: local_playlist_video_map; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.local_playlist_video_map (
    id integer NOT NULL,
    created_at date DEFAULT now() NOT NULL,
    local_playlist_id integer NOT NULL,
    video_id integer NOT NULL
);


ALTER TABLE public.local_playlist_video_map OWNER TO root;

--
-- Name: local_playlist_video_map_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.local_playlist_video_map_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.local_playlist_video_map_id_seq OWNER TO root;

--
-- Name: local_playlist_video_map_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.local_playlist_video_map_id_seq OWNED BY public.local_playlist_video_map.id;


--
-- Name: subscription; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.subscription (
    id integer NOT NULL,
    created_at date DEFAULT now() NOT NULL,
    channel_id integer NOT NULL
);


ALTER TABLE public.subscription OWNER TO root;

--
-- Name: subscription_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.subscription_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.subscription_id_seq OWNER TO root;

--
-- Name: subscription_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.subscription_id_seq OWNED BY public.subscription.id;


--
-- Name: video; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.video (
    id integer NOT NULL,
    created_at date DEFAULT now() NOT NULL,
    remote_id character varying(20) NOT NULL,
    last_synced_at date NOT NULL,
    title character varying(255) NOT NULL,
    likes_count bigint,
    channel_id integer,
    description character varying(2000)
);


ALTER TABLE public.video OWNER TO root;

--
-- Name: video_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.video_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.video_id_seq OWNER TO root;

--
-- Name: video_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.video_id_seq OWNED BY public.video.id;


--
-- Name: video_thumbnail_map; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.video_thumbnail_map (
    id integer NOT NULL,
    video_id integer,
    image_id integer
);


ALTER TABLE public.video_thumbnail_map OWNER TO root;

--
-- Name: video_thumbnail_map_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.video_thumbnail_map_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.video_thumbnail_map_id_seq OWNER TO root;

--
-- Name: video_thumbnail_map_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.video_thumbnail_map_id_seq OWNED BY public.video_thumbnail_map.id;


--
-- Name: watching_history; Type: TABLE; Schema: public; Owner: root
--

CREATE TABLE public.watching_history (
    id integer NOT NULL,
    started_at date DEFAULT now() NOT NULL,
    video_id integer NOT NULL,
    last_position integer NOT NULL,
    mark_as_watched boolean NOT NULL
);


ALTER TABLE public.watching_history OWNER TO root;

--
-- Name: watching_history_id_seq; Type: SEQUENCE; Schema: public; Owner: root
--

CREATE SEQUENCE public.watching_history_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.watching_history_id_seq OWNER TO root;

--
-- Name: watching_history_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: root
--

ALTER SEQUENCE public.watching_history_id_seq OWNED BY public.watching_history.id;


--
-- Name: channel id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.channel ALTER COLUMN id SET DEFAULT nextval('public.channel_id_seq'::regclass);


--
-- Name: channel_avatar_map id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.channel_avatar_map ALTER COLUMN id SET DEFAULT nextval('public.channel_avatars_map_id_seq'::regclass);


--
-- Name: channel_banner_map id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.channel_banner_map ALTER COLUMN id SET DEFAULT nextval('public.channel_banners_map_id_seq'::regclass);


--
-- Name: image id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.image ALTER COLUMN id SET DEFAULT nextval('public.image_id_seq'::regclass);


--
-- Name: local_playlist id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.local_playlist ALTER COLUMN id SET DEFAULT nextval('public.local_playlist_id_seq'::regclass);


--
-- Name: local_playlist_video_map id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.local_playlist_video_map ALTER COLUMN id SET DEFAULT nextval('public.local_playlist_video_map_id_seq'::regclass);


--
-- Name: subscription id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.subscription ALTER COLUMN id SET DEFAULT nextval('public.subscription_id_seq'::regclass);


--
-- Name: video id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.video ALTER COLUMN id SET DEFAULT nextval('public.video_id_seq'::regclass);


--
-- Name: video_thumbnail_map id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.video_thumbnail_map ALTER COLUMN id SET DEFAULT nextval('public.video_thumbnail_map_id_seq'::regclass);


--
-- Name: watching_history id; Type: DEFAULT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.watching_history ALTER COLUMN id SET DEFAULT nextval('public.watching_history_id_seq'::regclass);


--
-- Name: channel channel_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.channel
    ADD CONSTRAINT channel_pkey PRIMARY KEY (id);


--
-- Name: image image_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.image
    ADD CONSTRAINT image_pkey PRIMARY KEY (id);


--
-- Name: local_playlist local_playlist_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.local_playlist
    ADD CONSTRAINT local_playlist_pkey PRIMARY KEY (id);


--
-- Name: local_playlist_video_map local_playlist_video_map_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.local_playlist_video_map
    ADD CONSTRAINT local_playlist_video_map_pkey PRIMARY KEY (id);


--
-- Name: subscription subscription_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.subscription
    ADD CONSTRAINT subscription_pkey PRIMARY KEY (id);


--
-- Name: video video_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.video
    ADD CONSTRAINT video_pkey PRIMARY KEY (id);


--
-- Name: watching_history watching_history_pkey; Type: CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.watching_history
    ADD CONSTRAINT watching_history_pkey PRIMARY KEY (id);


--
-- Name: channel_avatar_map channel_avatars_map_channel_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.channel_avatar_map
    ADD CONSTRAINT channel_avatars_map_channel_id_fkey FOREIGN KEY (channel_id) REFERENCES public.channel(id);


--
-- Name: channel_avatar_map channel_avatars_map_image_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.channel_avatar_map
    ADD CONSTRAINT channel_avatars_map_image_id_fkey FOREIGN KEY (image_id) REFERENCES public.image(id);


--
-- Name: channel_banner_map channel_banner_map_channel_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.channel_banner_map
    ADD CONSTRAINT channel_banner_map_channel_id_fkey FOREIGN KEY (channel_id) REFERENCES public.channel(id);


--
-- Name: channel_banner_map channel_banner_map_image_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.channel_banner_map
    ADD CONSTRAINT channel_banner_map_image_id_fkey FOREIGN KEY (image_id) REFERENCES public.image(id);


--
-- Name: local_playlist_video_map local_playlist_video_map_relation_1; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.local_playlist_video_map
    ADD CONSTRAINT local_playlist_video_map_relation_1 FOREIGN KEY (local_playlist_id) REFERENCES public.local_playlist(id);


--
-- Name: local_playlist_video_map local_playlist_video_map_relation_2; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.local_playlist_video_map
    ADD CONSTRAINT local_playlist_video_map_relation_2 FOREIGN KEY (video_id) REFERENCES public.video(id);


--
-- Name: subscription subscription_relation_1; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.subscription
    ADD CONSTRAINT subscription_relation_1 FOREIGN KEY (channel_id) REFERENCES public.channel(id);


--
-- Name: video video_relation_1; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.video
    ADD CONSTRAINT video_relation_1 FOREIGN KEY (channel_id) REFERENCES public.channel(id);


--
-- Name: video_thumbnail_map video_thumbnail_map_image_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.video_thumbnail_map
    ADD CONSTRAINT video_thumbnail_map_image_id_fkey FOREIGN KEY (image_id) REFERENCES public.image(id);


--
-- Name: video_thumbnail_map video_thumbnail_map_video_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.video_thumbnail_map
    ADD CONSTRAINT video_thumbnail_map_video_id_fkey FOREIGN KEY (video_id) REFERENCES public.video(id);


--
-- Name: watching_history watching_history_video_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: root
--

ALTER TABLE ONLY public.watching_history
    ADD CONSTRAINT watching_history_video_id_fkey FOREIGN KEY (video_id) REFERENCES public.video(id);


--
-- PostgreSQL database dump complete
--

\unrestrict CMvutOo27kh2NKFSkA9GpiOmDg7KFyWtbwDiH6KBqKVfUF6Vgcdgs0F2GflOM5z

