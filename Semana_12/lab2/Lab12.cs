using Azure;
using Azure.Storage.Blobs;

const string containerName = "mia-archivos";
const string connectionStringVariable = "AZURE_STORAGE_CONNECTION_STRING";

string? connectionString = Environment.GetEnvironmentVariable(connectionStringVariable);
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine($"No se encontró la variable de entorno {connectionStringVariable}.");
    Console.WriteLine("Configúrala en el entorno desde el que iniciarás la aplicación y vuelve a intentarlo.");
    return;
}

BlobServiceClient blobServiceClient;
try
{
    blobServiceClient = new BlobServiceClient(connectionString);
}
catch (Exception ex) when (ex is ArgumentException or FormatException)
{
    Console.WriteLine("La configuración de conexión no tiene un formato válido.");
    return;
}

BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

bool salir = false;
while (!salir)
{
    MostrarMenu();
    string opcion = LeerTexto("Selecciona una opción: ");

    switch (opcion)
    {
        case "1":
            await SubirArchivoAsync(containerClient);
            break;
        case "2":
            await ListarArchivosAsync(containerClient);
            break;
        case "3":
            await DescargarArchivoAsync(containerClient);
            break;
        case "4":
            await EliminarArchivoAsync(containerClient);
            break;
        case "5":
            salir = true;
            Console.WriteLine("Aplicación finalizada.");
            break;
        default:
            Console.WriteLine("Opción no válida. Selecciona un número del 1 al 5.");
            break;
    }

    if (!salir)
    {
        Console.WriteLine();
        Console.WriteLine("Presiona una tecla para volver al menú...");
        Console.ReadKey(intercept: true);
        Console.WriteLine();
    }
}

static void MostrarMenu()
{
    Console.WriteLine("1. Subir archivo");
    Console.WriteLine("2. Listar archivos");
    Console.WriteLine("3. Descargar archivo");
    Console.WriteLine("4. Eliminar archivo");
    Console.WriteLine("5. Salir");
    Console.WriteLine("=================================");
}

static async Task SubirArchivoAsync(BlobContainerClient containerClient)
{
    string rutaTexto = QuitarComillasExteriores(LeerTexto("Ruta del archivo local: "));
    if (string.IsNullOrWhiteSpace(rutaTexto))
    {
        Console.WriteLine("La ruta no puede estar vacía.");
        return;
    }

    string rutaArchivo;
    try
    {
        rutaArchivo = Path.GetFullPath(rutaTexto);
    }
    catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
    {
        Console.WriteLine("La ruta indicada no es válida.");
        return;
    }

    if (!File.Exists(rutaArchivo))
    {
        Console.WriteLine("El archivo local no existe.");
        return;
    }

    string nombreBlob = Path.GetFileName(rutaArchivo);
    if (string.IsNullOrWhiteSpace(nombreBlob))
    {
        Console.WriteLine("No se pudo obtener un nombre válido para el archivo.");
        return;
    }

    BlobClient blobClient = containerClient.GetBlobClient(nombreBlob);
    try
    {
        if (await blobClient.ExistsAsync() &&
            !PedirConfirmacion($"El blob \"{nombreBlob}\" ya existe. ¿Deseas sobrescribirlo? (s/n): "))
        {
            Console.WriteLine("La operación fue cancelada.");
            return;
        }

        await blobClient.UploadAsync(rutaArchivo, overwrite: true);
        Console.WriteLine($"Archivo subido correctamente como \"{nombreBlob}\".");
    }
    catch (RequestFailedException ex)
    {
        MostrarErrorAzure(ex, "subir el archivo");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("No tienes permisos para leer el archivo local.");
    }
    catch (IOException)
    {
        Console.WriteLine("No se pudo leer el archivo local.");
    }
}

static async Task ListarArchivosAsync(BlobContainerClient containerClient)
{
    try
    {
        bool hayArchivos = false;
        Console.WriteLine();
        Console.WriteLine("{0,-60} {1,15}", "Nombre", "Tamaño (bytes)");
        Console.WriteLine(new string('-', 77));

        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            hayArchivos = true;
            long tamano = blobItem.Properties.ContentLength ?? 0;
            Console.WriteLine("{0,-60} {1,15:N0}", blobItem.Name, tamano);
        }

        if (!hayArchivos)
        {
            Console.WriteLine("El contenedor está vacío.");
        }
    }
    catch (RequestFailedException ex)
    {
        MostrarErrorAzure(ex, "listar los archivos");
    }
}

