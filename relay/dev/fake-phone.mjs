// Scripted fake phone. Usage: node fake-phone.mjs [room=dev-room] [--loop]
import WebSocket from 'ws';

const room = process.argv[2] && !process.argv[2].startsWith('--') ? process.argv[2] : 'dev-room';
const loop = process.argv.includes('--loop');
const ws = new WebSocket(`ws://localhost:8787/ws?room=${room}&role=phone`);

const HZ = 60, DT = 1000 / HZ;
let seq = 0, recenter = 0, alpha0 = 180, batch = [];
// phases: name, seconds, fn(t) -> {a,b,ay}
const phases = [
  ['neutral', 2, () => ({ a: 0, b: 0, ay: 0 })],
  ['yaw 5 deg left', 3, () => ({ a: 5, b: 0, ay: 0 })], // alpha increases counter-clockwise (left)
  ['neutral', 2, () => ({ a: 0, b: 0, ay: 0 })],
  ['pitch 10 deg up', 3, () => ({ a: 0, b: 10, ay: 0 })],
  ['push spike', 1, (t) => ({ a: 0, b: 0, ay: t < 0.06 ? 25 * (t / 0.06) : t < 0.12 ? 25 * (1 - (t - 0.06) / 0.06) : 0 })],
  ['recenter bump', 1, () => ({ a: 0, b: 0, ay: 0 })],
];

function sample(a, b, ay) {
  const al = (((alpha0 + a) % 360) + 360) % 360;
  return [++seq, al.toFixed(3), b.toFixed(3), (0).toFixed(3), (0).toFixed(3), ay.toFixed(3), (0).toFixed(3), recenter].join(',');
}

ws.on('message', (d) => { console.log('[phone] server says:', d.toString()); if (d.toString() === 'kicked') process.exit(0); });
ws.on('error', (e) => { console.log('[phone] error', e.message); process.exit(1); });
ws.on('close', (c) => { console.log('[phone] closed', c); process.exit(0); });

ws.on('open', async () => {
  console.log(`[phone] connected to room ${room}`);
  do {
    for (const [name, secs, fn] of phases) {
      console.log(`[phone] phase: ${name}`);
      if (name === 'recenter bump') recenter++;
      const n = Math.round(secs * HZ);
      for (let i = 0; i < n; i++) {
        const p = fn(i * DT / 1000);
        batch.push(sample(p.a, p.b, p.ay));
        if (batch.length === 2) { ws.send(batch.join(';')); batch = []; }
        await new Promise((r) => setTimeout(r, DT));
      }
    }
  } while (loop);
  console.log('[phone] done');
  ws.close();
});
