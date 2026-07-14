# Audio Metadata Manager v0.2 — Visual Studio 2022 / .NET 8 WPF

Aplicación Windows en **Modo Simulación**. Esta versión analiza la biblioteca y puede crear respaldos verificados, pero no renombra, mueve ni escribe etiquetas.

## Funciones incluidas

- Escaneo de MP3, WAV, FLAC, AIF y AIFF.
- Carpetas locales o montadas por Google Drive para escritorio.
- Prioridad: nombre del archivo; etiquetas como alternativa.
- Conservación de `&`, `feat.`, `vs` y `x`.
- Limpieza inicial de prefijos y ruido conocido.
- Lectura técnica: duración, bitrate, sample rate, bits, canales, códec, tamaño y carátula.
- Conflictos nombre/etiqueta y revisión manual.
- Duplicados probables agrupados sin eliminar nada.
- Proyectos `.ammproj` guardables y reanudables.
- Exportación CSV.
- Respaldo en `_Respaldo Audio` con verificación SHA-256 y manifiesto JSON.

## Abrir en Visual Studio

1. Visual Studio 2022 17.8 o posterior.
2. Carga de trabajo **Desarrollo de escritorio de .NET**.
3. Abrir `AudioMetadataManager.sln`.
4. Restaurar paquetes NuGet y ejecutar.

## Publicar versión portátil

Ejecutar `build-windows.ps1` desde PowerShell con .NET 8 SDK. El resultado queda en `publish/AudioMetadataManager`.

## Límite deliberado de la v0.2

El análisis espectral para detectar MP3 transcodificados y las búsquedas Discogs/Beatport/Spotify/SoundCloud no están activas aún. Se muestran como requisitos del roadmap y no se simulan con datos inventados.
