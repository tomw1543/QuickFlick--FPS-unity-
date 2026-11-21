// Local stand-in for the Cloudflare relay. Contract: PHONE-CONTROLLER-PLAN.md section 4.
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { WebSocketServer } from 'ws';

const here = path.dirname(fileURLToPath(import.meta.url));
const page = path.resolve(here, '../public/controller.html');
const PORT = 8787;
const ROOM_RX = /^[a-z0-9-]{3,40}$/;
const rooms = new Map(); // room -> { phone, game }

const server = http.createServer((req, res) => {
  const u = new URL(req.url, 'http://x');
  if (u.pathname === '/') {
    res.writeHead(200, { 'content-type': 'text/html; charset=utf-8' });
    res.end(fs.readFileSync(page));
  } else if (u.pathname === '/ws') {
    res.writeHead(426); res.end('websocket required');
  } else { res.writeHead(404); res.end('not found'); }
});

const wss = new WebSocketServer({ noServer: true, maxPayload: 1024 * 1024 });

server.on('upgrade', (req, socket, head) => {
  const u = new URL(req.url, 'http://x');
  const room = u.searchParams.get('room');
  const role = u.searchParams.get('role');
  if (u.pathname !== '/ws' || !ROOM_RX.test(room || '') || (role !== 'phone' && role !== 'game')) {
    socket.write('HTTP/1.1 400 Bad Request\r\nConnection: close\r\n\r\n');
    socket.destroy();
    return;
  }
  wss.handleUpgrade(req, socket, head, (ws) => join(ws, room, role));
});

function join(ws, room, role) {
  let r = rooms.get(room);
  if (!r) { r = { phone: null, game: null }; rooms.set(room, r); }
  const old = r[role];
  r[role] = ws;
  if (old) {
    if (role === 'phone') { try { old.send('kicked'); } catch {} old.close(4000, 'kicked'); }
    else old.close(4001, 'replaced');
  }
  log(`${role} joined ${room}`);
  if (role === 'phone' && r.game && r.game.readyState === 1) r.game.send('#phone-connected');
  if (role === 'game' && r.phone && r.phone.readyState === 1) ws.send('#phone-connected');

  ws.on('message', (data, isBinary) => {
    if (role !== 'phone' || isBinary || data.length > 1024) return;
    if (r.phone === ws && r.game && r.game.readyState === 1) r.game.send(data.toString());
  });
  ws.on('close', () => {
    if (r[role] !== ws) return; // already replaced
    r[role] = null;
    log(`${role} left ${room}`);
    if (role === 'phone' && r.game && r.game.readyState === 1) r.game.send('#phone-left');
    if (!r.phone && !r.game) rooms.delete(room);
  });
  ws.on('error', () => {});
}

function log(m) { console.log(`[relay] ${m}`); }
server.listen(PORT, () => log(`listening on http://localhost:${PORT}`));
