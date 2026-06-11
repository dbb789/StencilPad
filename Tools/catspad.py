#!/usr/bin/env python3

import gzip
import json
import sys
import argparse

def main():
    parser = argparse.ArgumentParser(description="Extract and print StencilPad (.spad) files.")
    parser.add_argument("file", help="Path to the .spad file")
    args = parser.parse_args()

    try:
        with gzip.open(args.file, 'rb') as f:
            data = json.load(f)
            print(json.dumps(data, indent=4))
    except FileNotFoundError:
        print(f"Error: File '{args.file}' not found.", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"Error processing file: {e}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
