//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;
import effectmotion;
import event;

import cw;


class SkillCard : base.CWBinaryBase {
    // """widファイルのスキルカードのデータ。;
    // hold(真偽値):true?だと自動選択されない。;
    // """;
    public SkillCard(UNK parent, UNK f, bool yadodata=false, bool nameonly=false, string materialdir="Material", bool image_export=true) : base(parent, f, yadodata, materialdir, image_export) {
        this.type = f.byte();
        this.image = f.image();
        this.imgpath = "";
        this.name = f.string();
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

        if (nameonly) {
            return;
        }

        if (5 <= dataversion) {
            this.fname = this.get_fname();
        }

        this.description = f.string(true);
        this.p_ability = f.dword();
        this.m_ability = f.dword();
        this.silence = f.bool();
        this.target_all = f.bool();
        this.target = f.byte();
        this.effect_type = f.byte();
        this.resist_type = f.byte();
        this.success_rate = f.dword();
        this.visual_effect = f.byte();
        motions_num = f.dword();
        this.motions = [effectmotion.EffectMotion(self, f, dataversion=dataversion) for _cnt in xrange(motions_num)]; // TODO
        this.enhance_avoid = f.dword();
        this.enhance_resist = f.dword();
        this.enhance_defense = f.dword();
        this.sound_effect = f.string();
        this.sound_effect2 = f.string();
        this.keycodes = [f.string() for _cnt in xrange(5)];
        if (2 < dataversion) {
            this.premium = f.byte();
            this.scenario_name = f.string();
            this.scenario_author = f.string();
            events_num = f.dword();
            this.events = [event.SimpleEvent(self, f) for _cnt in xrange(events_num)];
            this.hold = f.bool();
        } else {
            this.scenario_name = "";
            this.scenario_author = "";
            this.events = [];
            this.hold = false;
            if (0 < dataversion) {
                this.premium = f.byte();
            } else {
                this.premium = 0;
            }
        }

        // 宿データだとここに不明なデータ(4)が付加されている
        if (5 <= dataversion) {
            _dw = f.dword();
        }

        this.level = f.dword();
        this.limit = f.dword();

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            if (2 < this.premium) {
                // シナリオで入手したカード
                this.set_image_export(false, true);
            if (!this.imgpath) {
                if (this.image) {
                    this.imgpath = this.export_image();
                } else {
                    this.imgpath = "";
                }
            }
            this.data = cw.data.make_element("SkillCard");
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Id", (this.id).ToString());
            prop.append(e);
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("ImagePath", this.imgpath);
            prop.append(e);
            e = cw.data.make_element("Description", this.description);
            prop.append(e);
            e = cw.data.make_element("Scenario", this.scenario_name);
            prop.append(e);
            e = cw.data.make_element("Author", this.scenario_author);
            prop.append(e);
            e = cw.data.make_element("Level", (this.level).ToString());
            prop.append(e);
            e = cw.data.make_element("Ability");
            e.set("physical", this.conv_card_physicalability(this.p_ability));
            e.set("mental", this.conv_card_mentalability(this.m_ability));
            prop.append(e);
            e = cw.data.make_element("Target", this.conv_card_target(this.target));
            e.set("allrange", (this.target_all).ToString());
            prop.append(e);
            e = cw.data.make_element("EffectType", this.conv_card_effecttype(this.effect_type));
            e.set("spell", (this.silence).ToString());
            prop.append(e);
            e = cw.data.make_element("ResistType", this.conv_card_resisttype(this.resist_type));
            prop.append(e);
            e = cw.data.make_element("SuccessRate", (this.success_rate).ToString());
            prop.append(e);
            e = cw.data.make_element("VisualEffect", this.conv_card_visualeffect(this.visual_effect));
            prop.append(e);
            e = cw.data.make_element("Enhance");
            e.set("avoid", (this.enhance_avoid).ToString());
            e.set("resist", (this.enhance_resist).ToString());
            e.set("defense", (this.enhance_defense).ToString());
            prop.append(e);
            e = cw.data.make_element("SoundPath", this.get_materialpath(this.sound_effect));
            prop.append(e);
            e = cw.data.make_element("SoundPath2", this.get_materialpath(this.sound_effect2));
            prop.append(e);
            e = cw.data.make_element("KeyCodes", cw.util.encodetextlist(this.keycodes));
            prop.append(e);
            if (2 < this.premium) {
                this.data.set("scenariocard", "true");
                e = cw.data.make_element("Premium", this.conv_card_premium(this.premium - 3));
            } else {
                e = cw.data.make_element("Premium", this.conv_card_premium(this.premium));
            }
            prop.append(e);
            e = cw.data.make_element("UseLimit", (this.limit).ToString());
            prop.append(e);
            e = cw.data.make_element("Hold", (this.hold).ToString());
            prop.append(e);
            this.data.append(prop);
            e = cw.data.make_element("Motions");
            foreach (var motion in this.motions) {
                e.append(motion.get_data());
            }
            this.data.append(e);
            e = cw.data.make_element("Events");
            foreach (var event in this.events) {
                e.append(event.get_data());
            }
            this.data.append(e);
        return this.data;
    }

    public static void unconv(UNK f, UNK data, UNK ownerisadventurer) {
        restype = 0;
        image = null;
        name = "";
        resid = 0;
        description = "";
        p_ability = 0;
        m_ability = 0;
        silence = false;
        target_all = false;
        target = 0;
        effect_type = 0;
        resist_type = 0;
        success_rate = 0;
        visual_effect = 0;
        motions = [];
        enhance_avoid = 0;
        enhance_resist = 0;
        enhance_defense = 0;
        sound_effect = "";
        sound_effect2 = "";
        keycodes = [];
        premium = 0;
        scenario_name = "";
        scenario_author = "";
        events = [];
        hold = false;
        level = 0;
        limit = 0;
        scenariocard = cw.util.str2bool(data.get("scenariocard", "false"));

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        resid = int.Parse(prop.text);
                    } else if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag in ("ImagePath", "ImagePaths")) {
                        image = base.CWBinaryBase.import_image(f, prop);
                    } else if (prop.tag == "Description") {
                        description = prop.text;
                    } else if (prop.tag == "Scenario") {
                        scenario_name = prop.text;
                    } else if (prop.tag == "Author") {
                        scenario_author = prop.text;
                    } else if (prop.tag == "Level") {
                        level = int.Parse(prop.text);
                    } else if (prop.tag == "Ability") {
                        p_ability = base.CWBinaryBase.unconv_card_physicalability(prop.get("physical"));
                        m_ability = base.CWBinaryBase.unconv_card_mentalability(prop.get("mental"));
                    } else if (prop.tag == "Target") {
                        target = base.CWBinaryBase.unconv_card_target(prop.text);
                        target_all = cw.util.str2bool(prop.get("allrange"));
                    } else if (prop.tag == "EffectType") {
                        effect_type = base.CWBinaryBase.unconv_card_effecttype(prop.text);
                        silence = cw.util.str2bool(prop.get("spell"));
                    } else if (prop.tag == "ResistType") {
                        resist_type = base.CWBinaryBase.unconv_card_resisttype(prop.text);
                    } else if (prop.tag == "SuccessRate") {
                        success_rate = int.Parse(prop.text);
                    } else if (prop.tag == "VisualEffect") {
                        visual_effect = base.CWBinaryBase.unconv_card_visualeffect(prop.text);
                    } else if (prop.tag == "Enhance") {
                        enhance_avoid = int.Parse(prop.get("avoid"));
                        enhance_resist = int.Parse(prop.get("resist"));
                        enhance_defense = int.Parse(prop.get("defense"));
                    } else if (prop.tag == "SoundPath") {
                        sound_effect = base.CWBinaryBase.materialpath(prop.text);
                        f.check_soundoptions(prop);
                    } else if (prop.tag == "SoundPath2") {
                        sound_effect2 = base.CWBinaryBase.materialpath(prop.text);
                        f.check_soundoptions(prop);
                    } else if (prop.tag == "KeyCodes") {
                        keycodes = cw.util.decodetextlist(prop.text);
                        // 5件まで絞り込む
                        if (5 < keycodes.Count) {
                            keycodes2 = [];
                            foreach (var keycode in keycodes) {
                                if (keycode) {
                                    if (5 <= keycodes2.Count) {
                                        f.check_wsnversion("");
                                        break;
                                    } else {
                                        keycodes2.append(keycode);
                                    }
                                }
                            }
                            keycodes = keycodes2;
                        }
                        if (keycodes.Count < 5) {
                            keycodes.extend([""] * (5 - keycodes.Count));
                        }
                    } else if (prop.tag == "Premium") {
                        premium = base.CWBinaryBase.unconv_card_premium(prop.text);
                        if (ownerisadventurer && scenariocard) {
                            premium += 3;
                        }
                    } else if (prop.tag == "UseLimit") {
                        limit = int.Parse(prop.text);
                    } else if (prop.tag == "Hold") {
                        hold = cw.util.str2bool(prop.text);
                    } else if (prop.tag == "LinkId") {
                        if (prop.text && prop.text != "0") {
                            f.check_wsnversion("1");
                        }
                    }
                }
            } else if (e.tag == "Motions") {
                motions = e;
            } else if (e.tag == "Events") {
                events = e;
            }

        f.write_byte(restype);
        f.write_image(image);
        f.write_string(name);
        f.write_dword(resid + 50000);
        f.write_string(description, true);
        f.write_dword(p_ability);
        f.write_dword(m_ability);
        f.write_bool(silence);
        f.write_bool(target_all);
        f.write_byte(target);
        f.write_byte(effect_type);
        f.write_byte(resist_type);
        f.write_dword(success_rate);
        f.write_byte(visual_effect);
        f.write_dword(motions.Count);
        foreach (var motion in motions) {
            effectmotion.EffectMotion.unconv(f, motion);
        }
        f.write_dword(enhance_avoid);
        f.write_dword(enhance_resist);
        f.write_dword(enhance_defense);
        f.write_string(sound_effect);
        f.write_string(sound_effect2);
        foreach (var keycode in keycodes) {
            f.write_string(keycode);
        }
        f.write_byte(premium);
        f.write_string(scenario_name);
        f.write_string(scenario_author);
        f.write_dword(events.Count);
        foreach (var evt in events) {
            event.SimpleEvent.unconv(f, evt);
        }
        f.write_bool(hold);

        // 宿データだとここに不明なデータ(4)が付加されている
        f.write_dword(4);

        f.write_dword(level);
        f.write_dword(limit);
    }
}