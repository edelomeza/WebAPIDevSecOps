#!/bin/bash
set -euo pipefail
BASE_URL="${BASE_URL:-http://localhost:8080}"

echo "=== Smoke Test ==="
echo "Target: $BASE_URL"

# 1. Health check
echo "--- Health /health/ready ---"
curl -sf "$BASE_URL/health/ready" | head -c 200
echo ""

# 2. Login
echo "--- Login ---"
LOGIN_RESP=$(curl -sf -X POST "$BASE_URL/api/v1/login/login" \
  -H "Content-Type: application/json" \
  -d '{"user":"admin","password":"Admin123!"}')
TOKEN=$(echo "$LOGIN_RESP" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo "Token obtenido: ${TOKEN:0:20}..."

# 3. GET usuarios
echo "--- GET /api/v1/usuarios ---"
curl -sf -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/v1/usuarios" | head -c 200
echo ""

# 4. POST crear usuario
echo "--- POST /api/v1/usuarios ---"
curl -sf -X POST "$BASE_URL/api/v1/usuarios" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"strNombre":"smoke","strPWD":"Test123!","strCorreoElectronico":"smoke@test.com"}' | head -c 200
echo ""

echo "=== Smoke Test PASSED ==="
