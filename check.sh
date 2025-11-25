#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# check.sh — ensure this dir’s root contains only .gitignore, README.md, and this script.
#             Any .sln (or other code file) must live inside its own project directory.
#             (Tarkistaa, että juuressa on vain .gitignore, README.md ja tämä skripti.
#              Kaikki .sln- ja muut kooditiedostot on sijoitettava omiin kansioihinsa.)
# Usage: ./check.sh
# ─────────────────────────────────────────────────────────────────────────────

set -euo pipefail
IFS=$'\n\t'

# 1) Allowed root files
allowed=(.gitignore README.md check.sh)

# 2) Collect all regular files in root (including dotfiles)
shopt -s dotglob nullglob
entries=( * )

error=0

# 3) Check each entry
for f in "${entries[@]}"; do
  # skip directories
  [[ -f $f ]] || continue

  # allowed?
  if printf '%s\n' "${allowed[@]}" | grep -qx -- "$f"; then
    continue
  fi

  # special case: .sln in root
  if [[ $f == *.sln ]]; then
    proj="${f%.sln}"
    echo "❌ Found solution file '$f' in root. / Löydettiin sln-tiedosto '$f' juurikansiosta!"
    echo "   Please create a directory named '$proj' and move '$f' (and all related project files) into it."
    echo "   Luo kansio nimeltä '$proj' ja siirrä '$f' (sekä kaikki projektin tiedostot) sinne."
    error=1
    continue
  fi

  # any other unexpected file
  echo "❌ Unexpected file '$f' in root. / Väärä tiedosto '$f' juurikansiossa."
  echo "   Only .gitignore, README.md, and check.sh belong here."
  echo "   Juurihakemistossa saa olla vain .gitignore, README.md ja check.sh."
  error=1
done

# 4) Final status
if (( error )); then
  echo "❌ Root directory has issues. / Juurikansiossa on ongelmia."
  exit 1
else
  echo "✅ Root directory is clean. / Kansio on siisti. :)"
  exit 0
fi
