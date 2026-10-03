# MIA_AzureBlob — Laboratorio No. 2, Semana 12

## Objetivo

Aplicación de consola en C#/.NET para administrar archivos del contenedor privado
`mia-archivos` en Azure Blob Storage. Permite subir, listar, descargar y eliminar
blobs desde un menú interactivo.

## Tecnologías utilizadas

- C# con .NET 10 (`net10.0`).
- Paquete oficial `Azure.Storage.Blobs`.
- `async`/`await` para las operaciones contra Azure Storage.

## Configuración de Azure

La aplicación usa el contenedor existente `mia-archivos` de la cuenta de
almacenamiento configurada para el laboratorio. No crea recursos ni cambia los
permisos del contenedor.

La Connection String se obtiene de la variable de entorno
`AZURE_STORAGE_CONNECTION_STRING`. Al iniciar, la aplicación también lee el
archivo local `.env` y conserva el valor completo después del primer signo
`=`. Nunca se guarda una credencial en el código, en este README, en
`.env.production` ni en mensajes de consola.

Una variable configurada en Azure Cloud Shell no se transfiere automáticamente a
la computadora local. La aplicación debe iniciarse desde un entorno que tenga
configurada la variable.

### PowerShell

Usa una Connection String obtenida de forma segura; `<CONNECTION_STRING_DE_EJEMPLO>`
es únicamente un marcador y no debe copiarse como credencial real:

```powershell
$env:AZURE_STORAGE_CONNECTION_STRING = "<CONNECTION_STRING_DE_EJEMPLO>"
```

### Bash

```bash
export AZURE_STORAGE_CONNECTION_STRING="<CONNECTION_STRING_DE_EJEMPLO>"
```

La variable debe estar definida en la misma sesión de terminal desde la que se
ejecute la aplicación.

Como alternativa para desarrollo local, copia la plantilla `.env.production` a
`.env` y reemplaza únicamente `TU_CONNECTION_STRING` con tu valor real. El
archivo `.env` está excluido de Git; `.env.production` sí se puede publicar
porque solo contiene la plantilla.

## Arquitectura de la solución

El proyecto es una aplicación pequeña de consola con un único archivo de entrada,
`Lab12.cs`. El programa crea un `BlobServiceClient`, obtiene un
`BlobContainerClient` para `mia-archivos` y crea un `BlobClient` para cada blob
que se va a operar. Cada opción del menú está separada en un método asíncrono.

## Operaciones

1. **Subir archivo:** solicita una ruta local, acepta rutas entre comillas, usa
   `Path.GetFileName` como nombre del blob y confirma antes de sobrescribir.
2. **Listar archivos:** usa `GetBlobsAsync` y muestra el nombre y tamaño en bytes.
3. **Descargar archivo:** comprueba `ExistsAsync`, solicita una carpeta, confirma
   su creación y la sobrescritura de archivos. Construye una ruta local segura
   usando solo el nombre del blob.
4. **Eliminar archivo:** comprueba la existencia, solicita confirmación explícita
   con `s/n` y usa `DeleteIfExistsAsync`.

## Manejo de errores

Se validan entradas vacías, rutas inválidas, archivos inexistentes, permisos
locales y respuestas de Azure. Los errores de Azure se controlan mediante
`RequestFailedException` y se muestran mensajes en español sin exponer secretos.
Después de un error de una operación, la aplicación vuelve al menú cuando es
posible. Si falta la variable de entorno, explica cómo configurarla y termina
sin mostrar una excepción sin controlar.

## Restaurar dependencias y ejecutar

Desde la carpeta `Semana_12/lab2`:

```powershell
dotnet restore
dotnet build
dotnet run
```

En Bash se pueden usar los mismos comandos. Configura primero
`AZURE_STORAGE_CONNECTION_STRING` en la sesión correspondiente y no ejecutes
operaciones reales automáticamente: las operaciones ocurren únicamente cuando
se elige una opción del menú.

Para comprobar la conexión, inicia el programa, selecciona `2. Listar archivos`
y verifica que se muestre el contenido del contenedor `mia-archivos` o el
mensaje de que está vacío. Un error de autorización o de red indica que la
Connection String o el acceso al contenedor deben revisarse.

## Pruebas manuales y capturas requeridas

Realiza las pruebas en este orden y captura evidencia de cada resultado:

1. Configuración: iniciar sin la variable y comprobar el mensaje claro; después
   configurar la variable sin mostrar su valor.
2. **Subir:** subir un archivo de prueba, probar una ruta inexistente y probar la
   confirmación de sobrescritura.
3. **Listar:** comprobar que se muestra el nombre y el tamaño, y probar un
   contenedor vacío si está disponible.
4. **Descargar:** descargar el blob, probar una carpeta nueva y confirmar que el
   archivo local quedó guardado; probar cancelar la sobrescritura.
5. **Eliminar:** cancelar con `n` y después confirmar con `s`; comprobar que el
   blob ya no aparece al listar.
6. Validación: introducir una opción inválida y entradas vacías.

Las capturas deben mostrar únicamente mensajes y resultados de la aplicación.
No incluyas Connection Strings, claves, tokens ni otros secretos en las
capturas ni en la entrega.
