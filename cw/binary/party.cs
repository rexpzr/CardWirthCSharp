//!/usr/bin/env python
// -*- coding: utf-8 -*-

import os;
import io;
import sys;

import base;
import adventurer;
import util;

import cw;
import cw.binary.cwfile;
import cw.binary.summary;
import cw.binary.album;
import cw.binary.skill;
import cw.binary.item;
import cw.binary.beast;
import bgimage;


class Party : base.CWBinaryBase) {
    // """wplファイル(type=2)。パーティの見出しデータ。;
    // パーティの所持金や名前はここ。;
    // 宿の画像も格納しているが必要ないと思うので破棄。;
    // F9のためにゴシップと終了印を記憶しているような事は無い;
    // (その2つはF9で戻らない)。;
    // """;
    public Party(UNK parent, UNK f, bool yadodata=false, UNK dataversion=10) : base(parent, f, yadodata) {
        this.type = 2;
        this.fname = this.get_fname();
        if (10 <= dataversion) {
            // 1.28以降
            _w = f.word() // 不明(0)
            _yadoname = f.string();
            f.image() // 宿の埋め込み画像は破棄
            this.memberslist = [];
            foreach (var member in cw.util.decodetextlist(f.string(true))) {
                if (member != "") {
                    this.memberslist.append(util.check_filename(member));
                }
            }
            this.name = f.string();
            this.money = f.dword() // 冒険中の現在値
            this.nowadventuring = f.bool();
        } else {
            // 1.20
            this.memberslist = [];
            foreach (var member in cw.util.decodetextlist(f.string(true))) {
                if (member != "") {
                    this.memberslist.append(util.check_filename(member));
                }
            }
            dataversion_str = f.string();
            _scenarioname = f.string() // プレイ中のシナリオ名
            f.image() // 宿の埋め込み画像は破棄
            this.name = "";
            this.money = 0;
            this.nowadventuring = f.bool();
        }

        // 読み込み後に操作
        this.cards = [];
        // データの取得に失敗したカード。変換時に追加する
        this.errorcards = [];

        this.data = null;
    }

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Party");

            prop = cw.data.make_element("Property");

            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("Money", str(this.money));
            prop.append(e);

            me = cw.data.make_element("Members");
            // メンバーの追加はwpt側で
            prop.append(me);

            this.data.append(prop);
        }

        return this.data;
    }

    public UNK create_xml(dpath) {
        path = base.CWBinaryBase.create_xml(self, dpath);
        yadodb = this.get_root().yadodb;
        if (yadodb) {
            yadodb.insert_party(path, commit=false);
        }

        // 荷物袋内のカード
        cdpath = os.path.dirname(path);
        carddb = cw.yadodb.YadoDB(cdpath, cw.yadodb.PARTY);
        this.errorcards = [];
        order = 0;
        foreach (var card in this.cards) {
            if (card.data) {
                card.data.materialbasedir = dpath;
                cpath = card.create_xml(cdpath);
                carddb.insert_card(cpath, commit=false, cardorder=order);
                order += 1;
            } else {
                this.errorcards.append(card);
            }
        }
        carddb.commit();
        carddb.close();

        return path;
    }

    public static UNK unconv(UNK f, UNK data, UNK table, UNK scenarioname) {
        if (scenarioname) {
            yadoname = scenarioname;
            nowadventuring = true;
        } else {
            yadoname = table["yadoname"];
            nowadventuring = false;
        }
        imgpath = "Resource/Image/Card/COMMAND0";
        imgpath = cw.util.find_resource(cw.util.join_paths(cw.cwpy.skindir, imgpath), cw.cwpy.rsrc.ext_img);
        image = base.CWBinaryBase.import_image(f, imgpath, fullpath=true);
        memberslist = "";
        name = "";
        money = 0;

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Name") {
                        name = prop.text;
                    } else if (prop.tag == "Money") {
                        money = int(prop.text);
                        money = cw.util.numwrap(money, 0, 999999);
                    } else if (prop.tag == "Members") {
                        atbl = table["adventurers"];
                        seq = [];
                        foreach (var me in prop) {
                            if (me.tag == "Member" && me.text && me.text in atbl) {
                                seq.append(atbl[me.text]);
                            }
                        }
                        memberslist = cw.util.encodetextlist(seq);
                    }
                }
            }
        }

        f.write_word(0) // 不明
        f.write_string(yadoname);
        f.write_image(image);
        f.write_string(memberslist, true);
        f.write_string(name);
        f.write_dword(money);
        f.write_bool(nowadventuring);
    }
}

