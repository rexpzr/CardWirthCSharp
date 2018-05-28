//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;

import cw;


class Summary(base.CWBinaryBase):
    """見出しデータ(Summary.wsm)。;
    type:見出しデータには"-1"の値を付与する。;
    """;
    def __init__(self, parent, f, yadodata=false, nameonly=false, materialdir="Material", image_export=true,;
                 wpt120=false):
        base.CWBinaryBase.__init__(self, parent, f, yadodata, materialdir, image_export);
        this.type = -1;
        this.image = f.image();
        this.name = f.string();
        if (nameonly) {
            return;
        this.description = f.string();
        this.author = f.string();
        this.required_coupons = f.string(true);
        this.required_coupons_num = f.dword();
        this.area_id = f.dword();
        if (this.area_id <= 19999) {
            this.version = 0;
        } else if (this.area_id <= 39999) {
            this.version = 2;
            this.area_id = this.area_id - 20000;
        } else if (this.area_id <= 49999) {
            this.version = 4;
            this.area_id = this.area_id - 40000;
        } else {
            // version 5～6は存在しない
            this.version = 7;
            this.area_id = this.area_id - 70000;
        steps_num = f.dword();
        this.steps = [Step(self, f) for _cnt in xrange(steps_num)];
        flags_num = f.dword();
        this.flags = [Flag(self, f) for _cnt in xrange(flags_num)];
        if (wpt120) {
            return;
        _w = f.dword() // 不明
        if (0 < this.version) {
            this.level_min = f.dword();
            this.level_max = f.dword();
        } else {
            this.level_min = 0;
            this.level_max = 0;
        // タグとスキンタイプ。読み込みが終わった後から操作する
        this.skintype = "";
        this.tags = "";

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            if (this.image) {
                this.imgpath = this.export_image();
            } else {
                this.imgpath = "";
            this.data = cw.data.make_element("Summary");
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("ImagePath", this.imgpath);
            prop.append(e);
            e = cw.data.make_element("Author", this.author);
            prop.append(e);
            e = cw.data.make_element("Description", this.description);
            prop.append(e);
            e = cw.data.make_element("Level");
            e.set("min", str(this.level_min));
            e.set("max", str(this.level_max));
            prop.append(e);
            e = cw.data.make_element("RequiredCoupons", this.required_coupons);
            e.set("number", str(this.required_coupons_num));
            prop.append(e);
            e = cw.data.make_element("StartAreaId", str(this.area_id));
            prop.append(e);
            e = cw.data.make_element("Tags", this.tags);
            prop.append(e);
            e = cw.data.make_element("Type", this.skintype);
            prop.append(e);
            this.data.append(prop);
            e = cw.data.make_element("Flags");
            foreach (var flag in this.flags) {
                e.append(flag.get_data());
            this.data.append(e);
            e = cw.data.make_element("Steps");
            foreach (var step in this.steps) {
                e.append(step.get_data());
            this.data.append(e);
            e = cw.data.make_element("Labels", "");
            this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        image = null;
        name = "";
        description = "";
        author = "";
        required_coupons = "";
        required_coupons_num = 0;
        area_id = 0;
        steps = [];
        flags = [];
        level_min = 0;
        level_max = 0;

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag in ("ImagePath", "ImagePaths")) {
                        image = base.CWBinaryBase.import_image(f, prop);
                    } else if (prop.tag == "Author") {
                        author = prop.text;
                    } else if (prop.tag == "Description") {
                        description = prop.text;
                    } else if (prop.tag == "Level") {
                        level_min = int(prop.get("min"));
                        level_max = int(prop.get("max"));
                    } else if (prop.tag == "RequiredCoupons") {
                        required_coupons = prop.text;
                        required_coupons_num = int(prop.get("number"));
                    } else if (prop.tag == "StartAreaId") {
                        level_max = int(prop.text);
            } else if (e.tag == "Flags") {
                flags = e;
            } else if (e.tag == "Steps") {
                steps = e;

        f.write_image(image);
        f.write_string(name);
        f.write_string(description);
        f.write_string(author);
        f.write_string(required_coupons, true);
        f.write_dword(required_coupons_num);
        f.write_dword(area_id + 40000);
        f.write_dword(len(steps));
        foreach (var step in steps) {
            Step.unconv(f, step);
        f.write_dword(len(flags));
        foreach (var flag in flags) {
            Flag.unconv(f, flag);
        f.write_dword(0) // 不明
        f.write_dword(level_min);
        f.write_dword(level_max);

class Step(base.CWBinaryBase):
    """ステップ定義。""";
    public UNK __init__(parent, f, yadodata=false) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        this.name = f.string();
        this.default = f.dword();
        this.variable_names = [f.string() for _cnt in xrange(10)];

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Step");
            this.data.set("default", str(this.default));
            e = cw.data.make_element("Name", this.name);
            this.data.append(e);
            e = cw.data.make_element("Value0", this.variable_names[0]);
            this.data.append(e);
            e = cw.data.make_element("Value1", this.variable_names[1]);
            this.data.append(e);
            e = cw.data.make_element("Value2", this.variable_names[2]);
            this.data.append(e);
            e = cw.data.make_element("Value3", this.variable_names[3]);
            this.data.append(e);
            e = cw.data.make_element("Value4", this.variable_names[4]);
            this.data.append(e);
            e = cw.data.make_element("Value5", this.variable_names[5]);
            this.data.append(e);
            e = cw.data.make_element("Value6", this.variable_names[6]);
            this.data.append(e);
            e = cw.data.make_element("Value7", this.variable_names[7]);
            this.data.append(e);
            e = cw.data.make_element("Value8", this.variable_names[8]);
            this.data.append(e);
            e = cw.data.make_element("Value9", this.variable_names[9]);
            this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        name = "";
        default = int(data.get("default"));
        if (data.getbool(".", "spchars", false)) {
            f.check_wsnversion("2");
        variable_names = [""] * 10;
        foreach (var e in data) {
            if (e.tag == "Name") {
                name = e.text;
            } else if (e.tag.startswith("Value")) {
                variable_names[int(e.tag[5:])] = e.text;

        f.write_string(name);
        f.write_dword(default);
        foreach (var variable_name in variable_names) {
            f.write_string(variable_name);

class Flag(base.CWBinaryBase):
    """フラグ定義。""";
    public UNK __init__(parent, f, yadodata=false) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        this.name = f.string();
        this.default = f.bool();
        this.variable_names = [f.string() for _cnt in xrange(2)];

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Flag");
            this.data.set("default", str(this.default));
            e = cw.data.make_element("Name", this.name);
            this.data.append(e);
            e = cw.data.make_element("true", this.variable_names[0]);
            this.data.append(e);
            e = cw.data.make_element("false", this.variable_names[1]);
            this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        name = "";
        default = cw.util.str2bool(data.get("default"));
        if (data.getbool(".", "spchars", false)) {
            f.check_wsnversion("2");
        variable_names = [""] * 2;
        foreach (var e in data) {
            if (e.tag == "Name") {
                name = e.text;
            } else if (e.tag == "true") {
                variable_names[0] = e.text;
            } else if (e.tag == "false") {
                variable_names[1] = e.text;

        f.write_string(name);
        f.write_bool(default);
        foreach (var variable_name in variable_names) {
            f.write_string(variable_name);

def main():
    pass;

if __name__ == "__main__":
    main();