static async Task DescargarArchivoAsync(BlobContainerClient containerClient)
{
    string nombreBlob = LeerTexto("Nombre exacto del blob: ");
    if (string.IsNullOrWhiteSpace(nombreBlob))
    {
        Console.WriteLine("El nombre del blob no puede estar vacío.");
        return;
    }

    BlobClient blobClient = containerClient.GetBlobClient(nombreBlob);
    try
    {
        if (!await blobClient.ExistsAsync())
        {
            Console.WriteLine("El blob indicado no existe.");
            return;
        }

        string carpetaTexto = QuitarComillasExteriores(LeerTexto("Carpeta local de destino: "));
        if (string.IsNullOrWhiteSpace(carpetaTexto))
        {
            Console.WriteLine("La carpeta de destino no puede estar vacía.");
            return;
        }

        string carpetaDestino;
        try
        {
            carpetaDestino = Path.GetFullPath(carpetaTexto);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            Console.WriteLine("La ruta de destino no es válida.");
            return;
        }

        if (!Directory.Exists(carpetaDestino))
        {
            if (!PedirConfirmacion("La carpeta no existe. ¿Deseas crearla? (s/n): "))
            {
                Console.WriteLine("La operación fue cancelada.");
                return;
            }

            try
            {
                Directory.CreateDirectory(carpetaDestino);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("No tienes permisos para crear la carpeta indicada.");
                return;
            }
            catch (IOException)
            {
                Console.WriteLine("No se pudo crear la carpeta indicada.");
                return;
            }
        }

        string nombreLocal = Path.GetFileName(nombreBlob);
        if (string.IsNullOrWhiteSpace(nombreLocal) || nombreLocal is "." or "..")
        {
            Console.WriteLine("El nombre del blob no permite construir una ruta local segura.");
            return;
        }

        string rutaLocal = Path.GetFullPath(Path.Combine(carpetaDestino, nombreLocal));
        string carpetaConSeparador = Path.EndsInDirectorySeparator(carpetaDestino)
            ? carpetaDestino
            : carpetaDestino + Path.DirectorySeparatorChar;
        if (!rutaLocal.StartsWith(carpetaConSeparador, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("No se pudo construir una ruta local segura.");
            return;
        }

        if (File.Exists(rutaLocal) &&
            !PedirConfirmacion($"El archivo \"{rutaLocal}\" ya existe. ¿Deseas sobrescribirlo? (s/n): "))
        {
            Console.WriteLine("La operación fue cancelada.");
            return;
        }

        await blobClient.DownloadToAsync(rutaLocal);
        Console.WriteLine($"Archivo descargado correctamente en: {rutaLocal}");
    }
    catch (RequestFailedException ex)
    {
        MostrarErrorAzure(ex, "descargar el archivo");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("No tienes permisos para escribir en la carpeta de destino.");
    }
    catch (IOException)
    {
        Console.WriteLine("No se pudo guardar el archivo local.");
    }
}

static async Task EliminarArchivoAsync(BlobContainerClient containerClient)
{
    string nombreBlob = LeerTexto("Nombre exacto del blob: ");
    if (string.IsNullOrWhiteSpace(nombreBlob))
    {
        Console.WriteLine("El nombre del blob no puede estar vacío.");
        return;
    }

    BlobClient blobClient = containerClient.GetBlobClient(nombreBlob);
    try
    {
        if (!await blobClient.ExistsAsync())
        {
            Console.WriteLine("El blob no existe.");
            return;
        }

        if (!PedirConfirmacion($"¿Confirmas eliminar \"{nombreBlob}\"? (s/n): "))
        {
            Console.WriteLine("La operación fue cancelada.");
            return;
        }

        bool eliminado = await blobClient.DeleteIfExistsAsync();
        Console.WriteLine(eliminado ? "El archivo se eliminó correctamente." : "El blob no existía.");
    }
    catch (RequestFailedException ex)
    {
        MostrarErrorAzure(ex, "eliminar el archivo");
    }
}

static string LeerTexto(string mensaje)
{
    Console.Write(mensaje);
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

static string QuitarComillasExteriores(string texto)
{
    return texto.Length >= 2 && texto[0] == '"' && texto[^1] == '"'
        ? texto[1..^1].Trim()
        : texto;
}

static bool PedirConfirmacion(string mensaje)
{
    while (true)
    {
        string respuesta = LeerTexto(mensaje).ToLowerInvariant();
        if (respuesta is "s" or "si" or "sí")
        {
            return true;
        }

        if (respuesta is "n" or "no")
        {
            return false;
        }

        Console.WriteLine("Respuesta no válida. Escribe s o n.");
    }
}

static void MostrarErrorAzure(RequestFailedException ex, string operacion)
{
    string detalle = ex.Status switch
    {
        401 or 403 => "No fue posible autorizar la solicitud.",
        404 => "El contenedor o el blob no fue encontrado.",
        _ => "Azure Storage rechazó o no pudo completar la solicitud."
    };

    Console.WriteLine($"No se pudo {operacion}. {detalle}");
}
