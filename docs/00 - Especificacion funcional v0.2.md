# Especificación funcional v0.2

## Propósito
Crear una base segura y verificable para inspeccionar bibliotecas musicales grandes antes de cualquier cambio.

## Flujo
1. Seleccionar carpeta local o Google Drive montado.
2. Escanear recursivamente.
3. Interpretar nombre.
4. Usar etiquetas si el nombre no alcanza.
5. Mostrar propuesta y advertencias.
6. Guardar/reabrir proyecto.
7. Exportar informe.
8. Respaldar selección con SHA-256.

## Reglas confirmadas
- Conservar exactamente `&`, `feat.`, `vs` y `x`.
- Modo simulación obligatorio.
- Ningún duplicado se resuelve automáticamente.
- Ningún dato de SoundCloud se aprobará automáticamente.
- Sin coincidencia externa se clasificará aparte en versiones posteriores.
