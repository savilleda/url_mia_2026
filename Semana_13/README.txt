Protección de la credencial

La Connection String se almacenó en la variable de entorno `AZURE_STORAGE_CONNECTION_STRING` y la aplicación la lee mediante `Environment.GetEnvironmentVariable`. Así se evita escribirla directamente en el código fuente o incluirla en el repositorio de GitHub. Al ingresarla en PowerShell se utilizó una entrada oculta para no mostrarla en pantalla.

INSTRUCCIONES PARA EJECUTAR DESDE VISUAL STUDIO CODE

1. Abrir una terminal PowerShell en Visual Studio Code y entrar a la carpeta del proyecto:

cd "C:\Users\veget\OneDrive\Escritorio\url_mia_2026\Semana_12\lab2"

2. Ejecutar este bloque:

Read-Host "Copia la Cadena de conexion de Azure, vuelve aqui y pulsa Enter"
$env:AZURE_STORAGE_CONNECTION_STRING = (Get-Clipboard -Raw).Trim()

3. Mientras la terminal espera, ir a Azure Portal > Cuenta de almacenamiento > Claves de acceso > key1 y copiar la Cadena de conexión usando su botón Copiar. No copiar el campo Clave.

4. Regresar a la terminal y presionar Enter SIN PEGAR NADA. El comando obtiene la conexión del portapapeles y la guarda en una variable de entorno sin mostrarla en pantalla.

5. Ejecutar en esa misma terminal:

dotnet run

La credencial no se guarda en el código. Si se cierra la terminal y se abre otra sesión, se debe configurar nuevamente. No incluir la cadena de conexión en el README, las capturas ni el repositorio.
