# Практика: проходим лекцию шаг за шагом

Вы пройдёте весь путь: создадите таблицы, заполните их, поменяете данные и выполните все запросы со слайдов. Номера в скобках — номера слайдов презентации.

## Как устроена работа

На экране три вещи:

1. **Этот файл.** Отсюда копируете код. Копируйте именно отсюда, а не со слайдов: при копировании из презентации могут прилипнуть невидимые символы, и SQL сломается.
2. **Файл `UniversityApp/Program.cs`.** В нём уже есть подключение к базе и место с пометкой `Вставьте сюда пример`.
3. **Два терминала внизу.** В первом запускаете программу, во втором смотрите, что лежит в базе.

### Подготовьте терминалы (один раз)

Откройте терминал: меню ☰ → **Terminal → New Terminal**. Выполните в нём:

```
cd UniversityApp
```

Нажмите **+** в панели терминала, чтобы открыть второй, и выполните в нём:

```
psql
```

Появится приглашение `university=#`: вы подключены к базе. Здесь можно писать любой SQL, как в pgAdmin. Точка с запятой в конце обязательна. Выйти — `\q`.

### Цикл для каждого шага

1. В `Program.cs` **удалите** пример, который вставляли в прошлый раз (всё между пометками).
2. **Вставьте** новый пример из этого файла.
3. В первом терминале запустите: `dotnet run`. Первый запуск идёт 10–20 секунд, дальше быстрее.
4. Сверьте вывод программы с тем, что написано в шаге.
5. Во втором терминале выполните проверочный запрос и убедитесь, что в базе произошло то, что ожидалось.

Прошлый пример удаляем, потому что при каждом `dotnet run` выполняется всё, что есть в файле. Если оставить старые примеры, они выполнятся снова, например добавят ту же группу второй раз. А всё, что программа уже сделала с базой, в базе остаётся: удалять код можно смело.

Если в выводе ошибка, прочитайте её текст целиком: обычно в нём сказано, что не так. Частые причины — пропущенная скобка `}` или пример, вставленный не между пометками.

---

## Этап 1. Подключение

### Шаг 1. Версия сервера (12)

```csharp
{
    using var cmd = new NpgsqlCommand("SELECT version()", conn);  // запрос + через какое подключение
    var version = cmd.ExecuteScalar();  // выполнить и получить одно значение
    Console.WriteLine(version);  // выведет версию сервера
}
```

**Вывод программы:**
```
Подключились!
PostgreSQL 17.… on x86_64-pc-linux-gnu, compiled by gcc …
```

Строку `Подключились!` печатает заготовка, она будет в начале вывода на каждом шаге.

---

## Этап 2. Таблицы

### Шаг 2. Создаём таблицы (16)

```csharp
{
    var createSql = """
        CREATE TABLE IF NOT EXISTS groups (
            id SERIAL PRIMARY KEY,
            name TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS students (
            id SERIAL PRIMARY KEY,
            first_name TEXT NOT NULL,
            last_name TEXT NOT NULL,
            birth_date DATE,
            group_id INT REFERENCES groups(id)
        );
        CREATE TABLE IF NOT EXISTS grades (
            id SERIAL PRIMARY KEY,
            student_id INT NOT NULL REFERENCES students(id),
            subject TEXT NOT NULL,
            mark INT NOT NULL
        );
        """;
    using var cmd = new NpgsqlCommand(createSql, conn);
    cmd.ExecuteNonQuery();
}
```

**Вывод программы:** только `Подключились!`. Команда CREATE ничего не печатает.

**Проверка в psql:**
```sql
\dt
```
Должны появиться три таблицы: `grades`, `groups`, `students`.

Посмотреть, из каких колонок состоит таблица:
```sql
\d students
```

### Шаг 3. Удаляем таблицы (17)

```csharp
{
    using var drop = new NpgsqlCommand("DROP TABLE IF EXISTS grades, students, groups", conn);
    drop.ExecuteNonQuery();
}
```

**Проверка в psql:** `\dt` — список таблиц пуст.

### Шаг 4. Создаём таблицы ещё раз

Вставьте снова пример из шага 2 и запустите. Проверьте `\dt`: три таблицы на месте. Дальше нужны именно они.

---

## Этап 3. Добавляем записи

