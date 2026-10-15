#!/usr/bin/env bash
# Проверка практики «ADO.NET + PostgreSQL».
# Запуск из любой папки:  bash /workspaces/*/check.sh
# Смотрит, какие запросы ваша программа выполняла в базе и в каком состоянии база сейчас,
# и записывает итог в файл RESULT.md рядом с этим скриптом.

cd "$(dirname "$0")" || exit 1
OUT="RESULT.md"

# Запросы самой проверки не попадают в журнал (track = none), чтобы не засчитать их за вас.
q() { psql -X -q -t -A -c "SET pg_stat_statements.track = 'none'; $1" 2>/dev/null; }

# Выполнялся ли в этой базе запрос, в тексте которого есть фрагмент (регистр не важен).
ran() { [ "$(q "SELECT count(*) FROM pg_stat_statements WHERE dbid = (SELECT oid FROM pg_database WHERE datname = current_database()) AND query ILIKE \$p\$%$1%\$p\$")" -gt 0 ] 2>/dev/null; }

if [ "$(q "SELECT 1")" != "1" ]; then
  echo "Не удалось подключиться к базе. Подождите минуту после запуска codespace и попробуйте снова."
  exit 1
fi
if [ "$(q "SELECT count(*) FROM pg_extension WHERE extname = 'pg_stat_statements'")" != "1" ]; then
  echo "В этом codespace нет журнала запросов: он создан из старой версии шаблона."
  echo "Удалите codespace на github.com/codespaces и создайте новый."
  exit 1
fi

tables_ok() { [ "$(q "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name IN ('groups', 'students', 'grades')")" = "3" ]; }
groups_ok() { [ "$(q "SELECT string_agg(name, ',' ORDER BY id) FROM groups")" = "ИВТ-101,ИВТ-102,ПИ-201" ]; }

passed=0
lines=()
stage() {   # stage <номер> <название> <шаги практики> <условие...>
  local n="$1" title="$2" hint="$3"; shift 3
  if "$@"; then
    lines+=("- ✓ Этап $n: $title"); passed=$((passed + 1))
    printf '\033[32m✓\033[0m Этап %s: %s\n' "$n" "$title"
  else
    lines+=("- ✗ Этап $n: $title (шаги $hint)")
    printf '\033[31m✗\033[0m Этап %s: %s — пройдите шаги %s\n' "$n" "$title" "$hint"
  fi
}

s1()  { ran 'SELECT version()'; }
s2()  { ran 'CREATE TABLE IF NOT EXISTS students' && ran 'DROP TABLE IF EXISTS grades, students, groups' && tables_ok; }
s3()  { ran 'INSERT INTO students (first_name, last_name, group_id) VALUES' && ran 'INSERT INTO groups (name) VALUES ($1) RETURNING id'; }
s4()  { ran 'INSERT INTO grades (student_id, subject, mark) VALUES' && tables_ok && groups_ok; }
s5()  { ran 'UPDATE students SET last_name = $1 WHERE id = $2' && ran 'DELETE FROM grades WHERE student_id = $1 AND subject = $2'; }
s6()  { ran 'SELECT COUNT(*) FROM students' && ran 'SELECT id, first_name, last_name FROM students' && ran 'SELECT first_name, birth_date FROM students'; }
s7()  { ran 'WHERE group_id = $1' && ran 'ORDER BY last_name' && ran 'JOIN groups g ON g.id = s.group_id' \
        && ran 'GROUP BY g.name' && ran 'HAVING AVG(gr.mark) > $1' && ran 'ORDER BY avg_mark DESC'; }
s8()  { ran 'last_name = $1 OR' && ran 'SELECT COUNT(*) FROM students WHERE last_name = $1'; }
s9()  { ran 'DELETE FROM students WHERE id = $1' && tables_ok \
        && [ "$(q "SELECT count(*) FROM students WHERE id = 8")" = "0" ] \
        && [ "$(q "SELECT count(*) FROM grades WHERE student_id = 7")" = "2" ]; }
s10() { ran 'SELECT s.id, s.first_name, s.last_name, g.name' && ran 'VALUES ($1, $2, $3) RETURNING id'; }

echo
stage 1  "Подключение"                         "1"      s1
stage 2  "Создание и удаление таблиц"          "2–4"    s2
stage 3  "Добавление записей"                  "5–8"    s3
stage 4  "Учебная база"                        "9–10"   s4
stage 5  "Изменение и удаление"                "11–12"  s5
stage 6  "Чтение результата"                   "14–17"  s6
stage 7  "Знакомые запросы"                    "18–23"  s7
stage 8  "SQL-инъекция"                        "24"     s8
stage 9  "Транзакции"                          "25–26"  s9
stage 10 "Приложение «Студенты»"               "27"     s10

who="${GITHUB_USER:-неизвестно}"
when="$(date '+%Y-%m-%d %H:%M')"
{
  echo "# Результат практики"
  echo
  echo "Студент (GitHub): $who"
  echo "Проверено: $when"
  echo
  printf '%s\n' "${lines[@]}"
  echo
  echo "Пройдено $passed из 10"
} > "$OUT"

echo
echo "Пройдено $passed из 10. Итог записан в $OUT."
echo "Покажите этот результат преподавателю."
