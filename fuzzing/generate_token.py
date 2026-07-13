import jwt
import time

claims = {
    "sub": "admin",
    "jti": f"fuzzer-{int(time.time())}",
    "role": "Admin",
    "exp": int(time.time()) + 7200,
    "iss": "https://localhost:7000",
    "aud": "https://localhost:7000"
}
key = "CI_Test_Key_Minimum_32_Bytes_Required_For_JWT"
token = jwt.encode(claims, key, algorithm="HS256")
with open("fuzzing/token.txt", "w") as f:
    f.write(token)