### Шаг 5. Одна запись (18)

```csharp
{
    using var cmd = new NpgsqlCommand("INSERT INTO groups (name) VALUES ('ИВТ-101')", conn);
    int rows = cmd.ExecuteNonQuery();
    Console.WriteLine($"Добавлено строк: {rows}");  // Добавлено строк: 1
}
```

**Вывод программы:**
```
Добавлено строк: 1
```

**Проверка в psql:**
```sql
SELECT * FROM groups;
```
```
 id |  name
----+---------
  1 | ИВТ-101
```

### Шаг 6. Запись с параметрами (19)

```csharp
{
    string firstName = "Иван";
    string lastName = "Петров";
    int groupId = 1;

    using var cmd = new NpgsqlCommand(
        "INSERT INTO students (first_name, last_name, group_id) VALUES (@first, @last, @group)", conn);
    cmd.Parameters.AddWithValue("@first", firstName);
    cmd.Parameters.AddWithValue("@last", lastName);
    cmd.Parameters.AddWithValue("@group", groupId);
    cmd.ExecuteNonQuery();
}
```

**Проверка в psql:**
```sql
SELECT * FROM students;
```
```
 id | first_name | last_name | birth_date | group_id
----+------------+-----------+------------+----------
  1 | Иван       | Петров    |            |        1
```
Пустая `birth_date` — это NULL: дату рождения мы не передавали.

### Шаг 7. Узнаём id новой записи (20)

```csharp
{
    using var cmd = new NpgsqlCommand("INSERT INTO groups (name) VALUES (@name) RETURNING id", conn);
    cmd.Parameters.AddWithValue("@name", "ИВТ-102");

    int newId = Convert.ToInt32(cmd.ExecuteScalar());
    Console.WriteLine($"Новая группа получила id = {newId}");
}
```

**Вывод программы:**
```
Новая группа получила id = 2
```

### Шаг 8. Несколько записей в цикле (21)

```csharp
{
    string[] groupNames = { "ИВТ-101", "ИВТ-102", "ПИ-201" };

    foreach (var name in groupNames)
    {
        using var cmd = new NpgsqlCommand("INSERT INTO groups (name) VALUES (@name)", conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.ExecuteNonQuery();
    }
}
```

**Проверка в psql:** `SELECT * FROM groups;` — теперь 5 групп с id от 1 до 5. Группы ИВТ-101 и ИВТ-102 повторяются: их добавляли и в прошлых шагах.

**Вопрос:** что будет, если запустить этот пример ещё раз? Проверьте.

---

## Этап 4. Учебная база

Сейчас в таблицах накопились тестовые записи с повторами. Начнём с чистого листа и заполним базу данными, на которых построены все дальнейшие примеры.

### Шаг 9. Пересоздаём таблицы

1. Пример из шага 3 (удаление таблиц), запустить.
2. Пример из шага 2 (создание таблиц), запустить.

### Шаг 10. Заполняем базу (22–23)

```csharp
{
    var seedSql = """
        INSERT INTO groups (name) VALUES
            ('ИВТ-101'), ('ИВТ-102'), ('ПИ-201');

        INSERT INTO students (first_name, last_name, birth_date, group_id) VALUES
            ('Иван', 'Петров', '2005-03-14', 1),
            ('Мария', 'Иванова', '2005-07-02', 1),
            ('Алексей', 'Смирнов', '2004-11-20', 1),
            ('Анна', 'Кузнецова', '2005-01-09', 2),
            ('Дмитрий', 'Попов', '2004-05-30', 2),
            ('Елена', 'Васильева', '2005-09-12', 3),
            ('Сергей', 'Новиков', '2004-12-01', 3),
            ('Ольга', 'Морозова', NULL, 3);
        INSERT INTO grades (student_id, subject, mark) VALUES
            (1, 'Математика', 5), (1, 'Программирование', 4),
            (2, 'Математика', 5), (2, 'Программирование', 5),
            (3, 'Математика', 3), (3, 'Программирование', 4),
            (4, 'Математика', 4), (4, 'Программирование', 3),
            (5, 'Математика', 3), (5, 'Программирование', 3),
            (6, 'Математика', 5), (6, 'Программирование', 4),
            (7, 'Математика', 4), (7, 'Программирование', 5),
            (8, 'Математика', 2), (8, 'Программирование', 3);
        """;

    using var cmd = new NpgsqlCommand(seedSql, conn);
    int rows = cmd.ExecuteNonQuery();
    Console.WriteLine($"Добавлено строк: {rows}");  // 27
}
```

