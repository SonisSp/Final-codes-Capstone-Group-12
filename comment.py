import os

def count_comments_and_lines(directory):
    total_lines = 0
    comment_lines = 0
    docstring_lines = 0
    in_docstring = False

    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith(".py"):
                file_path = os.path.join(root, file)
                try:
                    with open(file_path, "r", encoding="utf-8") as f:
                        for line in f:
                            total_lines += 1
                            stripped = line.strip()

                            # Check if it's a comment
                            if stripped.startswith("#"):
                                comment_lines += 1

                            # Check for multi-line docstrings
                            if stripped.startswith(('"""', "'''")):
                                if in_docstring:
                                    in_docstring = False
                                    docstring_lines += 1
                                else:
                                    in_docstring = True
                                    docstring_lines += 1
                            elif in_docstring:
                                docstring_lines += 1
                except (UnicodeDecodeError, OSError):
                    # Skip unreadable files
                    continue

    return total_lines, comment_lines, docstring_lines


if __name__ == "__main__":
    folder = "yt_dlp"  # change if your folder name/path is different
    total, comments, docstrings = count_comments_and_lines(folder)
    total_comments = comments + docstrings
    density = (total_comments / total * 100) if total > 0 else 0

    print(f"Analyzed folder: {folder}")
    print(f"Total lines of code: {total}")
    print(f"Comment lines (#): {comments}")
    print(f"Docstring lines: {docstrings}")
    print(f"Total comment+docstring lines: {total_comments}")
    print(f"Comment density: {density:.2f}%")
