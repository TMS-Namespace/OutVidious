/**
 * Shaka Player JavaScript Interop for Blazor
 * Provides DASH playback support with automatic audio/video synchronization
 */

// Store player instances by element ID
const shakaPlayers = new Map();

/**
 * Initialize Shaka Player with a DASH manifest URL
 * @param {string} videoElementId - The ID of the video element
 * @param {string} manifestUrl - The DASH manifest URL
 * @param {object} dotNetHelper - DotNet object reference for callbacks
 * @returns {Promise<boolean>} - True if initialization succeeded
 */
window.shakaPlayerInterop = {
    
    /**
     * Check if Shaka Player is supported in this browser
     */
    isSupported: function() {
        return shaka.Player.isBrowserSupported();
    },

    /**
     * Initialize and load a DASH manifest
     */
    initialize: async function(videoElementId, manifestUrl, dotNetHelper) {
        try {
            // Check browser support
            if (!shaka.Player.isBrowserSupported()) {
                console.error('Shaka Player: Browser not supported');
                if (dotNetHelper) {
                    await dotNetHelper.invokeMethodAsync('OnPlayerError', 'Browser does not support DASH playback');
                }
                return false;
            }

            const videoElement = document.getElementById(videoElementId);
            if (!videoElement) {
                console.error('Shaka Player: Video element not found:', videoElementId);
                return false;
            }

            // Destroy existing player if any
            if (shakaPlayers.has(videoElementId)) {
                await this.destroy(videoElementId);
            }

            // Install polyfills if needed
            shaka.polyfill.installAll();

            // Create the player
            const player = new shaka.Player();
            await player.attach(videoElement);
            
            // Store the player
            shakaPlayers.set(videoElementId, { player, dotNetHelper });

            // Configure the player
            player.configure({
                streaming: {
                    bufferingGoal: 60,
                    rebufferingGoal: 2,
                    bufferBehind: 30,
                    retryParameters: {
                        maxAttempts: 5,
                        baseDelay: 1000,
                        backoffFactor: 2,
                        timeout: 30000
                    }
                },
                abr: {
                    enabled: true,
                    defaultBandwidthEstimate: 1000000 // 1 Mbps default
                }
            });

            // Set up event listeners
            player.addEventListener('error', (event) => {
                console.error('Shaka Player error:', event.detail);
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnPlayerError', event.detail.message || 'Unknown player error');
                }
            });

            player.addEventListener('adaptation', () => {
                const tracks = player.getVariantTracks();
                const activeTrack = tracks.find(t => t.active);
                if (activeTrack && dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnDashQualityAdapted', 
                        activeTrack.height ? `${activeTrack.height}p` : 'auto');
                }
            });

            videoElement.addEventListener('loadedmetadata', () => {
                console.log('Shaka Player: Video metadata loaded');
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnVideoLoaded');
                }
            });

            videoElement.addEventListener('playing', () => {
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnPlaybackStarted');
                }
            });

            // Load the manifest
            console.log('Shaka Player: Loading manifest:', manifestUrl);
            await player.load(manifestUrl);
            console.log('Shaka Player: Manifest loaded successfully');

            // Log available tracks
            const tracks = player.getVariantTracks();
            console.log('Shaka Player: Available tracks:', tracks.length);
            tracks.forEach(t => {
                console.log(`  - ${t.height}p @ ${Math.round(t.bandwidth / 1000)} kbps`);
            });

            return true;

        } catch (error) {
            console.error('Shaka Player: Initialization failed:', error);
            if (dotNetHelper) {
                await dotNetHelper.invokeMethodAsync('OnPlayerError', error.message || 'Failed to initialize player');
            }
            return false;
        }
    },

    /**
     * Get available quality levels
     */
    getQualityLevels: function(videoElementId) {
        const instance = shakaPlayers.get(videoElementId);
        if (!instance) return [];

        const tracks = instance.player.getVariantTracks();
        const qualities = [...new Set(tracks.map(t => t.height))].filter(h => h > 0);
        return qualities.sort((a, b) => b - a).map(h => `${h}p`);
    },

    /**
     * Set the maximum resolution for ABR
     */
    setMaxResolution: function(videoElementId, height) {
        const instance = shakaPlayers.get(videoElementId);
        if (!instance) return false;

        const config = { abr: { restrictions: { maxHeight: height } } };
        instance.player.configure(config);
        console.log('Shaka Player: Max resolution set to', height);
        return true;
    },

    /**
     * Set a specific quality (disables ABR for that track)
     */
    setQuality: function(videoElementId, qualityLabel) {
        const instance = shakaPlayers.get(videoElementId);
        if (!instance) return false;

        const height = parseInt(qualityLabel);
        if (isNaN(height)) {
            // Enable ABR
            instance.player.configure({ abr: { enabled: true } });
            console.log('Shaka Player: ABR enabled');
            return true;
        }

        const tracks = instance.player.getVariantTracks();
        const targetTrack = tracks.find(t => t.height === height);
        
        if (targetTrack) {
            // Disable ABR and select specific track
            instance.player.configure({ abr: { enabled: false } });
            instance.player.selectVariantTrack(targetTrack, true);
            console.log('Shaka Player: Quality set to', qualityLabel);
            return true;
        }

        return false;
    },

    /**
     * Get current playback quality
     */
    getCurrentQuality: function(videoElementId) {
        const instance = shakaPlayers.get(videoElementId);
        if (!instance) return null;

        const tracks = instance.player.getVariantTracks();
        const activeTrack = tracks.find(t => t.active);
        return activeTrack ? `${activeTrack.height}p` : null;
    },

    /**
     * Play the video
     */
    play: function(videoElementId) {
        const video = document.getElementById(videoElementId);
        if (video) {
            video.play().catch(e => console.warn('Autoplay prevented:', e));
        }
    },

    /**
     * Pause the video
     */
    pause: function(videoElementId) {
        const video = document.getElementById(videoElementId);
        if (video) {
            video.pause();
        }
    },

    /**
     * Get buffering info
     */
    getBufferInfo: function(videoElementId) {
        const instance = shakaPlayers.get(videoElementId);
        if (!instance) return null;

        const bufferedInfo = instance.player.getBufferedInfo();
        return {
            total: bufferedInfo.total,
            audio: bufferedInfo.audio,
            video: bufferedInfo.video
        };
    },

    /**
     * Get player statistics
     */
    getStats: function(videoElementId) {
        const instance = shakaPlayers.get(videoElementId);
        if (!instance) return null;

        return instance.player.getStats();
    },

    /**
     * Destroy the player instance
     */
    destroy: async function(videoElementId) {
        const instance = shakaPlayers.get(videoElementId);
        if (instance) {
            try {
                await instance.player.destroy();
                shakaPlayers.delete(videoElementId);
                console.log('Shaka Player: Destroyed instance for', videoElementId);
            } catch (error) {
                console.error('Shaka Player: Error destroying player:', error);
            }
        }
    }
};

// Log when the script loads
console.log('Shaka Player Interop loaded');
