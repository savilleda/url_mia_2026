using System.Text.Json;

// Ruta de los archivos CSV y JSON
const string rutaCsv = "estudiantes.csv";
const string rutaJson = "estudiantes.json";
// Según la documentación de microsoft, el !File.Exists() "Determina si el archivo especificado existe"
if (!File.Exists(rutaCsv))
{
    //Si el archivo no existe, se lanza una excepción con un mensaje de error
    throw new FileNotFoundException("No se existe el archivo estudiantes.csv");
}

// Es donde se guarda todo lo que se lea del CSV
List<Estudiante> estudiantes = new List<Estudiante>();

// El file.ReadAllLines se utiliza para leer un archivo completo y
//Devolver un arreglo en el cual cada posición es una línea del archivo
string[] lineas = File.ReadAllLines(rutaCsv);

// Según la documentación de microsoft, el skip 
// "Omite un número especificado de elementos en una secuencia y, a continuación, devuelve los elementos restantes"
foreach (string linea in lineas.Skip(1))
{
    // Si la línea está vacía, se omite (Son validaciones que ahorita no me sirven pero para una futura tarea si jeje)
    if (string.IsNullOrWhiteSpace(linea))
    {
        continue;
    }

    // Según la documentación de microsoft,
    //El split "divide una cadena de texto en un arreglo de subcadenas (string[]) usando un delimitador específico"
    string[] partes = linea.Split(',');
    // Nos ayuda a verificar que se divida en tres partes exactas, que es lo que pide la tarea
    if (partes.Length != 3)
    {
        throw new FormatException("Linea CSV invalida: '{linea}'");
    }

    // Se crea un objeto Estudiante
    Estudiante estudiante = new()
    {
        //El id se convierte en un entero
        Id = int.Parse(partes[0].Trim()),
        //El nombre junto a la carrera se guardan como string
        Nombre = partes[1].Trim(),
        Carrera = partes[2].Trim()
        //Como los indices de los arreglos empiezan en 0, por eso el 0 es el id, el 1 es el nombre y el 2 es la carrera
    };

    // Se agrega un estudiante a la lista de estudiantes
    estudiantes.Add(estudiante);
}

// Mostrar todos los estudiantes en consola
foreach (Estudiante estudiante in estudiantes)
{
    Console.WriteLine($"{estudiante.Id} - {estudiante.Nombre} - {estudiante.Carrera}");
}
// Convertir la lista a formato JSON
//Según la documentación de microsoft, 
// el JsonSerializer " convierte objetos de C# en formato JSON y viceversa"
string json = JsonSerializer.Serialize(estudiantes, new JsonSerializerOptions
{
    //El writeIndented "Indica si el JSON debe escribirse con sangría y saltos de línea para mejorar la legibilidad"
    WriteIndented = true
});

// El file.WriteAllText se utiliza para crear un archivo y escribir en el, si el archivo ya existe, se sobrescribe
File.WriteAllText(rutaJson, json);
//Por último se confima que el archivo se creo
Console.WriteLine("El archivo estudiantes.json ha sido creado :D");
