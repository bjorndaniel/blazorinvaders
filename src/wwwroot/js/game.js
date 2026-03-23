window.gameAudio = (() => {
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

window.localStorageInterop = {
    get: (key) => localStorage.getItem(key),
    set: (key, value) => localStorage.setItem(key, value)
};

window.canvasHelper = {
    drawTintedAliens: function (spriteSheet, aliens) {
        const canvas = document.querySelector('#canvasContainer canvas');
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        for (const a of aliens) {
            ctx.drawImage(spriteSheet, a.sx, a.sy, a.sw, a.sh, a.dx, a.dy, a.dw, a.dh);
            if (a.tint) {
                ctx.globalCompositeOperation = 'source-atop';
                ctx.globalAlpha = 0.65;
                ctx.fillStyle = a.tint;
                ctx.fillRect(a.dx, a.dy, a.dw, a.dh);
                ctx.globalAlpha = 1.0;
                ctx.globalCompositeOperation = 'source-over';
            }
        }
    }
};