**Вывод программы:**
```
Добавлено строк: 27
```

**Проверка в psql:**
```sql
SELECT COUNT(*) FROM groups;    -- 3
SELECT COUNT(*) FROM students;  -- 8
SELECT COUNT(*) FROM grades;    -- 16
```

---

## Этап 5. Меняем и удаляем

### Шаг 11. UPDATE (24)

```csharp
{
    // Мария сменила фамилию
    using var cmd = new NpgsqlCommand("UPDATE students SET last_name = @last WHERE id = @id", conn);
    cmd.Parameters.AddWithValue("@last", "Соколова");
    cmd.Parameters.AddWithValue("@id", 2);

    int rows = cmd.ExecuteNonQuery();
    if (rows == 0)
        Console.WriteLine("Студент не найден");
    else
        Console.WriteLine("Фамилия изменена");
}
```

**Вывод программы:** `Фамилия изменена`

**Проверка в psql:**
```sql
SELECT * FROM students WHERE id = 2;
```
Фамилия Марии теперь Соколова.

**Попробуйте:** замените в коде `2` на `999` и запустите. Программа напишет `Студент не найден`: такого студента нет, и ExecuteNonQuery вернул 0.

### Шаг 12. DELETE (25)

```csharp
{
    // удаляем оценку студента по одному предмету
    using var cmd = new NpgsqlCommand(
        "DELETE FROM grades WHERE student_id = @id AND subject = @subject", conn);
    cmd.Parameters.AddWithValue("@id", 8);
    cmd.Parameters.AddWithValue("@subject", "Математика");

    int rows = cmd.ExecuteNonQuery();
    Console.WriteLine($"Удалено строк: {rows}");  // Удалено строк: 1
}
```

**Вывод программы:** `Удалено строк: 1`

**Проверка в psql:**
```sql
SELECT * FROM grades WHERE student_id = 8;
```
У студента 8 осталась одна оценка, по программированию.

**Попробуйте прямо в psql** удалить студента, у которого есть оценки:
```sql
DELETE FROM students WHERE id = 1;
```
Будет ошибка `violates foreign key constraint`: на студента ссылаются строки в `grades`. Как сделать это правильно, увидим на этапе 9.

### Шаг 13. Возвращаем базу в исходное состояние

Дальше выводы программы будут сравниваться со слайдами, а мы только что поменяли данные. Повторите шаги 9 и 10: удалить таблицы, создать, заполнить.

---

## Этап 6. Читаем результат

Дробные числа и даты здесь могут печататься в английском формате: `4.33` вместо `4,33`, `03/14/2005` вместо `14.03.2005`. Это настройки языка в codespace, а не ошибка.

### Шаг 14. Одно значение (26)

```csharp
{
    using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM students", conn);
    int count = Convert.ToInt32(cmd.ExecuteScalar());
    Console.WriteLine($"Всего студентов: {count}");  // Всего студентов: 8
}
```

**Вывод программы:** `Всего студентов: 8`

### Шаг 15. Таблица (27)

```csharp
{
    using var cmd = new NpgsqlCommand("SELECT id, first_name, last_name FROM students", conn);
    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        int id = reader.GetInt32(0);  // колонка 0 — id
        string firstName = reader.GetString(1);  // колонка 1 — first_name
        string lastName = reader.GetString(2);  // колонка 2 — last_name
        Console.WriteLine($"{id}. {firstName} {lastName}");
    }
}
```

**Вывод программы:** 8 строк, от `1. Иван Петров` до `8. Ольга Морозова`.

**Попробуйте:** поменяйте в SQL порядок колонок на `first_name, id, last_name` и запустите. Программа упадёт с ошибкой: в колонке 0 теперь текст, а читаем мы его как число через `GetInt32`. Верните порядок обратно.

### Шаг 16. NULL (29)

