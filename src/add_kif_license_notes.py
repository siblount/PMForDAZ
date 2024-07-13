import os

SIMULATE = False
EXCLUDE_DIRS = {"bin", "external", "obj", "properties", "resources", "libs"}
HANDLE_EXTENSIONS = {".cs"}
LANGUAGE_TO_COMMENT = {".cs": "// ", ".py": "# ", ".cpp": "// ", ".js": "// ", ".java": "// "}
MESSAGE = """This code is licensed under the Keep It Free License V1.
You may find a full copy of this license at root project directory\LICENSE"""
SUFFIXES = {"Designer.cs", "AssemblyInfo.cs", "AssemblyAttributes.cs"}

EXCLUDE_CONDITIONS = {".cs": [lambda filename: not any(filename.endswith(x) for x in SUFFIXES)]}

def get_comment_message(file_path: str) -> str:
    _, ext = os.path.splitext(file_path)
    comment_prefix = LANGUAGE_TO_COMMENT.get(ext.lower(), "")
    return "\n".join(f"{comment_prefix}{line}" for line in MESSAGE.splitlines())

def should_process_file(file_path: str) -> bool:
    _, ext = os.path.splitext(file_path)
    if ext.lower() not in HANDLE_EXTENSIONS:
        return False
    if ext in EXCLUDE_CONDITIONS:
        return all(func(file_path) for func in EXCLUDE_CONDITIONS[ext])
    return True

def has_license_message(file_content: str, file_path: str) -> bool:
    comment_message = get_comment_message(file_path)
    return comment_message in file_content[:len(comment_message) * 2]

def add_license_to_file(file_path: str) -> None:
    with open(file_path, 'r', encoding='utf-8') as file:
        content = file.read().lstrip()

    if not has_license_message(content, file_path):
        new_content = get_comment_message(file_path) + '\n\n' + content
        if SIMULATE:
            print(f"Would add license to: {file_path}")
            print("First few lines of new content:")
            print("\n".join(new_content.splitlines()[:5]))
            print("...\n")
        else:
            with open(file_path, 'w', encoding='utf-8') as file:
                file.write(new_content)
            print(f"Added license to: {file_path}")
    else:
        print(f"License already present in: {file_path}")

def process_directory(directory: str) -> None:
    for root, dirs, files in os.walk(directory):
        dirs[:] = [d for d in dirs if d.lower() not in EXCLUDE_DIRS]
        
        for file in files:
            file_path = os.path.join(root, file)
            if should_process_file(file_path):
                add_license_to_file(file_path)

if __name__ == "__main__":
    current_directory = os.getcwd()
    process_directory(current_directory)