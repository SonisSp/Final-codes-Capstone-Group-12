#!/usr/bin/env python3

import radon 
import argparse
import os
import sys
import json
import csv

try:
    from radon.complexity import cc_visit
    from radon.visitors import Function
except Exception:
    print("This script requires 'radon'. Install it via: python3 -m pip install radon")
    sys.exit(2)


def analyze_file(path):
    
    with open(path, 'r', encoding='utf-8', errors='ignore') as fh:
        source = fh.read()

    try:
        blocks = cc_visit(source)
    except Exception as e:
        # radon may raise on strange source; skip file gracefully
        print(f"Warning: radon failed on {path}: {e}", file=sys.stderr)
        return []

    results = []
    for b in blocks:
        # radon Block objects have attributes: name, lineno, complexity, rank, kind (maybe)
        kind = getattr(b, 'kind', None)
        # Some radon versions use .type or .kind; normalize:
        if kind is None:
            # fallback inference
            name = getattr(b, 'name', '<unknown>')
            if name.startswith('class '):
                kind = 'class'
            elif '(' in name:
                # function/method detection not perfect but fine for summary
                kind = 'function'
            else:
                kind = 'function'
        results.append({
            'file': path,
            'name': getattr(b, 'name', '<anonymous>'),
            'type': kind,
            'lineno': getattr(b, 'lineno', None),
            'complexity': getattr(b, 'complexity', None),
            'rank': getattr(b, 'rank', None),
        })
    return results


def walk_python_files(root):
    """
    Yield all python files under root, skipping virtualenv/ .git/ __pycache__ etc.
    """
    skip_dirs = {'.git', '__pycache__', 'venv', '.venv', 'env', 'build', 'dist'}
    for dirpath, dirnames, filenames in os.walk(root):
        # mutate dirnames in-place to skip
        dirnames[:] = [d for d in dirnames if d not in skip_dirs and not d.startswith('.')]
        for fn in filenames:
            if fn.endswith('.py'):
                yield os.path.join(dirpath, fn)


def main():
    parser = argparse.ArgumentParser(description="Measure cyclomatic complexity using radon.")
    parser.add_argument('--path', '-p', default='.', help='Path to project root (default: .)')
    parser.add_argument('--top', '-t', type=int, default=25, help='Show top N complex blocks')
    parser.add_argument('--min', '-m', type=int, default=10, help='Minimum complexity to include in filtered list (default: 10)')
    parser.add_argument('--output', '-o', default='cc_report.csv', help='CSV output filename (also writes JSON sidecar)')
    args = parser.parse_args()

    root = os.path.abspath(args.path)
    if not os.path.isdir(root):
        print(f"Error: path {root} is not a directory", file=sys.stderr)
        sys.exit(2)

    print(f"Scanning Python files under: {root}\nThis may take a while for large repos...")

    all_blocks = []
    file_count = 0
    for pyf in walk_python_files(root):
        file_count += 1
        blocks = analyze_file(pyf)
        all_blocks.extend(blocks)

    if not all_blocks:
        print("No complexity blocks found (maybe radon failed or no .py files).")
        sys.exit(0)

    # Sort by complexity descending
    sorted_blocks = sorted(all_blocks, key=lambda b: (b['complexity'] if b['complexity'] is not None else 0), reverse=True)

    # Print top N
    topn = args.top
    print(f"\nTop {topn} most complex functions/methods/classes:")
    print("-" * 80)
    for b in sorted_blocks[:topn]:
        print(f"{b['complexity']:3}  {b['rank'] or '?'}  {os.path.relpath(b['file'], root)}:{b['lineno']:4}  {b['name']}  ({b['type']})")
    print("-" * 80)
    print(f"Total files scanned: {file_count}, total blocks: {len(all_blocks)}")

    # Filtered list (complexity >= min)
    min_cplx = args.min
    filtered = [b for b in sorted_blocks if (b['complexity'] or 0) >= min_cplx]
    print(f"\nBlocks with complexity >= {min_cplx}: {len(filtered)}")

    # Write CSV
    csv_path = args.output
    json_path = os.path.splitext(csv_path)[0] + '.json'
    with open(csv_path, 'w', newline='', encoding='utf-8') as csvfile:
        fieldnames = ['file', 'name', 'type', 'lineno', 'complexity', 'rank']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for b in sorted_blocks:
            writer.writerow({k: b.get(k) for k in fieldnames})
    with open(json_path, 'w', encoding='utf-8') as jf:
        json.dump(sorted_blocks, jf, indent=2)

    print(f"CSV report written to: {csv_path}")
    print(f"JSON report written to: {json_path}")

    # Optional: print a short summary of files containing worst offenders
    from collections import Counter
    file_counter = Counter([os.path.relpath(b['file'], root) for b in filtered[:topn]])
    if file_counter:
        print("\nFiles containing the most high-complexity blocks (top offenders):")
        for fn, cnt in file_counter.most_common(10):
            print(f"  {cnt:3} blocks  {fn}")

    print("\nDone.")


if __name__ == '__main__':
    main()
