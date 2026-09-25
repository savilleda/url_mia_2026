Protección de la credencial

La Connection String se almacenó en la variable de entorno `AZURE_STORAGE_CONNECTION_STRING` y la aplicación la lee mediante `Environment.GetEnvironmentVariable`. Así se evita escribirla directamente en el código fuente o incluirla en el repositorio de GitHub. Al ingresarla en PowerShell se utilizó una entrada oculta para no mostrarla en pantalla.