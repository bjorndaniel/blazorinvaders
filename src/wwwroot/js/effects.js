// Cosmetic effects driven by game events: particles, floating score labels, screen shake
// and a hit flash. They run on real time outside the fixed-step simulation, so they never
// affect gameplay. Used by game.js.

// Must match GameEventType in Enumerations.cs
const EVENT_ALIEN_KILLED = 1, EVENT_PLAYER_HIT = 2, EVENT_MOTHERSHIP_KILLED = 3, EVENT_BUNKER_HIT = 4,
    EVENT_COMBO_UP = 5, EVENT_POWER_UP = 6, EVENT_SHIELD_BLOCKED = 7, EVENT_BOSS_HIT = 8, EVENT_BOSS_KILLED = 9;
const BOSS_DEBRIS = ['#ff4040', '#ff9900', '#ffd700', '#ffffff'];
// Must match Scene.EventStride
const EVENT_STRIDE = 5;

const MAX_PARTICLES = 400;
const LABEL_MS = 600;
const LABEL_FONT = '14px Orbitron';

const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');

let particles = [];
let labels = [];
let shake = { strength: 0, start: 0, duration: 0 };
let flash = { start: 0, duration: 0 };
let clock = 0;      // effect time in ms; stands still while the game is paused
let lastNow = 0;
let textFloor = 0;  // Scene.MinTextSize; 0 off touch screens. Must match Scene.cs
const MUST_READ_SIZE = 18;  // Must match SceneBuilder.MustReadSize

export function reset() {
    particles = [];
    labels = [];
    shake = { strength: 0, start: 0, duration: 0 };
    flash = { start: 0, duration: 0 };
    lastNow = 0;
}

/** Advances effects by real elapsed time and spawns new ones for this frame's events. */
export function update(now, { events, strings, paused, minTextSize }) {
    textFloor = minTextSize || 0;
    // Clamp so a long gap (background tab) doesn't jump effects to their end
    const dt = lastNow && !paused ? Math.min(now - lastNow, 50) : 0;
    lastNow = now;
    clock += dt;
    step(dt / 1000);

    for (let i = 0; i < events.length; i += EVENT_STRIDE) {
        const x = events[i + 1], y = events[i + 2];
        const label = events[i + 3] >= 0 ? strings[events[i + 3]] : null;
        const color = events[i + 4] >= 0 ? strings[events[i + 4]] : '#fff';
        switch (events[i]) {
            case EVENT_ALIEN_KILLED:
                burst(x, y, color, 12, 160);
                if (label) addLabel(label, x, y, color);
                break;
            case EVENT_PLAYER_HIT:
                burst(x, y, color, 30, 220);
                startShake(8, 300);
                flash = { start: clock, duration: 150 };
                break;
            case EVENT_MOTHERSHIP_KILLED:
                burst(x, y, color, 20, 200);
                if (label) addLabel(label, x, y, color);
                startShake(4, 200);
                break;
            case EVENT_BUNKER_HIT:
                burst(x, y, color, 4, 80);
                break;
            case EVENT_COMBO_UP:
                burst(x, y - 20, color, 16, 120);
                if (label) addLabel(label, x, y - 30, color, '20px Orbitron');
                break;
            case EVENT_POWER_UP:
                burst(x, y, color, 16, 140);
                if (label) addLabel(label, x, y - 30, color, '16px Orbitron');
                break;
            case EVENT_SHIELD_BLOCKED:
                burst(x, y - 10, color, 20, 200);
                startShake(3, 150);
                break;
            case EVENT_BOSS_HIT:
                burst(x, y, color, 6, 120);
                startShake(2, 80);
                break;
            case EVENT_BOSS_KILLED:
                for (const debris of BOSS_DEBRIS) burst(x, y, debris, 30, 320);
                if (label) addLabel(label, x, y, color, '24px Orbitron');
                startShake(12, 500);
                break;
        }
    }
}

/** Applies screen shake; call before drawing the frame and pair with endFrame. */
export function beginFrame(ctx) {
    ctx.save();
    const elapsed = clock - shake.start;
    if (shake.strength > 0 && elapsed < shake.duration && !reducedMotion.matches) {
        const amount = shake.strength * (1 - elapsed / shake.duration);
        ctx.translate((Math.random() * 2 - 1) * amount, (Math.random() * 2 - 1) * amount);
    }
}

/** Draws particles and floating labels at the current point in the draw order. */
export function draw(ctx, viewWidth) {
    for (const p of particles) {
        ctx.globalAlpha = Math.max(0, p.life / p.maxLife);
        ctx.fillStyle = p.color;
        ctx.fillRect(p.x, p.y, p.size, p.size);
    }

    ctx.textAlign = 'center';
    for (const l of labels) {
        const t = (clock - l.start) / LABEL_MS;
        ctx.font = l.font;
        ctx.globalAlpha = Math.max(0, 1 - t);
        ctx.fillStyle = l.color;
        let x = l.x;
        if (textFloor > 0) {
            // Enlarged labels must not run off the canvas
            const half = ctx.measureText(l.text).width / 2;
            x = Math.max(half, Math.min(viewWidth - half, x));
        }
        ctx.fillText(l.text, x, l.y - t * 30);
    }
    ctx.globalAlpha = 1;
}

/** Undoes the shake and draws the hit flash over everything. */
export function endFrame(ctx) {
    ctx.restore();
    const elapsed = clock - flash.start;
    if (flash.duration > 0 && elapsed < flash.duration) {
        ctx.fillStyle = `rgba(255,0,0,${(0.35 * (1 - elapsed / flash.duration)).toFixed(3)})`;
        ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);
    }
}

function step(dt) {
    if (dt <= 0) return;
    for (const p of particles) {
        p.x += p.vx * dt;
        p.y += p.vy * dt;
        p.vy += 220 * dt;           // a little gravity
        p.life -= dt * 1000;
    }
    particles = particles.filter(p => p.life > 0);
    labels = labels.filter(l => clock - l.start < LABEL_MS);
}

function burst(x, y, color, count, speed) {
    for (let i = 0; i < count && particles.length < MAX_PARTICLES; i++) {
        const angle = Math.random() * Math.PI * 2;
        const v = speed * (0.4 + Math.random() * 0.6);
        const life = 300 + Math.random() * 300;
        particles.push({
            x, y,
            vx: Math.cos(angle) * v,
            vy: Math.sin(angle) * v,
            life, maxLife: life,
            size: 2 + Math.floor(Math.random() * 2),
            color,
        });
    }
}

function addLabel(text, x, y, color, font = LABEL_FONT) {
    // Labels of MUST_READ_SIZE design size and up (combo 20, boss 24) get the phone floor
    const size = parseInt(font, 10);
    if (textFloor > 0 && size >= MUST_READ_SIZE) font = `${Math.max(size, textFloor)}px Orbitron`;
    labels.push({ text, x, y, color, font, start: clock });
}

function startShake(strength, duration) {
    // A stronger shake replaces a weaker one still running
    const remaining = shake.duration - (clock - shake.start);
    if (remaining > 0 && shake.strength > strength) return;
    shake = { strength, start: clock, duration };
}
