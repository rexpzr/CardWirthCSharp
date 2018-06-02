//!/usr/bin/env python
// -*- coding: utf-8 -*-

import sys;

import base;
import item;
import skill;
import beast;
import coupon;

import cw;


class CastCard : base.CWBinaryBase
{
    // """キャストデータ(widファイル)。"""
    public CastCard(UNK parent, UNK f, bool yadodata=false, bool nameonly=false, string materialdir="Material", bool image_export=true) : base(parent, f, yadodata, materialdir, image_export) {
        this.type = f.byte();
        this.image = f.image();
        this.name = f.string();
        idl = f.dword();

        if (idl <= 19999) {
            dataversion = 0;
            this.id = idl;
        } else if (idl <= 39999) {
            dataversion = 2;
            this.id = idl - 20000;
        } else {
            dataversion = 4;
            this.id = idl - 40000;
        }

        if (nameonly) {
            return;
        }

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
        this.money = f.dword();
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

        if (0 < dataversion) {
            // クーポン
            coupons_num = f.dword();
            this.coupons = [coupon.Coupon(self, f) for _cnt in xrange(coupons_num)];
        } else {
            this.coupons = new List<UNK>();
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

            this.data = cw.data.make_element("CastCard");

            prop = cw.data.make_element("Property");

            e = cw.data.make_element("Id", (this.id).ToString());
            prop.append(e);
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("ImagePath", this.imgpath);
            prop.append(e);
            e = cw.data.make_element("Description", this.description);
            prop.append(e);
            e = cw.data.make_element("Level", (this.level).ToString());
            prop.append(e);
            e = cw.data.make_element("Money", (this.money).ToString());
            prop.append(e);
            e = cw.data.make_element("Life", (this.life).ToString());
            e.set("max", (this.maxlife).ToString());
            prop.append(e);

            fe = cw.data.make_element("Feature");
            e = cw.data.make_element("Type");
            e.set("undead", (this.undead).ToString());
            e.set("automaton", (this.automaton).ToString());
            e.set("unholy", (this.unholy).ToString());
            e.set("constructure", (this.constructure).ToString());
            fe.append(e);
            e = cw.data.make_element("NoEffect");
            e.set("weapon", (this.noeffect_weapon).ToString());
            e.set("magic", (this.noeffect_magic).ToString());
            fe.append(e);
            e = cw.data.make_element("Resist");
            e.set("fire", (this.resist_fire).ToString());
            e.set("ice", (this.resist_ice).ToString());
            fe.append(e);
            e = cw.data.make_element("Weakness");
            e.set("fire", (this.weakness_fire).ToString());
            e.set("ice", (this.weakness_ice).ToString());
            fe.append(e);
            prop.append(fe);

            ae = cw.data.make_element("Ability");
            e = cw.data.make_element("Physical");
            e.set("dex", (this.dex).ToString());
            e.set("agl", (this.agl).ToString());
            e.set("int", (this.int).ToString());
            e.set("str", (this.str).ToString());
            e.set("vit", (this.vit).ToString());
            e.set("min", (this.min).ToString());
            ae.append(e);
            e = cw.data.make_element("Mental");
            e.set("aggressive", (this.aggressive).ToString());
            e.set("cheerful", (this.cheerful).ToString());
            e.set("brave", (this.brave).ToString());
            e.set("cautious", (this.cautious).ToString());
            e.set("trickish", (this.trickish).ToString());
            ae.append(e);
            e = cw.data.make_element("Enhance");
            e.set("avoid", (this.avoid).ToString());
            e.set("resist", (this.resist).ToString());
            e.set("defense", (this.defense).ToString());
            ae.append(e);
            prop.append(ae);

            se = cw.data.make_element("Status");
            e = cw.data.make_element("Mentality", this.conv_mentality(this.mentality));
            e.set("duration", (this.duration_mentality).ToString());
            se.append(e);
            e = cw.data.make_element("Paralyze", (this.paralyze).ToString());
            se.append(e);
            e = cw.data.make_element("Poison", (this.poison).ToString());
            se.append(e);
            e = cw.data.make_element("Bind");
            e.set("duration", (this.duration_bind).ToString());
            se.append(e);
            e = cw.data.make_element("Silence");
            e.set("duration", (this.duration_silence).ToString());
            se.append(e);
            e = cw.data.make_element("FaceUp");
            e.set("duration", (this.duration_faceup).ToString());
            se.append(e);
            e = cw.data.make_element("AntiMagic");
            e.set("duration", (this.duration_antimagic).ToString());
            se.append(e);
            prop.append(se);

            ee = cw.data.make_element("Enhance");
            e = cw.data.make_element("Action", (this.enhance_action).ToString());
            e.set("duration", (this.duration_enhance_action).ToString());
            ee.append(e);
            e = cw.data.make_element("Avoid", (this.enhance_avoid).ToString());
            e.set("duration", (this.duration_enhance_avoid).ToString());
            ee.append(e);
            e = cw.data.make_element("Resist", (this.enhance_resist).ToString());
            e.set("duration", (this.duration_enhance_resist).ToString());
            ee.append(e);
            e = cw.data.make_element("Defense", (this.enhance_defense).ToString());
            e.set("duration", (this.duration_enhance_defense).ToString());
            ee.append(e);
            prop.append(ee);

            ce = cw.data.make_element("Coupons");
            foreach (var coupon in this.coupons) {
                ce.append(coupon.get_data());
            }
            prop.append(ce);

            this.data.append(prop);

            e = cw.data.make_element("ItemCards");
            foreach (var card in this.items) {
                e.append(card.get_data());
            }
            this.data.append(e);

            e = cw.data.make_element("SkillCards");
            foreach (var card in this.skills) {
                e.append(card.get_data());
            }
            this.data.append(e);

            e = cw.data.make_element("BeastCards");
            foreach (var card in this.beasts) {
                e.append(card.get_data());
            }
            this.data.append(e);
        }

        return this.data;
    }


