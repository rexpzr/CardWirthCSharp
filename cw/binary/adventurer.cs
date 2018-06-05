import sys;

import base;
import item;
import skill;
import beast;
import coupon;

import cw;


class Adventurer : base.CWBinaryBase {
    private void Adventurer__add_128coupons() {
        epc = coupon.Coupon(this, null);
        epc.name = "＠ＥＰ";
        epc.value = max(0, this.level - 1) * 10;
        this.coupons.insert(0, epc);
        lbc = coupon.Coupon(this, null);
        lbc.name = "＠レベル原点";
        lbc.value = this.level;
        this.coupons.insert(0, lbc);
    }
    
    // """冒険者データ。埋め込み画像はないので
    // wch・wptファイルから個別に引っ張ってくる必要がある。
    // """
    public Adventurer(UNK parent, UNK f, bool yadodata=false, bool nameonly=false, bool album120=false) : base(parent, f, yadodata) {

        if (album120) {
            // 1.20のアルバムデータ
            this.id = 0;

            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明(表示順？)
            this.name = f.string();
            this.imgpath = "";
            this.level = f.dword();
            this.money = f.dword();
            // 能力修正値(デフォルト)
            this.avoid = f.dword();
            this.resist = f.dword();
            this.defense = f.dword();
            // 各能力値*5
            this.dex = f.dword();
            this.agl = f.dword();
            this.int = f.dword();
            this.str = f.dword();
            this.vit = f.dword();
            this.min = f.dword();
            // 性格値*5
            this.aggressive = f.dword();
            this.cheerful = f.dword();
            this.brave = f.dword();
            this.cautious = f.dword();
            this.trickish = f.dword();
            // mate特有の属性値(真偽値)*10
            this.noeffect_weapon = f.bool();
            this.noeffect_magic = f.bool();
            this.undead = f.bool();
            this.automaton = f.bool();
            this.unholy = f.bool();
            this.constructure = f.bool();
            this.resist_fire = f.bool();
            this.resist_ice = f.bool();
            this.weakness_fire = f.bool();
            this.weakness_ice = f.bool();

            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明
            _dw = f.dword(); // 不明

            this.image = f.image();
            this.description = f.string(true).replace("TEXT\\n", "", 1);

            // 体力を計算
            vit = max(1, this.vit);
            minval = max(1, this.min);
            this.life = cw.character.calc_maxlife(vit, minval, this.level);
            this.maxlife = this.life;

            // クーポン
            this.is_dead = false;
            coupons_num = f.dword();
            this.coupons = [];
            foreach (var _cnt in xrange(coupons_num)) {
                c = coupon.Coupon(self, f, dataversion=4);
                if (c.name == "＿死亡") {
                    this.is_dead = true;
                }
                this.coupons.append(c);
            }
            this.Adventurer__add_128coupons();

            // 精神状態
            this.mentality = 0;
            this.duration_mentality = 0;

            // 状態異常の値(持続ターン数)
            this.paralyze = 0;
            this.poison = 0;

            // 各状態異常の持続ターン数
            this.duration_bind = 0;
            this.duration_silence = 0;
            this.duration_faceup = 0;
            this.duration_antimagic = 0;

            // 能力修正値(効果モーション)
            this.enhance_action = 0;
            this.duration_enhance_action = 0;
            this.enhance_avoid = 0;
            this.duration_enhance_avoid = 0;
            this.enhance_resist = 0;
            this.duration_enhance_resist = 0;
            this.enhance_defense = 0;
            this.duration_enhance_defense = 0;

            // 所持カード
            this.items = [];
            this.skills = [];
            this.beasts = [];

            this.data = null;
            this.f9data = null;

            return;
        }

        this.image = null;
        this.name = f.string();
        if (nameonly) {
            return;
        }

        idl = f.dword();

        if (idl <= 19999) {
            dataversion = 0;
            this.id = idl;
        } else if (idl <= 39999) {
            dataversion = 2;
            this.id = idl - 20000;
        } else if (idl <= 49999) {
            dataversion = 4;
            this.id = idl - 40000;
        } else {
            dataversion = 5;
            this.id = idl - 50000;
        }

        this.imgpath = "";

        // mate特有の属性値(真偽値)*10
        this.noeffect_weapon = f.bool();
        this.noeffect_magic = f.bool();
        this.undead = f.bool();
        this.automaton = f.bool();
        this.unholy = f.bool();
        this.constructure = f.bool();
        this.resist_fire = f.bool();
        this.resist_ice = f.bool();
        this.weakness_fire = f.bool();
        this.weakness_ice = f.bool();

        this.level = f.dword();
        if (dataversion <= 4) {
            this.money = f.dword();
        } else {
            this.money = 0;
        }
        this.description = f.string(true).replace("TEXT\\n", "", 1);
        this.life = f.dword();
        this.maxlife = f.dword();

        // 状態異常の値(持続ターン数)
        this.paralyze = f.dword();
        this.poison = f.dword();

        // 能力修正値(デフォルト)
        this.avoid = f.dword();
        this.resist = f.dword();
        this.defense = f.dword();

        // 各能力値*5
        this.dex = f.dword();
        this.agl = f.dword();
        this.int = f.dword();
        this.str = f.dword();
        this.vit = f.dword();
        this.min = f.dword();

        // 性格値*5
        this.aggressive = f.dword();
        this.cheerful = f.dword();
        this.brave = f.dword();
        this.cautious = f.dword();
        this.trickish = f.dword();

        // 精神状態
        this.mentality = f.byte();
        this.duration_mentality = f.dword();

        // 各状態異常の持続ターン数
        this.duration_bind = f.dword();
        this.duration_silence = f.dword();
        this.duration_faceup = f.dword();
        this.duration_antimagic = f.dword();

        // 能力修正値(効果モーション)
        this.enhance_action = f.dword();
        this.duration_enhance_action = f.dword();
        this.enhance_avoid = f.dword();
        this.duration_enhance_avoid = f.dword();
        this.enhance_resist = f.dword();
        this.duration_enhance_resist = f.dword();
        this.enhance_defense = f.dword();
        this.duration_enhance_defense = f.dword();

        // 所持カード
        items_num = f.dword();
        this.items = [item.ItemCard(self, f) for _cnt in xrange(items_num)];
        skills_num = f.dword();
        this.skills = [skill.SkillCard(self, f) for _cnt in xrange(skills_num)];
        beasts_num = f.dword();
        this.beasts = [beast.BeastCard(self, f) for _cnt in xrange(beasts_num)];

        // クーポン
        coupons_num = f.dword();
        this.coupons = [coupon.Coupon(self, f, dataversion=dataversion) for _cnt in xrange(coupons_num)];
        if (dataversion <= 4) {
            this.Adventurer__add_128coupons();
        }

        this.data = null;
        this.f9data = null;
    }

