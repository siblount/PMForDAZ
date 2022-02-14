import os

SIMULATE = False
EXCLUDE_DIRS = {"bin", "external", "obj", "properties", "resources", "libs"}
HANDLE_EXTENSIONS = {".cs"}
FILES_TO_PROCESS = set()
LANGUAGE_TO_COMMENT = {".cs" : "// ", 
                        ".py" : "# ", 
                        ".cpp" : "// ", 
                        ".js" : "// ", 
                        ".java" : "// "}
MESSAGE = """This code is licensed under the Keep It Free License V1.
You may find a full copy of this license at root project directory\LICENSE"""
MESSAGE_SPLIT = MESSAGE.splitlines()
COMMENT_CACHE = {}
BYTE_ORDERS = [(0xEF, 0xBB, 0xBF), 
                (0xFE, 0xFF), 
                (0xFF, 0xFE)]
SUFFIXES = {"Designer.cs", "AssemblyInfo.cs", "AssemblyAttributes.cs"}

EXCLUDE_CONDITIONS = {".cs" : [lambda filename: not any([x in filename for x in SUFFIXES])]}

def get_files_n_dirs(contents: list[str]) -> tuple[list[str], list[str]]:
    dirs = []
    files = []
    for content in contents:
        _, ext = os.path.splitext(content)
        if os.path.isdir(content) and content.lower() not in EXCLUDE_DIRS and passes_exclude_conditions(ext, content):
            dirs.append(os.path.abspath(content))
        elif os.path.isfile(content) and ext.lower() in HANDLE_EXTENSIONS and passes_exclude_conditions(ext, content):
            files.append(os.path.abspath(content))
    return files, dirs
def passes_exclude_conditions(key:str, x:str) -> bool:
    if key in EXCLUDE_CONDITIONS:
        for func in EXCLUDE_CONDITIONS[key]:
            if func(x) is False:
                return False
    return True
def compare_message(astr: list[str], file_path: str) -> bool:
    comment_msg = get_comment_message(file_path).split()
    msg_words = []
    for line in astr:
        msg_words.extend(line.split())

    if abs(len(comment_msg) - len(msg_words)) > 3:
        return False
    return True
def get_byte_order(str_) -> tuple[int] | None:
    try:
        byte_order = tuple(str_[:3])
        for bo in BYTE_ORDERS:
            next = False
            for a, b in zip(bo, byte_order):
                if a != ord(b):
                    next = True
                    continue
            if next:
                continue
            return bo
    except:
        return None
    return None
def trim_byte_order(bo:str, str_:str) -> str:
    return str_[len(bo):]
def get_comment_message(file_path: str) -> str:
    _, ext = os.path.splitext(file_path)
    stringBuilder = ""
    if ext in COMMENT_CACHE:
        return COMMENT_CACHE[ext]
    
    for line in MESSAGE_SPLIT:
        stringBuilder += LANGUAGE_TO_COMMENT[ext.lower()] + line + "\n"
    COMMENT_CACHE[ext.lower()] = stringBuilder
    return stringBuilder
def simulate(file_path: str) -> bool:
    """ Returns True to continue, returns False to exit."""
    two_lines = []
    try:
        with open(file_path, "r") as file:
            two_lines = [file.readline() for _ in range(2)]
        file.close()
    except:
        return False
    
    if len(two_lines) != 0 and not compare_message(two_lines, file_path):
        byte_order = get_byte_order(two_lines[0])
        if byte_order is not None:
            two_lines[0] = trim_byte_order(byte_order, two_lines[0])
        print(file_path[-40:])
        print(get_comment_message(file_path))
        print(two_lines[0], two_lines[1], sep="")
    else:
        print("Skipped due to message already in place.")
    return True if "y" in input("Do you wish to proceed? (Y/N) ").lower() else False

def execute(file_path: str) -> bool:
    try:
        with open(file_path, "r+") as file:
            data = file.read()
            two_lines = data.split("\n",2)
            byte_order = get_byte_order(data[:3])
            if len(two_lines) == 0 or compare_message(two_lines, file_path):
                return False
            if byte_order is not None:
                file.seek(0)
                file.truncate(0)
                file.write("".join([chr(x) for x in byte_order]) + get_comment_message(file_path) + "\n" + data[len(byte_order):])
            else:
                file.seek(0)
                file.truncate(0)
                file.write(get_comment_message(file_path) + "\n" + data)
        file.close()
        return True
    except:
        return False
def handle_dir(path: str) -> bool:
    # Get files and directories.
    files, dirs = get_files_n_dirs(os.listdir(path))
    for file in files:
        if SIMULATE:
            if not simulate(file):
                return False
        else:
            if not execute(file):
                return False

    for dir in dirs:
        os.chdir(os.path.join(os.getcwd(), dir))
        if not handle_dir(dir):
            return False
    return True


handle_dir(os.getcwd())