class PartyMembers : base.CWBinaryBase {
    // """wptファイル(type=3)。パーティメンバと;
    // 荷物袋に入っているカードリストを格納している。;
    // """;
    public PartyMembers(UNK parent, UNK f, bool yadodata=false, UNK dataversion=10) : base(parent, f, yadodata) {
        this.type = 3;
        this.fname = this.get_fname();
        if (10 <= dataversion) {
            adventurers_num = f.byte() - 30;
        } else {
            adventurers_num = f.byte() - 10;
        }
        _b = f.byte(); // 不明(0)
        _b = f.byte();// 不明(0)
        _b = f.byte(); // 不明(0)
        _b = f.byte(); // 不明(5)
        this.adventurers = [];
        vanisheds_num = 0;
        foreach (var i in xrange(adventurers_num)) {
            this.adventurers.append(adventurer.AdventurerWithImage(self, f));
            if (10 <= dataversion) {
                vanisheds_num = f.byte(); // 最後のメンバが消滅メンバの数を持っている？
            } else {
                _b = f.byte(); // 不明(0)
            }
        }
        this.vanisheds = [];
        if (0 < vanisheds_num) {
            _dw = f.dword() // 不明(0)
            foreach (var i in xrange(vanisheds_num)) {
                this.vanisheds.append(adventurer.AdventurerWithImage(self, f));
                if (i + 1 < vanisheds_num) {
                    _b = f.byte();
                }
            }
            this.vanisheds.reverse();
        } else {
            _b = f.byte() // 不明(0)
            _b = f.byte() // 不明(0)
            _b = f.byte() // 不明(0)
        }
        if (10 <= dataversion) {
            // 1.28以降
            this.name = f.string() // パーティ名
            // 荷物袋にあるカードリスト
            cards_num = f.dword();
            this.cards = [BackpackCard(self, f) for _cnt in xrange(cards_num)];
        } else {
            // 1.20
            f.seek(-4, io.SEEK_CUR);
            // パーティ名
            this.name = this.adventurers[0].adventurer.name + u"一行";
            // 荷物袋にあるカードリスト
            cards_num = f.dword();
            this.cards = [];
            foreach (var _cnt in xrange(cards_num)) {
                type = f.byte();
                if (type == 2) {
                    carddata = cw.binary.item.ItemCard(null, f, true);
                } else if (type == 1) {
                    carddata = cw.binary.skill.SkillCard(null, f, true);
                } else if (type == 3) {
                    carddata = cw.binary.beast.BeastCard(null, f, true);
                } else {
                    throw ValueError(this.fname);
                }
                card = BackpackCard(self, null);
                card.fname = carddata.name;
                if (type in (2, 3)) {
                    card.uselimit = carddata.limit;
                } else {
                    card.uselimit = 0;
                }
                // F9で戻るカードかどうかはレアリティの部分に格納されているため処理不要
                card.mine = true;
                card.set_data(carddata);
                this.cards.append(card);
            }
        }

        // 対応する *.wpl
        this.wpl = null;

        if (10 <= dataversion) {
            // *.wplにもあるパーティの所持金(冒険中の現在値)
            this.money = f.dword();

            // ここから先はプレイ中のシナリオの状況が記録されている
            this.money_beforeadventure = f.dword() // 冒険前の所持金。冒険中でなければ0
            this.nowadventuring = f.bool();
            if (this.nowadventuring) { // 冒険中か
                _w = f.word() // 不明(0)
                this.scenariopath = f.rawstring() // シナリオ
                this.areaid = f.dword();
                this.steps = this.split_variables(f.rawstring(), true);
                this.flags = this.split_variables(f.rawstring(), false);
                this.friendcards = this.split_ids(f.rawstring());
                this.infocards = this.split_ids(f.rawstring());
                this.music = f.rawstring();
                bgimgs_num = f.dword();
                this.bgimgs = [bgimage.BgImage(self, f) for _cnt in xrange(bgimgs_num)];
            }
        } else {
            // 1.20以前では個人別に所持金があるためパーティの財布に集める
            this.money = 0;
            foreach (var adv in this.adventurers) {
                this.money += adv.adventurer.money;
                adv.adventurer.money = 0;
            }
            this.money_beforeadventure = this.money // 1.20ではF9で所持金が戻らない

            this.nowadventuring = f.bool();
            if (this.nowadventuring) { //冒険中か
                this.scenariopath = u"";
                summary = cw.binary.summary.Summary(null, f, true, wpt120=true);
                this.steps = {}.copy();
                foreach (var step in summary.steps) {
                    this.steps[step.name] = step.default;
                }
                this.flags = {}.copy();
                foreach (var flag in summary.flags) {
                    this.flags[flag.name] = flag.default;
                }
                this.scenariopath = f.rawstring(); //シナリオ
                if (!os.path.isabs(this.scenariopath)) {
                    dpath = os.path.dirname(os.path.dirname(os.path.dirname(f.name)));
                    this.scenariopath = cw.util.join_paths(dpath, this.scenariopath);
                }
                this.areaid = f.dword();
                this.friendcards = [];
                fcardnum = f.dword();
                foreach (var _i in xrange(fcardnum)) {
                    this.friendcards.append(f.dword());
                }
                this.infocards = [];
                infonum = f.dword();
                foreach (var _i in xrange(infonum)) {
                    this.infocards.append(f.dword());
                }
                this.music = f.rawstring();
                bgimgs_num = f.dword();
                this.bgimgs = [bgimage.BgImage(self, f) for _cnt in xrange(bgimgs_num)];
            }
        }
    }

