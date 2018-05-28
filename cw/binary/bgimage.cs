//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;

import cw;


class BgImage(base.CWBinaryBase):
    """背景のセルデータ。""";
    public UNK __init__(parent, f, yadodata=false) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        this.left = f.dword();
        this.top = f.dword();
        this.width = f.dword();
        if (this.width <= 39999) {
            dataversion = 2;
        } else if (this.width <= 49999) {
            dataversion = 4;
            this.width -= 40000;
        } else if (this.width <= 59999) {
            dataversion = 5;
            this.width -= 50000;
        } else {
            dataversion = 6;
            this.width -= 60000;
        this.height = f.dword();
        if (dataversion <= 5) {
            this.type = cw.sprite.background.BG_IMAGE;
            this.imgpath = f.string();
            this.mask = f.bool();
            if (2 < dataversion) {
                this.flag = f.string();
                this.unknown = f.byte();
            } else {
                this.flag = "";
                this.unknown = 0;
        } else {
            bgtype = f.byte();
            if (bgtype == 2) {
                // テキストセル
                this.type = cw.sprite.background.BG_TEXT;
                this.mask = f.bool();
                this.text = f.string(true);
                this.fontface = f.string();
                this.fontsize = f.dword();
                r = f.ubyte();
                g = f.ubyte();
                b = f.ubyte();
                a = f.ubyte();
                this.color = (r, g, b, a);
                style = f.byte();
                this.bold      = (style & 0b00000001) != 0;
                this.italic    = (style & 0b00000010) != 0;
                this.underline = (style & 0b00000100) != 0;
                this.strike    = (style & 0b00001000) != 0;
                this.bordering = (style & 0b00010000) != 0;
                this.vertical  = (style & 0b00100000) != 0;
                if (this.bordering) {
                    this.btype = f.byte();
                    r = f.ubyte();
                    g = f.ubyte();
                    b = f.ubyte();
                    a = f.ubyte();
                    this.bcolor = (r, g, b, a);
                    this.bwidth = f.dword();
                } else {
                    this.btype = -1;
                    this.bcolor = (255, 255, 255, 255);
                    this.bwidth = 1;
                f.byte() // 不明(100)
                f.dword() // 不明(0)
                f.dword() // 不明(0)
                f.byte() // 不明(縦書き時:2,他:0)
                this.flag = f.string();
                this.unknown = f.byte();

            } else if (bgtype == 3) {
                // カラーセル
                this.type = cw.sprite.background.BG_COLOR;
                this.blend = f.byte();
                this.gradient = f.byte();
                b = f.ubyte() // RGBの順序が逆
                g = f.ubyte();
                r = f.ubyte();
                a = f.ubyte();
                this.color1 = (r, g, b, a);
                if (this.gradient != 0) {
                    b = f.ubyte() // RGBの順序が逆
                    g = f.ubyte();
                    r = f.ubyte();
                    a = f.ubyte();
                    this.color2 = (r, g, b, a);
                } else {
                    this.color2 = (255, 255, 255, 255);
                this.flag = f.string();
                this.unknown = f.byte();

            } else {
                throw new ValueError("Background type: %s" % (bgtype));

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            def makecolor(tag, color):
                return cw.data.make_element(tag, attrs={"r":str(color[0]),;
                                                        "g":str(color[1]),;
                                                        "b":str(color[2]),;
                                                        "a":str(color[3])});

            if (this.type == cw.sprite.background.BG_IMAGE) {
                this.data = cw.data.make_element("BgImage");
                this.data.set("mask", str(this.mask));
                e = cw.data.make_element("ImagePath", this.get_materialpath(this.imgpath));
                this.data.append(e);

            } else if (this.type == cw.sprite.background.BG_TEXT) {
                this.data = cw.data.make_element("TextCell");

                e = cw.data.make_element("Text", this.text);
                this.data.append(e);
                e = cw.data.make_element("Font", this.fontface, attrs={"size": str(this.fontsize),;
                                                                       "bold": str(this.bold),;
                                                                       "italic": str(this.italic),;
                                                                       "underline": str(this.underline),;
                                                                       "strike": str(this.strike)});
                this.data.append(e);
                e = cw.data.make_element("Vertical", str(this.vertical));
                this.data.append(e);
                e = makecolor("Color", this.color);
                this.data.append(e);

                if (this.bordering) {
                    btype = this.conv_borderingtype(this.btype);
                    e = cw.data.make_element("Bordering", attrs={"type": btype,;
                                                                 "width": str(this.bwidth)});
                    e.append(makecolor("Color", this.bcolor));
                    this.data.append(e);

            } else if (this.type == cw.sprite.background.BG_COLOR) {
                this.data = cw.data.make_element("ColorCell");

                e = cw.data.make_element("BlendMode", this.conv_blendmode(this.blend));
                this.data.append(e);
                e = makecolor("Color", this.color1);
                this.data.append(e);

                if (this.gradient != 0) {
                    dire = this.conv_gradientdir(this.gradient);
                    e = cw.data.make_element("Gradient", attrs={"direction": dire});
                    e.append(makecolor("EndColor", this.color2));
                    this.data.append(e);

            e = cw.data.make_element("Flag", this.flag);
            this.data.append(e);
            e = cw.data.make_element("Location");
            e.set("left", str(this.left));
            e.set("top", str(this.top));
            this.data.append(e);
            e = cw.data.make_element("Size");
            e.set("width", str(this.width));
            e.set("height", str(this.height));
            this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        left = 0;
        top = 0;
        width = 0;
        height = 0;
        flag = "";
        unknown = 0;

        if (data.get("cellname", "")) {
            f.check_wsnversion("1");

        // 背景画像
        imgpath = "";
        mask = cw.util.str2bool(data.get("mask", false));

        // テキストセル
        text = "";
        fontface = "";
        fontsize = 12;
        color = (0, 0, 0, 255);
        bold = false;
        italic = false;
        underline = false;
        strike = false;
        vertical = false;
        btype = -1;
        bcolor = (255, 255, 255, 255);
        bwidth = 1;

        // カラーセル
        blend = 0;
        color1 = (255, 255, 255, 255);
        gradient = 0;
        color2 = (0, 0, 0, 255);

        def getcolor(e, defcolor):
            r = int(e.get("r", str(defcolor[0])));
            g = int(e.get("g", str(defcolor[1])));
            b = int(e.get("b", str(defcolor[2])));
            a = int(e.get("a", str(defcolor[3])));
            return (r, g, b, a);

        foreach (var e in data) {
            if (e.tag == "Flag") {
                flag = e.text;
            } else if (e.tag == "Location") {
                left = int(e.get("left"));
                top = int(e.get("top"));
            } else if (e.tag == "Size") {
                width = int(e.get("width"));
                height = int(e.get("height"));

            } else if (data.tag == "BgImage") {
                if (e.tag == "ImagePath") {
                    imgpath = e.text;

            } else if (data.tag == "TextCell") {
                f.check_version(1.50);
                if (e.tag == "Text") {
                    text = e.text;
                } else if (e.tag == "Font") {
                    fontface = e.text;
                    fontsize = int(e.get("size", fontsize));
                    foreach (var e_font in e) {
                        if (e_font.tag == "Color") {
                            color = getcolor(e_font, color);
                    bold = cw.util.str2bool(e.get("bold", bold));
                    italic = cw.util.str2bool(e.get("italic", italic));
                    underline = cw.util.str2bool(e.get("underline", underline));
                    strike = cw.util.str2bool(e.get("strike", strike));
                } else if (e.tag == "Vertical") {
                    vertical = cw.util.str2bool(e.text);
                } else if (e.tag == "Bordering") {
                    btype = base.CWBinaryBase.unconv_borderingtype(e.get("type", "None"));
                    bwidth = int(e.get("width", "1"));
                    foreach (var e_bdr in e) {
                        if (e_bdr.tag == "Color") {
                            bcolor = getcolor(e_bdr, bcolor);

            } else if (data.tag == "ColorCell") {
                f.check_version(1.50);
                if (e.tag == "BlendMode") {
                    blend = base.CWBinaryBase.unconv_blendmode(e.text);
                } else if (e.tag == "Color") {
                    color1 = getcolor(e, color1);
                } else if (e.tag == "Gradient") {
                    gradient = base.CWBinaryBase.unconv_gradientdir(e.get("direction", "None"));
                    foreach (var e_grd in e) {
                        if (e_grd.tag == "EndColor") {
                            color2 = getcolor(e_grd, color2);

            } else if (data.tag == "PCCell") {
                f.check_wsnversion("1");

        if (data.tag == "BgImage") {
            f.write_dword(left);
            f.write_dword(top);
            f.write_dword(width + 50000);
            f.write_dword(height);
            f.write_string(imgpath);
            f.write_bool(mask);
            f.write_string(flag);
            f.write_byte(unknown);

        } else if (data.tag == "TextCell") {
            f.check_version(1.50);
            f.write_dword(left);
            f.write_dword(top);
            f.write_dword(width + 60000);
            f.write_dword(height);

            f.write_bool(mask);
            f.write_string(text, true);
            f.write_string(fontface);
            f.write_dword(fontsize);
            f.write_ubyte(color[0]);
            f.write_ubyte(color[1]);
            f.write_ubyte(color[2]);
            f.write_ubyte(color[3]);
            style = 0;
            if bold:        style |= 0b00000001;
            if italic:      style |= 0b00000010;
            if underline:   style |= 0b00000100;
            if strike:      style |= 0b00001000;
            if btype != -1: style |= 0b00010000;
            if vertical:    style |= 0b00100000;
            f.write_byte(style);
            if (btype != -1) {
                f.write_byte(btype);
                f.write_ubyte(bcolor[0]);
                f.write_ubyte(bcolor[1]);
                f.write_ubyte(bcolor[2]);
                f.write_ubyte(bcolor[3]);
                f.write_dword(bwidth);
            f.write_byte(100) // 不明(100)
            f.write_dword(0) // 不明(0)
            f.write_dword(0) // 不明(0)
            f.write_byte(2 if vertical else 0) // 不明(縦書き時:2,他:0)
            f.write_string(flag);
            f.write_byte(unknown);

        } else if (data.tag == "ColorCell") {
            f.check_version(1.50);
            f.write_byte(blend);
            f.write_byte(gradient);
            f.write_ubyte(color1[2]) // RGBの順序が逆
            f.write_ubyte(color1[1]);
            f.write_ubyte(color1[0]);
            f.write_ubyte(color1[3]);
            if (gradient != 0) {
                f.write_ubyte(color2[2]) // RGBの順序が逆
                f.write_ubyte(color2[1]);
                f.write_ubyte(color2[0]);
                f.write_ubyte(color2[3]);
            f.write_string(flag);
            f.write_byte(unknown);

        } else if (data.tag == "PCCell") {
            f.check_wsnversion("1");

def main():
    pass;

if __name__ == "__main__":
    main();
