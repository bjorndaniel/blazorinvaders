// Canvas renderer, frame loop, audio and storage for the game page.
// Loaded as an ES module from Pages/Index.razor. Each frame asks .NET for a Scene
// (see core/Rendering/Scene.cs) and executes its draw commands.

// Must match Scene.cs
const OP_SPRITE = 1, OP_RECT = 2, OP_TEXT = 3, OP_DIAMOND = 4;
// Must match SoundEffect in Enumerations.cs
const SOUND_SHOOT = 1, SOUND_ALIEN_DEATH = 2, SOUND_PLAYER_DEATH = 4, SOUND_MOTHERSHIP = 8, SOUND_STOP_MARCH = 16;
// Held controls must match GameInput in Enumerations.cs; one-shot presses must match Index.razor
const INPUT_LEFT = 1, INPUT_RIGHT = 2, INPUT_FIRE = 4, INPUT_START = 8, INPUT_PAUSE = 16;
const HELD_KEYS = { ArrowLeft: INPUT_LEFT, ArrowRight: INPUT_RIGHT, Space: INPUT_FIRE };
const PAUSE_KEYS = new Set(['KeyP', 'Escape']);

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

// Input: bits held right now, plus presses not yet delivered to .NET
let held = 0;
let pressed = 0;
let inputElement = null;

export async function start(dotnetRef, canvas, container, width, height) {
    stop();
    const current = session;
    listen(container);

    canvas.width = width;
    canvas.height = height;
    const context = canvas.getContext('2d');

    const image = new Image();
    image.src = SPRITE_SHEET_URL;
    // Make sure Orbitron is ready before the first text draw
    await Promise.all([image.decode(), document.fonts.load('16px Orbitron')]);

    // stop() may have been called (page left) while we were loading
    if (current !== session) return;

    dotnet = dotnetRef;
    ctx = context;
    sheet = image;
    starLayer = createStarLayer(width, height);
    tintedSheets.clear();
    loopId = requestAnimationFrame(frame);
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
    const input = held | pressed;
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

function listen(element) {
    inputElement = element;
    element.addEventListener('keydown', onKeyDown);
    element.addEventListener('keyup', onKeyUp);
    // Focus moving away (e.g. to the high score dialog) means keyup events won't arrive
    element.addEventListener('blur', releaseAll);
}

function unlisten() {
    if (!inputElement) return;
    inputElement.removeEventListener('keydown', onKeyDown);
    inputElement.removeEventListener('keyup', onKeyUp);
    inputElement.removeEventListener('blur', releaseAll);
    inputElement = null;
    releaseAll();
}

function onKeyDown(e) {
    const bit = HELD_KEYS[e.code];
    if (bit) {
        e.preventDefault();
        held |= bit;
        if (!e.repeat) pressed |= bit | (bit === INPUT_FIRE ? INPUT_START : 0);
    } else if (PAUSE_KEYS.has(e.code)) {
        e.preventDefault();
        if (!e.repeat) pressed |= INPUT_PAUSE;
    }
}

function onKeyUp(e) {
    const bit = HELD_KEYS[e.code];
    if (bit) held &= ~bit;
}

function releaseAll() {
    held = 0;
    pressed = 0;
}

function draw({ cmds, strings }) {
    const { width, height } = ctx.canvas;
    ctx.clearRect(0, 0, width, height);
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
            default:
                throw new Error(`Unknown draw op ${cmds[i]} at ${i}`);
        }
    }
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

function playSounds({ sounds, march }) {
    if (march >= 0) audio.march(march);
    // After the march update so a final alien hit can't restart it on game over
    if (sounds & SOUND_STOP_MARCH) audio.stopMarch();
    if (sounds & SOUND_SHOOT) audio.shoot();
    if (sounds & SOUND_ALIEN_DEATH) audio.alienDeath();
    if (sounds & SOUND_PLAYER_DEATH) audio.playerDeath();
    if (sounds & SOUND_MOTHERSHIP) audio.mothership();
}

const audio = (() => {
    let ctx = null;
    let marchInterval = null;
    let marchStep = 0;
    const marchNotes = [160, 130, 105, 85];

    function getCtx() {
        if (!ctx) ctx = new AudioContext();
        return ctx;
    }

    function playTone(freq, duration, type = 'square', vol = 0.3) {
        const c = getCtx();
        const osc = c.createOscillator();
        const gain = c.createGain();
        osc.connect(gain);
        gain.connect(c.destination);
        osc.type = type;
        osc.frequency.value = freq;
        gain.gain.setValueAtTime(vol, c.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, c.currentTime + duration);
        osc.start();
        osc.stop(c.currentTime + duration);
    }

    return {
        shoot() { playTone(880, 0.08, 'square', 0.15); },
        alienDeath() {
            const c = getCtx();
            const osc = c.createOscillator();
            const gain = c.createGain();
            osc.connect(gain);
            gain.connect(c.destination);
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
