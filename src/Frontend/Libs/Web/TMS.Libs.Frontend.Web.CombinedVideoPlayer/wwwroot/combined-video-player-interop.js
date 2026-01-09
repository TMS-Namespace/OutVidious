const dashNotSupportedMessage = "DASH playback is not supported in this browser";
const playbackErrorMessage = "Playback error";

const players = new Map();

const getEntry = (videoElementId) => {
    return players.get(videoElementId) ?? null;
};

const toNumber = (value) => {
    if (Number.isFinite(value)) {
        return value;
    }

    return 0;
};

const toBufferedRanges = (buffered) => {
    const ranges = [];
    if (!buffered) {
        return ranges;
    }

    for (let i = 0; i < buffered.length; i += 1) {
        const start = buffered.start(i);
        const end = buffered.end(i);
        ranges.push({
            startSeconds: toNumber(start),
            endSeconds: toNumber(end)
        });
    }

    return ranges;
};

const buildBufferingProgress = (video) => {
    const duration = toNumber(video.duration);
    const ranges = toBufferedRanges(video.buffered);
    const lastRange = ranges.length > 0 ? ranges[ranges.length - 1] : null;
    const bufferedUntilSeconds = lastRange ? lastRange.endSeconds : null;
    const bufferedRatio = duration > 0 && bufferedUntilSeconds !== null
        ? bufferedUntilSeconds / duration
        : null;

    return {
        bufferedUntilSeconds,
        bufferedRatio,
        ranges
    };
};

const buildPlaybackProgress = (video, isBuffering) => {
    const duration = toNumber(video.duration);
    const position = toNumber(video.currentTime);
    const progressRatio = duration > 0 ? position / duration : null;

    return {
        positionSeconds: position,
        durationSeconds: duration,
        isPaused: video.paused,
        isBuffering,
        playbackRate: toNumber(video.playbackRate),
        volume: toNumber(video.volume),
        isMuted: video.muted
    };
};

const buildPlaybackStatistics = (video, shakaPlayer) => {
    const playbackQuality = video.getVideoPlaybackQuality
        ? video.getVideoPlaybackQuality()
        : null;

    let droppedFrames = playbackQuality ? playbackQuality.droppedVideoFrames : null;
    let totalFrames = playbackQuality ? playbackQuality.totalVideoFrames : null;
    let estimatedBandwidthKbps = null;
    let streamBandwidthKbps = null;
    let bufferingSeconds = null;
    let width = null;
    let height = null;

    if (shakaPlayer && shakaPlayer.getStats) {
        const stats = shakaPlayer.getStats();
        if (stats) {
            droppedFrames = stats.droppedFrames ?? droppedFrames;
            totalFrames = stats.decodedFrames ?? totalFrames;
            estimatedBandwidthKbps = stats.estimatedBandwidth
                ? stats.estimatedBandwidth / 1000
                : null;
            streamBandwidthKbps = stats.streamBandwidth
                ? stats.streamBandwidth / 1000
                : null;
            bufferingSeconds = stats.bufferingTime ?? null;
            width = stats.width ?? null;
            height = stats.height ?? null;
        }
    }

    return {
        droppedFrames,
        totalFrames,
        estimatedBandwidthKbps,
        streamBandwidthKbps,
        bufferingSeconds,
        width,
        height
    };
};

const attachEventHandlers = (entry) => {
    const { video, dotNetHelper } = entry;

    const onLoadedMetadata = () => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnPlayerReady");
        }
    };

    const onLoadedData = () => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnPlayerReady");
        }
    };

    const onCanPlay = () => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnPlayerReady");
        }
    };

    const onTimeUpdate = () => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnPlaybackProgressChanged", buildPlaybackProgress(video, entry.isBuffering));
        }
    };

    const onProgress = () => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnBufferingProgressChanged", buildBufferingProgress(video));
        }
    };

    const onWaiting = () => {
        entry.isBuffering = true;
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnBufferingStateChanged", true);
        }
    };

    const onPlaying = () => {
        entry.isBuffering = false;
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnBufferingStateChanged", false);
        }
    };

    const onError = () => {
        if (dotNetHelper && video.error) {
            dotNetHelper.invokeMethodAsync("OnPlayerError", video.error.message || playbackErrorMessage);
        }
    };

    video.addEventListener("loadedmetadata", onLoadedMetadata);
    video.addEventListener("loadeddata", onLoadedData);
    video.addEventListener("canplay", onCanPlay);
    video.addEventListener("timeupdate", onTimeUpdate);
    video.addEventListener("progress", onProgress);
    video.addEventListener("waiting", onWaiting);
    video.addEventListener("playing", onPlaying);
    video.addEventListener("error", onError);

    entry.eventHandlers = {
        onLoadedMetadata,
        onLoadedData,
        onCanPlay,
        onTimeUpdate,
        onProgress,
        onWaiting,
        onPlaying,
        onError
    };
};

const detachEventHandlers = (entry) => {
    if (!entry || !entry.video || !entry.eventHandlers) {
        return;
    }

    const { video, eventHandlers } = entry;
    video.removeEventListener("loadedmetadata", eventHandlers.onLoadedMetadata);
    video.removeEventListener("loadeddata", eventHandlers.onLoadedData);
    video.removeEventListener("canplay", eventHandlers.onCanPlay);
    video.removeEventListener("timeupdate", eventHandlers.onTimeUpdate);
    video.removeEventListener("progress", eventHandlers.onProgress);
    video.removeEventListener("waiting", eventHandlers.onWaiting);
    video.removeEventListener("playing", eventHandlers.onPlaying);
    video.removeEventListener("error", eventHandlers.onError);
    entry.eventHandlers = null;
};

