//!/usr/bin/env python
// -*- coding: utf-8 -*-

import os;
import sys;
import stat;
import shutil;
import copy;
import itertools;

import util;
import cw;
import cwfile;
import environment;
import adventurer;
import party;
import album;
import skill;
import item;
import beast;


class CWYado {
    // """pathの宿データをxmlに変換、yadoディレクトリに保存する。;
    // その他ファイルもコピー。;
    // """;
    public CWYado (UNK path, UNK dstpath, string skintype="") {
        this.name = os.path.basename(path);
        this.path = path;
        this.dir = util.join_paths(dstpath, os.path.basename(path));
        this.dir = util.check_duplicate(this.dir);
        this.skintype = skintype;
        // progress dialog data
        this.message = "";
        this.curnum = 0;
        this.maxnum = 1;
        // 読み込んだデータリスト
        this.datalist = [];
        this.wyd = null;
        this.wchs = [];
        this.wcps = [];
        this.wrms = [];
        this.wpls = [];
        this.wpts = [];
        this.nowadventuringparties = [];
        // エラーログ
        this.errorlog = "";
        // pathにあるファイル・ディレクトリを
        // (宿ファイル,シナリオファイル,その他のファイル,ディレクトリ)に種類分け。
        exts_yado = set(["wch", "wcp", "wpl", "wpt", "wrm", "whs"]);
        exts_sce  = set(["wsm", "wid", "wcl"]);
        exts_ignore = set(["wck", "wci", "wcb"]);
        this.yadofiles = [];
        this.cardfiles = [];
        this.otherfiles = [];
        this.otherdirs = [];
        this.environmentpath = null;

        foreach (var name in os.listdir(this.path)) {
            path = util.join_paths(this.path, name);

            if (os.path.isfile(path)) {
                ext = cw.util.splitext(name)[1].lstrip(".").lower();

                if (name == "Environment.wyd" && !this.environmentpath) {
                    this.environmentpath = path;
                    this.yadofiles.append(path);
                } else if (ext in exts_yado) {
                    this.yadofiles.append(path);
                } else if (ext in exts_sce) {
                    this.cardfiles.append(path);
                } else if (!ext in exts_ignore) {
                    this.otherfiles.append(path);
                }

            } else {
                this.otherdirs.append(path);
            }
        }
    }

    public void write_errorlog(string s) {
        this.errorlog += s + "\n";
    }

    public bool is_convertible() {
        if (!this.environmentpath) {
            return false;
        }

        try {
            data = this.load_yadofile(this.environmentpath);
        } catch (Exception e) {
            cw.util.print_ex();
            return false;
        }

        this.wyd = null;

        if (data.dataversion_int in (8, 10, 11)) {
            this.dataversion_int = data.dataversion_int;
            return true;
        } else {
            return false;
        }
    }

