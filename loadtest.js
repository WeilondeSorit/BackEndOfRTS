import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '1m',  target: 200 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.05'],   // допускаем до 5% ошибок (на случай единичных сбоев)
  },
};

const MAIN = 'http://localhost:8080';
const SESSION = 'http://localhost:8082';
const STATS = 'http://localhost:8081';
const PASSWORD_HASH = '9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08'; // sha256('test')

export function setup() {
  // Предварительно регистрируем пользователей user1...user200
  for (let i = 1; i <= 200; i++) {
    http.post(`${MAIN}/auth/register`, JSON.stringify({
      Login: `user${i}`,
      Password: PASSWORD_HASH,
    }), { headers: { 'Content-Type': 'application/json' } });
  }
}

export default function () {
  const vu = __VU;
  const login = `user${vu}`;

  // ---- Логин ----
  const loginRes = http.post(`${MAIN}/auth/login`, JSON.stringify({
    Login: login,
    Password: PASSWORD_HASH,
  }), { headers: { 'Content-Type': 'application/json' } });

  if (!check(loginRes, { 'login status 200': (r) => r.status === 200 })) return;
  const token = loginRes.json().token;       // camelCase
  const playerId = loginRes.json().id;

  // ---- Начало сессии ----
  const startRes = http.post(`${SESSION}/session/start`, JSON.stringify({
    PlayerId: playerId,
  }), { headers: { 'Content-Type': 'application/json' } });
  if (!check(startRes, { 'start session 200': (r) => r.status === 200 })) return;
  const sessionId = startRes.json().sessionId;   // <-- ИСПРАВЛЕНО: sessionId (camelCase)

  // ---- Одна атака (для экономии запросов) ----
  const attackRes = http.post(`${SESSION}/session/${sessionId}/action`, JSON.stringify({
    Type: 'attack',
    Damage: 20,
  }), { headers: { 'Content-Type': 'application/json' } });
  check(attackRes, { 'attack 200': (r) => r.status === 200 });

  sleep(1);

  // ---- Завершение миссии ----
  const endRes = http.post(`${SESSION}/session/${sessionId}/end`, JSON.stringify({
    IsWin: true,
  }), { headers: { 'Content-Type': 'application/json' } });
  check(endRes, { 'end session 200': (r) => r.status === 200 });

  // ---- Лидерборд ----
  const lbRes = http.get(`${STATS}/leaderboard`);
  check(lbRes, { 'leaderboard 200': (r) => r.status === 200 });
}