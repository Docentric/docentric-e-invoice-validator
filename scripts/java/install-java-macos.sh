#!/bin/sh
# Checks for Java 11+ and installs Eclipse Temurin 21 JRE if it is missing (macOS).
#
# Usage: ./install-java.sh [--yes]
#   --yes   Non-interactive: install without asking for confirmation.
#
# Uses Homebrew if available, otherwise downloads a checksum-verified Temurin archive
# into ~/Library/Java/JavaVirtualMachines (no administrator rights required).
#
# Exit codes: 0 = Java 11+ available, 1 = installation failed, 2 = cancelled or unsupported system.
set -eu

MIN_MAJOR=11
TEMURIN_VERSION=21
ASSUME_YES=0

for arg in "$@"; do
  case "$arg" in
    --yes|-y) ASSUME_YES=1 ;;
    -h|--help) sed -n '2,9p' "$0"; exit 0 ;;
    *) echo "Unknown option: $arg" >&2; exit 2 ;;
  esac
done

# Prints the major version of the given java executable, or nothing if it cannot be determined.
java_major() {
  "$1" -version 2>&1 | sed -n 's/.*version "\([0-9]*\)\(\.\([0-9]*\)\)\{0,1\}.*/\1 \3/p' | head -n 1 | {
    read -r major minor || exit 0
    if [ "$major" = "1" ] && [ -n "$minor" ]; then echo "$minor"; else echo "$major"; fi
  }
}

# Prints the path of a suitable java and returns 0, or returns 1 if there is none.
# The validator launches JAVA_HOME/bin/java when it exists and java from PATH otherwise,
# so only that executable is checked.
find_java() {
  if [ -n "${JAVA_HOME:-}" ] && [ -x "$JAVA_HOME/bin/java" ]; then
    candidate="$JAVA_HOME/bin/java"
  else
    candidate=$(command -v java 2>/dev/null || true)
  fi
  [ -n "$candidate" ] && [ -x "$candidate" ] || return 1
  major=$(java_major "$candidate")
  if [ -n "$major" ] && [ "$major" -ge "$MIN_MAJOR" ] 2>/dev/null; then
    echo "$candidate"
    return 0
  fi
  return 1
}

if found=$(find_java); then
  echo "Java found at $found. Nothing to do."
  exit 0
fi

echo "Java $MIN_MAJOR or newer was not found."

if [ "$ASSUME_YES" -ne 1 ]; then
  printf "Install Temurin %s JRE now? [y/N] " "$TEMURIN_VERSION"
  read -r answer || answer=""
  case "$answer" in
    y|Y|yes|YES) ;;
    *) echo "Cancelled."; exit 2 ;;
  esac
fi

install_with_brew() {
  command -v brew >/dev/null 2>&1 || return 1
  echo "Installing Temurin with Homebrew..."
  brew install --cask "temurin@$TEMURIN_VERSION"
}

install_from_archive() {
  case "$(uname -m)" in
    arm64) arch="aarch64" ;;
    x86_64) arch="x64" ;;
    *) echo "Unsupported architecture: $(uname -m)" >&2; return 2 ;;
  esac

  url="https://api.adoptium.net/v3/binary/latest/$TEMURIN_VERSION/ga/mac/$arch/jre/hotspot/normal/eclipse"
  tmp=$(mktemp -d)
  trap 'rm -rf "$tmp"' EXIT

  echo "Downloading Temurin $TEMURIN_VERSION JRE ($arch)..."
  # The API answers with a redirect to the public release asset. Only this first redirect
  # is resolved, so the archive and its .sha256.txt come from the same release asset URL.
  asset_url=$(curl -fsS -o /dev/null -w '%{redirect_url}' "$url")
  if [ -z "$asset_url" ]; then
    echo "Could not resolve the Temurin release asset." >&2
    return 1
  fi
  curl -fsSL -o "$tmp/jre.tar.gz" "$asset_url"
  expected=$(curl -fsSL "$asset_url.sha256.txt" | awk '{print $1}')
  actual=$(shasum -a 256 "$tmp/jre.tar.gz" | awk '{print $1}')
  if [ -z "$expected" ] || [ "$expected" != "$actual" ]; then
    echo "Checksum mismatch for the downloaded JRE." >&2
    return 1
  fi

  target="$HOME/Library/Java/JavaVirtualMachines"
  mkdir -p "$target" "$tmp/extract"
  tar -xzf "$tmp/jre.tar.gz" -C "$tmp/extract"
  jre_dir=$(find "$tmp/extract" -maxdepth 1 -mindepth 1 -type d | head -n 1)
  rm -rf "$target/temurin-$TEMURIN_VERSION.jre"
  mv "$jre_dir" "$target/temurin-$TEMURIN_VERSION.jre"
  echo "Installed to $target/temurin-$TEMURIN_VERSION.jre"
}

if ! install_with_brew; then
  echo "Homebrew is unavailable or failed; using direct download."
  status=0
  install_from_archive || status=$?
  if [ "$status" -ne 0 ]; then
    [ "$status" -eq 2 ] && exit 2
    exit 1
  fi
fi

if found=$(find_java); then
  echo "Java is available at $found."
  exit 0
fi

echo "Installation finished but Java $MIN_MAJOR+ could not be verified. Run 'java -version'." >&2
exit 1
