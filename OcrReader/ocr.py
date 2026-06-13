#!/usr/bin/env python3
"""
OCR Script with Arabic and English language support.
Extracts text from images and PDFs using Tesseract OCR.
"""

import argparse
import json
import os
import sys
from pathlib import Path

# ── Supported formats ──────────────────────────────────────────────────────────
SUPPORTED_IMAGE_FORMATS = {".png", ".jpg", ".jpeg", ".tiff"}
SUPPORTED_FORMATS = SUPPORTED_IMAGE_FORMATS | {".pdf"}

# ── Exit codes ─────────────────────────────────────────────────────────────────
EXIT_SUCCESS = 0
EXIT_FILE_NOT_FOUND = 1
EXIT_UNSUPPORTED_FORMAT = 2
EXIT_OCR_FAILED = 3

# ── Max dimension before down-scaling ─────────────────────────────────────────
MAX_PIXEL_DIMENSION = 4000
OCR_DPI = 300


# ══════════════════════════════════════════════════════════════════════════════
# Helpers
# ══════════════════════════════════════════════════════════════════════════════

def _lazy_imports():
    """Import heavy third-party libraries lazily so argument-parsing errors are
    shown quickly without waiting for large imports."""
    global pytesseract, Image, convert_from_path
    try:
        import pytesseract as _pt
        pytesseract = _pt
    except ImportError:
        _die("pytesseract is not installed. Run: pip install pytesseract", EXIT_OCR_FAILED)

    try:
        from PIL import Image as _img
        Image = _img
    except ImportError:
        _die("Pillow is not installed. Run: pip install Pillow", EXIT_OCR_FAILED)

    try:
        from pdf2image import convert_from_path as _cfp
        convert_from_path = _cfp
    except ImportError:
        _die("pdf2image is not installed. Run: pip install pdf2image", EXIT_OCR_FAILED)


def _die(message: str, code: int) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    sys.exit(code)


def _validate_file(filepath: str) -> Path:
    path = Path(filepath)
    if not path.exists():
        _die(f"File not found: {filepath}", EXIT_FILE_NOT_FOUND)
    if path.suffix.lower() not in SUPPORTED_FORMATS:
        _die(
            f"Unsupported format '{path.suffix}'. "
            f"Supported: {', '.join(sorted(SUPPORTED_FORMATS))}",
            EXIT_UNSUPPORTED_FORMAT,
        )
    return path


def _preprocess_image(img):
    """Resize image if any dimension exceeds MAX_PIXEL_DIMENSION; keep aspect ratio."""
    w, h = img.size
    if max(w, h) > MAX_PIXEL_DIMENSION:
        scale = MAX_PIXEL_DIMENSION / max(w, h)
        img = img.resize((int(w * scale), int(h * scale)), Image.LANCZOS)
    return img


# ══════════════════════════════════════════════════════════════════════════════
# Core OCR logic
# ══════════════════════════════════════════════════════════════════════════════

def _ocr_image_text(img, lang: str) -> str:
    """Run Tesseract on a PIL image and return plain text."""
    try:
        # psm 3: fully automatic page segmentation (default)
        config = "--psm 3"
        return pytesseract.image_to_string(img, lang=lang, config=config)
    except pytesseract.TesseractError as exc:
        _die(f"Tesseract failed: {exc}", EXIT_OCR_FAILED)


def _ocr_image_json(img, lang: str, page_num: int) -> dict:
    """Run Tesseract on a PIL image and return a structured page dict."""
    try:
        data = pytesseract.image_to_data(
            img, lang=lang, config="--psm 3",
            output_type=pytesseract.Output.DICT,
        )
    except pytesseract.TesseractError as exc:
        _die(f"Tesseract failed: {exc}", EXIT_OCR_FAILED)

    words = []
    full_text_parts = []
    n = len(data["text"])

    for i in range(n):
        word = data["text"][i].strip()
        if not word:
            continue
        conf = float(data["conf"][i])
        if conf < 0:          # -1 means tesseract skipped it
            continue

        x, y, w, h = data["left"][i], data["top"][i], data["width"][i], data["height"][i]

        # Heuristic language tag: Arabic Unicode range U+0600–U+06FF
        detected_lang = "ara" if any("\u0600" <= c <= "\u06ff" for c in word) else "eng"

        words.append({
            "text": word,
            "bbox": [x, y, x + w, y + h],
            "confidence": round(conf, 1),
            "lang": detected_lang,
        })
        full_text_parts.append(word)

    return {
        "page_num": page_num,
        "text": " ".join(full_text_parts),
        "words": words,
    }


# ══════════════════════════════════════════════════════════════════════════════
# File handlers
# ══════════════════════════════════════════════════════════════════════════════

def _load_images_from_path(path: Path) -> list:
    """Return a list of PIL images for the given file (image or PDF)."""
    suffix = path.suffix.lower()
    if suffix == ".pdf":
        try:
            pages = convert_from_path(str(path), dpi=OCR_DPI)
        except Exception as exc:
            _die(f"Could not convert PDF '{path.name}': {exc}", EXIT_OCR_FAILED)
        return pages
    else:
        try:
            img = Image.open(str(path))
            img.load()           # force decode – catches corruption early
        except Exception as exc:
            _die(f"Could not open image '{path.name}': {exc}", EXIT_OCR_FAILED)
        return [img]


def process_text(path: Path, lang: str) -> None:
    """Print plain-text OCR result to stdout."""
    images = _load_images_from_path(path)
    for page_num, img in enumerate(images, start=1):
        img = _preprocess_image(img)
        text = _ocr_image_text(img, lang)
        print(f"[Page {page_num}]")
        print(text)


def process_json(path: Path, lang: str) -> None:
    """Print JSON OCR result to stdout."""
    images = _load_images_from_path(path)
    pages = []
    for page_num, img in enumerate(images, start=1):
        img = _preprocess_image(img)
        pages.append(_ocr_image_json(img, lang, page_num))

    result = {
        "file": str(path.resolve()),
        "pages": pages,
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))


# ══════════════════════════════════════════════════════════════════════════════
# CLI
# ══════════════════════════════════════════════════════════════════════════════

def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="ocr.py",
        description="Extract text from images and PDFs with Arabic & English support.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
examples:
  python ocr.py document.pdf
  python ocr.py arabic_image.png --lang ara
  python ocr.py mixed_document.pdf --lang ara+eng --output json
  python ocr.py english_scan.jpg --lang eng --output text
        """,
    )
    parser.add_argument(
        "filepath",
        metavar="/filepath",
        help="Path to input file (.png .jpg .jpeg .tiff .pdf)",
    )
    parser.add_argument(
        "--lang",
        default="ara+eng",
        choices=["ara", "eng", "ara+eng"],
        help='OCR language(s) (default: "ara+eng")',
    )
    parser.add_argument(
        "--output",
        default="text",
        choices=["text", "json"],
        help='Output format (default: "text")',
    )
    return parser


def main() -> None:
    parser = _build_parser()
    args = parser.parse_args()

    # Validate before loading heavy libs
    path = _validate_file(args.filepath)

    # Now load heavy imports
    _lazy_imports()

    try:
        if args.output == "text":
            process_text(path, args.lang)
        else:
            process_json(path, args.lang)
    except SystemExit:
        raise
    except Exception as exc:
        _die(f"Unexpected error: {exc}", EXIT_OCR_FAILED)

    sys.exit(EXIT_SUCCESS)


if __name__ == "__main__":
    main()