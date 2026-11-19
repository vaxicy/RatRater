# -*- coding: utf-8 -*-
"""扫描 .NET 程序集元数据：列出老鼠/基因/模组相关的类、字段、方法。"""
import sys
import dnfile

KEYWORDS = ("rat", "gene", "mouse", "mod", "cage", "breeding", "offspring")


def row_of(table_rows, idx):
    if idx is None:
        return None
    n = getattr(idx, "row_index", None)
    if n is None or n < 1 or n > len(table_rows):
        return None
    return table_rows[n - 1]


def idx_list(value):
    if value is None:
        return []
    if isinstance(value, list):
        return value
    return [value]


def scan(path, keywords):
    pe = dnfile.dnPE(path)
    t = pe.net.mdtables
    tdefs = t.TypeDef.rows
    frows = t.Field.rows if t.Field else []
    mrows = t.MethodDef.rows if t.MethodDef else []

    print("=== FILE:", path)
    print("typedefs:", len(tdefs))
    print()

    for td in tdefs:
        name = str(td.TypeName)
        ns = str(td.TypeNamespace)
        full = (ns + "." + name).lower()
        if not any(k in full for k in keywords):
            continue
        print("TYPE:", ns + "." + name)

        for idx in idx_list(td.FieldList):
            f = row_of(frows, idx)
            if f is not None:
                print("    FIELD:", str(f.Name))
        for idx in idx_list(td.MethodList):
            m = row_of(mrows, idx)
            if m is not None:
                print("    METHOD:", str(m.Name))
        print()


if __name__ == "__main__":
    scan(sys.argv[1], [k.lower() for k in sys.argv[2:]] or KEYWORDS)
