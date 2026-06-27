import jwt
import time

claims = {
    "sub": "admin",
    "jti": f"fuzzer-{int(time.time())}",
    "role": "Admin",
    "exp": int(time.time()) + 7200,
    "iss": "edelmeza.com",
    "aud": "edelmeza.com"
}
key = "01123581321345589144233377610987"
token = jwt.encode(claims, key, algorithm="HS256")
with open("fuzzing/token.txt", "w") as f:
    f.write(f"Bearer {token}")