    public UNK split_variables(UNK text, UNK step) {
        d = {};
        foreach (var l in text.splitlines()) {
            index = l.rfind('=');
            if (index != -1) {
                if (step) {
                    d[l[:index]] = int(l[index+1:]);
                } else {
                    d[l[:index]] = bool(int(l[index+1:]));
                }
            }
        }
        return d;
    }

    public UNK split_ids(UNK text) {
        seq = [];
        foreach (var l in text.splitlines()) {
            if (l) {
                seq.append(int(l));
            }
        }
        return seq;
    }

    public UNK create_xml(UNK dpath) {
        // """adventurercardだけxml化する。""";
        wpldata = this.wpl.get_data();
        me = wpldata.find("Property/Members");
        foreach (var adventurer in this.adventurers) {
            path = adventurer.create_xml(dpath);
            text = cw.util.splitext(os.path.basename(path))[0];
            me.append(cw.data.make_element("Member", text));
        }
    }

    public UNK create_vanisheds_xml(UNK dpath) {
        foreach (var adventurer in this.vanisheds) {
            data = adventurer.get_data();
            data.find("Property").set("lost", "true");
            adventurer.create_xml(dpath);
        }
    }

    public static UNK join_variables(UNK data) {
        seq = [];
        foreach (var e in data) {
            name = e.text;
            value = e.get("value");
            if (value == "true") {
                value = "1";
            } else if (value == "false") {
                value = "0";
            }
            seq.append(name + "=" + value);
        }
        if (seq) {
            seq.append("");
        }
        return "\r\n".join(seq);
    }

    public static UNK join_ids(UNK data) {
        seq = [];
        foreach (var e in data) {
            if (e.tag == "CastCard") {
                seq.append(e.find("Property/Id").text);
            } else {
                seq.append(e.text);
            }
        }
        if (seq) {
            seq.append("");
        }
        return "\r\n".join(seq);
    }

