using System.Text.Json;

// Rutas de los archivos de entrada y salida
const string rutaCsv = "estudiantes.csv";
const string rutaJson = "estudiantes.json";

if (!File.Exists(rutaCsv))
{
    throw new FileNotFoundException("No se encontró el archivo estudiantes.csv en la carpeta del proyecto.");
}

// Se van a guardar todos los estudiantes que se encuentran en el CSV
List<Estudiante> estudiantes = new();

// Leer todas las líneas del archivo CSV
string[] lineas = File.ReadAllLines(rutaCsv);

// Omitir la primera línea (encabezado)
foreach (string linea in lineas.Skip(1))
{
    if (string.IsNullOrWhiteSpace(linea))
    {
        continue;
    }

    // Split divide la línea usando la coma como separador
    string[] partes = linea.Split(',');
    if (partes.Length != 3)
    {
        throw new FormatException($"Línea CSV inválida: '{linea}'.");
    }

    // Crear objeto Estudiante con los datos del CSV
    Estudiante estudiante = new()
    {
        Id = int.Parse(partes[0].Trim()),
        Nombre = partes[1].Trim(),
        Carrera = partes[2].Trim()
    };

    // Agregar estudiante a la lista
    estudiantes.Add(estudiante);
}

// Mostrar todos los estudiantes en consola
foreach (Estudiante estudiante in estudiantes)
{
    Console.WriteLine($"{estudiante.Id} - {estudiante.Nombre} - {estudiante.Carrera}");
}

// Convertir la lista a formato JSON
string json = JsonSerializer.Serialize(estudiantes, new JsonSerializerOptions
{
    WriteIndented = true
});

// Guardar JSON en archivo y confirmar en consola
File.WriteAllText(rutaJson, json);
Console.WriteLine("Archivo estudiantes.json creado correctamente.");
