# Clinic web app

React + Vite + TypeScript client for the Dental Clinic Management System API.

```bash
cd frontend
copy .env.example .env
npm install
npm run dev
```

Dev server: `http://localhost:5173`  
API: `VITE_API_BASE_URL` (default `http://localhost:5062`)

```bash
npm run build
```

Attachments always download via `GET /api/attachments/{id}/download` with the signed-in JWT. Do not use public `/uploads/` URLs.