```csharp
{
    using var cmd = new NpgsqlCommand("SELECT first_name, birth_date FROM students", conn);
    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        string name = reader.GetString(0);

        string birth;
        if (reader.IsDBNull(1))
            birth = "не указана";
        else
            birth = reader.GetDateTime(1).ToShortDateString();

        Console.WriteLine($"{name}: {birth}");
    }
}
```

**Вывод программы:** у всех есть дата рождения, а последняя строка — `Ольга: не указана`.

**Попробуйте:** уберите проверку `IsDBNull` и оставьте только `birth = reader.GetDateTime(1).ToShortDateString();`. На Ольге программа упадёт с ошибкой. Затем верните проверку.

### Шаг 17. Строки в объекты (30)

```csharp
{
    var students = new List<Student>();
    var sql = """
        SELECT id, first_name, last_name FROM students
        """;
    using var cmd = new NpgsqlCommand(sql, conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        students.Add(new Student
        {
            Id = reader.GetInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2)
        });
    }
}
```

Этот пример ничего не печатает. **Добавьте сами** последней строкой перед закрывающей `}`:
```csharp
    Console.WriteLine($"Загружено студентов: {students.Count}");
```
**Вывод программы:** `Загружено студентов: 8`

Класс `Student` уже лежит в конце `Program.cs`, отдельно вставлять его не нужно.

---

## Этап 7. Знакомые запросы

На каждом шаге сначала выполните SQL из примера в psql, подставив значение вместо `@параметра`, и посмотрите результат. Потом запустите программу и сравните: результат должен совпасть.

### Шаг 18. WHERE (32)

```csharp
{
    var sql = """
        SELECT first_name, last_name
        FROM students
        WHERE group_id = @group
        """;
    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@group", 1);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"{reader.GetString(0)} {reader.GetString(1)}");
}
```

**Вывод программы:**
```
Иван Петров
Мария Иванова
Алексей Смирнов
```

**Попробуйте:** поменяйте `1` на `3` в `AddWithValue("@group", 1)`.

### Шаг 19. ORDER BY + LIMIT (33)

```csharp
{
    var sql = """
        SELECT last_name, first_name
        FROM students
        ORDER BY last_name
        LIMIT @count
        """;
    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@count", 3);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"{reader.GetString(0)} {reader.GetString(1)}");
}
```

**Вывод программы:**
```
Васильева Елена
Иванова Мария
Кузнецова Анна
```

### Шаг 20. JOIN (34)

```csharp
{
    var sql = """
        SELECT s.first_name, s.last_name, g.name
        FROM students s
        JOIN groups g ON g.id = s.group_id
        ORDER BY s.id
        """;
    using var cmd = new NpgsqlCommand(sql, conn);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        string name = reader.GetString(0) + " " + reader.GetString(1);
        Console.WriteLine($"{name} — {reader.GetString(2)}");
    }
}
```

**Вывод программы:** 8 строк вида `Иван Петров — ИВТ-101`.

### Шаг 21. GROUP BY (35)

```csharp
{
    var sql = """
        SELECT g.name, COUNT(*)
        FROM groups g
        JOIN students s ON s.group_id = g.id
        GROUP BY g.name
        ORDER BY g.name
        """;
    using var cmd = new NpgsqlCommand(sql, conn);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"{reader.GetString(0)}: {reader.GetInt64(1)}");
}
```

**Вывод программы:**
```
ИВТ-101: 3
ИВТ-102: 2
ПИ-201: 3
```

### Шаг 22. HAVING (36)

```csharp
{
    var sql = """
        SELECT g.name, ROUND(AVG(gr.mark), 2)
        FROM groups g
        JOIN students s ON s.group_id = g.id
        JOIN grades gr ON gr.student_id = s.id
        GROUP BY g.name
        HAVING AVG(gr.mark) > @min
        ORDER BY g.name
        """;
    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@min", 3.5);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"{reader.GetString(0)}: {reader.GetDecimal(1)}");
}
```

**Вывод программы:**
```
ИВТ-101: 4.33
ПИ-201: 3.83
```

**Попробуйте:** порог `3.5` замените на `4` и на `3`. Сколько групп останется?

### Шаг 23. Всё вместе (37)

