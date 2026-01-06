/**
 * Browser Console Capture Module
 * Intercepts browser console messages and sends them to the server for logging.
 */

let dotNetHelper = null;

/**
 * Initializes the console capture by storing the .NET interop reference.
 * @param {object} helper - The DotNetObjectReference for callbacks.
 */
export function initialize(helper) {
    dotNetHelper = helper;

    // Store original console methods
    const originalLog = console.log;
    const originalWarn = console.warn;
    const originalError = console.error;
    const originalInfo = console.info;
    const originalDebug = console.debug;

    // Override console.log
    console.log = function (...args) {
        originalLog.apply(console, args);
        sendToServer('Information', args);
    };

    // Override console.warn
    console.warn = function (...args) {
        originalWarn.apply(console, args);
        sendToServer('Warning', args);
    };

    // Override console.error
    console.error = function (...args) {
        originalError.apply(console, args);
        sendToServer('Error', args);
    };

    // Override console.info
    console.info = function (...args) {
        originalInfo.apply(console, args);
        sendToServer('Information', args);
    };

    // Override console.debug
    console.debug = function (...args) {
        originalDebug.apply(console, args);
        sendToServer('Debug', args);
    };

    // Capture unhandled errors
    window.addEventListener('error', function (event) {
        sendToServer('Error', [`Unhandled error: ${event.message} at ${event.filename}:${event.lineno}:${event.colno}`]);
    });

    // Capture unhandled promise rejections
    window.addEventListener('unhandledrejection', function (event) {
        sendToServer('Error', [`Unhandled promise rejection: ${event.reason}`]);
    });

    console.log('[BrowserConsoleCapture] Initialized - console messages will be sent to server.');
}

/**
 * Formats arguments for logging.
 * @param {Array} args - The console arguments.
 * @returns {string} Formatted message.
 */
function formatMessage(args) {
    return args.map(arg => {
        if (arg === null) return 'null';
        if (arg === undefined) return 'undefined';
        if (typeof arg === 'object') {
            try {
                return JSON.stringify(arg, null, 0);
            } catch {
                return String(arg);
            }
        }
        return String(arg);
    }).join(' ');
}

/**
 * Sends the log message to the server.
 * @param {string} level - Log level (Information, Warning, Error, Debug).
 * @param {Array} args - The console arguments.
 */
function sendToServer(level, args) {
    if (!dotNetHelper) return;

    try {
        const message = formatMessage(args);
        // Fire and forget - don't await to avoid blocking console
        dotNetHelper.invokeMethodAsync('OnBrowserConsoleMessage', level, message);
    } catch {
        // Silently ignore errors to prevent infinite loops
    }
}

/**
 * Disposes the console capture.
 */
export function dispose() {
    dotNetHelper = null;
}