    public UNK get_data() {
        return this._get_data(false);
    }

    public UNK get_f9data() {
        return this._get_data(true);
    }

    public UNK _get_data(f9data) {
        if (f9data) {
            data = this.f9data;
        } else {
            data = this.data;
        }

        if (data == null) {
            if (!this.imgpath) {
                if (this.image) {
                    this.imgpath = this.export_image();
                } else {
                    this.imgpath = "";
                }
            }

            data = cw.data.make_element("Adventurer");

            prop = cw.data.make_element("Property");

            e = cw.data.make_element("Id", str(this.id));
            prop.append(e);
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("ImagePath", this.imgpath);
            prop.append(e);
            e = cw.data.make_element("Description", this.description);
            prop.append(e);
            e = cw.data.make_element("Level", str(this.level));
            prop.append(e);
            e = cw.data.make_element("Life", str(this.life));
            e.set("max", str(this.maxlife));
            prop.append(e);

            fe = cw.data.make_element("Feature");
            e = cw.data.make_element("Type");
            e.set("undead", str(this.undead));
            e.set("automaton", str(this.automaton));
            e.set("unholy", str(this.unholy));
            e.set("constructure", str(this.constructure));
            fe.append(e);
            e = cw.data.make_element("NoEffect");
            e.set("weapon", str(this.noeffect_weapon));
            e.set("magic", str(this.noeffect_magic));
            fe.append(e);
            e = cw.data.make_element("Resist");
            e.set("fire", str(this.resist_fire));
            e.set("ice", str(this.resist_ice));
            fe.append(e);
            e = cw.data.make_element("Weakness");
            e.set("fire", str(this.weakness_fire));
            e.set("ice", str(this.weakness_ice));
            fe.append(e);
            prop.append(fe);

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

            se = cw.data.make_element("Status");
            e = cw.data.make_element("Mentality", this.conv_mentality(this.mentality));
            e.set("duration", str(this.duration_mentality));
            se.append(e);
            e = cw.data.make_element("Paralyze", str(this.paralyze));
            se.append(e);
            e = cw.data.make_element("Poison", str(this.poison));
            se.append(e);
            e = cw.data.make_element("Bind");
            e.set("duration", str(this.duration_bind));
            se.append(e);
            e = cw.data.make_element("Silence");
            e.set("duration", str(this.duration_silence));
            se.append(e);
            e = cw.data.make_element("FaceUp");
            e.set("duration", str(this.duration_faceup));
            se.append(e);
            e = cw.data.make_element("AntiMagic");
            e.set("duration", str(this.duration_antimagic));
            se.append(e);
            prop.append(se);

            ee = cw.data.make_element("Enhance");
            e = cw.data.make_element("Action", str(this.enhance_action));
            e.set("duration", str(this.duration_enhance_action));
            ee.append(e);
            e = cw.data.make_element("Avoid", str(this.enhance_avoid));
            e.set("duration", str(this.duration_enhance_avoid));
            ee.append(e);
            e = cw.data.make_element("Resist", str(this.enhance_resist));
            e.set("duration", str(this.duration_enhance_resist));
            ee.append(e);
            e = cw.data.make_element("Defense", str(this.enhance_defense));
            e.set("duration", str(this.duration_enhance_defense));
            ee.append(e);
            prop.append(ee);

            ce = cw.data.make_element("Coupons");
            if (f9data) {
                // "＿１"などの番号クーポン以降を除去
                numcoupons = set(["＿１", "＿２", "＿３", "＿４", "＿５", "＿６"]);
                coupons = this.coupons;
                cut = false;
                foreach (var i, coupon in enumerate(coupons)) {
                    if (coupon.name in numcoupons) {
                        coupons = coupons[:i];
                        cut = true;
                        break;
                    }
                }

                if (!cut && cw.cwpy.msgs["number_1_coupon"] != "＿１") {
                    // バリアントによっては"＿１"が別の文字列に置換されている
                    // 可能性があるので、それを加えて再度判断する
                    numcoupons.add(cw.cwpy.msgs["number_1_coupon"]);
                    foreach (var i, coupon in enumerate(coupons)) {
                        if (coupon.name in numcoupons) {
                            coupons = coupons[:i];
                            break;
                        }
                    }
                }

                // '＾'で始まるクーポンは'＾'を取り除く
                foreach (var coupon in coupons) {
                    if (coupon.name.startswith("＾")) {
                        cdata = coupon.get_data();
                        cdata.text = coupon.name[1:];
                        ce.append(cdata);
                    } else {
                        ce.append(coupon.get_data());
                    }
                }
            } else {
                // '＾'で始まるクーポンはシナリオ内で削除済みのもの
                foreach (var coupon in this.coupons) {
                    if (!coupon.name.startswith("＾")) {
                        ce.append(coupon.get_data());
                    }
                }
            }

            prop.append(ce);

            data.append(prop);

            e = cw.data.make_element("ItemCards");
            foreach (var card in this.items) {
                if (!f9data || card.premium <= 2) {
                    card.set_image_export(false, f9data);
                    e.append(card.get_data());
                }
            }
            data.append(e);

            e = cw.data.make_element("SkillCards");
            foreach (var card in this.skills) {
                if (!f9data || card.premium <= 2) {
                    card.set_image_export(false, f9data);
                    e.append(card.get_data());
                }
            }
            data.append(e);

            e = cw.data.make_element("BeastCards");
            foreach (var card in this.beasts) {
                if (!f9data || card.premium <= 2) {
                    if (f9data && card.attachment) {
                        continue;
                    }
                    card.set_image_export(false, f9data);
                    e.append(card.get_data());
                }
            }
            data.append(e);

            if (f9data) {
                ccard = cw.character.Character(cw.data.CWPyElementTree(element=data));
                ccard.set_fullrecovery();
            }
        }

        if (f9data) {
            this.f9data = data;
        } else {
            this.data = data;
        }

        return data;
    }

