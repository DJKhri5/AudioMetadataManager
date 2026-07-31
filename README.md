# Audio Metadata Manager — Visual Studio 2022+ / .NET 8 WPF

Aplicación Windows para gestionar metadatos de una biblioteca de audio local. Crea siempre un respaldo verificado antes de escribir, y puede escribir etiquetas y carátulas reales mediante TagLibSharp cuando el usuario aprueba los cambios propuestos.

Para el detalle completo y actualizado de qué está implementado y qué sigue pendiente, ver `src/AudioMetadataManager.UI/CHANGELOG.md` y `src/AudioMetadataManager.UI/Documentation/Milestones/`.

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
- Búsqueda de metadatos externos en Discogs y Spotify; adquisición de carátula con aprobación manual.

## Abrir en Visual Studio

1. Visual Studio 2022 17.8 o posterior (o Visual Studio Build Tools con el mismo workload).
2. Carga de trabajo **Desarrollo de escritorio de .NET**.
3. Abrir `src/AudioMetadataManager.slnx`.
4. Restaurar paquetes NuGet y ejecutar.

## Publicar versión portátil y generar el instalador

Ejecutar `build-windows.ps1` desde PowerShell con el SDK de .NET 8 instalado. Publica un ejecutable autocontenido de un solo archivo en `publish/AudioMetadataManager`.

Si además está instalado [Inno Setup](https://jrsoftware.org/isinfo.php) (`winget install --id JRSoftware.InnoSetup -e`), el mismo script compila automáticamente un instalador real en `installer/Output/AudioMetadataManager-Setup.exe`, usando el script `installer/AudioMetadataManager.iss`.

## Estado del roadmap

El análisis espectral para detectar MP3 transcodificados todavía no está implementado. SoundCloud y Beatport siguen sin una fuente real (SoundCloud restringe el alta de nuevas apps en su API; Beatport no tiene API pública). Ver el CHANGELOG para el detalle completo.
