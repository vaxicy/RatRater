# -*- coding: utf-8 -*-
"""dncil reader adapter backed by dnfile."""
import io
from dncil.cil.body.reader import CilMethodBodyReaderBase


class DnfileCilReader(CilMethodBodyReaderBase):
    def __init__(self, pe, rva):
        self.pe = pe
        self.rva = rva
        self.off = pe.get_offset_from_rva(rva)
        self.buf = pe.__data__

    def read(self, n):
        b = self.buf[self.off:self.off + n]
        self.off += n
        self.rva += n
        return bytes(b)

    def tell(self):
        return self.rva

    def seek(self, rva):
        self.rva = rva
        self.off = self.pe.get_offset_from_rva(rva)
        return self.rva