    public UNK create_xml(UNK dpath) {
        path = base.CWBinaryBase.create_xml(self, dpath);
        yadodb = this.get_root().yadodb;
        if (yadodb) {
            yadodb.insert_adventurer(path, album=false, commit=false);
        }
        return path;
    }

    public static void unconv(f, data, logdata) {
        if (!logdata == null) {
            // 変換用にクーポンを整理
            coupons = cw.data.make_element("Coupons");
            coupons1 = {};
            coupons2 = set();
            foreach (var e in data.getfind("Property/Coupons")) {
                coupons1[e.text] = e;
                coupons2.add(e);
            }

            foreach (var e in logdata.getfind("Property/Coupons")) {
                if (e.text in coupons1) {
                    coupons.append(e);
                    coupons2.remove(coupons1[e.text]);
                    del coupons1[e.text];
                } else {
                    // 削除されたクーポン
                    coupons.append(cw.data.make_element("Coupon", "＾" + e.text, { "value":e.get("value", "0") }));
                }
            }
            // 追加されたクーポン
            foreach (var e in data.getfind("Property/Coupons")) {
                if (e in coupons2) {
                    coupons.append(e);
                }
            }
        } else {
            coupons = data.find("Property/Coupons");
        }

        name = "";
        resid = 0;

        noeffect_weapon = false;
        noeffect_magic = false;
        undead = false;
        automaton = false;
        unholy = false;
        constructure = false;
        resist_fire = false;
        resist_ice = false;
        weakness_fire = false;
        weakness_ice = false;

        level = 0;
        description = "";
        life = 0;
        maxlife = 0;

        paralyze = 0;
        poison = 0;

        avoid = 0;
        resist = 0;
        defense = 0;

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

        mentality = 0;
        duration_mentality = 0;

        duration_bind = 0;
        duration_silence = 0;
        duration_faceup = 0;
        duration_antimagic = 0;

        enhance_action = 0;
        duration_enhance_action = 0;
        enhance_avoid = 0;
        duration_enhance_avoid = 0;
        enhance_resist = 0;
        duration_enhance_resist = 0;
        enhance_defense = 0;
        duration_enhance_defense = 0;

        items = [];
        skills = [];
        beasts = [];

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        resid = int(prop.text);
                    } else if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag == "Description") {
                        description = prop.text;
                    } else if (prop.tag == "Level") {
                        level = int(prop.text);
                    } else if (prop.tag == "Life") {
                        life = int(prop.text);
                        maxlife = int(float(prop.get("max")));
                    } else if (prop.tag == "Feature") {
                        foreach (var fe in prop) {
                            if (fe.tag == "Type") {
                                undead = cw.util.str2bool(fe.get("undead"));
                                automaton = cw.util.str2bool(fe.get("automaton"));
                                unholy = cw.util.str2bool(fe.get("unholy"));
                                constructure = cw.util.str2bool(fe.get("constructure"));
                            } else if (fe.tag == "NoEffect") {
                                noeffect_weapon = cw.util.str2bool(fe.get("weapon"));
                                noeffect_magic = cw.util.str2bool(fe.get("magic"));
                            } else if (fe.tag == "Resist") {
                                resist_fire = cw.util.str2bool(fe.get("fire"));
                                resist_ice = cw.util.str2bool(fe.get("ice"));
                            } else if (fe.tag == "Weakness") {
                                weakness_fire = cw.util.str2bool(fe.get("fire"));
                                weakness_ice = cw.util.str2bool(fe.get("ice"));
                            }
                        }
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
                                aggressive = int(float(ae.get("aggressive")));
                                cheerful = int(float(ae.get("cheerful")));
                                brave = int(float(ae.get("brave")));
                                cautious = int(float(ae.get("cautious")));
                                trickish = int(float(ae.get("trickish")));
                            } else if (ae.tag == "Enhance") {
                                avoid = int(ae.get("avoid"));
                                resist = int(ae.get("resist"));
                                defense = int(ae.get("defense"));
                            }
                        }
                    } else if (prop.tag == "Status") {
                        foreach (var se in prop) {
                            if (se.tag == "Mentality") {
                                mentality = base.CWBinaryBase.unconv_mentality(se.text);
                                duration_mentality = int(se.get("duration"));
                            } else if (se.tag == "Paralyze") {
                                paralyze = int(se.text);
                            } else if (se.tag == "Poison") {
                                poison = int(se.text);
                            } else if (se.tag == "Bind") {
                                duration_bind = int(se.get("duration"));
                            } else if (se.tag == "Silence") {
                                duration_silence = int(se.get("duration"));
                            } else if (se.tag == "FaceUp") {
                                duration_faceup = int(se.get("duration"));
                            } else if (se.tag == "AntiMagic") {
                                duration_antimagic = int(se.get("duration"));
                            }
                        }
                    } else if (prop.tag == "Enhance") {
                        foreach (var ee in prop) {
                            if (ee.tag == "Action") {
                                enhance_action = int(ee.text);
                                duration_enhance_action = int(ee.get("duration"));
                            } else if (ee.tag == "Avoid") {
                                enhance_avoid = int(ee.text);
                                duration_enhance_avoid = int(ee.get("duration"));
                            } else if (ee.tag == "Resist") {
                                enhance_resist = int(ee.text);
                                duration_enhance_resist = int(ee.get("duration"));
                            } else if (ee.tag == "Defense") {
                                enhance_defense = int(ee.text);
                                duration_enhance_defense = int(ee.get("duration"));
                            }
                        }
                    } else if (prop.tag == "Coupons") {
                        // このメソッドの冒頭を参照
                        // pass;
                    }
                }
            } else if (e.tag == "ItemCards") {
                items = e;

            } else if (e.tag == "SkillCards") {
                skills = e;

            } else if (e.tag == "BeastCards") {
                beasts = e;
            }
        }

        f.write_string(name);
        f.write_dword(resid + 50000);

        f.write_bool(noeffect_weapon);
        f.write_bool(noeffect_magic);
        f.write_bool(undead);
        f.write_bool(automaton);
        f.write_bool(unholy);
        f.write_bool(constructure);
        f.write_bool(resist_fire);
        f.write_bool(resist_ice);
        f.write_bool(weakness_fire);
        f.write_bool(weakness_ice);

        f.write_dword(level);
        f.write_string("TEXT\n" + (description if description else ""), true);
        f.write_dword(life);
        f.write_dword(maxlife);

        f.write_dword(paralyze);
        f.write_dword(poison);

        f.write_dword(avoid);
        f.write_dword(resist);
        f.write_dword(defense);

        f.write_dword(dex);
        f.write_dword(agl);
        f.write_dword(inte);
        f.write_dword(stre);
        f.write_dword(vit);
        f.write_dword(mind);

        f.write_dword(aggressive);
        f.write_dword(cheerful);
        f.write_dword(brave);
        f.write_dword(cautious);
        f.write_dword(trickish);

        f.write_byte(mentality);
        f.write_dword(duration_mentality);

        f.write_dword(duration_bind);
        f.write_dword(duration_silence);
        f.write_dword(duration_faceup);
        f.write_dword(duration_antimagic);

        f.write_dword(enhance_action);
        f.write_dword(duration_enhance_action);
        f.write_dword(enhance_avoid);
        f.write_dword(duration_enhance_avoid);
        f.write_dword(enhance_resist);
        f.write_dword(duration_enhance_resist);
        f.write_dword(enhance_defense);
        f.write_dword(duration_enhance_defense);

        lenpos = f.tell();
        f.write_dword(0);
        cardslen = 0;
        foreach (var card in items) {
            try {
                pos = f.tell();
                item.ItemCard.unconv(f, card, true);
                cardslen += 1;
            }
            catch (cw.binary.cwfile.UnsupportedError e) {
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = "%s の所持する %s は対象エンジンで使用できないため、変換しません。\n" % (name, cardname);
                    f.write_errorlog(s);
                }
            }
            catch (Exception e) {
                cw.util.print_ex(file=sys.stderr);
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = "%s の所持する %s は変換できませんでした。\n" % (name, cardname);
                    f.write_errorlog(s);
                }
            }
        }
        tell = f.tell();
        f.seek(lenpos);
        f.write_dword(cardslen);
        f.seek(tell);

        lenpos = f.tell();
        f.write_dword(0);
        cardslen = 0;
        foreach (var card in skills) {
            try {
                pos = f.tell();
                skill.SkillCard.unconv(f, card, true);
                cardslen += 1;
            } catch (cw.binary.cwfile.UnsupportedError e) {
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = "%s の所持する %s は対象エンジンで使用できないため、変換しません。\n" % (name, cardname); // TODO
                    f.write_errorlog(s);
                }
            } catch (Exception e) {
                cw.util.print_ex(file=sys.stderr);
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = "%s の所持する %s は変換できませんでした。\n" % (name, cardname);
                    f.write_errorlog(s);
                }
            }
        }
        tell = f.tell();
        f.seek(lenpos);
        f.write_dword(cardslen);
        f.seek(tell);

        lenpos = f.tell();
        f.write_dword(0);
        cardslen = 0;
        foreach (var card in beasts) {
            try {
                pos = f.tell();
                beast.BeastCard.unconv(f, card, true);
                cardslen += 1;
            } catch (cw.binary.cwfile.UnsupportedError e) {
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = "%s の所持する %s は対象エンジンで使用できないため、変換しません。\n" % (name, cardname); // TODO
                    f.write_errorlog(s);
                }
            } catch (Exception e) {
                cw.util.print_ex(file=sys.stderr);
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = "%s の所持する %s は変換できませんでした。\n" % (name, cardname); // TODO 
                    f.write_errorlog(s);
                }
            }
        }
        tell = f.tell();
        f.seek(lenpos);
        f.write_dword(cardslen);
        f.seek(tell);

        f.write_dword(len(coupons));
        foreach (var cp in coupons) {
            coupon.Coupon.unconv(f, cp);
        }

        f.truncate();
    }
}

