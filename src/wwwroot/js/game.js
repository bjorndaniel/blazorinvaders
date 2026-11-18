// Canvas renderer, frame loop, audio and storage for the game page.
// Loaded as an ES module from Pages/Index.razor. Each frame asks .NET for a Scene
// (see core/Rendering/Scene.cs) and executes its draw commands.

import * as effects from './effects.js';

// Must match Scene.cs
const OP_SPRITE = 1, OP_RECT = 2, OP_TEXT = 3, OP_DIAMOND = 4, OP_CIRCLE = 5, OP_EFFECTS = 6;
// Must match SoundEffect in Enumerations.cs
const SOUND_SHOOT = 1, SOUND_ALIEN_DEATH = 2, SOUND_PLAYER_DEATH = 4, SOUND_MOTHERSHIP = 8, SOUND_STOP_MARCH = 16,
    SOUND_POWER_UP = 32, SOUND_SHIELD_BLOCK = 64, SOUND_BOSS_HIT = 128, SOUND_BOSS_DEFEATED = 256;
// Held controls must match GameInput in Enumerations.cs; one-shot presses must match Index.razor
const INPUT_LEFT = 1, INPUT_RIGHT = 2, INPUT_FIRE = 4, INPUT_START = 8, INPUT_PAUSE = 16, INPUT_DAILY = 32, INPUT_MUTE = 64;
const HELD_KEYS = { ArrowLeft: INPUT_LEFT, ArrowRight: INPUT_RIGHT, Space: INPUT_FIRE };
const PRESS_KEYS = { KeyP: INPUT_PAUSE, Escape: INPUT_PAUSE, KeyD: INPUT_DAILY };

const SPRITE_SHEET_URL = 'assets/sprites_cleaned.png';
const TINT_ALPHA = 0.65;

let session = 0;
let loopId = 0;
let frameBusy = false;
let dotnet = null;
let ctx = null;
let sheet = null;
let starLayer = null;
const tintedSheets = new Map();

let viewWidth = 0;
let viewHeight = 0;

// Input: keys held, touch buttons held (pointerId -> bit), and presses not yet delivered to .NET
let keysHeld = 0;
const touchesHeld = new Map();
let pressed = 0;
let listeners = [];

/**
 * Starts the frame loop drawing into canvas at a logical size of width x height.
 * Keyboard input comes from container, touch buttons ([data-input] held, [data-press] tapped) from touchRoot.
 * Returns true on touch screens, where the on-screen buttons are shown.
 */
export async function start(dotnetRef, canvas, container, touchRoot, width, height) {
    stop();
    const current = session;
    const touch = matchMedia('(pointer: coarse)').matches;
    listen(container, touchRoot);

    // Render at device resolution so text and pixels stay sharp on high-DPI screens; CSS scales the element
    const dpr = Math.min(window.devicePixelRatio || 1, 3);
    canvas.width = width * dpr;
    canvas.height = height * dpr;
    const context = canvas.getContext('2d');
    context.setTransform(dpr, 0, 0, dpr, 0, 0);
    context.imageSmoothingEnabled = false;

    const image = new Image();
    image.src = SPRITE_SHEET_URL;
    // Make sure Orbitron is ready before the first text draw
    await Promise.all([image.decode(), document.fonts.load('16px Orbitron')]);

    // stop() may have been called (page left) while we were loading
    if (current !== session) return touch;

    dotnet = dotnetRef;
    ctx = context;
    viewWidth = width;
    viewHeight = height;
    sheet = image;
    starLayer = createStarLayer(width, height);
    tintedSheets.clear();
    effects.reset();
    loopId = requestAnimationFrame(frame);
    return touch;
}

export function stop() {
    session++;
    if (loopId) cancelAnimationFrame(loopId);
    loopId = 0;
    frameBusy = false;
    dotnet = null;
    ctx = null;
    unlisten();
    audio.stopMarch();
}

export function getItem(key) {
    try { return localStorage.getItem(key); } catch { return null; }
}

export function setItem(key, value) {
    try { localStorage.setItem(key, value); } catch { /* storage unavailable */ }
}

function frame(timeStamp) {
    loopId = requestAnimationFrame(frame);
    // Skip this frame if .NET is still working on the previous one
    if (frameBusy || !dotnet) return;
    frameBusy = true;
    const current = session;
    // A press shorter than a frame still counts once, even though the key is already up
    let input = keysHeld | pressed;
    for (const bit of touchesHeld.values()) input |= bit;
    pressed = 0;
    dotnet.invokeMethodAsync('Tick', timeStamp, input)
        .then(scene => {
            if (current !== session) return;
            draw(scene);
            playSounds(scene);
        })
        .catch(e => console.error(e))
        .finally(() => { if (current === session) frameBusy = false; });
}