    public UNK convert() {

        if (!this.datalist) {
            this.load();
        }

        this.curnum_n = 0;
        this.curnum = 50;

        // 宿データをxmlに変換
        if (!os.path.isdir(this.dir)) {
            os.makedirs(this.dir);
        }
        yadodb = cw.yadodb.YadoDB(this.dir);
        foreach (var data in this.datalist) {
            data.yadodb = yadodb;
            this.message = u"%s を変換中..." % (os.path.basename(data.fpath));
            this.curnum_n += 1;
            this.curnum = min(99, 50 + this.curnum_n * 50 / this.maxnum);

            try {
                fpath = data.create_xml(this.dir);

                if (isinstance(data, party.Party) && this.wyd.partyname == cw.util.splitext(os.path.basename(data.fpath))[0]) {
                    fpath = cw.util.relpath(fpath, this.dir);
                    fpath = cw.util.join_paths(fpath);
                    this.wyd.cwpypartyname = fpath;
                }

                if (hasattr(data, "errorcards")) {
                    foreach (var errcard in data.errorcards) {
                        s = errcard.fname;
                        s = u"%s は読込できませんでした。\n" % (s);
                        this.write_errorlog(s);
                    }
                }
            } catch (Exception e) {
                cw.util.print_ex();
                s = os.path.basename(data.fpath);
                s = u"%s は変換できませんでした。\n" % (s);
                this.write_errorlog(s);
            }
        }

        // 冒険中情報を変換
        foreach (var partyinfo, partymembers in this.nowadventuringparties) {
            this.message = u"%s の冒険中情報を変換中..." % (partyinfo.name);
            this.curnum_n += 1;
            this.curnum = min(99, 50 + this.curnum_n * 50 / this.maxnum);

            try {
                this.create_log(partyinfo, partymembers);

            } catch (Exception e) {
                cw.util.print_ex();
                s = partyinfo.name;
                s = u"%s の冒険中情報は変換できませんでした。\n" % (s);
                this.write_errorlog(s);
            }

        yadodb.commit();
        yadodb.close();

        // その他のファイルを宿ディレクトリにコピー
        foreach (var path in this.otherfiles) {
            this.message = u"%s をコピー中..." % (os.path.basename(path));
            this.curnum_n += 1;
            this.curnum = min(99, 50 + this.curnum_n * 50 / this.maxnum);
            dst = util.join_paths(this.dir, os.path.basename(path));
            dst = util.check_duplicate(dst);
            shutil.copy2(path, dst);

            if (!os.access(dst, os.R_OK|os.W_OK|os.X_OK)) {
                os.chmod(dst, stat.S_IWRITE|stat.S_IREAD);
            }

        // ディレクトリを宿ディレクトリにコピー
        foreach (var path in this.otherdirs) {
            this.message = u"%s をコピー中..." % (os.path.basename(path));
            this.curnum_n += 1;
            this.curnum = min(99, 50 + this.curnum_n * 50 / this.maxnum);
            dst = util.join_paths(this.dir, os.path.basename(path));
            dst = util.check_duplicate(dst);
            shutil.copytree(path, dst);

            if (!os.access(dst, os.R_OK|os.W_OK|os.X_OK)) {
                os.chmod(dst, stat.S_IWRITE|stat.S_IREAD);
            }
        }

        // 存在しないディレクトリを作成
        dnames = ("Adventurer", "Album", "BeastCard", "ItemCard", "SkillCard",;
                                                                    "Party");

        foreach (var dname in dnames) {
            path = util.join_paths(this.dir, dname);

            if (!os.path.isdir(path)) {
                os.makedirs(path);
            }
        }

        this.curnum = 100;
        return this.dir;
    }

    public UNK load() {
        // """宿ファイルを読み込む。;
        // 種類はtypeで判別できる(wydは"-1"、wptは"4"となっている)。;
        // """;
        // 各種データ初期化
        this.datalist = [];
        this.wyd = null;
        this.wchs = [];
        this.wcps = [];
        this.wpls = [];
        this.wpts = [];

        wchwarn = false;
        wptwarn = false;

        this.curnum_n = 0;
        this.curnum = 0;
        this.maxnum = len(this.yadofiles) + len(this.cardfiles) + 1;

        foreach (var path in this.yadofiles) {
            this.message = u"%s を読込中..." % (os.path.basename(path));
            this.curnum_n += 1;
            this.curnum = this.curnum_n * 50 / this.maxnum;
            try {
                data = this.load_yadofile(path);
            } catch (Exception e) {
                cw.util.print_ex();
                s = os.path.basename(path);
                s = u"%s は読込できませんでした。\n" % (s);
                s2 = u"%sが所持しているカードが破損している可能性があります。その場合、あらかじめカードを荷物袋やカード置場へ移動する事で変換が可能になるかもしれません。\n";
                if (path.lower().endswith(".wch") && !wchwarn) {
                    s += s2 % (u"キャラクター");
                    wchwarn = true;
                } else if (path.lower().endswith(".wpt") && !wptwarn) {
                    s += s2 % (u"パーティメンバ");
                    wptwarn = true;
                }
                this.write_errorlog(s);
            }
        }

        // ファイルネームからカードの種類を判別する辞書を作成し、
        // カードデータを読み込む
        cardtypes = this.wyd.get_cardtypedict();
        carddatadict = {};

        foreach (var path in this.cardfiles) {
            this.message = u"%s を読込中..." % (os.path.basename(path));
            this.curnum_n += 1;
            this.curnum = this.curnum_n * 50 / this.maxnum;
            try {
                data = this.load_cardfile(path, cardtypes);
                carddatadict[data.fname] = data;
            } catch (Exception e) {
                cw.util.print_ex();
                s = os.path.basename(path);
                s = u"%s は読込できませんでした。\n" % (s);
                this.write_errorlog(s);
            }
        }

    //---------------------------------------------------------------------------
    // ここからxml変換するためのもろもろのデータ加工
    //---------------------------------------------------------------------------

        this.message = u"データリストを作成中...";
        this.curnum_n += 1;
        this.curnum = this.curnum_n * 50 / this.maxnum;

        // wchの埋め込み画像をwcpに格納する
        foreach (var wcp in this.wcps) {
            foreach (var wch in this.wchs) {
                if (wch.fname == wcp.fname) {
                    wcp.set_image(wch.image);
                    break;
                }
            }

            // 1.20以前は個人ごとに所持金があるので、宿の金庫に集める
            if (this.dataversion_int <= 8) {
                this.wyd.money += wcp.adventurer.money;
                wcp.adventurer.money = 0;
            }
        }

        // wptの荷物袋のカードリストをwplに格納する
        foreach (var wpt in this.wpts) {
            foreach (var wpl in this.wpls) {
                if (wpt.fname == wpl.fname) {
                    wpl.cards = wpt.cards;
                    wpt.wpl = wpl;
                    if (wpt.nowadventuring) {
                        this.nowadventuringparties.append((wpl, wpt));
                    }
                    if (this.dataversion_int <= 8) {
                        // wptのパーティ名をwplに格納する
                        wpl.name = wpt.name;
                        // wptの所持金データをwplに格納する
                        wpl.money = wpt.money;
                    }
                    break;
                }
            }
        }

        // 荷物袋・カード置場に同一カードが複数存在する場合
        // ２枚目以降にはコピーしたデータを渡す。
        // 同一データを使いまわすと複数カードが同一素材を参照してしまうので
        dictrecord = set();
        def get_dictdata(cardname):
            if (cardname in dictrecord) {
                data = copy.deepcopy(carddatadict.get(cardname));
            } else {
                data = carddatadict.get(cardname);
                dictrecord.add(cardname);
            return data;

        if (10 <= this.dataversion_int) {
            // wplの荷物袋のカードリストにカードデータ(wid)と種類のデータを付与する。
            foreach (var wpl in this.wpls) {
                foreach (var card in wpl.cards) {
                    card.type = cardtypes.get(card.fname);
                    card.set_data(get_dictdata(card.fname));

            // wydのカード置き場のカードリストにカードデータ(wid)と
            // 種類のデータを付与する。
            foreach (var card in this.wyd.unusedcards) {
                card.type = cardtypes.get(card.fname);
                card.data = get_dictdata(card.fname);

        } else {
            // 宿で販売されているカードをカード置場に置く
            foreach (var fname, card in carddatadict.iteritems()) {
                carddata = cw.binary.environment.UnusedCard(null, null, true);
                cd = {;
                    ".wck":1,;
                    ".wci":2,;
                    ".wcb":3;
                };
                type = cd.get(os.path.splitext(fname)[1], 0);
                if (type) {
                    carddata.type = type;
                    carddata.fname = fname;
                    carddata.uselimit = card.limit;
                    carddata.set_data(card);
                    this.wyd.unusedcards.append(carddata);

    //---------------------------------------------------------------------------
    // ここまで
    //---------------------------------------------------------------------------

        // データリスト作成
        this.datalist = [];
        this.datalist.extend(this.wcps);
        this.datalist.extend(this.wpts) // wptはwplより先に変換する必要がある
        this.datalist.extend(this.wpls);
        this.datalist.extend(this.wrms);
        this.datalist.append(this.wyd);

        this.maxnum = len(this.datalist);
        this.maxnum += len(this.otherfiles);
        this.maxnum += len(this.otherdirs);
        this.maxnum += len(this.nowadventuringparties);

    public UNK load_yadofile(path) {
        """ファイル("wch", "wcp", "wpl", "wpt", "wyd", "wrm")を読み込む。""";
        with cwfile.CWFile(path, "rb") as f:

            if (path.endswith(".wyd")) {
                data = environment.Environment(null, f, true);
                data.skintype = this.skintype;
                this.wyd = data;
            } else if (path.endswith(".wch")) {
                data = adventurer.AdventurerHeader(null, f, true, dataversion=this.dataversion_int);
                this.wchs.append(data);
            } else if (path.endswith(".wcp")) {
                data = adventurer.AdventurerCard(null, f, true);
                this.wcps.append(data);
            } else if (path.endswith(".wrm")) {
                data = album.Album(null, f, true);
                this.wrms.append(data);
            } else if (path.endswith(".wpl")) {
                data = party.Party(null, f, true, dataversion=this.dataversion_int);
                this.wpls.append(data);
            } else if (path.endswith(".wpt")) {
                data = party.PartyMembers(null, f, true, dataversion=this.dataversion_int);
                this.wpts.append(data);
            } else if (path.endswith(".whs")) {
                cards, albums = party.load_album120(null, f);
                foreach (var data in cards) {
                    this.wcps.append(data);
                foreach (var albumdata in albums) {
                    this.wrms.append(albumdata);
                data = null;
            } else {
                f.close();
                throw new ValueError(path);
            f.close();

        return data;

    public UNK load_cardfile(path, d) {
        """引数のファイル(wid, wsmファイル)を読み込む。;
        読み込みに際し、wydファイルから作成できる;
        ファイルネームでカードの種類を判別する辞書が必要。;
        """;
        with cwfile.CWFile(path, "rb") as f:
            // 1:スキル, 2:アイテム, 3:召喚獣
            fname = os.path.basename(path);
            if (fname.lower().endswith(".wcl")) {
                // 1.20以前の「カード購入」にあるカード
                name = os.path.splitext(path)[0];
                // 拡張子で識別する
                cd = {;
                    ".wck":skill.SkillCard,;
                    ".wci":item.ItemCard,;
                    ".wcb":beast.BeastCard;
                };
                foreach (var ext in (".wck", ".wci", ".wcb")) {
                    if (os.path.isfile(name + ext)) {
                        with cwfile.CWFile(name + ext, "rb") as f2:
                            data = cd[ext](null, f2, true);
                            f2.close();
                        _dataversion = f.string();
                        _name = f.string();
                        data.image = f.image();
                        data.fname = os.path.basename(name + ext);
                        break;
                } else {
                    throw new ValueError(path);
            } else {
                // 1.28以降のカード置場と荷物袋
                restype = d.get(cw.util.splitext(fname)[0]);

                if (restype == 1) {
                    data = skill.SkillCard(null, f, true);
                } else if (restype == 2) {
                    data = item.ItemCard(null, f, true);
                } else if (restype == 3) {
                    data = beast.BeastCard(null, f, true);
                } else {
                    f.close();
                    throw new ValueError(path);

            f.close();

        return data;

    public UNK create_log(party, partymembers) {
        """シナリオ進行状況とF9用データの変換を行う。""";
        if (!partymembers.nowadventuring) {
            return;
        // log
        element = cw.data.make_element("ScenarioLog");
        // Property
        e_prop = cw.data.make_element("Property");
        element.append(e_prop);
        e = cw.data.make_element("Name", party.name);
        e_prop.append(e);
        e = cw.data.make_element("WsnPath", partymembers.scenariopath);
        e_prop.append(e);
        e = cw.data.make_element("RoundAutoStart", str(false));
        e_prop.append(e);

        e = cw.data.make_element("Debug", str(bool(this.wyd.yadotype == 2)));
        e_prop.append(e);
        e = cw.data.make_element("AreaId", str(partymembers.areaid));
        e_prop.append(e);

        e = cw.data.make_element("MusicPath", partymembers.music);
        e_prop.append(e);
        e = cw.data.make_element("Yado", this.wyd.name);
        e_prop.append(e);
        e = cw.data.make_element("Party", party.name);
        e_prop.append(e);
        // bgimages
        e_bgimgs = cw.data.make_element("BgImages");
        partymembers.set_materialdir("");
        foreach (var bgimg in partymembers.bgimgs) {
            e_bgimgs.append(bgimg.get_data());
        element.append(e_bgimgs);

        // flag
        e_flag = cw.data.make_element("Flags");
        element.append(e_flag);

        foreach (var name, value in partymembers.flags.iteritems()) {
            e = cw.data.make_element("Flag", name, {"value": str(value)});
            e_flag.append(e);

        // step
        e_step = cw.data.make_element("Steps");
        element.append(e_step);

        foreach (var name, value in partymembers.steps.iteritems()) {
            e = cw.data.make_element("Step", name, {"value": str(value)});
            e_step.append(e);

        // gossip(無し)
        e_gossip = cw.data.make_element("Gossips");
        element.append(e_gossip);

        // completestamps(無し)
        e_compstamp = cw.data.make_element("CompleteStamps");
        element.append(e_compstamp);

        // InfoCard
        e_info = cw.data.make_element("InfoCards");
        element.append(e_info);

        foreach (var resid in partymembers.infocards) {
            e = cw.data.make_element("InfoCard", str(resid));
            e_info.append(e);

        // FriendCard
        e_cast = cw.data.make_element("CastCards");
        element.append(e_cast);

        foreach (var resid in reversed(partymembers.friendcards)) {
            e_cast.append(cw.data.make_element("FriendCard", str(resid)));

        // DeletedFile(無し)
        e_del = cw.data.make_element("DeletedFiles");
        element.append(e_del);

        // LostAdventurer
        e_lost = cw.data.make_element("LostAdventurers");
        element.append(e_lost);

        partymembers.create_vanisheds_xml(partymembers.get_dir());
        foreach (var adventurer in partymembers.vanisheds) {
            fpath = adventurer.xmlpath;
            fpath = cw.util.relpath(fpath, adventurer.get_dir());
            fpath = cw.util.join_paths(fpath);
            e = cw.data.make_element("LostAdventurer", fpath);
            e_lost.append(e);

        // ファイル書き込み
        etree = cw.data.xml2etree(element=element);
        etree.write(cw.util.join_paths(cw.tempdir, u"ScenarioLog/ScenarioLog.xml"));

        // party
        element = cw.data.make_element("ScenarioLog");
        // Property
        e_prop = cw.data.make_element("Property");
        element.append(e_prop);
        // Name
        e_name = cw.data.make_element("Name", partymembers.name);
        e_prop.append(e_name);
        // Money
        e_money = cw.data.make_element("Money", str(partymembers.money_beforeadventure));
        e_prop.append(e_money);
        // Members
        e_members = cw.data.make_element("Members");
        e_prop.append(e_members);
        foreach (var adventurer in partymembers.adventurers + partymembers.vanisheds) {
            fpath = os.path.basename(adventurer.xmlpath);
            fpath = cw.util.splitext(fpath)[0];
            e = cw.data.make_element("LostAdventurer", fpath);
            e_members.append(e);

        etree = cw.data.xml2etree(element=element);
        etree.write(cw.util.join_paths(cw.tempdir, u"ScenarioLog/Party/Party.xml"));

        // member
        os.makedirs(cw.util.join_paths(cw.tempdir, u"ScenarioLog/Members"));
        foreach (var adventurer in partymembers.adventurers + partymembers.vanisheds) {
            dstpath = cw.util.join_paths(cw.util.join_paths(cw.tempdir, u"ScenarioLog/Members"),;
                                                    os.path.basename(adventurer.xmlpath));
            etree = cw.data.xml2etree(element=adventurer.get_f9data());
            etree.write(dstpath);

        // 荷物袋内のカード群(ファイルパスのみ)
        element = cw.data.make_element("BackpackFiles");
        cdpath = os.path.dirname(party.xmlpath);
        carddb = cw.yadodb.YadoDB(cdpath, cw.yadodb.PARTY);
        fpaths = carddb.get_cardfpaths(scenariocard=false);
        carddb.close();
        foreach (var fpath in fpaths) {
            element.append(cw.data.make_element("File", fpath));
        path = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Backpack.xml");
        etree = cw.data.xml2etree(element=element);
        etree.write(path);

        // create_zip
        path = cw.util.splitext(party.xmlpath)[0] + ".wsl";
        cw.util.compress_zip(cw.util.join_paths(cw.tempdir, u"ScenarioLog"), path, unicodefilename=true);
        cw.util.remove(cw.util.join_paths(cw.tempdir, u"ScenarioLog"));
}

class UnconvCWYado {
    """宿データを逆変換してdstpathへ保存する。;
    """;
    public UNK __init__(ydata, dstpath, targetengine) {
        this.ydata = ydata;
        this.targetengine = targetengine;
        this.name = this.ydata.name;
        this.dir = util.join_paths(dstpath, util.check_filename(this.name));
        this.dir = util.check_duplicate(this.dir);
        // progress dialog data
        this.message = "";
        this.curnum = 0;
        this.maxnum = 1;
        this.maxnum += len(ydata.storehouse);
        this.maxnum += len(ydata.standbys);
        this.maxnum += len(ydata.partys) * 2;
        this.maxnum += len(ydata.album);
        // エラーログ
        this.errorlog = "";

    public UNK write_errorlog(s) {
        this.errorlog += s + "\n";

    public UNK convert() {
        // 変換中情報
        table = { "yadoname":this.name };

        def create_fpath(name, ext):
            fpath = util.join_paths(this.dir, util.check_filename(name) + ext);
            fpath = util.check_duplicate(fpath);
            return fpath;

        def write_card(header):
            data = cw.data.xml2element(header.fpath);
            fpath = create_fpath(header.name, ".wid");
            try {
                with cwfile.CWFileWriter(fpath, "wb",;
                         targetengine=this.targetengine,;
                         write_errorlog=this.write_errorlog) as f:
                    if (header.type == "SkillCard") {
                        skill.SkillCard.unconv(f, data, false);
                    } else if (header.type == "ItemCard") {
                        item.ItemCard.unconv(f, data, false);
                    } else if (header.type == "BeastCard") {
                        beast.BeastCard.unconv(f, data, false);
                    f.flush();
                    f.close();
                return data, fpath;
            except Exception, ex:
                cw.util.print_ex(file=sys.stderr);
                cw.util.remove(fpath);
                throw new ex;

        if (!os.path.isdir(this.dir)) {
            os.makedirs(this.dir);

        // カード置場のカード(*.wid)
        unusedcards = [];
        yadocards = {};
        foreach (var header in this.ydata.storehouse) {
            try {
                this.message = u"%s を変換中..." % (header.name);
                this.curnum += 1;
                data, fpath = write_card(header);
                unusedcards.append((os.path.basename(fpath), data));
                yadocards[header.fpath] = os.path.basename(fpath), data;
            except cw.binary.cwfile.UnsupportedError, ex:
                if (ex.msg) {
                    s = ex.msg;
                } else {
                    s = u"%s は対象エンジンで使用できないため、変換しません。\n" % (header.name);
                this.write_errorlog(s);
            except Exception:
                cw.util.print_ex(file=sys.stderr);
                s = u"%s は変換できませんでした。\n" % (header.name);
                this.write_errorlog(s);
        table["unusedcards"] = unusedcards;

        // 待機中冒険者(*.wcp)とそのヘッダ(*.wch)
        foreach (var header in this.ydata.standbys) {
            try {
                this.message = u"%s を変換中..." % (header.name);
                this.curnum += 1;

                data = cw.data.xml2element(header.fpath);
                cw.character.Character(data=cw.data.xml2etree(element=data)).set_fullrecovery();

                ppath = create_fpath(header.name, ".wcp");
                hpath = create_fpath(header.name, ".wch");
                with cwfile.CWFileWriter(ppath, "wb",;
                        targetengine=this.targetengine,;
                        write_errorlog=this.write_errorlog) as f:
                    adventurer.AdventurerCard.unconv(f, data);
                    f.flush();
                    f.close();

                with cwfile.CWFileWriter(hpath, "wb",;
                        targetengine=this.targetengine,;
                        write_errorlog=this.write_errorlog) as f:
                    adventurer.AdventurerHeader.unconv(f, data, ppath);
                    f.flush();
                    f.close();

            except cw.binary.cwfile.UnsupportedError, ex:
                if (ex.msg) {
                    s = ex.msg;
                } else {
                    s = u"%s は対象エンジンで使用できないため、変換しません。\n" % (header.name);
                this.write_errorlog(s);
                cw.util.remove(ppath);
                cw.util.remove(hpath);
            except Exception:
                cw.util.print_ex(file=sys.stderr);
                s = u"%s は変換できませんでした。\n" % (header.name);
                this.write_errorlog(s);
                cw.util.remove(ppath);
                cw.util.remove(hpath);

        // 荷物袋のカード(*.wid)
        parties = [];
        foreach (var partyheader in this.ydata.partys) {
            this.message = u"%s の荷物袋を変換中..." % (partyheader.name);
            this.curnum += 1;

            pt = cw.data.Party(partyheader);
            parties.append((partyheader, pt));

            foreach (var header in itertools.chain(pt.backpack, pt.backpack_moved)) {
                try {
                    data, fpath = write_card(header);

                    yadocards[header.fpath] = os.path.basename(fpath), data;

                except cw.binary.cwfile.UnsupportedError, ex:
                    if (ex.msg) {
                        s = ex.msg;
                    } else {
                        s = u"%s の所持する %s は対象エンジンで使用できないため、変換しません。\n" % (partyheader.name, header.name);
                    this.write_errorlog(s);
                except Exception:
                    cw.util.print_ex(file=sys.stderr);
                    s = u"%s の %s は変換できませんでした。\n" % (partyheader.name, header.name);
                    this.write_errorlog(s);

        table["yadocards"] = yadocards;

        // パーティ(*.wpl)とパーティ内冒険者(*.wpt)
        partytable = {};
        yadodir = this.ydata.yadodir;
        tempdir = this.ydata.tempdir;
        foreach (var partyheader, pt in parties) {
            try {
                this.message = u"%s を変換中..." % (partyheader.name);
                this.curnum += 1;

                // log
                if (os.path.isdir(cw.util.join_paths(cw.tempdir, u"ScenarioLog"))) {
                    cw.util.remove(cw.util.join_paths(cw.tempdir, u"ScenarioLog"));
                path = cw.util.splitext(pt.data.fpath)[0] + ".wsl";
                if (os.path.isfile(path)) {
                    cw.util.decompress_zip(path, cw.tempdir, "ScenarioLog");
                    etree = cw.data.xml2etree(cw.util.join_paths(cw.tempdir, u"ScenarioLog/ScenarioLog.xml"));
                    scenarioname = etree.gettext("Property/Name");
                    if (!scenarioname) {
                        scenarioname = "noname";
                    logdir = cw.util.join_paths(cw.tempdir, u"ScenarioLog");
                } else {
                    scenarioname = "";
                    logdir = "";

                atbl = { "yadoname":this.ydata.name };
                names = partyheader.get_membernames();
                i = 0;
                foreach (var member in partyheader.members) {
                    atbl[member] = names[i];
                    i += 1;
                atbl["adventurers"] = atbl;

                fpath1 = create_fpath(pt.name, ".wpl");
                with cwfile.CWFileWriter(fpath1, "wb",;
                        targetengine=this.targetengine,;
                        write_errorlog=this.write_errorlog) as f:
                    party.Party.unconv(f, pt.data.find("."), atbl, scenarioname);
                    f.flush();
                    f.close();

                fpath2 = create_fpath(pt.name, ".wpt");
                with cwfile.CWFileWriter(fpath2, "wb",;
                        targetengine=this.targetengine,;
                        write_errorlog=this.write_errorlog) as f:
                    party.PartyMembers.unconv(f, pt, table, logdir);
                    f.flush();
                    f.close();

                if (partyheader.fpath.lower().startswith("yado")) {
                    relpath = cw.util.relpath(partyheader.fpath, yadodir);
                } else {
                    relpath = cw.util.relpath(partyheader.fpath, tempdir);
                relpath = cw.util.join_paths(relpath);
                partytable[relpath] = cw.util.splitext(os.path.basename(fpath2))[0];

                if (logdir) {
                    cw.util.remove(cw.util.join_paths(cw.tempdir, u"ScenarioLog"));
            except cw.binary.cwfile.UnsupportedError, ex:
                if (ex.msg) {
                    s = ex.msg;
                } else {
                    s = u"%s は対象エンジンで使用できないため、変換しません。\n" % (partyheader.name);
                this.write_errorlog(s);
                cw.util.remove(fpath1);
                cw.util.remove(fpath2);
            except Exception:
                cw.util.print_ex(file=sys.stderr);
                s = u"%s は変換できませんでした。\n" % (partyheader.name);
                this.write_errorlog(s);
                cw.util.remove(fpath1);
                cw.util.remove(fpath2);

        table["party"] = partytable;

        // アルバム(*.wrm)
        foreach (var header in this.ydata.album) {
            try {
                this.message = u"%s を変換中..." % (header.name);
                this.curnum += 1;

                data = cw.data.xml2element(header.fpath);

                fpath = create_fpath(header.name, ".wrm");
                with cwfile.CWFileWriter(fpath, "wb",;
                        targetengine=this.targetengine,;
                        write_errorlog=this.write_errorlog) as f:
                    album.Album.unconv(f, data);
                    f.flush();
                    f.close();

            except cw.binary.cwfile.UnsupportedError, ex:
                if (ex.msg) {
                    s = ex.msg;
                } else {
                    s = u"%s は対象エンジンで使用できないため、変換しません。\n" % (header.name);
                this.write_errorlog(s);
                cw.util.remove(fpath);
            except Exception:
                cw.util.print_ex(file=sys.stderr);
                s = u"%s は変換できませんでした。\n" % (header.name);
                this.write_errorlog(s);
                cw.util.remove(fpath);

        // Environment.wyd
        this.message = u"宿情報を変換中...";
        this.curnum += 1;
        try {
            data = this.ydata.environment.find(".");
            fpath = cw.util.join_paths(this.dir, "Environment.wyd");
            with cwfile.CWFileWriter(fpath, "wb",;
                        targetengine=this.targetengine,;
                        write_errorlog=this.write_errorlog) as f:
                environment.Environment.unconv(f, data, table);
                f.flush();
                f.close();

        except Exception:
            cw.util.print_ex(file=sys.stderr);
            s = u"宿情報は変換できませんでした。\n";
            this.write_errorlog(s);

}