    public static UNK unconv(UNK f, UNK party, UNK table, UNK logdir) {
        adventurers = [];
        vanisheds = [];
        cards = [];
        money_beforeadventure = 0;
        nowadventuring = false;
        scenariopath = "";
        areaid = 0;
        steps = "";
        flags = "";
        friendcards = "";
        infocards = "";
        music = "";
        bgimgs = [];

        foreach (var member in party.members) {
            adventurers.append(member.find("."));
        }
        name = party.name;

        if (logdir) {
            // プレイ中のシナリオの状況
            e_log = cw.data.xml2etree(cw.util.join_paths(logdir, "ScenarioLog.xml"));
            e_party = cw.data.xml2etree(cw.util.join_paths(logdir, "Party/Party.xml"));
            money_beforeadventure = e_party.getint("Property/Money", party.money);
            money_beforeadventure = cw.util.numwrap(money_beforeadventure, 0, 999999);
            nowadventuring = true;
            scenariopath = e_log.gettext("Property/WsnPath", "");
            areaid = e_log.getint("Property/AreaId", 0);
            steps = PartyMembers.join_variables(e_log.getfind("Steps"));
            flags = PartyMembers.join_variables(e_log.getfind("Flags"));
            friendcards = PartyMembers.join_ids(e_log.getfind("CastCards"));
            infocards = PartyMembers.join_ids(e_log.getfind("InfoCards"));
            music = e_log.gettext("Property/MusicPath", "");
            if (!music) {
                music = e_log.gettext("Property/MusicPaths/MusicPath", "");
            }
            bgimgs = e_log.find("BgImages");

            foreach (var e in e_log.getfind("LostAdventurers")) {
                path = cw.util.join_yadodir(e.text);
                vanisheds.append(cw.data.xml2element(path));
            }
            vanisheds.reverse();
        }

        advnumpos = f.tell();
        advnum = 0;
        f.write_byte(len(adventurers) + 30);
        f.write_byte(0) // 不明
        f.write_byte(0) // 不明
        f.write_byte(0) // 不明
        f.write_byte(5) // 不明
        errorlog = [];
        foreach (var i, member in enumerate(adventurers)) {
            if (logdir) {
                fpath = cw.util.join_paths(logdir, "Members", os.path.basename(member.fpath));
                logdata = cw.data.xml2element(fpath);
            } else {
                logdata = null;
            }
            try {
                pos = f.tell();
                adventurer.AdventurerWithImage.unconv(f, member, logdata);
                if (i + 1 < len(adventurers)) {
                    f.write_byte(0); // 不明
                }
                advnum += 1;
            } catch (cw.binary.cwfile.UnsupportedError e) {
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = member.gettext("Property/Name", "");
                    s = u"%s の %s は対象エンジンで使用できないため、変換しません。\n" % (name, cardname);
                    errorlog.append(s);
                }
            } catch (Exception e) {
                cw.util.print_ex(file=sys.stderr);
                f.seek(pos);
                if (f.write_errorlog) {
                    cardname = member.gettext("Property/Name", "");
                    s = u"%s の %s は変換できませんでした。\n" % (name, cardname);
                    errorlog.append(s);
                }
            }
        }

        if (advnum == 0) {
            s = u"%s は全メンバが変換に失敗したため、変換しません。\n" % (name);
            throw cw.binary.cwfile.UnsupportedError(s);
        }

        if (f.write_errorlog) {
            foreach (var s in errorlog) {
                f.write_errorlog(s);
            }
        }

        tell = f.tell();
        f.seek(advnumpos);
        f.write_byte(advnum + 30);
        f.seek(tell);

        vannumpos = f.tell();
        vannum = 0;
        f.write_byte(len(vanisheds)) // 消滅メンバの数？
        if (vanisheds) {
            f.write_dword(0) // 不明
            foreach (var i, member in enumerate(vanisheds)) {
                if (logdir) {
                    fpath = cw.util.join_paths(logdir, "Members", os.path.basename(member.fpath));
                    logdata = cw.data.xml2element(fpath);
                } else {
                    logdata = null;
                }
                try {
                    pos = f.tell();
                    adventurer.AdventurerWithImage.unconv(f, member, logdata);
                    if (i + 1 < len(vanisheds)) {
                        f.write_byte(0); // 不明
                    }
                    vannum += 1;
                } catch (cw.binary.cwfile.UnsupportedError e) {
                    f.seek(pos);
                    if (f.write_errorlog) {
                        cardname = member.gettext("Property/Name", "");
                        s = u"%s の %s(消去前データ) は対象エンジンで使用できないため、変換しません。\n" % (name, cardname);
                        f.write_errorlog(s);
                    }
                } catch (Exception e) {
                    cw.util.print_ex(file=sys.stderr);
                    f.seek(pos);
                    if (f.write_errorlog) {
                        cardname = member.gettext("Property/Name", "");
                        s = u"%s の %s(消去前データ) は変換できませんでした。\n" % (name, cardname);
                        f.write_errorlog(s);
                    }
                }
            }
            tell = f.tell();
            f.seek(vannumpos);
            f.write_byte(vannum);
            f.seek(tell);

        } else {
            f.write_byte(0); // 不明
            f.write_byte(0); // 不明
            f.write_byte(0); // 不明
        }
        f.write_string(name);

