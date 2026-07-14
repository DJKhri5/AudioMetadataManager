# Seguridad y respaldo

- No se modifica audio en v0.2.
- Respaldo dentro de `_Respaldo Audio/<fecha>/Originales`.
- Se conserva la ruta relativa.
- Cada copia se compara con SHA-256.
- Si falla una verificación, se detiene la operación.
- Se crea `manifest.json` con rutas, tamaño, hash y fecha original.
