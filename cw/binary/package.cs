//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;
import event;

import cw;


class Package(base.CWBinaryBase):
    """widファイルの情報カードのデータ。;
    type:InfoCardと区別が付くように、Packageは暫定的に"7"とする。;
    """;
    public UNK __init__(parent, f, yadodata=false, nameonly=false, materialdir="Material", image_export=true) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata, materialdir, image_export);
        this.type = 7;
        f.dword() // 不明
        this.name = f.string();
        this.id = f.dword();
        if (nameonly) {
            return;
        events_num = f.dword();
        this.events = [event.SimpleEvent(self, f) for _cnt in xrange(events_num)];

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Package");
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Id", str(this.id));
            prop.append(e);
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            this.data.append(prop);
            e = cw.data.make_element("Events");
            foreach (var event in this.events) {
                e.append(event.get_data());
            this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        name = "";
        resid = 0;
        events = [];

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        resid = int(prop.text);
                    } else if (prop.tag == "Name") {
                        name = prop.text;
            } else if (e.tag == "Events") {
                events = e;

        f.write_dword(0) // 不明
        f.write_string(name);
        f.write_dword(resid);
        f.write_dword(len(events));
        foreach (var evt in events) {
            event.SimpleEvent.unconv(f, evt);

def main():
    pass;

if __name__ == "__main__":
    main();