        backpacknumpos = f.tell();
        backpacknum = 0;
        f.write_dword(len(party.backpack));
        btbl = table["yadocards"];
        // CardWirthでは削除されたカードはF9でも復活しないので変換不要
        foreach (var header in party.backpack) {
            // バージョン不一致で不変換のデータもあるので所在チェック
            if (header.fpath in btbl) {
                fpath, data = btbl[header.fpath];
                scenariocard = cw.util.str2bool(data.get("scenariocard", "false"));
                cards.append(BackpackCard.unconv(f, data, fpath, !scenariocard));
                backpacknum += 1;
            }
        }
        tell = f.tell();
        f.seek(backpacknumpos);
        f.write_dword(backpacknum);
        f.seek(tell);

        f.write_dword(cw.util.numwrap(party.money, 0, 999999)); // パーティの所持金(現在値)

        // プレイ中のシナリオの状況
        if (nowadventuring) {
            f.write_dword(money_beforeadventure);
            f.write_bool(nowadventuring);
            f.write_word(0); // 不明(0)
            f.write_rawstring(os.path.abspath(scenariopath));
            f.write_dword(areaid);
            f.write_rawstring(steps);
            f.write_rawstring(flags);
            f.write_rawstring(friendcards);
            f.write_rawstring(infocards);
            f.write_rawstring(music);
            f.write_dword(len(bgimgs));
            foreach (var bgimg in bgimgs) {
                bgimage.BgImage.unconv(f, bgimg);
            }
        } else {
            f.write_dword(0);
            f.write_bool(false);
        }
    }
}

class BackpackCard : base.CWBinaryBase {
    // """荷物袋に入っているカードのデータ。;
    // this.dataにwidファイルから読み込んだカードデータがある。;
    // """;
    public BackpackCard(UNK parent, UNK f, bool yadodata=false) : base(parent, f, yadodata) {
        if (f) {
            this.fname = f.rawstring();
            this.uselimit = f.dword();
            this.mine = f.bool();
        } else {
            this.fname = u"";
            this.uselimit = 0;
            this.mine = false;
        }
        this.data = null;
    }

    public UNK set_data(data) {
        // """widファイルから読み込んだカードデータを関連づける""";
        this.data = data;
    }

    public UNK get_data() {
        return this.data.get_data();
    }

    public UNK create_xml(dpath) {
        // """this.data.create_xml()""";
        this.data.limit = this.uselimit;
        if (!this.mine) {
            this.data.set_image_export(false, true);
        }
        data = this.data.get_data();
        if (!this.mine) {
            data.set("scenariocard", "true");
        }
        return this.data.create_xml(dpath);
    }

    public static UNK unconv(UNK f, UNK data, UNK fname, UNK mine) {
        f.write_rawstring(cw.util.splitext(fname)[0]);
        f.write_dword(data.getint("Property/UseLimit", 0));
        f.write_bool(mine);
    }
}

UNK load_album120(UNK parent, UNK f) {
    _dw = f.dword() // 不明
    cardnum = f.dword() // アルバム人数
    cards = [];
    albums = [];
    foreach (var _i in xrange(cardnum)) {
        card = cw.binary.adventurer.AdventurerCard(parent, null, true);
        card.fname = f.name;
        card.adventurer = cw.binary.adventurer.Adventurer(card, f, true, album120=true);
        if (card.adventurer.is_dead) {
            albumdata = cw.binary.album.Album(parent, null, true);
            albumdata.name = card.adventurer.name;
            albumdata.image = card.adventurer.image;
            albumdata.level = card.adventurer.level;
            albumdata.dex = card.adventurer.dex;
            albumdata.agl = card.adventurer.agl;
            albumdata.int = card.adventurer.int;
            albumdata.str = card.adventurer.str;
            albumdata.vit = card.adventurer.vit;
            albumdata.min = card.adventurer.min;
            albumdata.aggressive = card.adventurer.aggressive;
            albumdata.cheerful = card.adventurer.cheerful;
            albumdata.brave = card.adventurer.brave;
            albumdata.cautious = card.adventurer.cautious;
            albumdata.trickish = card.adventurer.trickish;
            albumdata.avoid = card.adventurer.avoid;
            albumdata.resist = card.adventurer.resist;
            albumdata.defense = card.adventurer.defense;
            albumdata.description = card.adventurer.description;
            albumdata.coupons = card.adventurer.coupons;

            albums.append(albumdata);
        } else {
            cards.append(card);
        }
    }

    return cards, albums;
}