export const registerVideoElement = (videoElementId, dotNetHelper) => {
    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    let entry = players.get(videoElementId);
    if (entry) {
        entry.dotNetHelper = dotNetHelper;
        return true;
    }

    entry = {
        video,
        dotNetHelper,
        eventHandlers: null,
        shakaPlayer: null,
        isBuffering: false
    };

    players.set(videoElementId, entry);
    attachEventHandlers(entry);
    return true;
};

export const unregisterVideoElement = (videoElementId) => {
    const entry = getEntry(videoElementId);
    if (!entry) {
        return;
    }

    detachEventHandlers(entry);
    players.delete(videoElementId);
};

export const initializeDashPlayer = async (videoElementId, manifestUrl, dotNetHelper) => {
    if (!window.shaka || !window.shaka.Player) {
        if (dotNetHelper) {
            await dotNetHelper.invokeMethodAsync("OnPlayerError", dashNotSupportedMessage);
        }
        return false;
    }

    if (!window.shaka.Player.isBrowserSupported()) {
        if (dotNetHelper) {
            await dotNetHelper.invokeMethodAsync("OnPlayerError", dashNotSupportedMessage);
        }
        return false;
    }

    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    registerVideoElement(videoElementId, dotNetHelper);

    const entry = getEntry(videoElementId);
    if (!entry) {
        return false;
    }

    if (entry.shakaPlayer) {
        await destroyDashPlayer(videoElementId);
    }

    window.shaka.polyfill.installAll();

    const player = new window.shaka.Player();
    await player.attach(video);
    entry.shakaPlayer = player;

    player.addEventListener("error", (event) => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync("OnPlayerError", event?.detail?.message || playbackErrorMessage);
        }
    });

    await player.load(manifestUrl);
    return true;
};

export const destroyDashPlayer = async (videoElementId) => {
    const entry = getEntry(videoElementId);
    if (!entry || !entry.shakaPlayer) {
        return;
    }

    try {
        await entry.shakaPlayer.destroy();
    } finally {
        entry.shakaPlayer = null;
    }
};

export const setDashAutoQuality = (videoElementId) => {
    const entry = getEntry(videoElementId);
    if (!entry || !entry.shakaPlayer) {
        return false;
    }

    entry.shakaPlayer.configure({ abr: { enabled: true, restrictions: {} } });
    return true;
};

export const setDashMaxResolution = (videoElementId, height) => {
    const entry = getEntry(videoElementId);
    if (!entry || !entry.shakaPlayer) {
        return false;
    }

    entry.shakaPlayer.configure({ abr: { enabled: true, restrictions: { maxHeight: height } } });
    return true;
};

export const setDashQuality = (videoElementId, height) => {
    const entry = getEntry(videoElementId);
    if (!entry || !entry.shakaPlayer) {
        return false;
    }

    const tracks = entry.shakaPlayer.getVariantTracks();
    const targetTrack = tracks.find((track) => track.height === height);
    if (!targetTrack) {
        return false;
    }

    entry.shakaPlayer.configure({ abr: { enabled: false } });
    entry.shakaPlayer.selectVariantTrack(targetTrack, true);
    return true;
};

export const play = async (videoElementId) => {
    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    try {
        await video.play();
        return true;
    } catch {
        return false;
    }
};

export const pause = (videoElementId) => {
    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    video.pause();
    return true;
};

export const seek = (videoElementId, positionSeconds) => {
    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    video.currentTime = positionSeconds;
    return true;
};

export const setVolume = (videoElementId, volume) => {
    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    video.volume = volume;
    return true;
};

export const setPlaybackRate = (videoElementId, playbackRate) => {
    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    video.playbackRate = playbackRate;
    return true;
};

export const setMuted = (videoElementId, isMuted) => {
    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    video.muted = isMuted;
    return true;
};

export const setCaption = (videoElementId, captionId) => {
    const video = document.getElementById(videoElementId);
    if (!video || !video.textTracks) {
        return false;
    }

    const trackId = captionId || "";
    const hasSelection = trackId.length > 0;
    let matched = false;

    for (let i = 0; i < video.textTracks.length; i += 1) {
        const track = video.textTracks[i];
        const trackKey = track.id || track.label || track.language || "";
        const isMatch = hasSelection && trackKey === trackId;
        track.mode = isMatch ? "showing" : "disabled";
        matched = matched || isMatch;
    }

    if (!hasSelection) {
        return true;
    }

    return matched;
};

export const reload = (videoElementId) => {
    const video = document.getElementById(videoElementId);
    if (!video) {
        return false;
    }

    video.load();
    return true;
};

export const getPlaybackProgress = (videoElementId) => {
    const entry = getEntry(videoElementId);
    if (!entry || !entry.video) {
        return null;
    }

    return buildPlaybackProgress(entry.video, entry.isBuffering);
};

export const getBufferingProgress = (videoElementId) => {
    const entry = getEntry(videoElementId);
    if (!entry || !entry.video) {
        return null;
    }

    return buildBufferingProgress(entry.video);
};

export const getPlaybackStatistics = (videoElementId) => {
    const entry = getEntry(videoElementId);
    if (!entry || !entry.video) {
        return null;
    }

    return buildPlaybackStatistics(entry.video, entry.shakaPlayer);
};
