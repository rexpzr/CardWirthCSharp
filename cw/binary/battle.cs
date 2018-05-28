//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;
import event;

import cw;


class Battle(base.CWBinaryBase):
    """widファイルのバトルデータ。""";
    public UNK __init__(parent, f, yadodata=false, nameonly=false, materialdir="Material", image_export=true) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata, materialdir, image_export);
        this.type = f.byte();

        // データバージョンによって処理を分岐する
        b = f.byte();
        if (b == ord('B')) {
            f.read(69) // 不明
            this.name = f.string();
            idl = f.dword();
            if (idl <= 19999) {
                dataversion = 0;
                this.id = idl;
            } else {
                dataversion = 2;
                this.id = idl - 20000;
        } else {
            dataversion = 4;
            f.byte();
            f.byte();
            f.byte();
            this.name = f.string();
            this.id = f.dword() - 40000;

        if (nameonly) {
            return;

        events_num = f.dword();
        this.events = [event.Event(self, f) for _cnt in xrange(events_num)];
        this.spreadtype = f.byte();
        ecards_num = f.dword();
        this.ecards = [EnemyCard(self, f) for _cnt in xrange(ecards_num)];
        if (0 < dataversion) {
            this.bgm = f.string();
        } else {
            this.bgm = "DefBattle.mid";

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Battle");
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Id", str(this.id));
            prop.append(e);
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("MusicPath", this.get_materialpath(this.bgm));
            prop.append(e);
            this.data.append(prop);
            e = cw.data.make_element("EnemyCards");
            e.set("spreadtype", this.conv_spreadtype(this.spreadtype));
            foreach (var ecard in this.ecards) {
                e.append(ecard.get_data());
            this.data.append(e);
            e = cw.data.make_element("Events");
            foreach (var event in this.events) {
                e.append(event.get_data());
            this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        restype = 1;
        name = "";
        resid = 0;
        events = [];
        spreadtype = 0;
        ecards = [];
        bgm = "";

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        resid = int(prop.text);
                    } else if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag == "MusicPath") {
                        bgm = base.CWBinaryBase.materialpath(prop.text);
                        f.check_bgmoptions(prop);
            } else if (e.tag == "PlayerCardEvents") {
                if (len(e)) {
                    f.check_wsnversion("2");
            } else if (e.tag == "EnemyCards") {
                ecards = e;
                spreadtype = base.CWBinaryBase.unconv_spreadtype(e.get("spreadtype"));
            } else if (e.tag == "Events") {
                events = e;

        f.write_byte(restype);
        f.write_dword(0) // 不明
        f.write_string(name);
        f.write_dword(resid + 40000);
        f.write_dword(len(events));
        foreach (var evt in events) {
            event.Event.unconv(f, evt);
        f.write_byte(spreadtype);
        f.write_dword(len(ecards));
        foreach (var ecard in ecards) {
            EnemyCard.unconv(f, ecard);
        f.write_string(bgm);

class EnemyCard(base.CWBinaryBase):
    """エネミーカード。;
    主要なデータはキャストカードを参照する。;
    escape:逃走フラグ(真偽値)。;
    """;
    public UNK __init__(parent, f, yadodata=false) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        this.cast_id = f.dword();
        events_num = f.dword();
        this.events = [event.Event(self, f) for _cnt in xrange(events_num)];
        this.flag = f.string();
        this.scale = f.dword();
        this.left = f.dword();
        this.top = f.dword();
        this.escape = f.bool();

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("EnemyCard");
            this.data.set("escape", str(this.escape));
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Id", str(this.cast_id));
            prop.append(e);
            e = cw.data.make_element("Flag", this.flag);
            prop.append(e);
            e = cw.data.make_element("Location");
            e.set("left", str(this.left));
            e.set("top", str(this.top));
            prop.append(e);
            e = cw.data.make_element("Size");
            e.set("scale", "%s%%" % (this.scale));
            prop.append(e);
            this.data.append(prop);
            e = cw.data.make_element("Events");
            foreach (var event in this.events) {
                e.append(event.get_data());
            this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        cast_id = 0;
        events = [];
        flag = "";
        scale = 0;
        left = 0;
        top = 0;
        escape = cw.util.str2bool(data.get("escape"));

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        cast_id = int(prop.text);
                    } else if (prop.tag == "Flag") {
                        flag = prop.text;
                    } else if (prop.tag == "Location") {
                        left = int(prop.get("left"));
                        top = int(prop.get("top"));
                    } else if (prop.tag == "Size") {
                        scale = prop.get("scale");
                        if (scale.endswith("%")) {
                            scale = int(scale[:-1]);
                        } else {
                            scale = int(scale);
            } else if (e.tag == "Events") {
                events = e;

        f.write_dword(cast_id);
        f.write_dword(len(events));
        foreach (var evt in events) {
            event.Event.unconv(f, evt);
        f.write_string(flag);
        f.write_dword(scale);
        f.write_dword(left);
        f.write_dword(top);
        f.write_bool(escape);

def main():
    pass;

if __name__ == "__main__":
    main();
