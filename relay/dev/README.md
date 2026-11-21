# relay/dev

Local stand-in for Tom's Cloudflare relay (contract: `PHONE-CONTROLLER-PLAN.md` section 4). For testing the phone controller without deploying.

1. `npm install` (in `relay/dev`)
2. `node relay-stub.mjs` serves http://localhost:8787 (and `relay/public/controller.html` at `GET /`)
3. `node fake-phone.mjs [room] [--loop]` simulates a phone (default room: `dev-room`)

Unity `.env` lines:

    PHONE_RELAY_URL=http://localhost:8787
    PHONE_ROOM=dev-room
