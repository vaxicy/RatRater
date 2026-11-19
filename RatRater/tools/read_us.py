# -*- coding: utf-8 -*-
"""按 token 读取 #US heap 用户字符串。"""
import sys
from dnfile import dnPE


def read_user_string(pe, token):
    off = token & 0xFFFFFF
    us = pe.net.user_strings
    base = us.rva if hasattr(us, 'rva') else None
    data = pe.__data__
    fo = pe.get_offset_from_rva(base) + off
    # compressed length prefix
    b = data[fo]
    if b & 0x80 == 0:
        length = b & 0x7F
        start = fo + 1
    elif b & 0xC0 == 0x80:
        length = ((b & 0x3F) << 8) | data[fo + 1]
        start = fo + 2
    else:
        length = ((b & 0x1F) << 24) | (data[fo+1] << 16) | (data[fo+2] << 8) | data[fo+3]
        start = fo + 4
    raw = data[start:start + length]
    if raw and raw[-1] in (0, 1):
        raw = raw[:-1]
    return raw.decode('utf-16-le', 'replace')


if __name__ == '__main__':
    pe = dnPE(sys.argv[1])
    for tok in sys.argv[2:]:
        t = int(tok, 16)
        print(hex(t), '=>', repr(read_user_string(pe, t)))
