# /// script
# requires-python = ">=3.10"
# dependencies = ["resvg-py==0.5.0", "pillow==11.3.0"]
# ///
"""Builds the Navpaky app icon and mark images from the SVG masters in ../app.

Run from the repository root: uv run docs/design/icons/tools/build_icons.py
Sizes up to 32 px come from the hand-simplified masters (no arrows); larger ones from the
full mark. The output is committed; run this after changing a master.
"""
import io
import struct
from pathlib import Path

import resvg_py
from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
APP = ROOT / "docs/design/icons/app"
OUT = ROOT / "src/Navpaky.App"
ICO_SIZES = [16, 20, 24, 32, 40, 48, 64, 96, 128, 256]


def master(size: int) -> Path:
    for small in (16, 24, 32):
        if size <= small:
            return APP / f"navpaky-{small}.svg"
    return APP / "navpaky.svg"


def render(size: int) -> Image.Image:
    svg = master(size).read_text(encoding="utf-8")
    png = resvg_py.svg_to_bytes(svg_string=svg, width=size, height=size)
    return Image.open(io.BytesIO(bytes(png))).convert("RGBA")


def png_bytes(image: Image.Image) -> bytes:
    data = io.BytesIO()
    image.save(data, format="PNG", optimize=True)
    return data.getvalue()


# Every frame is stored as PNG, written by hand so each size keeps its own master.
def write_ico(path: Path, sizes: list[int]) -> None:
    images = [png_bytes(render(size)) for size in sizes]
    header = struct.pack("<HHH", 0, 1, len(images))
    offset = len(header) + 16 * len(images)
    entries = b""
    for size, data in zip(sizes, images):
        side = 0 if size >= 256 else size
        entries += struct.pack("<BBBBHHII", side, side, 0, 0, 1, 32, len(data), offset)
        offset += len(data)
    path.write_bytes(header + entries + b"".join(images))


def main() -> None:
    write_ico(OUT / "Navpaky.ico", ICO_SIZES)
    for size in (64, 128):
        render(size).save(OUT / f"Assets/mark-{size}.png", optimize=True)


if __name__ == "__main__":
    main()
