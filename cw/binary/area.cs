//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;
import event;
import bgimage;

import cw;


class Area : base.CWBinaryBase {
    // """widファイルのエリアデータ。""";
    public Area(UNK parent, UNK f, bool yadodata=false, bool nameonly=false, string materialdir="Material", bool image_export=true) : base(parent, f, yadodata, materialdir, image_export){
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
            }
        } else {
            dataversion = 4;
            b = f.byte();
            b = f.byte();
            b = f.byte();
            this.name = f.string();
            this.id = f.dword() - 40000;
        }

        if (nameonly) {
            return;
        }

        events_num = f.dword();
        this.events = [event.Event(self, f) for _cnt in xrange(events_num)];
        this.spreadtype = f.byte();
        mcards_num = f.dword();
        this.mcards = [MenuCard(self, f, dataversion=dataversion) for _cnt in xrange(mcards_num)];
        bgimgs_num = f.dword();
        this.bgimgs = [bgimage.BgImage(self, f) for _cnt in xrange(bgimgs_num)];

        this.data = null;
    }

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Area");
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Id", str(this.id));
            prop.append(e);
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            this.data.append(prop);
            e = cw.data.make_element("BgImages");
            foreach (var bgimg in this.bgimgs) {
                e.append(bgimg.get_data());
            }
            this.data.append(e);
            e = cw.data.make_element("MenuCards");
            e.set("spreadtype", this.conv_spreadtype(this.spreadtype));
            foreach (var mcard in this.mcards) {
                e.append(mcard.get_data());
            }
            this.data.append(e);
            e = cw.data.make_element("Events");
            foreach (var event in this.events) {
                e.append(event.get_data());
            }
            this.data.append(e);
        }
        return this.data;
    }

    public static UNK unconv(UNK f, UNK data) {
        restype = 0;
        name = "";
        resid = 0;
        events = [];
        spreadtype = 0;
        mcards = [];
        bgimgs = [];

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        resid = int(prop.text);
                    } else if (prop.tag == "Name") {
                        name = prop.text;
                    }
                }
            } else if (e.tag == "BgImages") {
                bgimgs = e;
            } else if (e.tag == "PlayerCardEvents") {
                if (e.Count) {
                    f.check_wsnversion("2");
                }
            } else if (e.tag == "MenuCards") {
                mcards = e;
                spreadtype = base.CWBinaryBase.unconv_spreadtype(e.get("spreadtype"));
            } else if (e.tag == "Events") {
                events = e;
            }
        }

        f.write_byte(restype);
        f.write_dword(0); // 不明
        f.write_string(name);
        f.write_dword(resid + 40000);
        f.write_dword(events.Count);
        foreach (var evt in events) {
            event.Event.unconv(f, evt);
        }
        f.write_byte(spreadtype);
        f.write_dword(mcards.Count);
        foreach (var mcard in mcards) {
            MenuCard.unconv(f, mcard);
        }
        f.write_dword(bgimgs.Count);
        foreach (var bgimg in bgimgs) {
            bgimage.BgImage.unconv(f, bgimg);
        }
    }
}

class MenuCard : base.CWBinaryBase {
    // """メニューカードのデータ。""";
    public MenuCard(UNK parent, UNK f, bool yadodata=false, UNK dataversion=4) : base(parent, f, yadodata) {
        _b = f.byte(); // 不明
        this.image = f.image();
        this.name = f.string();
        _dw = f.dword() // 不明
        this.description = f.string(true);
        events_num = f.dword();
        this.events = [event.Event(self, f) for _cnt in xrange(events_num)];
        this.flag = f.string();
        this.scale = f.dword();
        this.left = f.dword();
        this.top = f.dword();
        if (dataversion <= 2) {
            this.imgpath = "";
        } else {
            this.imgpath = f.string();
        }
        this.data = null;
    }

    public UNK get_data() {
        if (this.data == null) {
            if (this.image) {
                this.imgpath = this.export_image();
            } else {
                this.imgpath = this.get_materialpath(this.imgpath);
            }
            this.data = cw.data.make_element("MenuCard");
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            if (this.imgpath in ("1", "2", "3", "4", "5", "6")) {
                e = cw.data.make_element("PCNumber", this.imgpath);
            } else {
                e = cw.data.make_element("ImagePath", this.imgpath);
            }
            prop.append(e);
            e = cw.data.make_element("Description", this.description);
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
            }
            this.data.append(e);
        }
        return this.data;
    }

    public static UNK unconv(UNK f, UNK data) {
        image = null;
        name = "";
        description = "";
        events = [];
        flag = "";
        scale = 0;
        left = 0;
        top = 0;
        imgpath = "";

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag == "ImagePath") {
                        base.CWBinaryBase.check_imgpath(f, prop, "TopLeft");
                        imgpath = base.CWBinaryBase.materialpath(prop.text);
                    } else if (prop.tag == "ImagePaths") {
                        if (1 < prop.Count) {
                            f.check_wsnversion("1");
                        } else {
                            base.CWBinaryBase.check_imgpath(f, prop.find("ImagePath"), "TopLeft");
                            imgpath2 = prop.gettext("ImagePath", "");
                            if (imgpath2) {
                                imgpath = base.CWBinaryBase.materialpath(imgpath2);
                            }
                        }
                    } else if (prop.tag == "PCNumber") {
                        f.check_version(1.50);
                        imgpath = prop.text;
                    } else if (prop.tag == "Description") {
                        description = prop.text;
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
                        }
                    }
                }
            } else if (e.tag == "Events") {
                events = e;
            }
        }

        f.write_byte(0) // 不明
        f.write_image(image);
        f.write_string(name);
        f.write_dword(0) // 不明
        f.write_string(description, true);
        f.write_dword(events.Count);
        foreach (var evt in events) {
            event.Event.unconv(f, evt);
        }
        f.write_string(flag);
        f.write_dword(scale);
        f.write_dword(left);
        f.write_dword(top);
        f.write_string(imgpath);
    }
}