# -*- coding: utf-8 -*-
"""dump 程序集引用的外部类型/成员与用户字符串。"""
import sys
from dnfile import dnPE

TABLES = ('TypeRef', 'MemberRef', 'Field', 'MethodDef', 'TypeSpec')


def s(v):
    if v is None:
        return ''
    if isinstance(v, bytes):
        return v.decode('utf-8', 'replace')
    return str(v)


def main(path):
    pe = dnPE(path)
    t = pe.net.mdtables

    print("=== TypeRef ===")
    for row in t.TypeRef.rows:
        print(s(row.TypeNamespace) + '.' + s(row.TypeName))

    print("\n=== MemberRef ===")
    for row in t.MemberRef.rows:
        cls = ''
        c = getattr(row, 'Class', None)
        if c is not None:
            rid = getattr(c, 'row_index', 0)
            tbl = getattr(c, 'table', None)
            tnum = getattr(tbl, 'value', None)
            tab = t.tables.get(tnum)
            if tab and 1 <= rid <= len(tab.rows):
                r = tab.rows[rid - 1]
                cls = s(getattr(r, 'TypeName', ''))
        print(cls + '::' + s(row.Name))

    print("\n=== UserStrings ===")
    try:
        us = pe.net.user_strings
        for item in us:
            v = getattr(item, 'value', None)
            if v:
                print(repr(s(v)))
    except Exception as e:
        print('<err>', e)


if __name__ == '__main__':
    main(sys.argv[1])
