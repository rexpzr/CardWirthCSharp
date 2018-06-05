class Album : base.CWBinaryBase {
    public int type;
    public UNK fname;
    public string name;
    public UNK image;
    public UNK level;
    
    // """wrmファイル(type=4)。鬼籍に入った冒険者のデータ。""";
    public Album(UNK parent, UNK f, bool yadodata=false) : base(parent, f, yadodata) {
        this.type = 4;
        this.fname = this.get_fname();
        if (f) {
            f.byte();
            f.byte();
            this.name = f.string();
            this.image = f.image();
            this.level = f.word();
            _w = f.word(); // 不明(能力修正？)
            _w = f.word(); // 不明(能力修正？)
            _w = f.word(); // 不明(能力修正？)
            // ここからは16ビット符号付き整数が並んでると思われるが面倒なので
            // 能力値
            this.dex = f.byte();
            f.byte();
            this.agl = f.byte();
            f.byte();
            this.int = f.byte();
            f.byte();
            this.str = f.byte();
            f.byte();
            this.vit = f.byte();
            f.byte();
            this.min = f.byte();
            f.byte();
            // 性格値
            this.aggressive = f.byte();
            f.byte();
            this.cheerful = f.byte();
            f.byte();
            this.brave = f.byte();
            f.byte();
            this.cautious = f.byte();
            f.byte();
            this.trickish = f.byte();
            f.byte();
            // 修正能力値
            this.avoid = f.byte();
            f.byte();
            this.resist = f.byte();
            f.byte();
            this.defense = f.byte();
            f.byte();
            f.dword();
            this.description = f.string(true).replace("TEXT\\n", "", 1);
            // クーポン
            coupons_num = f.dword();
            this.coupons = [coupon.Coupon(self, f) for _cnt in xrange(coupons_num)];
        } else {
            this.name = 0;
            this.image = 0;
            this.level = 0;
            this.dex = 0;
            this.agl = 0;
            this.int = 0;
            this.str = 0;
            this.vit = 0;
            this.min = 0;
            this.aggressive = 0;
            this.cheerful = 0;
            this.brave = 0;
            this.cautious = 0;
            this.trickish = 0;
            this.avoid = 0;
            this.resist = 0;
            this.defense = 0;
            this.description = u"";
            this.coupons = [];
        }

        this.data = null;
    }

    public UNK get_data() {
        if (this.data == null) {
            if (this.image) {
                this.imgpath = this.export_image();
            } else {
                this.imgpath = "";
            }

            this.data = cw.data.make_element("Album");

            prop = cw.data.make_element("Property");

            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("ImagePath", this.imgpath);
            prop.append(e);
            e = cw.data.make_element("Description", this.description);
            prop.append(e);
            e = cw.data.make_element("Level", str(this.level));
            prop.append(e);

            ae = cw.data.make_element("Ability");
            e = cw.data.make_element("Physical");
            e.set("dex", str(this.dex));
            e.set("agl", str(this.agl));
            e.set("int", str(this.int));
            e.set("str", str(this.str));
            e.set("vit", str(this.vit));
            e.set("min", str(this.min));
            ae.append(e);
            e = cw.data.make_element("Mental");
            e.set("aggressive", str(this.aggressive));
            e.set("cheerful", str(this.cheerful));
            e.set("brave", str(this.brave));
            e.set("cautious", str(this.cautious));
            e.set("trickish", str(this.trickish));
            ae.append(e);
            e = cw.data.make_element("Enhance");
            e.set("avoid", str(this.avoid));
            e.set("resist", str(this.resist));
            e.set("defense", str(this.defense));
            ae.append(e);
            prop.append(ae);

            ce = cw.data.make_element("Coupons");
            foreach (var coupon in this.coupons) {
                ce.append(coupon.get_data());
            }
            prop.append(ce);

            this.data.append(prop);
        }

        return this.data;
    }

    public UNK create_xml(string dpath) {
        path = base.CWBinaryBase.create_xml(self, dpath);
        yadodb = this.get_root().yadodb;
        if (yadodb) {
            yadodb.insert_adventurer(path, album=true, commit=false);
        }
        return path;
    }

    public static void unconv(f, data) {
        name = "";
        image = null;
        level = 0;
        dex = 0;
        agl = 0;
        inte = 0;
        stre = 0;
        vit = 0;
        mind = 0;
        aggressive = 0;
        cheerful = 0;
        brave = 0;
        cautious = 0;
        trickish = 0;
        avoid = 0;
        resist = 0;
        defense = 0;
        description = "";
        coupons = [];

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag in ("ImagePath", "ImagePaths")) {
                        image = base.CWBinaryBase.import_image(f, prop, defpostype="Center");
                    } else if (prop.tag == "Description") {
                        description = prop.text;
                    } else if (prop.tag == "Level") {
                        level = int(prop.text);
                    } else if (prop.tag == "Ability") {
                        foreach (var ae in prop) {
                            if (ae.tag == "Physical") {
                                dex = int(ae.get("dex"));
                                agl = int(ae.get("agl"));
                                inte = int(ae.get("int"));
                                stre = int(ae.get("str"));
                                vit = int(ae.get("vit"));
                                mind = int(ae.get("min"));
                            } else if (ae.tag == "Mental") {
                                aggressive = int(ae.get("aggressive"));
                                cheerful = int(ae.get("cheerful"));
                                brave = int(ae.get("brave"));
                                cautious = int(ae.get("cautious"));
                                trickish = int(ae.get("trickish"));
                            } else if (ae.tag == "Enhance") {
                                avoid = int(ae.get("avoid"));
                                resist = int(ae.get("resist"));
                                defense = int(ae.get("defense"));
                            }
                        }
                    } else if (prop.tag == "Coupons") {
                        coupons = prop;
                    }
                }
            }
        }

        f.write_byte(0);
        f.write_byte(0);
        f.write_string(name);
        f.write_image(image);
        f.write_word(level);
        f.write_word(0); // 不明
        f.write_word(0); // 不明
        f.write_word(0); // 不明
        f.write_word(dex);
        f.write_word(agl);
        f.write_word(inte);
        f.write_word(stre);
        f.write_word(vit);
        f.write_word(mind);
        f.write_word(aggressive);
        f.write_word(cheerful);
        f.write_word(brave);
        f.write_word(cautious);
        f.write_word(trickish);
        f.write_word(avoid);
        f.write_word(resist);
        f.write_word(defense);
        f.write_dword(0);
        f.write_string("TEXT\n" + (description if description else u""), true);
        f.write_dword(len(coupons));
        foreach (var cp in coupons) {
            coupon.Coupon.unconv(f, cp);
        }
    }
}
