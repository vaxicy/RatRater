# -*- coding: utf-8 -*-
"""反编译 .NET 程序集里指定方法的 IL，打印 token 对应的成员名。"""
import sys
from dnfile import dnPE
from dncil.cil.body import CilMethodBody
from dncil_reader import DnfileCilReader


def resolve(pe, token):
    t = pe.net.mdtables
    rid = getattr(token, 'rid', 0)
    tnum = getattr(token, 'table', None)
    tbl = t.tables.get(tnum)
    if tbl is not None:
        rows = tbl.rows
        if rows and 1 <= rid <= len(rows):
            row = rows[rid - 1]
            parts = [str(getattr(row, k, '')) for k in ('TypeNamespace', 'TypeName', 'ClassName', 'Name') if getattr(row, k, None) is not None]
            parts = [p for p in parts if p]
            if parts:
                return '.'.join(parts)
        return '<table%s rid=%s>' % (tnum, rid)
    return '<tok %s>' % getattr(token, 'value', token)


def dump_method(pe, method):
    print("METHOD:", str(method.Name))
    rva = getattr(method, 'Rva', 0)
    if not rva:
        print("  <no body>")
        return
    body = CilMethodBody(DnfileCilReader(pe, rva))
    from dncil.cil.opcode import OpCodes
    for ins in body.instructions:
        op = ins.opcode
        operand = ins.operand
        extra = ""
        if operand is not None:
            if hasattr(operand, 'table'):
                extra = " ; " + resolve(pe, operand)
            elif isinstance(operand, str):
                extra = " ; " + operand
        print("  %-12s %s%s" % (op.name, operand if operand is not None else '', extra))


def main(path, filter_text):
    pe = dnPE(path)
    t = pe.net.mdtables
    mrows = t.MethodDef.rows if t.MethodDef else []
    tdefs = t.TypeDef.rows
    for td in tdefs:
        full = str(td.TypeNamespace) + "." + str(td.TypeName)
        mids = []
        fl = td.MethodList
        if isinstance(fl, list):
            mids = [getattr(i, 'row_index', None) for i in fl]
        elif fl is not None:
            mids = [getattr(fl, 'row_index', None)]
        for mid in mids:
            if not mid or mid < 1 or mid > len(mrows):
                continue
            m = mrows[mid - 1]
            if filter_text.lower() in (full + "." + str(m.Name)).lower():
                print("=== TYPE:", full)
                dump_method(pe, m)
                print()


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else "")