class AdventurerCard : base.CWBinaryBase {
    // """wcpファイル(type=1)。冒険者データが中に入っているだけ。""";
    public AdventurerCard(UNK parent, UNK f, bool yadodata=false) : base(parent, f, yadodata) {
        this.type = 1;
        this.fname = this.get_fname();

        if (f) {
            // 不明(0,0,0,0,0)
            foreach (var _cnt in xrange(5)) {
                _b = f.byte();
            }

            this.adventurer = Adventurer(self, f, yadodata=yadodata);

        } else {
            this.adventurer = null;
        }
    }

    public void set_image(image) {
        // """埋め込み画像を取り込む時のメソッド。""";
        this.adventurer.image = image;
    }

    public UNK get_data() {
        return this.adventurer.get_data();
    }

    public UNK create_xml(dpath) {
        // """adventurerのデータだけxml化する。""";
        return this.adventurer.create_xml(dpath);
    }

    public static void unconv(UNK f, UNK data) {
        f.write_byte(0); // 不明
        f.write_byte(0); // 不明
        f.write_byte(0); // 不明
        f.write_byte(0); // 不明
        f.write_byte(0); // 不明
        Adventurer.unconv(f, data, null);
    }
}

class AdventurerWithImage : base.CWBinaryBase {
    // """埋め込み画像付き冒険者データ。
    // パーティデータを読み込むときに使う。
    // """
    public AdventurerWithImage(UNK parent, UNK f, bool yadodata=false) : base(parent, f, yadodata) {
        image = f.image();
        this.adventurer = Adventurer(self, f);
        this.adventurer.image = image;
    }