function listen(container, touchRoot) {
    on(container, 'keydown', onKeyDown);
    on(container, 'keyup', onKeyUp);
    // Focus moving away (e.g. to the high score dialog) means keyup events won't arrive
    on(container, 'blur', () => { keysHeld = 0; });
    // Tapping the playfield starts a game; a mouse click only focuses it
    on(container, 'pointerdown', e => { if (e.pointerType !== 'mouse') pressed |= INPUT_START; });

    for (const button of touchRoot.querySelectorAll('[data-input], [data-press]')) {
        on(button, 'pointerdown', onButtonDown);
        on(button, 'pointerup', onButtonUp);
        on(button, 'pointercancel', onButtonUp);
        on(button, 'lostpointercapture', onButtonUp);
        on(button, 'contextmenu', e => e.preventDefault());
    }
}

function on(element, type, handler) {
    element.addEventListener(type, handler);
    listeners.push([element, type, handler]);
}

function unlisten() {
    for (const [element, type, handler] of listeners)
        element.removeEventListener(type, handler);
    listeners = [];
    keysHeld = 0;
    touchesHeld.clear();
    pressed = 0;
}

function onKeyDown(e) {
    const bit = HELD_KEYS[e.code];
    if (bit) {
        e.preventDefault();
        keysHeld |= bit;
        if (!e.repeat) pressed |= bit | (bit === INPUT_FIRE ? INPUT_START : 0);
    } else if (e.code === 'KeyM') {
        // No modifier, so browser shortcuts like Ctrl+M keep working
        if (e.ctrlKey || e.altKey || e.metaKey) return;
        e.preventDefault();
        if (!e.repeat) pressed |= INPUT_MUTE;
    } else if (PRESS_KEYS[e.code]) {
        e.preventDefault();
        if (!e.repeat) pressed |= PRESS_KEYS[e.code];
    }
}

function onKeyUp(e) {
    const bit = HELD_KEYS[e.code];
    if (bit) keysHeld &= ~bit;
}

// Each finger is tracked separately, so moving and firing at the same time works
function onButtonDown(e) {
    e.preventDefault();     // no focus change, text selection or emulated mouse events
    const button = e.currentTarget;
    button.setPointerCapture(e.pointerId);
    button.classList.add('active');
    const bit = Number(button.dataset.input ?? 0);
    if (bit) {
        touchesHeld.set(e.pointerId, bit);
        pressed |= bit | (bit === INPUT_FIRE ? INPUT_START : 0);
    }
    pressed |= Number(button.dataset.press ?? 0);
}

function onButtonUp(e) {
    touchesHeld.delete(e.pointerId);
    e.currentTarget.classList.remove('active');
}

function draw(scene) {
    const { cmds, strings } = scene;
    effects.update(performance.now(), scene);

    ctx.clearRect(0, 0, viewWidth, viewHeight);
    effects.beginFrame(ctx);
    ctx.drawImage(starLayer, 0, 0);

    for (let i = 0; i < cmds.length;) {
        switch (cmds[i]) {
            case OP_SPRITE: {
                const tint = cmds[i + 9];
                const source = tint < 0 ? sheet : tintedSheet(strings[tint]);
                ctx.drawImage(source,
                    cmds[i + 1], cmds[i + 2], cmds[i + 3], cmds[i + 4],
                    cmds[i + 5], cmds[i + 6], cmds[i + 7], cmds[i + 8]);
                i += 10;
                break;
            }
            case OP_RECT:
                ctx.fillStyle = strings[cmds[i + 5]];
                ctx.fillRect(cmds[i + 1], cmds[i + 2], cmds[i + 3], cmds[i + 4]);
                i += 6;
                break;
            case OP_TEXT:
                ctx.font = strings[cmds[i + 4]];
                ctx.fillStyle = strings[cmds[i + 5]];
                ctx.textAlign = cmds[i + 6] ? 'center' : 'left';
                ctx.fillText(strings[cmds[i + 1]], cmds[i + 2], cmds[i + 3]);
                i += 7;
                break;
            case OP_DIAMOND: {
                const x = cmds[i + 1], y = cmds[i + 2], half = cmds[i + 3] / 2;
                ctx.fillStyle = strings[cmds[i + 4]];
                ctx.beginPath();
                ctx.moveTo(x + half, y);
                ctx.lineTo(x + half * 2, y + half);
                ctx.lineTo(x + half, y + half * 2);
                ctx.lineTo(x, y + half);
                ctx.closePath();
                ctx.fill();
                i += 5;
                break;
            }
            case OP_CIRCLE: {
                const lineWidth = cmds[i + 5];
                ctx.beginPath();
                ctx.arc(cmds[i + 1], cmds[i + 2], cmds[i + 3], 0, Math.PI * 2);
                if (lineWidth > 0) {
                    ctx.lineWidth = lineWidth;
                    ctx.strokeStyle = strings[cmds[i + 4]];
                    ctx.stroke();
                } else {
                    ctx.fillStyle = strings[cmds[i + 4]];
                    ctx.fill();
                }
                i += 6;
                break;
            }
            case OP_EFFECTS:
                effects.draw(ctx);
                i += 1;
                break;
            default:
                throw new Error(`Unknown draw op ${cmds[i]} at ${i}`);
        }
    }
    effects.endFrame(ctx);
}

