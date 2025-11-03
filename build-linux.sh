#!/usr/bin/env bash
set -euo pipefail

APP_NAME="HedgeConfig"
BIN_NAME="HedgeConfig"
CSProj="HedgeConfig/HedgeConfig.Avalonia.csproj"
VERSION="${VERSION:-1.0.0}"
ARCH="${ARCH:-x86_64}"
RID="${RID:-linux-x64}"
PUBLISH_DIR="./publish/${RID}"

echo "Publishing ${CSProj} for ${RID} (self-contained)…"
dotnet publish "${CSProj}" \
  -c Release \
  -r "${RID}" \
  --self-contained true \
  -p:PublishSingleFile=false \
  -p:PublishReadyToRun=true \
  -o "${PUBLISH_DIR}"

APPDIR="${APP_NAME}_Linux"
rm -rf "${APPDIR}"
mkdir -p "${APPDIR}/usr/bin" \
         "${APPDIR}/usr/share/applications" \
         "${APPDIR}/usr/share/icons/hicolor/256x256/apps"

cp -r "${PUBLISH_DIR}/"* "${APPDIR}/usr/bin/"

chmod +x "${APPDIR}/usr/bin/${BIN_NAME}"

cat > "${APPDIR}/${APP_NAME}.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=${APP_NAME}
Comment=Hedgehog Engine 2 Mod Configuration Tool
Exec=${BIN_NAME} %U
Icon=${APP_NAME}
Categories=Utility;Development;
Terminal=false
EOF

install -D "${APPDIR}/${APP_NAME}.desktop" "${APPDIR}/usr/share/applications/${APP_NAME}.desktop"

ICON_SRC="HedgeConfig.Avalonia/Assets/Untitled.ico"
ICON_DST="${APPDIR}/usr/share/icons/hicolor/256x256/apps/${APP_NAME}.png"
if [[ -f "${ICON_SRC}" ]]; then
  if command -v convert >/dev/null 2>&1; then
    convert "${ICON_SRC}[0]" -resize 256x256 "${ICON_DST}"
  else
    echo "No ImageMagick 'convert' found; creating placeholder icon."
    convert_not_found=true
    printf '\211PNG\r\n\032\n' > "${ICON_DST}" || touch "${ICON_DST}"
  fi
else
  echo "Icon not found at ${ICON_SRC}; creating placeholder."
  printf '\211PNG\r\n\032\n' > "${ICON_DST}" || touch "${ICON_DST}"
fi
cp "${ICON_DST}" "${APPDIR}/${APP_NAME}.png" || true

cat > "${APPDIR}/AppRun" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
SELF="$(readlink -f "$0")"
HERE="${SELF%/*}"
export PATH="${HERE}/usr/bin:${PATH}"
exec "${HERE}/usr/bin/HedgeConfig.Avalonia" "$@"
EOF
chmod +x "${APPDIR}/AppRun"

APPIMAGETOOL="./appimagetool-${ARCH}.AppImage"
if [[ ! -x "${APPIMAGETOOL}" ]]; then
  echo "Downloading appimagetool for ${ARCH}…"
  url_base="https://github.com/AppImage/AppImageKit/releases/download/continuous"
  curl -L -o "${APPIMAGETOOL}" "${url_base}/appimagetool-${ARCH}.AppImage"
  chmod +x "${APPIMAGETOOL}"
fi

OUT="${APP_NAME}-${VERSION}-${ARCH}.AppImage"
echo "Building ${OUT}…"
ARCH="${ARCH}" "${APPIMAGETOOL}" "${APPDIR}" "${OUT}"

echo "Done: ${OUT}"
