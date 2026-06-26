import os
from pathlib import Path

# Список имён файлов, которые нужно исключить (только имя, без пути)
EXCLUDED_FILENAMES = {
    "combined_python_files.txt",
    # добавьте свои имена
}

# Список имён каталогов, которые нужно полностью исключить из обхода
EXCLUDED_DIRS = {
    ".git",
    "__pycache__",
    ".venv",
    "venv",
    "tests",        # пример: папка с тестами
    "migrations",   # пример: папка с миграциями БД
    "logs",
    "temp",
    "docs"
    # добавьте свои каталоги
}

project_root = Path(__file__).resolve().parent
output_path = Path(__file__).resolve().parent / "combined_python_files.txt"

def collect_python_files(project_root, output_path):
    with open(output_path, "w", encoding="utf-8") as outfile:
        for root, dirs, files in os.walk(project_root):
            # Исключаем ненужные каталоги (и служебные, и из списка EXCLUDED_DIRS)
            dirs[:] = [d for d in dirs if d not in EXCLUDED_DIRS]

            for file in files:
                # Проверяем расширение и исключения по имени файла
                if (file.endswith(".cs") or file.endswith(".txt") or file.endswith(".yaml")) \
                   and file not in EXCLUDED_FILENAMES:

                    full_path = os.path.join(root, file)
                    relative_path = os.path.relpath(full_path, project_root)

                    try:
                        with open(full_path, "r", encoding="utf-8") as f:
                            content = f.read()
                    except Exception as e:
                        content = f"[ERROR READING FILE: {e}]"

                    outfile.write(f"{relative_path}\n")
                    outfile.write("***\n")
                    outfile.write(content)
                    outfile.write("\n\n")

    print(f"Готово. Файлы .py/.txt/.yaml собраны, исключены папки: {', '.join(sorted(EXCLUDED_DIRS))}")

if __name__ == "__main__":
    collect_python_files(project_root, output_path)