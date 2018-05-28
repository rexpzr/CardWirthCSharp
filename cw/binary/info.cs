//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;

import cw;


class InfoCard(base.CWBinaryBase):
    """widファイルの情報カードのデータ。""";
    public UNK __init__(parent, f, yadodata=false, nameonly=false, materialdir="Material", image_export=true) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata, materialdir, image_export);
        this.type = f.byte();
        this.image = f.image();
        this.name = f.string();
        idl = f.dword();

        if (idl <= 19999) {
            _dataversion = 0;
            this.id = idl;
        } else if (idl <= 39999) {
            _dataversion = 2;
            this.id = idl - 20000;
        } else {
            _dataversion = 4;
            this.id = idl - 40000;

        if (nameonly) {
            return;

        this.description = f.string(true);

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            if (this.image) {
                this.imgpath = this.export_image();
            } else {
                this.imgpath = "";
            this.data = cw.data.make_element("InfoCard");
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Id", str(this.id));
            prop.append(e);
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("ImagePath", this.imgpath);
            prop.append(e);
            e = cw.data.make_element("Description", this.description);
            prop.append(e);
            this.data.append(prop);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        restype = 4;
        image = null;
        name = "";
        resid = 0;
        description = "";

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        resid = int(prop.text);
                    } else if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag in ("ImagePath", "ImagePaths")) {
                        image = base.CWBinaryBase.import_image(f, prop);
                    } else if (prop.tag == "Description") {
                        description = prop.text;

        f.write_byte(restype);
        f.write_image(image);
        f.write_string(name);
        f.write_dword(resid + 40000);
        f.write_string(description, true);

def main():
    pass;

if __name__ == "__main__":
    main();
