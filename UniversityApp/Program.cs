// Практика к лекции «ADO.NET + PostgreSQL».
// Порядок работы — в файле PRACTICE.md.
// Запуск: в терминале  dotnet run  (из папки UniversityApp)

using Npgsql;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var connString = "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=university";

using var conn = new NpgsqlConnection(connString);  // создали подключение
conn.Open();  // подключились
Console.WriteLine("Подключились!");

// ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
// Вставьте сюда пример. Перед вставкой нового удалите предыдущий.



// ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑


// Класс для шага 17 (слайд 30). Не удаляйте его.
class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
