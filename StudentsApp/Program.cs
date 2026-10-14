// Приложение «Студенты» со слайдов 45–46: всё изученное вместе.
// Перед запуском таблицы должны быть созданы и заполнены (слайды 16 и 22–23 в UniversityApp).
// Запуск: cd StudentsApp  и  dotnet run

using Npgsql;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var connString = "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=university";

using var conn = new NpgsqlConnection(connString);
conn.Open();

while (true)
{
    Console.WriteLine("1 — показать, 2 — добавить, 3 — фамилия, 4 — отчислить, 0 — выход");
    string choice = Console.ReadLine() ?? "";

    try
    {
        if (choice == "1") ShowStudents();
        else if (choice == "2") AddStudent();
        else if (choice == "3") ChangeLastName();
        else if (choice == "4") ExpelStudent();
        else if (choice == "0") break;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Ошибка: " + ex.Message);
    }
}

void ShowStudents()
{
    var sql = """
        SELECT s.id, s.first_name, s.last_name, g.name
        FROM students s
        JOIN groups g ON g.id = s.group_id
        ORDER BY s.id
        """;
    using var cmd = new NpgsqlCommand(sql, conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"{reader.GetInt32(0)}. {reader.GetString(1)} {reader.GetString(2)} ({reader.GetString(3)})");
}

void AddStudent()
{
    Console.Write("Имя: ");
    string firstName = Console.ReadLine() ?? "";
    Console.Write("Фамилия: ");
    string lastName = Console.ReadLine() ?? "";
    Console.Write("id группы: ");
    int groupId = int.Parse(Console.ReadLine() ?? "");

    using var cmd = new NpgsqlCommand(
        "INSERT INTO students (first_name, last_name, group_id) VALUES (@first, @last, @group) RETURNING id", conn);
    cmd.Parameters.AddWithValue("@first", firstName);
    cmd.Parameters.AddWithValue("@last", lastName);
    cmd.Parameters.AddWithValue("@group", groupId);

    int newId = Convert.ToInt32(cmd.ExecuteScalar());
    Console.WriteLine($"Добавлен студент с id = {newId}");
}

void ChangeLastName()
{
    Console.Write("id студента: ");
    int id = int.Parse(Console.ReadLine() ?? "");
    Console.Write("Новая фамилия: ");
    string lastName = Console.ReadLine() ?? "";

    using var cmd = new NpgsqlCommand("UPDATE students SET last_name = @last WHERE id = @id", conn);
    cmd.Parameters.AddWithValue("@last", lastName);
    cmd.Parameters.AddWithValue("@id", id);

    int rows = cmd.ExecuteNonQuery();
    if (rows == 0)
        Console.WriteLine("Студент не найден");
    else
        Console.WriteLine("Фамилия изменена");
}

void ExpelStudent()
{
    Console.Write("id студента: ");
    int id = int.Parse(Console.ReadLine() ?? "");

    using var tx = conn.BeginTransaction();
    try
    {
        using var deleteGrades = new NpgsqlCommand("DELETE FROM grades WHERE student_id = @id", conn, tx);
        deleteGrades.Parameters.AddWithValue("@id", id);
        deleteGrades.ExecuteNonQuery();

        using var deleteStudent = new NpgsqlCommand("DELETE FROM students WHERE id = @id", conn, tx);
        deleteStudent.Parameters.AddWithValue("@id", id);
        int rows = deleteStudent.ExecuteNonQuery();

        tx.Commit();
        if (rows == 0)
            Console.WriteLine("Студент не найден");
        else
            Console.WriteLine("Студент отчислен");
    }
    catch (Exception ex)
    {
        tx.Rollback();
        Console.WriteLine("Ошибка, ничего не изменилось: " + ex.Message);
    }
}
