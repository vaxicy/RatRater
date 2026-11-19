# -*- coding: utf-8 -*-
"""RatRater Nexus 素材：header 1300x372 + gallery 中英两版 1920x1080。从零重绘，不 patch 旧图。"""
import os
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


def font(size, bold=False, cn=False):
    if cn:
        name = "msyhbd.ttc" if bold else "msyh.ttc"
    else:
        name = "segoeuib.ttf" if bold else "segoeui.ttf"
    path = os.path.join("C:\\Windows\\Fonts", name)
    if not os.path.exists(path):
        path = os.path.join("C:\\Windows\\Fonts", "arialbd.ttf" if bold else "arial.ttf")
    return ImageFont.truetype(path, size)


def load_caps():
    os.makedirs(SRC, exist_ok=True)
    # 优先用带怀孕蓝色的新截图
    new_cap = os.path.join(ASSETS, "image.01b0bf5040.png")
    if os.path.exists(new_cap):
        caps = [Image.open(new_cap).convert("RGB")]
    else:
        files = sorted(glob.glob(os.path.join(ASSETS, "屏幕截图*.png")))
        if len(files) < 1:
            raise SystemExit("no screenshots found")
        caps = [Image.open(f).convert("RGB") for f in files[:2]]
    for i, im in enumerate(caps):
        im.save(os.path.join(SRC, "cap%d.png" % (i + 1)))
    return caps


def text_fit(d, x, y, txt, size, max_w, fill, bold=False, cn=False):
    """按最大宽度绘制文字，必要时缩小字号，返回实际用的字号。"""
    f = font(size, bold, cn)
    b = d.textbbox((0, 0), txt, font=f)
    w = b[2] - b[0]
    while w > max_w and size > 12:
        size -= 1
        f = font(size, bold, cn)
        b = d.textbbox((0, 0), txt, font=f)
        w = b[2] - b[0]
    assert w <= max_w, "text overflow: %s" % txt
    d.text((x, y), txt, font=f, fill=fill)
    return size


def legend(d, x, y, gap, fs, labels, cn=False):
    f = font(fs, False, cn)
    for color, label in zip((GREEN, RED, BLUE), labels):
        d.rounded_rectangle((x, y + 2, x + 14, y + 16), radius=3, fill=color)
        d.text((x + 21, y), label, font=f, fill=WHITE)
        b = d.textbbox((0, 0), label, font=f)
        x += 21 + (b[2] - b[0]) + gap
    return x


def save(im, name):
    """中文绝对路径下 PIL 会间歇 OSError 22，取 bytes 自己写盘最稳。"""
    import io
    buf = io.BytesIO()
    im.save(buf, format="PNG")
    path = os.path.join(OUT, name)
    with open(path, "wb") as f:
        f.write(buf.getvalue())
    print("saved", name, im.size)


def compose_header(cap):
    # Nexus 推荐 1300x372，但其裁剪框实际约 3.3:1；用 1300x396 才能铺满不留棋盘格
    W, H = 1300, 396
    im = Image.new("RGB", (W, H), BG)
    d = ImageDraw.Draw(im)
    d.rectangle((0, 0, 7, H - 1), fill=RED)

    # 右侧截图占约 68% 宽，几乎贴右边
    SW = 880
    SH = H
    cw, ch = cap.size
    src_h = int(cw / (SW / SH))
    src_h = min(src_h, ch)
    src_y = max(0, min(ch - src_h, int(ch * 0.18)))
    crop = cap.crop((0, src_y, cw, src_y + src_h)).resize((SW, SH), Image.LANCZOS)
    sx = W - SW
    im.paste(crop, (sx, 0))
    d.rectangle((sx - 2, 0, sx, H - 1), fill=BORDER)  # 分隔线
    tx = 40
    d.text((tx, 70), "RatRater", font=font(62, True), fill=WHITE)
    d.text((tx + 2, 150), "CULLING HIGHLIGHTER", font=font(21, True), fill=(190, 202, 218))
    text_fit(d, tx + 2, 186, "Probably Stolen  /  native mod  /  no MelonLoader", 14, sx - tx - 40, DIM)
    legend(d, tx + 2, 236, 24, 17, ("KEEP", "CULL", "PREGNANT"))
    d.text((tx + 2, 288), "See which rats to cull at a glance", font=font(16), fill=MUTED)
    d.text((tx + 2, 326), "Blade-click to butcher  /  rat meat sorts to bottom", font=font(15), fill=(150, 162, 180))
    assert tx + 350 < sx, "text overlaps screenshot"
    save(im, "header.png")


T_EN = {
    "top_sub": "Culling Highlighter for Probably Stolen",
    "glance": "AT A GLANCE",
    "legend": ("KEEP", "CULL", "PREGNANT"),
    "red_intro": "Red = cull. Driven by the game's own status flags:",
    "red_items": ("Starving / diseased / prion", "Scratch marks / overcrowding",
                  "Low immunity / old age", "Low fertility (<30% or <50% base)"),
    "blue": "Blue = pregnant, always protected",
    "green": "Green = no status, good genetics",
    "hotkey": "H: toggle highlights",
    "tidy": "BLADE BATCH BUTCHER",
    "tidy1": "Blade-click a rat to butcher all red/dead in that container",
    "tidy2": "",
    "meat": "SORT ORDER",
    "meat1": "Healthy rats, then red/dead rats, then rat meat at the bottom",
    "install": "INSTALL",
    "ins_items": ("Unzip and copy RatRater into the game's Mods folder", "Enable in Mods menu and restart"),
    "foot": "Toggle in mod settings. Native loader, no MelonLoader.",
}

