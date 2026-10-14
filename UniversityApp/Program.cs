// Примеры со слайдов лекции «ADO.NET + PostgreSQL».
// Верхняя часть (подключение) работает всегда.
// Чтобы запустить пример: выделите его строки и нажмите Ctrl+/ — комментарии снимутся.
// Запуск: в терминале  cd UniversityApp  и  dotnet run
// Совет: держите включённым один пример, остальные закомментируйте обратно (Ctrl+/).

using Npgsql;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var connString = "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=university";

using var conn = new NpgsqlConnection(connString);  // создали подключение
conn.Open();  // подключились
Console.WriteLine("Подключились!");

// ===== Слайд 12: Версия сервера =====
// {
//     using var cmd = new NpgsqlCommand("SELECT version()", conn);  // запрос + через какое подключение
//     var version = cmd.ExecuteScalar();  // выполнить и получить одно значение
//     Console.WriteLine(version);  // выведет версию сервера
// }

// ===== Слайд 16: Создаём таблицы =====
// {
//     var createSql = """
//         CREATE TABLE IF NOT EXISTS groups (
//             id SERIAL PRIMARY KEY,
//             name TEXT NOT NULL
//         );
//         CREATE TABLE IF NOT EXISTS students (
//             id SERIAL PRIMARY KEY,
//             first_name TEXT NOT NULL,
//             last_name TEXT NOT NULL,
//             birth_date DATE,
//             group_id INT REFERENCES groups(id)
//         );
//         CREATE TABLE IF NOT EXISTS grades (
//             id SERIAL PRIMARY KEY,
//             student_id INT NOT NULL REFERENCES students(id),
//             subject TEXT NOT NULL,
//             mark INT NOT NULL
//         );
//         """;
//     using var cmd = new NpgsqlCommand(createSql, conn);
//     cmd.ExecuteNonQuery();
// }

// ===== Слайд 17: Удаляем таблицы (начать с чистого листа) =====
// {
//     using var drop = new NpgsqlCommand("DROP TABLE IF EXISTS grades, students, groups", conn);
//     drop.ExecuteNonQuery();
// }

// ===== Слайд 18: Добавляем одну запись =====
// {
//     using var cmd = new NpgsqlCommand("INSERT INTO groups (name) VALUES ('ИВТ-101')", conn);
//     int rows = cmd.ExecuteNonQuery();
//     Console.WriteLine($"Добавлено строк: {rows}");  // Добавлено строк: 1
// }

// ===== Слайд 19: INSERT с параметрами =====
// {
//     string firstName = "Иван";
//     string lastName = "Петров";
//     int groupId = 1;
//
//     using var cmd = new NpgsqlCommand(
//         "INSERT INTO students (first_name, last_name, group_id) VALUES (@first, @last, @group)", conn);
//     cmd.Parameters.AddWithValue("@first", firstName);
//     cmd.Parameters.AddWithValue("@last", lastName);
//     cmd.Parameters.AddWithValue("@group", groupId);
//     cmd.ExecuteNonQuery();
// }

// ===== Слайд 20: Узнаём id новой записи (RETURNING) =====
// {
//     using var cmd = new NpgsqlCommand("INSERT INTO groups (name) VALUES (@name) RETURNING id", conn);
//     cmd.Parameters.AddWithValue("@name", "ИВТ-102");
//
//     int newId = Convert.ToInt32(cmd.ExecuteScalar());
//     Console.WriteLine($"Новая группа получила id = {newId}");
// }

// ===== Слайд 21: Несколько записей в цикле =====
// {
//     string[] groupNames = { "ИВТ-101", "ИВТ-102", "ПИ-201" };
//
//     foreach (var name in groupNames)
//     {
//         using var cmd = new NpgsqlCommand("INSERT INTO groups (name) VALUES (@name)", conn);
//         cmd.Parameters.AddWithValue("@name", name);
//         cmd.ExecuteNonQuery();
//     }
// }