```csharp
{
    var sql = """
        SELECT s.first_name, s.last_name, ROUND(AVG(gr.mark), 2) AS avg_mark
        FROM students s
        JOIN grades gr ON gr.student_id = s.id
        GROUP BY s.id, s.first_name, s.last_name
        ORDER BY avg_mark DESC, s.last_name
        LIMIT @top
        """;
    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@top", 5);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        string name = reader.GetString(0) + " " + reader.GetString(1);
        Console.WriteLine($"{name} — {reader.GetDecimal(2)}");
    }
}
```

**Вывод программы:**
```
Мария Иванова — 5.00
Елена Васильева — 4.50
Сергей Новиков — 4.50
Иван Петров — 4.50
Анна Кузнецова — 3.50
```

---

## Этап 8. SQL-инъекция

На слайдах 38–40 код только собирает запрос, но не выполняет его. Здесь — версия, которая выполняет оба варианта и показывает разницу.

### Шаг 24. Склейка против параметра

```csharp
{
    string input = "' OR '1'='1";   // «фамилия», которую ввёл хакер

    // ТАК НЕЛЬЗЯ: склейка строк
    var badSql = "SELECT COUNT(*) FROM students WHERE last_name = '" + input + "'";
    using var bad = new NpgsqlCommand(badSql, conn);
    Console.WriteLine($"Склейка нашла студентов: {bad.ExecuteScalar()}");

    // Правильно: параметр
    using var good = new NpgsqlCommand("SELECT COUNT(*) FROM students WHERE last_name = @last", conn);
    good.Parameters.AddWithValue("@last", input);
    Console.WriteLine($"Параметр нашёл студентов: {good.ExecuteScalar()}");
}
```

**Вывод программы:**
```
Склейка нашла студентов: 8
Параметр нашёл студентов: 0
```

Склейка вернула всех: ввод стал частью SQL. Параметр искал студента с фамилией `' OR '1'='1` и честно никого не нашёл.

**Попробуйте:** замените `input` на `"Петров"`. Оба варианта найдут одного студента: на честном вводе склейка работает, поэтому ошибка и коварна.

---

## Этап 9. Транзакции

### Шаг 25. Отчисляем студента (43)

```csharp
{
    int studentId = 8;
    using var tx = conn.BeginTransaction();  // начали транзакцию
    try
    {
        using var deleteGrades = new NpgsqlCommand("DELETE FROM grades WHERE student_id = @id", conn, tx);
        deleteGrades.Parameters.AddWithValue("@id", studentId);
        deleteGrades.ExecuteNonQuery();

        using var deleteStudent = new NpgsqlCommand("DELETE FROM students WHERE id = @id", conn, tx);
        deleteStudent.Parameters.AddWithValue("@id", studentId);
        deleteStudent.ExecuteNonQuery();

        tx.Commit();  // всё прошло — сохраняем
        Console.WriteLine("Студент отчислен");
    }
    catch (Exception ex)
    {
        tx.Rollback();  // что-то сломалось — отменяем всё
        Console.WriteLine("Ошибка, ничего не изменилось: " + ex.Message);
    }
}
```

**Вывод программы:** `Студент отчислен`

**Проверка в psql:**
```sql
SELECT COUNT(*) FROM students;                      -- 7
SELECT COUNT(*) FROM grades WHERE student_id = 8;   -- 0
```

### Шаг 26. Ломаем транзакцию (44)

В том же примере сделайте две правки:
- `int studentId = 8;` замените на `int studentId = 7;`
- во втором запросе `DELETE FROM students` замените на `DELETE FROM studentz` (опечатка нарочно).

Запустите. **Вывод программы:**
```
Ошибка, ничего не изменилось: 42P01: relation "studentz" does not exist
POSITION: 13
```

**Проверка в psql:**
```sql
SELECT COUNT(*) FROM grades WHERE student_id = 7;   -- 2
```
Первый DELETE успел выполниться, но Rollback отменил его: обе оценки на месте.

---

## Этап 10. Приложение целиком

### Шаг 27. «Студенты» (45–46)

Готовое приложение лежит в папке `StudentsApp`. В первом терминале выполните:

```
cd ../StudentsApp
dotnet run
```

Пройдите по всем пунктам меню и после каждого проверяйте результат в psql:
- **1** — список студентов;
- **2** — добавить себя в группу 1;
- **3** — сменить себе фамилию;
- **4** — отчислить себя.

Выход — **0**. Вернуться к примерам: `cd ../UniversityApp`.