    public UNK get_data() {
        return this.adventurer.get_data();
    }

    public UNK get_f9data() {
        return this.adventurer.get_f9data();
    }

    public UNK create_xml(UNK dpath) {
        // """adventurerのデータだけxml化する。""";
        path = this.adventurer.create_xml(dpath);
        this.xmlpath = this.adventurer.xmlpath;
        return path;
    }

    public static void unconv(UNK f, UNK data, UNK logdata) {
        var e = data.find("Property/ImagePaths");
        if (e == null) {
            e = data.find("Property/ImagePath");
        }
        f.write_image(base.CWBinaryBase.import_image(f, e, defpostype="Center"));
        if (logdata == null) {
            cw.character.Character(data=cw.data.xml2etree(element=data)).set_fullrecovery();
        }
        Adventurer.unconv(f, data, logdata);
    }
}

class AdventurerHeader : base.CWBinaryBase {
    // """wchファイル(type=0)。おそらく宿帳表示用の簡易データと思われる。;
    // 必要なデータは埋め込み画像くらい？;
    // """;
    public AdventurerHeader(UNK parent, UNK f, bool yadodata=false, int dataversion=10) : base(parent, f, yadodata) {
        this.type = 0;
        this.fname = this.get_fname();
        if (10 <= dataversion) {
            // 1.28以降
            _b = f.byte(); // 不明(0)
            _b = f.byte(); // 不明(0)
            this.name = f.string();
            this.image = f.image();
            this.level = f.byte();
            _b = f.byte(); // 不明(0)
            this.coupons = f.string(true);
            _w = f.word(); // 不明(0)
            // ここからは16ビット符号付き整数が並んでると思われるが面倒なので
            this.ep = f.byte();
            _b = f.byte();
            this.dex = f.byte();
            _b = f.byte();
            this.agl = f.byte();
            _b = f.byte();
            this.int = f.byte();
            _b = f.byte();
            this.str = f.byte();
            _b = f.byte();
            this.vit = f.byte();
            _b = f.byte();
            this.min = f.byte();
            _b = f.byte();
        } else {
            // 1.20
            _dataversion = f.string();
            this.name = f.string();
            this.image = f.image();
            this.level = f.dword();
            _dw = f.dword(); // 不明(F)
            this.coupons = [];
            couponnum = f.dword();
            foreach (var _i in xrange(couponnum)) {
                this.coupons.append(f.string());
            }
            this.ep = this.level * 10;
        }
    }

    public static void unconv(UNK f, UNK data, UNK fname) {
        name = "";
        image = null;
        level = 0;
        coupons = "";
        dex = 0;
        agl = 0;
        inte = 0;
        stre = 0;
        vit = 0;
        mind = 0;
        ep = 0;

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag in ("ImagePath", "ImagePaths")) {
                        image = base.CWBinaryBase.import_image(f, prop, defpostype="Center");
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
                            }
                        }
                    } else if (prop.tag == "Coupons") {
                        seq = [];
                        foreach (var ce in prop) {
                            seq.append(ce.text);
                            if (ce.text == "＠ＥＰ") {
                                ep = int(ce.get("value", "0"));
                            }
                        }
                        coupons = cw.util.encodetextlist(seq);
                    }
                }
            }
        }

        f.write_word(0); // 不明
        f.write_string(name);
        f.write_image(image);
        f.write_word(level);
        f.write_string(coupons, true);
        f.write_word(0); // 不明
        f.write_word(ep);
        f.write_word(dex);
        f.write_word(agl);
        f.write_word(inte);
        f.write_word(stre);
        f.write_word(vit);
        f.write_word(mind);
    }
}