// ===== Слайд 22–23: Заполняем учебную базу =====
// {
//     var seedSql = """
//         INSERT INTO groups (name) VALUES
//             ('ИВТ-101'), ('ИВТ-102'), ('ПИ-201');
//
//         INSERT INTO students (first_name, last_name, birth_date, group_id) VALUES
//             ('Иван', 'Петров', '2005-03-14', 1),
//             ('Мария', 'Иванова', '2005-07-02', 1),
//             ('Алексей', 'Смирнов', '2004-11-20', 1),
//             ('Анна', 'Кузнецова', '2005-01-09', 2),
//             ('Дмитрий', 'Попов', '2004-05-30', 2),
//             ('Елена', 'Васильева', '2005-09-12', 3),
//             ('Сергей', 'Новиков', '2004-12-01', 3),
//             ('Ольга', 'Морозова', NULL, 3);
//         INSERT INTO grades (student_id, subject, mark) VALUES
//             (1, 'Математика', 5), (1, 'Программирование', 4),
//             (2, 'Математика', 5), (2, 'Программирование', 5),
//             (3, 'Математика', 3), (3, 'Программирование', 4),
//             (4, 'Математика', 4), (4, 'Программирование', 3),
//             (5, 'Математика', 3), (5, 'Программирование', 3),
//             (6, 'Математика', 5), (6, 'Программирование', 4),
//             (7, 'Математика', 4), (7, 'Программирование', 5),
//             (8, 'Математика', 2), (8, 'Программирование', 3);
//         """;
//
//     using var cmd = new NpgsqlCommand(seedSql, conn);
//     int rows = cmd.ExecuteNonQuery();
//     Console.WriteLine($"Добавлено строк: {rows}");  // 27
// }

// ===== Слайд 24: UPDATE =====
// {
//     // Мария сменила фамилию
//     using var cmd = new NpgsqlCommand("UPDATE students SET last_name = @last WHERE id = @id", conn);
//     cmd.Parameters.AddWithValue("@last", "Соколова");
//     cmd.Parameters.AddWithValue("@id", 2);
//
//     int rows = cmd.ExecuteNonQuery();
//     if (rows == 0)
//         Console.WriteLine("Студент не найден");
//     else
//         Console.WriteLine("Фамилия изменена");
// }

// ===== Слайд 25: DELETE =====
// {
//     // удаляем оценку студента по одному предмету
//     using var cmd = new NpgsqlCommand(
//         "DELETE FROM grades WHERE student_id = @id AND subject = @subject", conn);
//     cmd.Parameters.AddWithValue("@id", 8);
//     cmd.Parameters.AddWithValue("@subject", "Математика");
//
//     int rows = cmd.ExecuteNonQuery();
//     Console.WriteLine($"Удалено строк: {rows}");  // Удалено строк: 1
// }

// ===== Слайд 26: ExecuteScalar: одно значение =====
// {
//     using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM students", conn);
//     int count = Convert.ToInt32(cmd.ExecuteScalar());
//     Console.WriteLine($"Всего студентов: {count}");  // Всего студентов: 8
// }

// ===== Слайд 27: ExecuteReader: читаем таблицу =====
// {
//     using var cmd = new NpgsqlCommand("SELECT id, first_name, last_name FROM students", conn);
//     using var reader = cmd.ExecuteReader();
//
//     while (reader.Read())
//     {
//         int id = reader.GetInt32(0);  // колонка 0 — id
//         string firstName = reader.GetString(1);  // колонка 1 — first_name
//         string lastName = reader.GetString(2);  // колонка 2 — last_name
//         Console.WriteLine($"{id}. {firstName} {lastName}");
//     }
// }

// ===== Слайд 29: NULL и IsDBNull =====
// {
//     using var cmd = new NpgsqlCommand("SELECT first_name, birth_date FROM students", conn);
//     using var reader = cmd.ExecuteReader();
//
//     while (reader.Read())
//     {
//         string name = reader.GetString(0);
//
//         string birth;
//         if (reader.IsDBNull(1))
//             birth = "не указана";
//         else
//             birth = reader.GetDateTime(1).ToShortDateString();
//
//         Console.WriteLine($"{name}: {birth}");
//     }
// }

// ===== Слайд 30: Строки в объекты Student =====
// {
//     var students = new List<Student>();
//     var sql = """
//         SELECT id, first_name, last_name FROM students
//         """;
//     using var cmd = new NpgsqlCommand(sql, conn);
//     using var reader = cmd.ExecuteReader();
//     while (reader.Read())
//     {
//         students.Add(new Student
//         {
//             Id = reader.GetInt32(0),
//             FirstName = reader.GetString(1),
//             LastName = reader.GetString(2)
//         });
//     }
// }

// ===== Слайд 32: WHERE =====
// {
//     var sql = """
//         SELECT first_name, last_name
//         FROM students
//         WHERE group_id = @group
//         """;
//     using var cmd = new NpgsqlCommand(sql, conn);
//     cmd.Parameters.AddWithValue("@group", 1);
//
//     using var reader = cmd.ExecuteReader();
//     while (reader.Read())
//         Console.WriteLine($"{reader.GetString(0)} {reader.GetString(1)}");
// }

// ===== Слайд 33: ORDER BY + LIMIT =====
// {
//     var sql = """
//         SELECT last_name, first_name
//         FROM students
//         ORDER BY last_name
//         LIMIT @count
//         """;
//     using var cmd = new NpgsqlCommand(sql, conn);
//     cmd.Parameters.AddWithValue("@count", 3);
//
//     using var reader = cmd.ExecuteReader();
//     while (reader.Read())
//         Console.WriteLine($"{reader.GetString(0)} {reader.GetString(1)}");
// }

