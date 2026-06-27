import json
import os
import sys

results_dir = "fuzzing/RestlerResults"
if not os.path.exists(results_dir):
    print("::notice::No RESTler results directory found — fuzzing may not have run")
    sys.exit(0)

bug_count = 0
for root, dirs, files in os.walk(results_dir):
    for f in files:
        if f == "bug_buckets.json":
            path = os.path.join(root, f)
            with open(path) as fh:
                bugs = json.load(fh)
            if bugs:
                bug_count += len(bugs)
                for bug_id, bug in bugs.items():
                    error = bug.get("error", "unknown")
                    print(f"::warning title=RESTler Bug::{bug_id} — {error}")

if bug_count > 0:
    print(f"::error::RESTler found {bug_count} bug(s)")
    sys.exit(1)
else:
    print("::notice::RESTler completed — no bugs detected")
