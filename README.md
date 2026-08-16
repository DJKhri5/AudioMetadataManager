# Audio Metadata Manager v0.4 — Visual Studio 2022 / .NET 8 WPF

Aplicación Windows para analizar, revisar y corregir metadatos de bibliotecas
musicales. Cada modificación productiva pasa por simulación, aprobación del
usuario, copia aislada, respaldo verificado, comprobación posterior y una
segunda confirmación antes de reemplazar el archivo original.

## Funciones incluidas

- Escaneo de MP3, WAV, FLAC, AIF y AIFF.
- Carpetas locales o montadas por Google Drive para escritorio.
- Prioridad: nombre del archivo; etiquetas como alternativa.
- Conservación de `&`, `feat.`, `vs` y `x`.
- Limpieza inicial de prefijos y ruido conocido.
- Lectura técnica: duración, bitrate, sample rate, bits, canales, códec, tamaño y carátula.
- Conflictos nombre/etiqueta y revisión manual.
- Duplicados probables agrupados sin eliminar nada.
- Consulta de Discogs y evaluación de coincidencias con confianza auditable.
- Revisión manual de propuestas y valores introducidos por el usuario cuando
  las fuentes externas no aportan evidencia suficiente.
- Escritura productiva de artista, título, versión, álbum, sello y género.
- Preparación y aplicación por lotes con detención segura ante errores.
- Proyectos `.ammproj` guardables y reanudables.
- Exportación CSV.
- Respaldo, promoción, rollback y verificación SHA-256.

## Abrir en Visual Studio

1. Visual Studio 2022 17.8 o posterior.
2. Carga de trabajo **Desarrollo de escritorio de .NET**.
3. Abrir `src/AudioMetadataManager.slnx`.
4. Restaurar paquetes NuGet y ejecutar.

## Publicar versión portátil

Ejecutar `build-windows.ps1` desde PowerShell con .NET 8 SDK. El resultado queda en `publish/AudioMetadataManager`.

## Límites actuales de la v0.4

El programa todavía no renombra ni mueve archivos y no modifica carátulas.
Beatport, Spotify y SoundCloud permanecen como integraciones futuras. Cuando
no existe evidencia externa suficiente, la aplicación no inventa metadatos:
permite introducir un valor manual que debe aprobarse y verificarse mediante
el mismo flujo productivo protegido.