T_ZH = {
    "top_sub": "深空当铺（Probably Stolen）老鼠淘汰高亮模组",
    "glance": "一眼看懂",
    "legend": ("留种", "淘汰", "怀孕"),
    "red_intro": "红色 = 建议淘汰，直接读游戏自己的状态：",
    "red_items": ("挨饿 / 患病 / 朊病毒", "抓痕 / 过度拥挤",
                  "免疫力低 / 老年", "繁殖力低（当前<30% 或 基础<50%）"),
    "blue": "蓝色 = 怀孕母鼠，一律保护",
    "green": "绿色 = 无负面状态，基因好",
    "hotkey": "H：开关高亮",
    "tidy": "用刀批量屠宰",
    "tidy1": "拿剥皮刀点老鼠，同箱红鼠和死鼠一起变肉",
    "tidy2": "",
    "meat": "整理排序",
    "meat1": "健康鼠 → 红鼠/死鼠 → 鼠肉（最底）",
    "install": "安装",
    "ins_items": ("解压，把 RatRater 文件夹放进游戏 Mods 目录", "在模组菜单启用并重启"),
    "foot": "模组设置里可开关。游戏原生加载器，无需 MelonLoader。",
}


def compose_gallery(cap1, lang, t, outname):
    W, H = 1920, 1080
    M = 48
    RIGHT = W - M
    cn = lang == "zh"
    im = Image.new("RGB", (W, H), BG)
    d = ImageDraw.Draw(im)

    d.rounded_rectangle((M, 26, RIGHT, 132), radius=10, fill=PANEL, outline=BORDER, width=2)
    d.text((M + 26, 38), "RatRater", font=font(42, True), fill=WHITE)
    text_fit(d, M + 26, 92, t["top_sub"], 18, RIGHT - M - 52, MUTED, cn=cn)

    top, bottom = 150, H - 40
    avail_h = bottom - top
    ratio = avail_h / cap1.height
    sw = int(cap1.width * ratio)
    sh = avail_h
    main = cap1.resize((sw, sh), Image.LANCZOS)
    mx, my = M, top
    mask = Image.new("L", main.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, main.width - 1, main.height - 1), radius=8, fill=255)
    im.paste(main, (mx, my), mask)
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((mx, my, mx + main.width - 1, my + main.height - 1), radius=8, outline=BORDER, width=2)

    px = mx + sw + 40
    pw = RIGHT - px
    assert pw > 500, "info column too narrow"
    d.rounded_rectangle((px, top, RIGHT, bottom), radius=10, fill=PANEL, outline=BORDER, width=2)

    inner = pw - 60
    y = top + 32
    text_fit(d, px + 30, y, t["glance"], 28, inner, WHITE, bold=True, cn=cn); y += 52
    legend(d, px + 30, y, 40, 20, t["legend"], cn=cn); y += 70
    text_fit(d, px + 30, y, t["red_intro"], 18, inner, MUTED, cn=cn); y += 38
    for line in t["red_items"]:
        text_fit(d, px + 46, y, "- " + line, 17, inner - 16, WHITE, cn=cn); y += 33
    y += 20
    text_fit(d, px + 30, y, t["blue"], 18, inner, MUTED, cn=cn); y += 34
    text_fit(d, px + 30, y, t["green"], 18, inner, MUTED, cn=cn); y += 34
    text_fit(d, px + 30, y, t["hotkey"], 18, inner, MUTED, cn=cn); y += 58
    text_fit(d, px + 30, y, t["tidy"], 24, inner, WHITE, bold=True, cn=cn); y += 44
    text_fit(d, px + 30, y, t["tidy1"], 17, inner, MUTED, cn=cn); y += 58
    text_fit(d, px + 30, y, t["meat"], 24, inner, WHITE, bold=True, cn=cn); y += 44
    text_fit(d, px + 30, y, t["meat1"], 17, inner, MUTED, cn=cn); y += 58
    text_fit(d, px + 30, y, t["install"], 24, inner, WHITE, bold=True, cn=cn); y += 44
    for line in t["ins_items"]:
        text_fit(d, px + 46, y, "- " + line, 17, inner - 16, WHITE, cn=cn); y += 34
    y += 26
    text_fit(d, px + 30, y, t["foot"], 17, inner, MUTED, cn=cn); y += 24

    assert bottom <= H - 12
    assert y < bottom - 8, "info text overflow (y=%d bottom=%d)" % (y, bottom)
    save(im, outname)


def main():
    caps = load_caps()
    cap = caps[-1]
    compose_header(cap)
    compose_gallery(cap, "en", T_EN, "gallery-1.png")
    compose_gallery(cap, "zh", T_ZH, "gallery-2-zh.png")


if __name__ == "__main__":
    main()