    public static UNK unconv(UNK f, UNK data) {
        restype = 2;
        image = null;
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
        money = 0;
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

        items = new List<UNK>();
        skills = [];
        beasts = [];

        coupons = [];

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        resid = int.Parse(prop.text);
                    } else if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag in ("ImagePath", "ImagePaths")) {
                        image = base.CWBinaryBase.import_image(f, prop, defpostype="Center");
                    } else if (prop.tag == "Description") {
                        description = prop.text;
                    } else if (prop.tag == "Level") {
                        level = int.Parse(prop.text);
                    } else if (prop.tag == "Money") {
                        money = int.Parse(prop.text);
                        money = cw.util.numwrap(money, 0, 100000);
                    } else if (prop.tag == "Life") {
                        life = int.Parse(prop.text);
                        maxlife = int.Parse(prop.get("max"));
                    } else if (prop.tag == "Feature") {
                        foreach (var fe in prop) {
                            if (fe.tag == "Type") {
                                undead = cw.util.str2bool(fe.get("undead"));
                                automaton = cw.util.str2bool(fe.get("automaton"));
                                unholy = cw.util.str2bool(fe.get("unholy"));
                                constructure = cw.util.str2bool(fe.get("constructure"));
                            } else if (fe.tag == "NoEffect") {
                                noeffect_weapon = cw.util.str2bool(fe.get("noeffect_weapon"));
                                noeffect_magic = cw.util.str2bool(fe.get("noeffect_magic"));
                            } else if (fe.tag == "Resist") {
                                resist_fire = cw.util.str2bool(fe.get("resist_fire"));
                                resist_ice = cw.util.str2bool(fe.get("resist_ice"));
                            } else if (fe.tag == "Weakness") {
                                weakness_fire = cw.util.str2bool(fe.get("weakness_fire"));
                                weakness_ice = cw.util.str2bool(fe.get("weakness_ice"));
                            }
                        }
                    } else if (prop.tag == "Ability") {
                        foreach (var ae in prop) {
                            if (ae.tag == "Physical") {
                                dex = int.Parse(ae.get("dex"));
                                agl = int.Parse(ae.get("agl"));
                                inte = int.Parse(ae.get("int"));
                                stre = int.Parse(ae.get("str"));
                                vit = int.Parse(ae.get("vit"));
                                mind = int.Parse(ae.get("min"));
                            } else if (ae.tag == "Mental") {
                                aggressive = int.Parse(ae.get("aggressive"));
                                cheerful = int.Parse(ae.get("cheerful"));
                                brave = int.Parse(ae.get("brave"));
                                cautious = int.Parse(ae.get("cautious"));
                                trickish = int.Parse(ae.get("trickish"));
                            } else if (ae.tag == "Enhance") {
                                avoid = int.Parse(ae.get("avoid"));
                                resist = int.Parse(ae.get("resist"));
                                defense = int.Parse(ae.get("defense"));
                            }
                        }
                    } else if (prop.tag == "Status") {
                        foreach (var se in prop) {
                            if (se.tag == "Mentality") {
                                mentality = base.CWBinaryBase.unconv_mentality(se.text);
                                duration_mentality = int.Parse(se.get("duration"));
                            } else if (se.tag == "Paralyze") {
                                paralyze = int.Parse(se.text);
                            } else if (se.tag == "Poison") {
                                poison = int.Parse(se.text);
                            } else if (se.tag == "Bind") {
                                duration_bind = int.Parse(se.get("duration"));
                            } else if (se.tag == "Silence") {
                                duration_silence = int.Parse(se.get("duration"));
                            } else if (se.tag == "FaceUp") {
                                duration_faceup = int.Parse(se.get("duration"));
                            } else if (se.tag == "AntiMagic") {
                                duration_antimagic = int.Parse(se.get("duration"));
                            }
                        }
                    } else if (prop.tag == "Enhance") {
                        foreach (var ee in prop) {
                            if (ee.tag == "Action") {
                                enhance_action = int.Parse(ee.text);
                                duration_enhance_action = int.Parse(ee.get("duration"));
                            } else if (ee.tag == "Avoid") {
                                enhance_avoid = int.Parse(ee.text);
                                duration_enhance_avoid = int.Parse(ee.get("duration"));
                            } else if (ee.tag == "Resist") {
                                enhance_resist = int.Parse(ee.text);
                                duration_enhance_resist = int.Parse(ee.get("duration"));
                            } else if (ee.tag == "Defense") {
                                enhance_defense = int.Parse(ee.text);
                                duration_enhance_defense = int.Parse(ee.get("duration"));
                            }
                        }
                    } else if (prop.tag == "Coupons") {
                        coupons = prop;
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

        f.write_byte(restype);
        f.write_image(image);
        f.write_string(name);
        f.write_dword(resid + 40000);

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
        f.write_dword(money);
        f.write_string("TEXT\n" + (description if description else u""), true);
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
                item.ItemCard.unconv(f, card, false);
                cardslen += 1;
            }
            catch (cw.binary.cwfile.UnsupportedError e)
            {
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = u"%s の所持する %s は対象エンジンで使用できないため、変換しません。\n" % (name, cardname);
                    f.write_errorlog(s);
                }
            }
            catch (Exception e)
            {
                cw.util.print_ex(file=sys.stderr);
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = u"%s の所持する %s は変換できませんでした。\n" % (name, cardname);
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
                skill.SkillCard.unconv(f, card, false);
                cardslen += 1;
            }
            catch (cw.binary.cwfile.UnsupportedError e)
            {
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = u"%s の所持する %s は対象エンジンで使用できないため、変換しません。\n" % (name, cardname);
                    f.write_errorlog(s);
                }
            }
            catch (Exception e)
            {
                cw.util.print_ex(file=sys.stderr);
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = u"%s の所持する %s は変換できませんでした。\n" % (name, cardname);
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
                beast.BeastCard.unconv(f, card, false);
                cardslen += 1;
            }
            catch (cw.binary.cwfile.UnsupportedError e)
            {
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = u"%s の所持する %s は対象エンジンで使用できないため、変換しません。\n" % (name, cardname);
                    f.write_errorlog(s);
                }
            }
            catch (Exception e)
            {
                cw.util.print_ex(file=sys.stderr);
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = card.gettext("Property/Name", "");
                    s = u"%s の所持する %s は変換できませんでした。\n" % (name, cardname);
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