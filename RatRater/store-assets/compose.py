# -*- coding: utf-8 -*-
"""RatRater Nexus 素材：header 1300x372 + gallery 1920x1080。从零重绘，不 patch 旧图。"""
import os
import sys
import glob
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = HERE
SRC = os.path.join(HERE, "src")
ASSETS = r"C:\Users\16704\AppData\Local\CodeBuddyExtension\Data\29778d92-91e1-4369-8953-0334b572f040\VSCode\29778d92-91e1-4369-8953-0334b572f040\history\fc17b2b10a8cdeb8b8a77ff31f062cd8\30f8602e56984219a1ca49f6500f2f67\assets"

BG = (18, 21, 28)
PANEL = (28, 32, 42)
BORDER = (44, 50, 64)
WHITE = (238, 242, 248)
MUTED = (150, 162, 180)
DIM = (104, 116, 136)
GREEN = (74, 176, 106)
RED = (214, 74, 82)
BLUE = (72, 142, 214)


def font(size, bold=False):
    name = "segoeuib.ttf" if bold else "segoeui.ttf"
    path = os.path.join("C:\\Windows\\Fonts", name)
    if not os.path.exists(path):
        path = os.path.join("C:\\Windows\\Fonts", "arialbd.ttf" if bold else "arial.ttf")
    return ImageFont.truetype(path, size)


def load_caps():
    os.makedirs(SRC, exist_ok=True)
    files = sorted(glob.glob(os.path.join(ASSETS, "屏幕截图*.png")))
    if len(files) < 2:
        raise SystemExit("need 2 screenshots, found %d in %s" % (len(files), ASSETS))
    caps = []
    for i, f in enumerate(files[:2]):
        im = Image.open(f).convert("RGB")
        dst = os.path.join(SRC, "cap%d.png" % (i + 1))
        im.save(dst)
        caps.append(im)
    return caps


def text_centered(d, box, txt, fnt, fill):
    x0, y0, x1, y1 = box
    b = d.textbbox((0, 0), txt, font=fnt)
    w, h = b[2] - b[0], b[3] - b[1]
    d.text((x0 + (x1 - x0 - w) / 2 - b[0], y0 + (y1 - y0 - h) / 2 - b[1]), txt, font=fnt, fill=fill)


def legend(d, x, y, gap=34, fs=18):
    f = font(fs)
    for color, label in ((GREEN, "KEEP"), (RED, "CULL"), (BLUE, "PREGNANT")):
        d.rounded_rectangle((x, y + 2, x + 14, y + 16), radius=3, fill=color)
        d.text((x + 21, y), label, font=f, fill=WHITE)
        b = d.textbbox((0, 0), label, font=f)
        x += 21 + (b[2] - b[0]) + gap


def save(im, name):
    path = os.path.join(OUT, name)
    try:
        im.save(path)
    except OSError:
        rel = os.path.relpath(path, os.getcwd())
        im.save(rel)
    print("saved", name, im.size)


def compose_header(cap):
    W, H = 1300, 372
    M = 40
    RIGHT = W - M
    im = Image.new("RGB", (W, H), BG)
    d = ImageDraw.Draw(im)

    # 左侧强调竖条
    d.rounded_rectangle((0, 0, 8, H), radius=0, fill=RED)

    # 截图裁切：源约 747x1080，取中间偏下含绿红分界的横条
    SW, SH = 660, 300
    cw, ch = cap.size
    src_h = int(cw / (SW / SH))
    src_y = max(0, min(ch - src_h, int(ch * 0.45)))
    crop = cap.crop((0, src_y, cw, src_y + src_h))
    crop = crop.resize((SW, SH), Image.LANCZOS)
    mask = Image.new("L", (SW, SH), 0)
    md = ImageDraw.Draw(mask)
    md.rounded_rectangle((0, 0, SW - 1, SH - 1), radius=10, fill=255)
    sx, sy = RIGHT - SW, 36
    im.paste(crop, (sx, sy), mask)
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((sx, sy, sx + SW - 1, sy + SH - 1), radius=10, outline=BORDER, width=2)

    # 文字区
    tx = M + 12
    d.text((tx, 66), "RatRater", font=font(64, True), fill=WHITE)
    d.text((tx + 2, 150), "CULLING HIGHLIGHTER", font=font(22, True), fill=(186, 198, 214))
    d.text((tx + 2, 184), "Probably Stolen  /  native mod  /  no MelonLoader", font=font(15), fill=DIM)
    legend(d, tx + 2, 232, gap=26, fs=16)
    d.text((tx + 2, 282), "See which rats to cull at a glance", font=font(16), fill=MUTED)

    assert tx + 540 < sx, "text overlaps screenshot"
    save(im, "header.png")


