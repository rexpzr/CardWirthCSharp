//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;

import cw;


class Dialog(base.CWBinaryBase):
    """台詞データ""";
    public UNK __init__(parent, f, yadodata=false) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        this.coupons = f.string(true);
        this.text = f.string(true);

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Dialog");
            e = cw.data.make_element("RequiredCoupons", this.coupons);
            this.data.append(e);
            e = cw.data.make_element("Text", this.text);
            this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        coupons = "";
        text = "";

        foreach (var e in data) {
            if (e.tag == "RequiredCoupons") {
                coupons = e.text;
            } else if (e.tag == "Text") {
                text = e.text;

        f.write_string(coupons, true);
        f.write_string(text, true);

def main():
    pass;

if __name__ == "__main__":
    main();
