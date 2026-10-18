#!/bin/sh
# Checks for Java 11+ and installs OpenJDK 21 headless JRE if it is missing (Linux).
#
# Usage: ./install-java.sh [--yes]
#   --yes   Non-interactive: install without asking for confirmation.
#
# Exit codes: 0 = Java 11+ available, 1 = installation failed, 2 = cancelled or unsupported system.
set -eu

MIN_MAJOR=11
INSTALL_MAJOR=21
ASSUME_YES=0

for arg in "$@"; do
  case "$arg" in
    --yes|-y) ASSUME_YES=1 ;;
    -h|--help) sed -n '2,7p' "$0"; exit 0 ;;
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

# Returns 0 and prints the path if the java that the validator uses is suitable.
# The validator launches JAVA_HOME/bin/java when it exists and java from PATH otherwise.
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

if [ "$(id -u)" -eq 0 ]; then
  SUDO=""
elif command -v sudo >/dev/null 2>&1; then
  SUDO="sudo"
else
  echo "Root privileges are required and sudo is not available." >&2
  exit 1
fi

if command -v apt-get >/dev/null 2>&1; then
  MANAGER="apt"
elif command -v dnf >/dev/null 2>&1; then
  MANAGER="dnf"
elif command -v yum >/dev/null 2>&1; then
  MANAGER="yum"
elif command -v zypper >/dev/null 2>&1; then
  MANAGER="zypper"
elif command -v apk >/dev/null 2>&1; then
  MANAGER="apk"
else
  echo "No supported package manager found (apt, dnf, yum, zypper, apk). Install Java $MIN_MAJOR+ manually." >&2
  exit 2
fi

if [ "$ASSUME_YES" -ne 1 ]; then
  printf "Install OpenJDK %s JRE using %s? [y/N] " "$INSTALL_MAJOR" "$MANAGER"
  read -r answer || answer=""
  case "$answer" in
    y|Y|yes|YES) ;;
    *) echo "Cancelled."; exit 2 ;;
  esac
fi

case "$MANAGER" in
  apt)
    $SUDO apt-get update
    $SUDO env DEBIAN_FRONTEND=noninteractive apt-get install -y "openjdk-$INSTALL_MAJOR-jre-headless"
    ;;
  dnf)
    $SUDO dnf install -y "java-$INSTALL_MAJOR-openjdk-headless"
    ;;
  yum)
    $SUDO yum install -y "java-$INSTALL_MAJOR-openjdk-headless"
    ;;
  zypper)
    $SUDO zypper --non-interactive install "java-$INSTALL_MAJOR-openjdk-headless"
    ;;
  apk)
    $SUDO apk add --no-cache "openjdk$INSTALL_MAJOR-jre-headless"
    ;;
esac

if found=$(find_java); then
  echo "Java is available at $found."
  exit 0
fi

echo "Installation finished but Java $MIN_MAJOR+ could not be verified. Run 'java -version'." >&2
exit 1
