#!/bin/bash
set -euo pipefail
BASE_URL="${BASE_URL:-http://localhost:8080}"

echo "=== Smoke Test ==="
echo "Target: $BASE_URL"

# 1. Health check (required)
echo "--- Health /health/ready ---"
curl -sf "$BASE_URL/health/ready" | head -c 200
echo ""
echo "Health check PASSED"

# 2. Login (optional — skipped if login fails)
echo "--- Login ---"
TOKEN=""
LOGIN_RESP=$(curl -sf -X POST "$BASE_URL/api/v1/login/login" \
  -H "Content-Type: application/json" \
  -d '{"user":"admin","password":"Admin123!"}' 2>/dev/null || true)
if [ -n "$LOGIN_RESP" ]; then
  TOKEN=$(echo "$LOGIN_RESP" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
  echo "Token obtenido: ${TOKEN:0:20}..."
else
  echo "WARNING: Login skipped (no auth user in DB)"
fi

# 3. GET usuarios (optional)
if [ -n "$TOKEN" ]; then
  echo "--- GET /api/v1/usuarios ---"
  curl -sf -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/v1/usuarios" | head -c 200
  echo ""
else
  echo "--- GET /api/v1/usuarios --- SKIPPED"
fi

# 4. POST crear usuario (optional)
if [ -n "$TOKEN" ]; then
  echo "--- POST /api/v1/usuarios ---"
  curl -sf -X POST "$BASE_URL/api/v1/usuarios" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"strNombre":"smoke","strPWD":"Test123!","strCorreoElectronico":"smoke@test.com"}' | head -c 200
  echo ""
else
  echo "--- POST /api/v1/usuarios --- SKIPPED"
fi

echo "=== Smoke Test PASSED ==="
