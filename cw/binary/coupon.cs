//!/usr/bin/env python
// -*- coding: utf-8 -*-

import re;

import base;

import cw;

_120gene = re.compile(ur"\A＠Ｇ[01]{10}-[0-9]+\Z");

class Coupon(base.CWBinaryBase):
    """クーポンデータ。""";
    public UNK __init__(parent, f, yadodata=false, dataversion=5) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        if (f) {
            this.name = f.string();
            this.value = f.dword();
            if (dataversion <= 4) {
                if (_120gene.match(this.name)) {
                    this.value = int(this.name[13:]);
                    this.name = this.name[:12];
        } else {
            this.name = "";
            this.value = 0;

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Coupon", this.name);
            this.data.set("value", str(this.value));
        return this.data;

    @staticmethod;
    def unconv(f, data):
        name = data.text;
        value = int(data.get("value"));

        f.write_string(name);
        f.write_dword(value);

def main():
    pass;

if __name__ == "__main__":
    main();