// ===== Слайд 34: JOIN =====
// {
//     var sql = """
//         SELECT s.first_name, s.last_name, g.name
//         FROM students s
//         JOIN groups g ON g.id = s.group_id
//         ORDER BY s.id
//         """;
//     using var cmd = new NpgsqlCommand(sql, conn);
//
//     using var reader = cmd.ExecuteReader();
//     while (reader.Read())
//     {
//         string name = reader.GetString(0) + " " + reader.GetString(1);
//         Console.WriteLine($"{name} — {reader.GetString(2)}");
//     }
// }

// ===== Слайд 35: GROUP BY =====
// {
//     var sql = """
//         SELECT g.name, COUNT(*)
//         FROM groups g
//         JOIN students s ON s.group_id = g.id
//         GROUP BY g.name
//         ORDER BY g.name
//         """;
//     using var cmd = new NpgsqlCommand(sql, conn);
//
//     using var reader = cmd.ExecuteReader();
//     while (reader.Read())
//         Console.WriteLine($"{reader.GetString(0)}: {reader.GetInt64(1)}");
// }

// ===== Слайд 36: HAVING =====
// {
//     var sql = """
//         SELECT g.name, ROUND(AVG(gr.mark), 2)
//         FROM groups g
//         JOIN students s ON s.group_id = g.id
//         JOIN grades gr ON gr.student_id = s.id
//         GROUP BY g.name
//         HAVING AVG(gr.mark) > @min
//         ORDER BY g.name
//         """;
//     using var cmd = new NpgsqlCommand(sql, conn);
//     cmd.Parameters.AddWithValue("@min", 3.5);
//
//     using var reader = cmd.ExecuteReader();
//     while (reader.Read())
//         Console.WriteLine($"{reader.GetString(0)}: {reader.GetDecimal(1)}");
// }

// ===== Слайд 37: Всё вместе: топ-5 =====
// {
//     var sql = """
//         SELECT s.first_name, s.last_name, ROUND(AVG(gr.mark), 2) AS avg_mark
//         FROM students s
//         JOIN grades gr ON gr.student_id = s.id
//         GROUP BY s.id, s.first_name, s.last_name
//         ORDER BY avg_mark DESC, s.last_name
//         LIMIT @top
//         """;
//     using var cmd = new NpgsqlCommand(sql, conn);
//     cmd.Parameters.AddWithValue("@top", 5);
//
//     using var reader = cmd.ExecuteReader();
//     while (reader.Read())
//     {
//         string name = reader.GetString(0) + " " + reader.GetString(1);
//         Console.WriteLine($"{name} — {reader.GetDecimal(2)}");
//     }
// }

// ===== Слайд 38: ТАК НЕЛЬЗЯ: склейка строк (код только собирает строку, не выполняет) =====
// {
//     Console.Write("Введите фамилию: ");
//     string input = Console.ReadLine() ?? "";  // ничего не ввели — пустая строка
//
//     // ТАК НЕЛЬЗЯ
//     var sql = "SELECT first_name, last_name FROM students WHERE last_name = '" + input + "'";
// }

// ===== Слайд 40: Правильно: параметр =====
// {
//     string input = Console.ReadLine() ?? "";
//     var sql = "SELECT first_name, last_name FROM students WHERE last_name = @last";
//     using var cmd = new NpgsqlCommand(sql, conn);
//     cmd.Parameters.AddWithValue("@last", input);
// }

// ===== Слайд 43: Транзакция: отчисляем студента =====
// {
//     int studentId = 8;
//     using var tx = conn.BeginTransaction();  // начали транзакцию
//     try
//     {
//         using var deleteGrades = new NpgsqlCommand("DELETE FROM grades WHERE student_id = @id", conn, tx);
//         deleteGrades.Parameters.AddWithValue("@id", studentId);
//         deleteGrades.ExecuteNonQuery();
//
//         using var deleteStudent = new NpgsqlCommand("DELETE FROM students WHERE id = @id", conn, tx);
//         deleteStudent.Parameters.AddWithValue("@id", studentId);
//         deleteStudent.ExecuteNonQuery();
//
//         tx.Commit();  // всё прошло — сохраняем
//         Console.WriteLine("Студент отчислен");
//     }
//     catch (Exception ex)
//     {
//         tx.Rollback();  // что-то сломалось — отменяем всё
//         Console.WriteLine("Ошибка, ничего не изменилось: " + ex.Message);
//     }
// }

// Класс со слайда 30. Он нужен примеру со слайда 30, остальным не мешает.
class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