// Tinting the whole sheet once per colour replaces per-sprite compositing every frame
function tintedSheet(color) {
    let tinted = tintedSheets.get(color);
    if (!tinted) {
        tinted = document.createElement('canvas');
        tinted.width = sheet.naturalWidth;
        tinted.height = sheet.naturalHeight;
        const c = tinted.getContext('2d');
        c.drawImage(sheet, 0, 0);
        c.globalCompositeOperation = 'source-atop';
        c.globalAlpha = TINT_ALPHA;
        c.fillStyle = color;
        c.fillRect(0, 0, tinted.width, tinted.height);
        tintedSheets.set(color, tinted);
    }
    return tinted;
}

function createStarLayer(width, height) {
    const layer = document.createElement('canvas');
    layer.width = width;
    layer.height = height;
    const c = layer.getContext('2d');
    for (let i = 0; i < 80; i++) {
        const size = 1 + Math.floor(Math.random() * 2);
        c.fillStyle = `rgba(255,255,255,${(0.3 + Math.random() * 0.7).toFixed(2)})`;
        c.fillRect(Math.random() * width, Math.random() * height, size, size);
    }
    return layer;
}

function playSounds({ sounds, march, muted }) {
    audio.setMuted(muted);
    if (march >= 0) audio.march(march);
    // After the march update so a final alien hit can't restart it on game over
    if (sounds & SOUND_STOP_MARCH) audio.stopMarch();
    if (sounds & SOUND_SHOOT) audio.shoot();
    if (sounds & SOUND_ALIEN_DEATH) audio.alienDeath();
    if (sounds & SOUND_PLAYER_DEATH) audio.playerDeath();
    if (sounds & SOUND_MOTHERSHIP) audio.mothership();
    if (sounds & SOUND_POWER_UP) audio.powerUp();
    if (sounds & SOUND_SHIELD_BLOCK) audio.shieldBlock();
    if (sounds & SOUND_BOSS_HIT) audio.bossHit();
    if (sounds & SOUND_BOSS_DEFEATED) audio.bossDefeated();
}

const audio = (() => {
    let ctx = null;
    let master = null;
    let muted = false;
    let marchInterval = null;
    let marchStep = 0;
    const marchNotes = [160, 130, 105, 85];

    function getCtx() {
        if (!ctx) {
            ctx = new AudioContext();
            // Every sound goes through this gain, so muting silences tones already playing
            master = ctx.createGain();
            master.gain.value = muted ? 0 : 1;
            master.connect(ctx.destination);
        }
        return ctx;
    }

    function playTone(freq, duration, type = 'square', vol = 0.3) {
        if (muted) return;
        const c = getCtx();
        const osc = c.createOscillator();
        const gain = c.createGain();
        osc.connect(gain);
        gain.connect(master);
        osc.type = type;
        osc.frequency.value = freq;
        gain.gain.setValueAtTime(vol, c.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, c.currentTime + duration);
        osc.start();
        osc.stop(c.currentTime + duration);
    }

    return {
        setMuted(value) {
            if (value === muted) return;
            muted = value;
            if (master) master.gain.value = muted ? 0 : 1;
        },
        shoot() { playTone(880, 0.08, 'square', 0.15); },
        alienDeath() {
            if (muted) return;
            const c = getCtx();
            const osc = c.createOscillator();
            const gain = c.createGain();
            osc.connect(gain);
            gain.connect(master);
            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(400, c.currentTime);
            osc.frequency.exponentialRampToValueAtTime(40, c.currentTime + 0.35);
            gain.gain.setValueAtTime(0.3, c.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, c.currentTime + 0.35);
            osc.start();
            osc.stop(c.currentTime + 0.35);
        },
        playerDeath() {
            [800, 500, 300, 150].forEach((freq, i) =>
                setTimeout(() => playTone(freq, 0.18, 'sawtooth', 0.35), i * 140));
        },
        mothership() { playTone(100 + Math.random() * 60, 0.09, 'sine', 0.12); },
        powerUp() {
            [523, 659, 784, 1047].forEach((freq, i) =>
                setTimeout(() => playTone(freq, 0.08, 'square', 0.12), i * 60));
        },
        shieldBlock() { playTone(220, 0.2, 'triangle', 0.3); },
        bossHit() { playTone(90, 0.06, 'square', 0.2); },
        bossDefeated() {
            [600, 450, 300, 200, 120, 60].forEach((freq, i) =>
                setTimeout(() => playTone(freq, 0.25, 'sawtooth', 0.3), i * 120));
        },
        march(alienCount) {
            if (marchInterval) clearInterval(marchInterval);
            const interval = Math.max(80, 480 - (55 - Math.max(0, alienCount)) * 8);
            marchInterval = setInterval(() => {
                playTone(marchNotes[marchStep % 4], 0.055, 'square', 0.18);
                marchStep++;
            }, interval);
        },
        stopMarch() {
            if (marchInterval) { clearInterval(marchInterval); marchInterval = null; }
            marchStep = 0;
        }
    };
})();