def compose_gallery(cap1, cap2):
    W, H = 1920, 1080
    M = 48
    RIGHT = W - M
    im = Image.new("RGB", (W, H), BG)
    d = ImageDraw.Draw(im)

    # 顶栏
    d.rounded_rectangle((M, 26, RIGHT, 122), radius=10, fill=PANEL, outline=BORDER, width=2)
    d.text((M + 26, 46), "RatRater", font=font(46, True), fill=WHITE)
    d.text((M + 26, 100), "Culling Highlighter for Probably Stolen", font=font(18), fill=MUTED)

    # 左侧主截图：撑满内容高度，居中裁切
    top, bottom = 150, H - 40
    avail_h = bottom - top
    sw = 920
    ratio = sw / cap1.width
    sh = int(cap1.height * ratio)
    if sh > avail_h:
        ratio = avail_h / cap1.height
        sh = avail_h
        sw = int(cap1.width * ratio)
    main = cap1.resize((sw, sh), Image.LANCZOS)
    mx, my = M, top + (avail_h - sh) // 2
    mask = Image.new("L", main.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, main.width - 1, main.height - 1), radius=8, fill=255)
    im.paste(main, (mx, my), mask)
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((mx, my, mx + main.width - 1, my + main.height - 1), radius=8, outline=BORDER, width=2)

    # 右侧信息栏
    px = mx + sw + 40
    pw = RIGHT - px
    assert pw > 500, "info column too narrow"
    d.rounded_rectangle((px, top, RIGHT, bottom), radius=10, fill=PANEL, outline=BORDER, width=2)

    y = top + 32
    d.text((px + 30, y), "AT A GLANCE", font=font(28, True), fill=WHITE); y += 52
    legend(d, px + 30, y, gap=40, fs=20); y += 70
    d.text((px + 30, y), "Red = cull. Driven by the game's own status flags:", font=font(18), fill=MUTED); y += 38
    for line in ("Starving / diseased / prion", "Scratch marks / overcrowding", "Low immunity / old age", "Low fertility (<30% or <50% base)"):
        d.text((px + 46, y), "- " + line, font=font(17), fill=WHITE); y += 33
    y += 20
    d.text((px + 30, y), "Blue = pregnant, always protected", font=font(18), fill=MUTED); y += 34
    d.text((px + 30, y), "Green = no status, good genetics", font=font(18), fill=MUTED); y += 34
    d.text((px + 30, y), "Press H to toggle highlights", font=font(18), fill=MUTED); y += 58
    d.text((px + 30, y), "TIDY WITH R", font=font(24, True), fill=WHITE); y += 44
    d.text((px + 30, y), "Cull-flagged and dead rats drop to the", font=font(17), fill=MUTED); y += 30
    d.text((px + 30, y), "end, ready to drag away in one motion.", font=font(17), fill=MUTED); y += 64

    d.text((px + 30, y), "INSTALL", font=font(24, True), fill=WHITE); y += 44
    for line in ("Unzip RatRater-1.3.1.zip", "Copy the RatRater folder into Mods\\", "Enable in the in-game Mods menu", "Restart the game"):
        d.text((px + 46, y), "- " + line, font=font(17), fill=WHITE); y += 34
    y += 26
    d.text((px + 30, y), "Native mod loader. No MelonLoader.", font=font(17), fill=MUTED)

    assert bottom <= H - 12
    assert y < bottom - 16, "info text overflow"
    save(im, "gallery-1.png")


def main():
    caps = load_caps()
    compose_header(caps[1])
    compose_gallery(caps[0], caps[1])


if __name__ == "__main__":
    main()
