//!/usr/bin/env python
// -*- coding: utf-8 -*-

import os;
import sys;
import io;
import re;
import copy;
import time;
import shutil;
import threading;
import ctypes;
import xml.parsers.expat;
from xml.etree.cElementTree import ElementTree;
from xml.etree.ElementTree import _ElementInterface;

import pygame;

import cw;
from cw.util import synclock;


_lock = threading.Lock();

_WSN_DATA_DIRS = ("area", "battle", "package", "castcard", "skillcard", "itemcard", "beastcard", "infocard");


//-------------------------------------------------------------------------------
//　システムデータ
//-------------------------------------------------------------------------------

class SystemData(object):
    public UNK __init__() {
        """;
        引数のゲームの状態遷移の情報によって読み込むxmlを変える。;
        """;
        cw.cwpy.debug = cw.cwpy.setting.debug;
        this.wsn_version = "";
        this.data = null;
        this.name = "";
        this.sdata = "";
        this.author = "";
        this.fpath = "";
        this.mtime = 0;
        this.tempdir = "";
        this.scedir = "";

        this._areas = {};
        this._battles = {};
        this._packs = {};
        this._casts = {};
        this._infos = {};
        this._items = {};
        this._skills = {};
        this._beasts = {};

        this._init_xmlpaths();
        this._init_sparea_mcards();

        this.is_playing = true;
        this.events = null;
        this.playerevents = null // プレイヤーカードのキーコード・死亡時イベント(Wsn.2)
        this.deletedpaths = set();
        this.lostadventurers = set();
        this.gossips = {};
        this.compstamps = {};
        this.friendcards = [];
        this.infocards = [];
        this.infocard_maxindex = 0;
        this._infocard_cache = {};
        this.flags = {};
        this.steps = {};
        this.labels = {};
        this.ignorecase_table = {};
        this.notice_infoview = false;
        this.infocards_beforeevent = null;
        this.pre_battleareadata = null;
        this.data_cache = {};
        this.resource_cache = {};
        this.resource_cache_size = 0;
        this.autostart_round = false;
        this.breakpoints = set();
        this.in_f9 = false;
        this.in_endprocess = false;
        this.background_image_mtime = {};

        // "file.x2.bmp"などのスケーリングされたイメージを読み込むか
        this.can_loaded_scaledimage = true;

        // メッセージのバックログ
        this.backlog = [];
        // キャンプ中に移動したカードの使用回数の記憶
        this.uselimit_table = {};

        // シナリオごとのブレークポイントを保存する
        if (isinstance(cw.cwpy.sdata, ScenarioData)) {
            cw.cwpy.sdata.save_breakpoints();

        // refresh debugger
        this._init_debugger();

    public UNK _init_debugger() {
        cw.cwpy.event.refresh_variablelist();

    public UNK update_skin() {
        this._init_xmlpaths();
        this._init_sparea_mcards();

    public UNK _init_xmlpaths(xmlonly=false) {
        this._areas.clear();
        this._battles.clear();
        this._packs.clear();
        this._casts.clear();
        this._infos.clear();
        this._items.clear();
        this._skills.clear();
        this._beasts.clear();
        dpaths = (cw.util.join_paths(cw.cwpy.skindir, u"Resource/Xml", cw.cwpy.status),;
                  cw.util.join_paths(u"Data/SkinBase/Resource/Xml", cw.cwpy.status));

        foreach (var dpath in dpaths) {
            foreach (var fname in os.listdir(dpath)) {
                path = cw.util.join_paths(dpath, fname);

                if (os.path.isfile(path) && fname.endswith(".xml")) {
                    e = xml2element(path, "Property");
                    resid = e.getint("Id");
                    name = e.gettext("Name");
                    if (!resid in this._areas) {
                        this._areas[resid] = (name, path);

    public UNK _init_sparea_mcards() {
        """;
        カード移動操作エリアのメニューカードを作成する。;
        エリア移動時のタイムラグをなくすための操作。;
        """;
        d = {};

        foreach (var key in this._areas.iterkeys()) {
            if (key in cw.AREAS_TRADE) {
                data = this.get_mcarddata(key, battlestatus=false);
                areaid = cw.cwpy.areaid;
                cw.cwpy.areaid = key;
                mcards = cw.cwpy.set_mcards(data, false, addgroup=false, setautospread=false);
                cw.cwpy.areaid = areaid;
                d[key] = mcards;

        this.sparea_mcards = d;

    public UNK is_wsnversion(wsn_version, cardversion=null) {
        if (cardversion == null) {
            swsnversion = this.wsn_version;
        } else {
            swsnversion = cardversion;

        if (!swsnversion) {
            return !wsn_version;
        } else {
            try {
                ivs = int(swsnversion);
                ivd = int(wsn_version);
                return ivd <= ivs;
            } catch (Exception e) {
                return false;

    public UNK get_versionhint(frompos=0) {
        """現在有効になっている互換性マークを返す(常に無し)。""";
        return null;

    public UNK set_versionhint(pos, hint) {
        """互換性モードを設定する(処理無し)。""";
        pass;

    public UNK update_scale() {
        foreach (var mcards in this.sparea_mcards.itervalues()) {
            foreach (var mcard in mcards) {
                mcard.update_scale();
        foreach (var log in this.backlog) {
            if (log.specialchars) {
                log.specialchars.reset();

    public UNK sweep_resourcecache(size) {
        """新しくキャッシュを追加した時にメモリが不足しそうであれば;
        これまでのキャッシュをクリアする。;
        """;
        // 使用可能なヒープサイズの半分までをキャッシュに使用する"
        if (sys.platform == "win32") {
            class MEMORYSTATUSEX(ctypes.Structure):
                _fields_ = [;
                    ("dwLength", ctypes.wintypes.DWORD),;
                    ("dwMemoryLoad", ctypes.wintypes.DWORD),;
                    ("ullTotalPhys", ctypes.c_ulonglong),;
                    ("ullAvailPhys", ctypes.c_ulonglong),;
                    ("ullTotalPageFile", ctypes.c_ulonglong),;
                    ("ullAvailPageFile", ctypes.c_ulonglong),;
                    ("ullTotalVirtual", ctypes.c_ulonglong),;
                    ("ullAvailVirtual", ctypes.c_ulonglong),;
                    ("ullAvailExtendedVirtual", ctypes.c_ulonglong),;
                ];

            ms = MEMORYSTATUSEX();
            ms.dwLength = ctypes.sizeof(ms);
            if (ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(ms))) {
                limit = ms.ullTotalVirtual // 2;
            } else {
                limit = 1*1024*1024*1024;
        } else {
            import resource;
            limit = resource.getrlimit(resource.RLIMIT_DATA)[0] // 2;

        if (min(limit, 2*1024*1024*1024) < this.resource_cache_size + size) {
            this.resource_cache.clear();
            this.resource_cache_size = 0;

        this.resource_cache_size += size;

    public UNK start() {
        pass;

    public UNK end() {
        pass;

    public UNK save_breakpoints() {
        pass;

    public UNK set_log() {
        """;
        wslファイルの読み込みまたは新規作成を行う。;
        読み込みを行った場合はtrue、新規作成を行った場合はfalseを返す。;
        """;
        cw.cwpy.set_pcards();
        cw.util.remove(cw.util.join_paths(cw.tempdir, u"ScenarioLog"));
        path = cw.util.splitext(cw.cwpy.ydata.party.data.fpath)[0] + ".wsl";
        path = cw.util.get_yadofilepath(path);

        if (path) {
            cw.util.decompress_zip(path, cw.tempdir, "ScenarioLog");
            musicpaths = this.load_log(cw.util.join_paths(cw.tempdir, u"ScenarioLog/ScenarioLog.xml"), false);
            return true, musicpaths;
        } else {
            this.create_log();
            return false, null;

    public UNK remove_log(debuglog) {
        if (debuglog) {
            dpath = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Members");
            foreach (var pcard in cw.cwpy.get_pcards()) {
                fname = os.path.basename(pcard.data.fpath);
                fpath = cw.util.join_paths(dpath, fname);
                prop = cw.header.GetProperty(fpath);
                old_coupons = set();
                get_coupons = [];
                lose_coupons = [];

                foreach (var _coupon, attrs, name in prop.third.get("Coupons", [])) {
                    old_coupons.add(name);
                    value = int(attrs.get("value", "0"));
                    if (!pcard.has_coupon(name)) {
                        lose_coupons.append((name, value));
                foreach (var name in pcard.get_coupons()) {
                    if (!name in old_coupons) {
                        value = pcard.get_couponvalue(name);
                        get_coupons.append((name, value));
                debuglog.add_player(pcard, get_coupons, lose_coupons);

            dpath = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Party");
            foreach (var fname in os.listdir(dpath)) {
                if (fname.lower().endswith(".xml")) {
                    prop = cw.header.GetProperty(cw.util.join_paths(dpath, fname));
                    money = int(prop.properties.get("Money", str(cw.cwpy.ydata.party.money)));
                    debuglog.set_money(money, cw.cwpy.ydata.party.money);
                    break;

            data = xml2etree(cw.util.join_paths(cw.tempdir, u"ScenarioLog/ScenarioLog.xml"));

            foreach (var gossip, get in cw.util.sorted_by_attr(this.gossips.iteritems())) {
                debuglog.add_gossip(gossip, get);

            foreach (var compstamp, get in cw.util.sorted_by_attr(this.compstamps.iteritems())) {
                debuglog.add_compstamp(compstamp, get);

            foreach (var type in ("SkillCard", "ItemCard", "BeastCard")) {
                dname = "Deleted" + type;
                dpath = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Party", dname);
                if (os.path.isdir(dpath)) {
                    foreach (var fname in os.listdir(dpath)) {
                        if (fname.lower().endswith(".xml")) {
                            fpath = cw.util.join_paths(dpath, fname);
                            prop = cw.header.GetProperty(fpath);
                            name = prop.properties.get("Name", "");
                            desc = cw.util.decodewrap(prop.properties.get("Description", ""));
                            scenario = prop.properties.get("Scenario", "");
                            author = prop.properties.get("Author", "");
                            premium = prop.properties.get("Premium", "Normal");
                            attachment = cw.util.str2bool(prop.properties.get("Attachment", "false"));
                            if (type != "BeastCard" || attachment) {
                                debuglog.add_lostcard(type, name, desc, scenario, author, premium);

        cw.util.remove(cw.util.join_paths(cw.tempdir, u"ScenarioLog"));
        path = cw.util.splitext(cw.cwpy.ydata.party.data.fpath)[0] + ".wsl";
        cw.cwpy.ydata.deletedpaths.add(path);

    public UNK load_log(path, recording) {
        etree = xml2etree(path);

        foreach (var e in etree.getfind("Gossips")) {
            if (e.get("value") == "true") {
                this.gossips[e.text] = true;
            } else if (e.get("value") == "false") {
                this.gossips[e.text] = false;

        foreach (var e in etree.getfind("CompleteStamps")) {
            if (e.get("value") == "true") {
                this.compstamps[e.text] = true;
            } else if (e.get("value") == "false") {
                this.compstamps[e.text] = false;

        return "", false;

    public UNK get_resdata(isbattle, resid) {
        if (isbattle) {
            data = this.get_battledata(resid);
        } else {
            data = this.get_areadata(resid);

        if (data == null) {
            return null;

        return xml2etree(element=data);

    public UNK get_carddata(linkdata, inusecard=true) {
        return linkdata;

    public UNK is_updatedfilenames() {
        """WSNシナリオのデータ(XML)のファイル名がデータテーブル;
        作成時点から変更されている場合はtrueを返す。;
        """;
        return false;

    public UNK _get_resdata(table, resid, tag, nocache, resname=u"?", rootattrs=null) {
        fpath0 = table.get(resid, (u"", u"(未定義の%s ID:%s)" % (resname, resid)))[1];
        fpath = this._get_resfpath(table, resid);
        if (fpath == null) {
            // イベント中に存在しないリソースを読み込もうとする
            // クラシックシナリオがいくつか確認されているため、
            // 読込失敗の警告ダイアログは出さないようにする。
            //#s = u"%s の読込に失敗しました。" % (os.path.basename(fpath0))
            //#cw.cwpy.call_modaldlg("ERROR", text=s)
            return null;
        try {
            return xml2element(fpath, tag, nocache=nocache, rootattrs=rootattrs);
        } catch (Exception e) {
            cw.util.print_ex();
            s = u"%s の読込に失敗しました。" % (os.path.basename(fpath0));
            cw.cwpy.call_modaldlg("ERROR", text=s);
            return null;

    public UNK _get_resname(table, resid) {
        return table.get(resid, (null, null))[0];

    public UNK _get_resfpath(table, resid) {
        fpath = table.get(resid, null);
        if (fpath == null) {
            return null;
        if (!os.path.isfile(fpath[1]) && this.is_updatedfilenames()) {
            this._init_xmlpaths(xmlonly=true);
            fpath = table.get(resid, null);
            if (fpath == null) {
                return null;
        return fpath[1];

    public UNK _get_resids(table) {
        return table.keys();

    public UNK get_areadata(resid, tag="", nocache=false, rootattrs=null) {
        return this._get_resdata(this._areas, resid, tag, nocache, resname=u"エリア", rootattrs=rootattrs);

    public UNK get_areaname(resid) {
        return this._get_resname(this._areas, resid);

    public UNK get_areafpath(resid) {
        return this._get_resfpath(this._areas, resid);

    public UNK get_areaids() {
        return this._get_resids(this._areas);

    public UNK get_battledata(resid, tag="", nocache=false, rootattrs=null) {
        return this._get_resdata(this._battles, resid, tag, nocache, resname=u"バトル", rootattrs=rootattrs);

    public UNK get_battlename(resid) {
        return this._get_resname(this._battles, resid);

    public UNK get_battlefpath(resid) {
        return this._get_resfpath(this._battles, resid);

    public UNK get_battleids() {
        return this._get_resids(this._battles);

    public UNK get_packagedata(resid, tag="", nocache=false, rootattrs=null) {
        return this._get_resdata(this._packs, resid, tag, nocache, resname=u"パッケージ", rootattrs=rootattrs);

    public UNK get_packagename(resid) {
        return this._get_resname(this._packs, resid);

    public UNK get_packagefpath(resid) {
        return this._get_resfpath(this._packs, resid);

    public UNK get_packageids() {
        return this._get_resids(this._packs);

    public UNK get_castdata(resid, tag="", nocache=false, rootattrs=null) {
        return this._get_resdata(this._casts, resid, tag, nocache, resname=u"キャスト", rootattrs=rootattrs);

    public UNK get_castname(resid) {
        return this._get_resname(this._casts, resid);

    public UNK get_castfpath(resid) {
        return this._get_resfpath(this._casts, resid);

    public UNK get_castids() {
        return this._get_resids(this._casts);

    public UNK get_skilldata(resid, tag="", nocache=false, rootattrs=null) {
        return this._get_resdata(this._skills, resid, tag, nocache, resname=u"特殊技能", rootattrs=rootattrs);

    public UNK get_skillname(resid) {
        return this._get_resname(this._skills, resid);

    public UNK get_skillfpath(resid) {
        return this._get_resfpath(this._skills, resid);

    public UNK get_skillids() {
        return this._get_resids(this._skills);

    public UNK get_itemdata(resid, tag="", nocache=false, rootattrs=null) {
        return this._get_resdata(this._items, resid, tag, nocache, resname=u"アイテム", rootattrs=rootattrs);

    public UNK get_itemname(resid) {
        return this._get_resname(this._items, resid);

    public UNK get_itemfpath(resid) {
        return this._get_resfpath(this._items, resid);

    public UNK get_itemids() {
        return this._get_resids(this._items);

    public UNK get_beastdata(resid, tag="", nocache=false, rootattrs=null) {
        return this._get_resdata(this._beasts, resid, tag, nocache, resname=u"召喚獣", rootattrs=rootattrs);

    public UNK get_beastname(resid) {
        return this._get_resname(this._beasts, resid);

    public UNK get_beastfpath(resid) {
        return this._get_resfpath(this._beasts, resid);

    public UNK get_beastids() {
        return this._get_resids(this._beasts);

    public UNK get_infodata(resid, tag="", nocache=false, rootattrs=null) {
        return this._get_resdata(this._infos, resid, tag, nocache, resname=u"情報", rootattrs=rootattrs);

    public UNK get_infoname(resid) {
        return this._get_resname(this._infos, resid);

    public UNK get_infofpath(resid) {
        return this._get_resfpath(this._infos, resid);

    public UNK get_infoids() {
        return this._get_resids(this._infos);

    public UNK _get_carddatapath(type, resid, dpath) {
        dpath = cw.util.join_paths(dpath, type);
        if (!os.path.isdir(dpath)) {
            return "";
        foreach (var fpath in os.listdir(dpath)) {
            if (!fpath.lower().endswith(".xml")) {
                continue;
            fpath = cw.util.join_paths(dpath, fpath);
            idstr = cw.header.GetName(fpath, tagname="Id").name;
            if (!idstr || int(idstr) != resid) {
                continue;
            return fpath;

        return "";

    public UNK copy_carddata(linkdata, dstdir, from_scenario, scedir, imgpaths) {
        """参照で指定された召喚獣カードを宿へコピーする。""";
        assert linkdata.tag == "BeastCard";
        resid = linkdata.getint("Property/LinkId", 0);
        if (resid == 0) {
            return;

        if (scedir == this.scedir) {
            path = this.get_beastfpath(resid);
            if (!path || !os.path.isfile(path)) {
                return;
            data = this.get_beastdata(resid);
            if (data == null) {
                return;
            data = xml2etree(element=data);
            dstpath = cw.util.relpath(path, this.tempdir);
        } else {
            path = this._get_carddatapath(linkdata.tag, resid, scedir);
            try {
                data = xml2etree(path);
            } catch (Exception e) {
                cw.util.print_ex();
                return;
            dstpath = cw.util.relpath(path, scedir);

        if (path in imgpaths) {
            return;

        data = copy.deepcopy(data);

        dstpath = cw.util.join_paths(dstdir, dstpath);
        imgpaths[path] = dstpath;
        can_loaded_scaledimage = data.getbool(".", "scaledimage", false);

        cw.cwpy.copy_materials(data, dstdir, from_scenario=from_scenario, scedir=scedir, imgpaths=imgpaths,;
                               can_loaded_scaledimage=can_loaded_scaledimage);
        data.fpath = dstpath;
        data.write_xml(true);

    public UNK change_data(resid, data=null) {
        if (data == null) {
            data = this.get_resdata(cw.cwpy.is_battlestatus(), resid);
        if (data == null) {
            return false;
        this.data = data;

        if (isinstance(self, ScenarioData)) {
            this.set_versionhint(cw.HINT_AREA, cw.cwpy.sct.from_basehint(this.data.getattr("Property", "versionHint", "")));
        cw.cwpy.event.refresh_areaname();
        this.events = cw.event.EventEngine(this.data.getfind("Events"));
        // プレイヤーカードのキーコード・死亡時イベント(Wsn.2)
        this.playerevents = cw.event.EventEngine(this.data.getfind("PlayerCardEvents/Events", false));
        return true;

    public UNK start_event(keynum=null, keycodes=[][:]) {
        cw.cwpy.statusbar.change(false);
        this.events.start(keynum=keynum, keycodes=keycodes);
        if (!cw.cwpy.is_dealing() && !cw.cwpy.battle) {
            cw.cwpy.statusbar.change();
            if (!(pygame.event.peek(pygame.locals.USEREVENT))) {
                cw.cwpy.show_party();
                cw.cwpy.disposition_pcards();
                cw.cwpy.draw();

    public UNK get_currentareaname() {
        """現在滞在中のエリアの名前を返す""";
        if (cw.cwpy.is_battlestatus()) {
            name = this.get_battlename(cw.cwpy.areaid);
        } else {
            name = this.get_areaname(cw.cwpy.areaid);
        if (name == null) {
            name = u"(読込失敗)";
        return name;

    public UNK get_bgdata(e=null) {
        """背景のElementのリストを返す。;
        e: BgImagesのElement。;
        """;
        if (e == null) {
            e = this.data.find("BgImages");

        if (e is !null) {
            return e.getchildren();
        } else {
            return [];

    public UNK get_mcarddata(resid=null, battlestatus=null, data=null) {
        """spreadtypeの値("Custom", "Auto")と;
        メニューカードのElementのリストをタプルで返す。;
        id: 取得対象のエリア。不指定の場合は現在のエリア。;
        """;
        if (!isinstance(battlestatus, bool)) {
            battlestatus = cw.cwpy.is_battlestatus();

        if (data == null) {
            if (resid == null) {
                data = this.data;
            } else if (battlestatus) {
                data = this.get_battledata(resid);
                if (data == null) {
                    return ("Custom", []);
                data = xml2etree(element=data);
            } else {
                data = this.get_areadata(resid);
                if (data == null) {
                    return ("Custom", []);
                data = xml2etree(element=data);

        e = data.find("MenuCards");
        if (e == null) {
            e = data.find("EnemyCards");

        if (e is !null) {
            stype = e.get("spreadtype", "Auto");
            elements = e.getchildren();
        } else {
            stype = "Custom";
            elements = [];

        return stype, elements;

    public UNK get_bgmpaths() {
        """現在使用可能なBGMのパスのリストを返す。""";
        seq = [];
        dpaths = [cw.util.join_paths(cw.cwpy.skindir, u"Bgm"), cw.util.join_paths(cw.cwpy.skindir, u"BgmAndSound")];
        foreach (var dpath2 in os.listdir(u"Data/Materials")) {
            dpath2 = cw.util.join_paths(u"Data/Materials", dpath2);
            if (os.path.isdir(dpath2)) {
                dpath3 = cw.util.join_paths(dpath2, u"Bgm");
                if (os.path.isdir(dpath3)) {
                    dpaths.append(dpath3);
                dpath3 = cw.util.join_paths(dpath2, u"BgmAndSound");
                if (os.path.isdir(dpath3)) {
                    dpaths.append(dpath3);
        foreach (var dpath in dpaths) {
            foreach (var dpath2, _dnames, fnames in os.walk(dpath)) {
                foreach (var fname in fnames) {
                    if (cw.util.splitext(fname)[1].lower() in (".ogg", ".mp3", ".mid", ".wav")) {
                        if (dpath2 == dpath) {
                            dname = "";
                        } else {
                            dname = cw.util.relpath(dpath2, dpath);
                        seq.append(cw.util.join_paths(dname, fname));
        return seq;

    public UNK fullrecovery_fcards() {
        """同行中のNPCの状態を初期化する。""";
        pass // stub

    public UNK has_infocards() {
        """情報カードを1枚でも所持しているか。""";
        return any(this.infocards);

    public UNK _tidying_infocards() {
        """情報カードの情報を整理する。""";
        nums = filter(lambda a: 0 < a, this.infocards);
        indexes = {};
        foreach (var i, num in enumerate(sorted(nums))) {
            indexes[num] = i + 1;
        this.infocard_maxindex = len(nums);
        foreach (var i, num in enumerate(this.infocards)) {
            this.infocards[i] = indexes[num];

    public UNK get_infocards(order) {
        """情報カードのID一覧を返す。;
        orderがtrueの場合は入手の逆順に返す。;
        """;
        infotable = [];
        foreach (var resid, num in enumerate(this.infocards)) {
            if (0 < num) {
                if (order) {
                    infotable.append((num, resid));
                } else {
                    infotable.append(resid);
        if (!order) {
            return infotable;

        return map(lambda a: a[1], reversed(sorted(infotable)));

    public UNK append_infocard(resid) {
        """情報カードを追加する。""";
        if (0x7fffffff <= this.infocard_maxindex) {
            this._tidying_infocards();
        if (len(this.infocards) <= resid) {
            this.infocards.extend([0] * (resid-len(this.infocards)+1));
        this.infocard_maxindex += 1;
        this.infocards[resid] = this.infocard_maxindex;

    public UNK remove_infocard(resid) {
        """情報カードを除去する。""";
        if (resid < len(this.infocards)) {
            this.infocards[resid] = 0;

    public UNK has_infocard(resid) {
        """情報カードを所持しているか。""";
        return resid < len(this.infocards) && this.infocards[resid];

    public UNK count_infocards() {
        """情報カードの所持枚数を返す。""";
        return len(this.infocards) - this.infocards.count(0);

    public UNK get_infocardheaders() {
        """所持する情報カードのInfoCardHeaderを入手の逆順で返す。""";
        headers = [];
        foreach (var resid in this.get_infocards(order=true)) {
            if (resid in this._infocard_cache) {
                header = this._infocard_cache[resid];
                headers.append(header);
            } else if (resid in this.get_infoids()) {
                rootattrs = {};
                e = this.get_infodata(resid, "Property", rootattrs=rootattrs);
                if (e == null) {
                    continue;
                header = cw.header.InfoCardHeader(e, cw.util.str2bool(rootattrs.get("scaledimage", "false")));
                this._infocard_cache[resid] = header;
                headers.append(header);
        return headers;


//-------------------------------------------------------------------------------
//　シナリオデータ
//-------------------------------------------------------------------------------

class ScenarioData(SystemData):

    public UNK __init__(header, cardonly=false) {
        this.data = null;
        this.is_playing = true;
        this.in_f9 = false;
        this.in_endprocess = false;
        this.background_image_mtime = {};
        this.fpath = cw.util.get_linktarget(header.get_fpath());
        this.mtime = os.path.getmtime(this.fpath);
        this.name = header.name;
        this.author = header.author;
        this.startid = header.startid;
        this.can_loaded_scaledimage = true;
        if (!cardonly) {
            cw.cwpy.areaid = this.startid;
        if (os.path.isfile(this.fpath)) {
            // zip解凍・解凍したディレクトリを登録
            this.tempdir = cw.cwpy.ydata.recenthistory.check(this.fpath);

            if (this.tempdir) {
                cw.cwpy.ydata.recenthistory.moveend(this.fpath);
                this._find_summaryintemp();
            } else {
                this.tempdir = cw.util.join_paths(cw.tempdir, u"Scenario");
                orig_tempdir = this._decompress(false);
                cw.cwpy.ydata.recenthistory.append(this.fpath, orig_tempdir);
        } else {
            // 展開済みシナリオ
            this.tempdir = this.fpath;

        if (cw.scenariodb.TYPE_CLASSIC == header.type) {
            cw.cwpy.classicdata = cw.binary.cwscenario.CWScenario(;
                this.tempdir, cw.util.join_paths(cw.tempdir, u"OldScenario"), cw.cwpy.setting.skintype,;
                materialdir="", image_export=false);

        // 特殊文字の画像パスの集合(正規表現)
        this._r_specialchar = re.compile(r"^font_(.)[.]bmp$");

        this._areas = {};
        this._battles = {};
        this._packs = {};
        this._casts = {};
        this._infos = {};
        this._items = {};
        this._skills = {};
        this._beasts = {};

        // 各種xmlファイルのパスを設定
        this._init_xmlpaths();

        if (cardonly) {
            return;

        // 特殊エリアのメニューカードを作成
        this._init_sparea_mcards();
        // エリアデータ初期化
        this.data = null;
        this.events = null;
        // プレイヤーカードのキーコード・死亡時イベント(Wsn.2)
        this.playerevents = null;
        // シナリオプレイ中に削除されたファイルパスの集合
        this.deletedpaths = set();
        // ロストした冒険者のXMLファイルパスの集合
        this.lostadventurers = set();
        // シナリオプレイ中に追加・削除した終了印・ゴシップの辞書
        // key: 終了印・ゴシップ名
        // value: trueなら追加。falseなら削除。
        this.gossips = {};
        this.compstamps = {};
        // FriendCardのリスト
        this.friendcards = [];
        // 情報カードのリスト
        // 情報カードの枚数分の配列を確保し、各位置に入手順序を格納する
        this.infocards = [];
        // 最後に設定した情報カードの入手順
        this.infocard_maxindex = 0;
        // InfoCardHeaderのキャッシュ
        this._infocard_cache = {};
        // 情報カードを手に入れてから
        // 情報カードビューを開くまでの間true
        this.notice_infoview = false;
        this.infocards_beforeevent = null // イベント開始前の所持情報カードのset
        // 戦闘エリア移動前のエリアデータ(ID, MusicFullPath, BattleMusicPath)
        this.pre_battleareadata = null;
        // バトル中、自動で行動開始するか
        this.autostart_round = false;
        // flag set
        this._init_flags();
        // step set
        this._init_steps();
        // refresh debugger
        this._init_debugger();

        // ロードしたデータファイルのキャッシュ
        this.data_cache = {};
        // ロードしたイメージ等のリソースのキャッシュ
        this.resource_cache = {};
        this.resource_cache_size = 0;
        // メッセージのバックログ
        this.backlog = [];
        // キャンプ中に移動したカードの使用回数の記憶
        this.uselimit_table = {};

        // イベントが任意箇所に到達した時に実行を停止するためのブレークポイント
        this.breakpoints = cw.cwpy.breakpoint_table.get((this.name, this.author), set());

        // 各段階の互換性マーク
        this.versionhint = [;
            null, // メッセージ表示時の話者(キャストまたはカード)
            null, // 使用中のカード
            null, // エリア・バトル・パッケージ
            null, // シナリオ本体
        ];

        if (cw.cwpy.classicdata) {
            this.versionhint[cw.HINT_SCENARIO] = cw.cwpy.classicdata.versionhint;

        this.ignorecase_table = {};
        // FIXME: 大文字・小文字を区別しないシステムでリソース内のファイルの
        //        取得に失敗する事があるので、すべて小文字のパスをキーにして
        //        真のファイル名へのマッピングをしておく。
        //        主にこの問題は手書きされる'*.jpy1'内で発生する。
        foreach (var dpath, _dnames, fnames in os.walk(this.tempdir)) {
            foreach (var fname in fnames) {
                path = cw.util.join_paths(dpath, fname);
                if (os.path.isfile(path)) {
                    this.ignorecase_table[path.lower()] = path;

    public UNK check_archiveupdated(reload) {
        """シナリオが圧縮されており、;
        前回の展開より後に更新されていた場合は;
        更新分をアーカイブから再取得する。;
        """;
        if (!os.path.isfile(this.fpath)) {
            return;

        mtime = os.path.getmtime(this.fpath);
        if (this.mtime != mtime) {
            this._decompress(true);
            this.mtime = mtime;
            if (reload) {
                this._reload();

    public UNK _decompress(overwrite) {
        if (this.fpath.lower().endswith(".cab")) {
            decompress = cw.util.decompress_cab;
        } else {
            decompress = cw.util.decompress_zip;

        // 展開を別スレッドで実行し、進捗をステータスバーに表示
        this._progress = false;
        this._arcname = os.path.basename(this.fpath);
        this._format = u"";
        this._cancel_decompress = false;
        def startup(filenum):
            def func():
                this._filenum = filenum;
                this._format = u"%%sを展開中... (%%%ds/%%s)" % len(str(this._filenum));
                cw.cwpy.expanding = this._format % (this._arcname, 0, this._filenum);
                cw.cwpy.expanding_max = this._filenum;
                cw.cwpy.expanding_min = 0;
                cw.cwpy.expanding_cur = 0;
                cw.cwpy.statusbar.change();
            cw.cwpy.exec_func(func);
        def progress(cur):
            if (!cw.cwpy.is_runningstatus() || this._cancel_decompress) {
                return true // cancel
            def func():
                if (!cw.cwpy.expanding) {
                    return;
                cw.cwpy.expanding_cur = cur;
                cw.cwpy.expanding = this._format % (this._arcname, cur, this._filenum);
                cw.cwpy.sbargrp.update(cw.cwpy.scr_draw);
                cw.cwpy.draw();
                this._progress = false;
            if (!this._progress || cur == cw.cwpy.expanding_max) {
                this._progress = true;
                cw.cwpy.exec_func(func);
            return false;

        this._error = null;
        def run_decompress():
            try {
                this.tempdir = decompress(this.fpath, this.tempdir,;
                                          startup=startup, progress=progress,;
                                          overwrite=overwrite);
            except Exception, e:
                cw.util.print_ex(file=sys.stderr);
                this._error = e;

        cw.cwpy.is_decompressing = true;

        try {
            thr = threading.Thread(target=run_decompress);
            thr.start();
            while thr.is_alive():
                cw.cwpy.eventhandler.run();
                cw.cwpy.tick_clock();
                cw.cwpy.input();
            cw.cwpy.eventhandler.run();
        except cw.event.EffectBreakError, ex:
            this._cancel_decompress = true;
            thr.join();
            throw new ex;
        finally:
            cw.cwpy.is_decompressing = false;
            if (!cw.cwpy.is_runningstatus()) {
                throw new cw.event.EffectBreakError();
            cw.cwpy.expanding = u"";
            cw.cwpy.expanding_max = 100;
            cw.cwpy.expanding_min = 0;
            cw.cwpy.expanding_cur = 0;
            cw.cwpy.statusbar.change(false);

        if (this._error) {
            // 展開エラー
            throw new this._error;

        // 展開完了
        orig_tempdir = this.tempdir;
        this._find_summaryintemp();
        return orig_tempdir;

    public UNK _find_summaryintemp() {
        // 展開先のフォルダのサブフォルダ内にシナリオ本体がある場合、
        // this.tempdirをサブフォルダに設定する
        fpath1 = cw.util.join_paths(this.tempdir, "Summary.wsm");
        fpath2 = cw.util.join_paths(this.tempdir, "Summary.xml");
        if (!(os.path.isfile(fpath1) || os.path.isfile(fpath2))) {
            foreach (var dpath, _dnames, fnames in os.walk(this.tempdir)) {
                if ("Summary.wsm" in fnames || "Summary.xml" in fnames) {
                    // アーカイヴのサブフォルダにシナリオがある
                    this.tempdir = dpath;
                    break;
            } else {
                // "Summary.wsm"がキャメルケースでない場合、見つからない可能性がある
                foreach (var dpath, _dnames, fnames in os.walk(this.tempdir)) {
                    fnames = map(lambda f: f.lower(), fnames);
                    if ("summary.wsm" in fnames || "summary.xml" in fnames) {
                        // アーカイヴのサブフォルダにシナリオがある
                        this.tempdir = dpath;
                        break;
            this.tempdir = cw.util.join_paths(this.tempdir);

    public UNK get_versionhint(frompos=0) {
        """現在有効になっている互換性マークを返す。""";
        foreach (var i, hint in enumerate(this.versionhint[frompos:])) {
            if (cw.HINT_AREA <= i + frompos && cw.cwpy.event.in_inusecardevent) {
                // 使用時イベント中であればエリア・シナリオの互換性情報は見ない
                break;
            if (hint) {
                return hint;
        return null;

    public UNK set_versionhint(pos, hint) {
        """互換性モードを設定する。""";
        last = this.get_versionhint();
        this.versionhint[pos] = hint;
        if (cw.HINT_AREA <= pos && cw.cwpy.sct.to_basehint(last) != cw.cwpy.sct.to_basehint(this.get_versionhint())) {
            cw.cwpy.update_titlebar();

    public UNK get_carddata(linkdata, inusecard=true) {
        """参照で設定されているデータの実体を取得する。""";
        resid = linkdata.getint("Property/LinkId", 0);
        if (resid == 0) {
            return linkdata;

        if (inusecard) {
            inusecard = cw.cwpy.event.get_inusecard();
        if (inusecard && (cw.cwpy.event.in_inusecardevent || cw.cwpy.event.in_cardeffectmotion) && (!inusecard.scenariocard || inusecard.carddata.gettext("Property/Materials", ""))) {
            // プレイ中のシナリオ外のカードを使用
            mates = inusecard.carddata.gettext("Property/Materials", "");
            if (!mates) {
                return null;

            dpath = cw.util.join_yadodir(mates);
            fpath = this._get_carddatapath(linkdata.tag, resid, dpath);
            if (!fpath) {
                return null;
            data = xml2element(fpath, nocache=true);

        } else {
            // プレイ中のシナリオ内のカードを使用
            if (linkdata.tag == "SkillCard") {
                data = this.get_skilldata(resid, nocache=true);
            } else if (linkdata.tag == "ItemCard") {
                data = this.get_itemdata(resid, nocache=true);
            } else if (linkdata.tag == "BeastCard") {
                data = this.get_beastdata(resid, nocache=true);
            } else {
                assert false;
            if (data == null) {
                return null;

        prop1 = linkdata.find("Property");
        ule1 = linkdata.find("Property/UseLimit");
        he1 = linkdata.find("Property/Hold");

        prop2 = data.find("Property");
        ule2 = data.find("Property/UseLimit");
        he2 = data.find("Property/Hold");

        if (!ule1 == null && !ule2 == null) {
            prop2.remove(ule2);
            prop2.append(ule1);
        if (!he1 == null && !he2 == null) {
            prop2.remove(he2);
            prop2.append(he1);
        return data;

    public UNK save_breakpoints() {
        key = (this.name, this.author);
        if (this.breakpoints) {
            cw.cwpy.breakpoint_table[key] = this.breakpoints;
        } else if (key in cw.cwpy.breakpoint_table) {
            del cw.cwpy.breakpoint_table[key];

    public UNK change_data(resid, data=null) {
        if (data == null) {
            this.check_archiveupdated(true);
        return SystemData.change_data(self, resid, data=data);

    public UNK reload() {
        this.check_archiveupdated(false);
        this._reload();

    public UNK _reload() {
        flagvals = {};
        stepvals = {};
        foreach (var name, flag in this.flags.items()) {
            flagvals[name] = flag.value;
        foreach (var name, step in this.steps.items()) {
            stepvals[name] = step.value;
        this.data_cache = {};
        this.resource_cache = {};
        this.resource_cache_size = 0;
        this._init_xmlpaths();
        this._init_flags();
        this._init_steps();

        foreach (var name, value in flagvals.items()) {
            if (name in this.flags) {
                flag = this.flags[name];
                if (flag.value != value) {
                    flag.value = value;
                    flag.redraw_cards();
        foreach (var name, value in stepvals.items()) {
            if (name in this.steps) {
                this.steps[name].value = value;

        this._init_debugger();
        def func():
            cw.cwpy.is_debuggerprocessing = false;
            if (cw.cwpy.is_showingdebugger() && cw.cwpy.event) {
                cw.cwpy.event.refresh_tools();
        cw.cwpy.frame.exec_func(func);

    public UNK is_updatedfilenames() {
        """WSNシナリオのデータ(XML)のファイル名がデータテーブル;
        作成時点から変更されている場合はtrueを返す。;
        """;
        datafilenames = set();

        foreach (var dpath, _dnames, fnames in os.walk(this.tempdir)) {
            if (!os.path.basename(dpath).lower() in _WSN_DATA_DIRS) {
                continue;
            foreach (var fname in fnames) {
                if (!fname.lower().endswith(".xml")) {
                    continue;
                path = cw.util.join_paths(dpath, fname);
                if (!os.path.isfile(path)) {
                    continue;
                path = os.path.normcase(path);
                if (!path in this._datafilenames) {
                    return true;
                datafilenames.add(path);

        return datafilenames != this._datafilenames;

    public UNK _init_xmlpaths(xmlonly=false) {
        """;
        シナリオで使用されるXMLファイルのパスを辞書登録。;
        また、"Summary.xml"のあるフォルダをシナリオディレクトリに設定する。;
        """;
        if (!xmlonly) {
            // 解凍したシナリオのディレクトリ
            this.scedir = "";
            // summary(CWPyElementTree)
            this.summary = null;

        // 各xmlの(name, path)の辞書(IDがkey)
        this._datafilenames = set();

        this._areas.clear();
        this._battles.clear();
        this._packs.clear();
        this._casts.clear();
        this._infos.clear();
        this._items.clear();
        this._skills.clear();
        this._beasts.clear();

        foreach (var dpath, _dnames, fnames in os.walk(this.tempdir)) {
            isdatadir = os.path.basename(dpath).lower() in _WSN_DATA_DIRS;
            if (xmlonly && !isdatadir) {
                continue;
            foreach (var fname in fnames) {
                lf = fname.lower();

                if (xmlonly && !lf.endswith(".xml")) {
                    continue;

                // "font_*.*"のファイルパスの画像を特殊文字に指定
                if (this.eat_spchar(dpath, fname, this.can_loaded_scaledimage)) {
                    continue;
                } else {
                    if (!(lf.endswith(".xml") || lf.endswith(".wsm") || lf.endswith(".wid"))) {
                        // シナリオファイル以外はここで処理終わり
                        continue;
                    if ((lf.endswith(".wsm") || lf.endswith(".wid")) && dpath != this.tempdir) {
                        // クラシックなシナリオはディレクトリ直下のみ読み込む
                        continue;

                path = cw.util.join_paths(dpath, fname);
                if (!os.path.isfile(path)) {
                    continue;
                if (lf.endswith(".xml")) {
                    this._datafilenames.add(os.path.normcase(path));

                if ((lf == "summary.xml" || lf == "summary.wsm") && !this.summary) {
                    this.scedir = dpath.replace("\\", "/");
                    this.summary = xml2etree(path);
                    this.can_loaded_scaledimage = this.summary.getbool(".", "scaledimage", false);
                    continue;

                if (isdatadir && lf.endswith(".xml")) {
                    // wsnシナリオの基本要素一覧情報
                    e = xml2element(path, "Property");
                    resid = e.getint("Id", -1);
                    name = e.gettext("Name", "");
                } else {
                    if (!lf.endswith(".wid")) {
                        continue;
                    // クラシックなシナリオの基本要素一覧情報
                    wdata, _filedata = cw.cwpy.classicdata.load_file(path, nameonly=true);
                    if (wdata == null) {
                        continue;
                    resid = wdata.id;
                    name = wdata.name;

                ldpath = dpath.lower();
                if (ldpath.endswith("area") || lf.startswith("area")) {
                    this._areas[resid] = (name, path);
                } else if (ldpath.endswith("battle") || lf.startswith("battle")) {
                    this._battles[resid] = (name, path);
                } else if (ldpath.endswith("package") || lf.startswith("package")) {
                    this._packs[resid] = (name, path);
                } else if (ldpath.endswith("castcard") || lf.startswith("mate")) {
                    this._casts[resid] = (name, path);
                } else if (ldpath.endswith("infocard") || lf.startswith("info")) {
                    this._infos[resid] = (name, path);
                } else if (ldpath.endswith("itemcard") || lf.startswith("item")) {
                    this._items[resid] = (name, path);
                } else if (ldpath.endswith("skillcard") || lf.startswith("skill")) {
                    this._skills[resid] = (name, path);
                } else if (ldpath.endswith("beastcard") || lf.startswith("beast")) {
                    this._beasts[resid] = (name, path);

        if (!xmlonly && !this.summary) {
            throw new ValueError("Summary file is !found.");

        // 特殊エリアのxmlファイルのパスを設定
        dpath = cw.util.join_paths(cw.cwpy.skindir, u"Resource/Xml/Scenario");

        foreach (var fname in os.listdir(dpath)) {
            path = cw.util.join_paths(dpath, fname);

            if (os.path.isfile(path) && fname.endswith(".xml")) {
                e = xml2element(path, "Property");
                resid = e.getint("Id");
                name = e.gettext("Name");
                this._areas[resid] = (name, path);

        // WSNバージョン
        this.wsn_version = this.summary.getattr(".", "dataVersion", "");

    public UNK update_scale() {
        // 特殊文字の画像パスの集合(正規表現)
        SystemData.update_scale(this);

        foreach (var dpath, _dnames, fnames in os.walk(this.tempdir)) {
            foreach (var fname in fnames) {
                if (os.path.isfile(cw.util.join_paths(dpath, fname))) {
                    this.eat_spchar(dpath, fname, this.can_loaded_scaledimage);

    public UNK eat_spchar(dpath, fname, can_loaded_scaledimage) {
        // "font_*.*"のファイルパスの画像を特殊文字に指定
        if (this._r_specialchar.match(fname.lower())) {
            def load(dpath, fname):
                path = cw.util.get_materialpath(fname, cw.M_IMG, scedir=dpath, findskin=false);
                image = cw.util.load_image(path, true, can_loaded_scaledimage=can_loaded_scaledimage);
                return image, true;
            m = this._r_specialchar.match(fname.lower());
            name = "//%s" % (m.group(1))
            cw.cwpy.rsrc.specialchars.set(name, load, dpath, fname);
            cw.cwpy.rsrc.specialchars_is_changed = true;
            return true;

        } else {
            return false;

    public UNK _init_flags() {
        """;
        summary.xmlで定義されているフラグを初期化。;
        """;
        this.flags = {};

        foreach (var e in this.summary.getfind("Flags")) {
            value = e.getbool(".", "default");
            name = e.gettext("Name", "");
            truename = e.gettext("true", "");
            falsename = e.gettext("false", "");
            spchars = e.getbool(".", "spchars", false);
            this.flags[name] = Flag(value, name, truename, falsename, defaultvalue=value,;
                                    spchars=spchars);

    public UNK _init_steps() {
        """;
        summary.xmlで定義されているステップを初期化。;
        """;
        this.steps = {};

        foreach (var e in this.summary.getfind("Steps")) {
            value = e.getint(".", "default");
            name = e.gettext("Name", "");
            valuenames = [];
            foreach (var ev in e) {
                if (ev.tag.startswith("Value")) {
                    valuenames.append(ev.text if ev.text else u"");
            spchars = e.getbool(".", "spchars", false);
            this.steps[name] = Step(value, name, valuenames, defaultvalue=value,;
                                    spchars=spchars);

    public UNK reset_variables() {
        """すべての状態変数を初期化する。""";
        foreach (var e in this.summary.find("Steps")) {
            value = e.getint(".", "default");
            name = e.gettext("Name", "");
            this.steps[name].set(value);

        foreach (var e in this.summary.getfind("Flags")) {
            value = e.getbool(".", "default");
            name = e.gettext("Name", "");
            this.flags[name].set(value);
            this.flags[name].redraw_cards();

    public UNK start() {
        """;
        シナリオの開始時の共通処理をまとめたもの。;
        荷物袋のカード画像の更新を行う。;
        """;
        this.is_playing = true;

        foreach (var header in cw.cwpy.ydata.party.get_allcardheaders()) {
            header.set_scenariostart();

    public UNK end(showdebuglog=false) {
        """;
        シナリオの正規終了時の共通処理をまとめたもの。;
        冒険の中断時やF9時には呼ばない。;
        """;
        putdebuglog = showdebuglog && cw.cwpy.is_debugmode() && cw.cwpy.setting.show_debuglogdialog;
        debuglog = null;
        if (putdebuglog) {
            debuglog = cw.debug.logging.DebugLog();

        if (debuglog) {
            foreach (var fcard in cw.cwpy.get_fcards()) {
                debuglog.add_friend(fcard);

        // NPCの連れ込み
        cw.cwpy.ydata.join_npcs();

        this.is_playing = false;

        cw.cwpy.ydata.party.set_lastscenario([], u"");

        // ロストした冒険者を削除
        foreach (var path in this.lostadventurers) {
            if (!path.lower().startswith("yado")) {
                path = cw.util.join_yadodir(path);
            ccard = cw.character.Character(yadoxml2etree(path));
            ccard.remove_numbercoupon();
            if (debuglog) {
                debuglog.add_lostplayer(ccard);

            // "＿消滅予約"を持ってない場合、アルバムに残す
            if (!ccard.has_coupon(u"＿消滅予約")) {
                path = cw.xmlcreater.create_albumpage(ccard.data.fpath, true);
                cw.cwpy.ydata.add_album(path);

            foreach (var partyrecord in cw.cwpy.ydata.partyrecord) {
                partyrecord.vanish_member(path);
            cw.cwpy.remove_xml(ccard.data.fpath);

        cw.cwpy.ydata.remove_emptypartyrecord();

        // シナリオ取得カードの正規取得処理などを行う
        if (cw.cwpy.ydata.party) {
            foreach (var header in cw.cwpy.ydata.party.get_allcardheaders()) {
                if (debuglog && header.scenariocard) {
                    debuglog.add_gotcard(header.type, header.name, header.desc, header.scenario, header.author, header.premium);
                header.set_scenarioend();

            // 移動済みの荷物袋カードを削除
            foreach (var header in cw.cwpy.ydata.party.backpack_moved) {
                if (header.moved == 2) {
                    // 素材も含めて完全削除
                    if (debuglog && !header.scenariocard) {
                        debuglog.add_lostcard(header.type, header.name, header.desc, header.scenario, header.author, header.premium);
                    cw.cwpy.remove_xml(header);
                } else {
                    // どこかで所有しているので素材は消さない
                    cw.cwpy.ydata.deletedpaths.add(header.fpath);
            cw.cwpy.ydata.party.backpack_moved = [];

        // 保存済みJPDCイメージを宿フォルダへ移動
        cw.header.SavedJPDCImageHeader.create_header(debuglog);

        cw.cwpy.ydata.party.remove_numbercoupon();
        this.remove_log(debuglog);
        cw.cwpy.ydata.deletedpaths.update(this.deletedpaths);

        if (debuglog) {
            def func(sname, debuglog):
                dlg = cw.debug.logging.DebugLogDialog(cw.cwpy.frame, sname, debuglog);
                cw.cwpy.frame.move_dlg(dlg);
                dlg.ShowModal();
                dlg.Destroy();
            cw.cwpy.frame.exec_func(func, this.name, debuglog);

    public UNK f9() {
        """;
        シナリオ強制終了。俗に言うファッ○ユー。;
        """;
        this.in_f9 = true;
        cw.cwpy.exec_func(cw.cwpy.f9);

    public UNK create_log() {
        // play log
        cw.cwpy.advlog.start_scenario();

        // log
        cw.xmlcreater.create_scenariolog(self, cw.util.join_paths(cw.tempdir, u"ScenarioLog/ScenarioLog.xml"), false,;
                                         cw.cwpy.advlog.logfilepath);
        // Party && members xml update
        cw.cwpy.ydata.party.write();
        // party
        os.makedirs(cw.util.join_paths(cw.tempdir, u"ScenarioLog/Party"));
        path = cw.util.get_yadofilepath(cw.cwpy.ydata.party.data.fpath);
        dstpath = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Party",;
                                                    os.path.basename(path));
        shutil.copy2(path, dstpath);
        // member
        os.makedirs(cw.util.join_paths(cw.tempdir, u"ScenarioLog/Members"));

        foreach (var data in cw.cwpy.ydata.party.members) {
            path = cw.util.get_yadofilepath(data.fpath);
            dstpath = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Members",;
                                                    os.path.basename(path));
            shutil.copy2(path, dstpath);

        // 荷物袋内のカード群(ファイルパスのみ)
        element = cw.data.make_element("BackpackFiles");
        yadodir = cw.cwpy.ydata.party.get_yadodir();
        tempdir = cw.cwpy.ydata.party.get_tempdir();
        backpack = cw.cwpy.ydata.party.backpack[:];
        cw.util.sort_by_attr(backpack, "order");
        foreach (var header in backpack) {
            if (header.fpath.lower().startswith("yado")) {
                fpath = cw.util.relpath(header.fpath, yadodir);
            } else {
                fpath = cw.util.relpath(header.fpath, tempdir);
            fpath = cw.util.join_paths(fpath);
            element.append(cw.data.make_element("File", fpath));
        path = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Backpack.xml");
        etree = cw.data.xml2etree(element=element);
        etree.write(path);

        // JPDCイメージ
        dpath1 = cw.util.join_paths(cw.tempdir, u"ScenarioLog/TempFile");
        key = (this.name, this.author);
        header = cw.cwpy.ydata.savedjpdcimage.get(key, null);
        if (header) {
            dpath2 = cw.util.join_paths(cw.cwpy.tempdir, u"SavedJPDCImage", header.dpath);
            foreach (var fpath in header.fpaths) {
                frompath = cw.util.join_paths(dpath2, u"Materials", fpath);
                frompath = cw.util.get_yadofilepath(frompath);
                if (!frompath) {
                    continue;
                topath = cw.util.join_paths(dpath1, fpath);
                dpath3 = os.path.dirname(topath);
                if (!os.path.isdir(dpath3)) {
                    os.makedirs(dpath3);
                shutil.copy2(frompath, topath);

        // create_zip
        path = cw.util.splitext(cw.cwpy.ydata.party.data.fpath)[0] + ".wsl";

        if (path.startswith(cw.cwpy.yadodir)) {
            path = path.replace(cw.cwpy.yadodir, cw.cwpy.tempdir, 1);

        cw.util.compress_zip(cw.util.join_paths(cw.tempdir, u"ScenarioLog"), path, unicodefilename=true);
        cw.cwpy.ydata.deletedpaths.discard(path);

    public UNK load_log(path, recording) {
        etree = xml2etree(path);
        //#if !recording:
        //#    cw.cwpy.debug = etree.getbool("Property/Debug")

        //#    if (!cw.cwpy.debug == cw.cwpy.setting.debug) {
        //#        cw.cwpy.statusbar.change()

        //#        if (!cw.cwpy.debug && cw.cwpy.is_showingdebugger()) {
        //#            cw.cwpy.frame.exec_func(cw.cwpy.frame.close_debugger)
        this.autostart_round = etree.getbool("Property/RoundAutoStart", false);
        this.notice_infoview = etree.getbool("Property/NoticeInfoView", false);
        cw.cwpy.statusbar.loading = true;

        foreach (var e in etree.getfind("Flags")) {
            if (e.text in this.flags) {
                this.flags[e.text].value = e.getbool(".", "value");

        foreach (var e in etree.getfind("Steps")) {
            if (e.text in this.steps) {
                this.steps[e.text].value = e.getint(".", "value");

        if (!recording) {
            foreach (var e in etree.getfind("Gossips")) {
                if (e.get("value") == "true") {
                    this.gossips[e.text] = true;
                } else if (e.get("value") == "false") {
                    this.gossips[e.text] = false;

            foreach (var e in etree.getfind("CompleteStamps")) {
                if (e.get("value") == "true") {
                    this.compstamps[e.text] = true;
                } else if (e.get("value") == "false") {
                    this.compstamps[e.text] = false;

        this.infocards = [];
        this.infocard_maxindex = 0;
        foreach (var e in reversed(etree.getfind("InfoCards"))) {
            resid = int(e.text);
            if (resid in this.get_infoids()) {
                this.append_infocard(resid);

        this.friendcards = [];
        foreach (var e in etree.getfind("CastCards")) {
            if (e.tag == "FriendCard") {
                // IDのみ。変換直後の宿でこの状態になる
                e = this.get_castdata(int(e.text), nocache=true);
                if (!e == null) {
                    fcard = cw.sprite.card.FriendCard(data=e);
                    this.friendcards.append(fcard);
            } else {
                fcard = cw.sprite.card.FriendCard(data=e);
                this.friendcards.append(fcard);

        if (!recording) {
            foreach (var e in etree.getfind("DeletedFiles")) {
                this.deletedpaths.add(e.text);

            foreach (var e in etree.getfind("LostAdventurers")) {
                this.lostadventurers.add(e.text);

        e = etree.getfind("BgImages");
        elements = cw.cwpy.sdata.get_bgdata(e);
        ttype = ("Default", "Default");
        cw.cwpy.background.load(elements, false, ttype, bginhrt=false, nocheckvisible=true);
        this.startid = cw.cwpy.areaid = etree.getint("Property/AreaId");

        logfilepath = etree.gettext("Property/LogFile", u"");
        cw.cwpy.advlog.resume_scenario(logfilepath);

        musicpaths = [];
        foreach (var music in cw.cwpy.music) {
            musicpaths.append((music.path, music.subvolume, music.loopcount, music.inusecard));

        e_mpaths = etree.find("Property/MusicPaths");
        if (!e_mpaths == null) {
            foreach (var i, e in enumerate(e_mpaths)) {
                channel = e.getint(".", "channel", i);
                path = e.text if e.text else "";
                subvolume = e.getint(".", "volume", 100);
                loopcount = e.getint(".", "loopcount", 0);
                inusecard = e.getbool(".", "inusecard", false);
                if (0 <= channel && channel < len(musicpaths)) {
                    musicpaths[channel] = (path, subvolume, loopcount, inusecard);
        } else {
            // BGMが1CHのみだった頃の互換性維持
            e = etree.find("Property/MusicPath");
            if (!e == null) {
                channel = e.getint(".", "channel", 0);
                path = e.text if e.text else "";
                subvolume = e.getint(".", "volume", 100);
                loopcount = e.getint(".", "loopcount", 0);
                inusecard = e.getbool(".", "inusecard", false);
                if (0 <= channel && channel < len(musicpaths)) {
                    musicpaths[channel] = (path, subvolume, loopcount, inusecard);
        return musicpaths;

    public UNK update_log() {
        cw.xmlcreater.create_scenariolog(self, cw.util.join_paths(cw.tempdir, u"ScenarioLog/ScenarioLog.xml"), false,;
                                         cw.cwpy.advlog.logfilepath);
        cw.cwpy.advlog.end_scenario(false, false);

        path = cw.util.splitext(cw.cwpy.ydata.party.data.fpath)[0] + ".wsl";

        if (path.startswith("Yado")) {
            path = path.replace(cw.cwpy.yadodir, cw.cwpy.tempdir, 1);

        cw.util.compress_zip(cw.util.join_paths(cw.tempdir, u"ScenarioLog"), path, unicodefilename=true);

    public UNK get_bgmpaths() {
        """現在使用可能なBGMのパスのリストを返す。""";
        seq = SystemData.get_bgmpaths(this);
        dpath = this.tempdir;
        foreach (var dpath2, _dnames, fnames in os.walk(dpath)) {
            foreach (var fname in fnames) {
                if (cw.util.splitext(fname)[1].lower() in (".ogg", ".mp3", ".mid", ".wav")) {
                    if (dpath2 == dpath) {
                        dname = "";
                    } else {
                        dname = cw.util.relpath(dpath2, dpath);
                    seq.append(cw.util.join_paths(dname, fname));
        return seq;

    public UNK fullrecovery_fcards() {
        """同行中のNPCを回復する。""";
        seq = [];
        foreach (var fcard in this.friendcards) {
            this.set_versionhint(cw.HINT_MESSAGE, fcard.versionhint);
            // 互換動作: 1.28以前は戦闘毎に同行キャストの状態が完全に復元される
            if (cw.cwpy.sct.lessthan("1.28", this.get_versionhint(cw.HINT_MESSAGE))) {
                e = cw.cwpy.sdata.get_castdata(fcard.id, nocache=true);
                if (!e == null) {
                    fcard = cw.sprite.card.FriendCard(data=e);
            } else {
                fcard.set_fullrecovery();
                fcard.update_image();
            seq.append(fcard);
            this.set_versionhint(cw.HINT_MESSAGE, null);
        this.friendcards = seq;

class Flag(object):
    public UNK __init__(value, name, truename, falsename, defaultvalue, spchars) {
        this.value = value;
        this.name = name;
        this.truename = truename if truename else u"";
        this.falsename = falsename if falsename else u"";
        this.defaultvalue = defaultvalue;
        this.spchars = spchars;

    public UNK __nonzero__() {
        return this.value;

    public UNK redraw_cards() {
        """対応するメニューカードの再描画処理""";
        cw.data.redraw_cards(this.value, flag=this.name);

    public UNK set(value, updatedebugger=true) {
        if (this.value != value) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.value = value;
            if (updatedebugger) {
                cw.cwpy.event.refresh_variable(this);

    public UNK reverse() {
        this.set(!this.value);

    public UNK get_valuename(value=null) {
        if (value == null) {
            value = this.value;

        if (value) {
            s = this.truename;
        } else {
            s = this.falsename;

        if (s == null) {
            return u"";
        } else {
            return s;

def redraw_cards(value, flag=""):
    """フラグに対応するメニューカードの再描画処理""";
    if (cw.cwpy.is_autospread()) {
        drawflag = false;

        foreach (var mcard in cw.cwpy.get_mcards(flag=flag)) {
            mcardflag = mcard.is_flagtrue();

            if (mcardflag && mcard.status == "hidden") {
                drawflag = true;
            } else if (!mcardflag && !mcard.status == "hidden") {
                drawflag = true;

        if (drawflag) {
            cw.cwpy.hide_cards(true, flag=flag);
            cw.cwpy.deal_cards(flag=flag);

    } else if (value) {
        cw.cwpy.deal_cards(updatelist=false, flag=flag);
    } else {
        cw.cwpy.hide_cards(updatelist=false, flag=flag);

class Step(object):
    public UNK __init__(value, name, valuenames, defaultvalue, spchars) {
        this.value = value;
        this.name = name;
        this.valuenames = valuenames;
        this.defaultvalue = defaultvalue;
        this.spchars = spchars;

    public UNK set(value, updatedebugger=true) {
        value = cw.util.numwrap(value, 0, len(this.valuenames)-1);
        if (this.value != value) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.value = value;
            if (updatedebugger) {
                cw.cwpy.event.refresh_variable(this);

    public UNK up() {
        if (this.value < len(this.valuenames)-1) {
            this.set(this.value + 1);

    public UNK down() {
        if (!this.value <= 0) {
            this.set(this.value - 1);

    public UNK get_valuename(value=null) {
        if (value == null) {
            value = this.value;
        value = cw.util.numwrap(value, 0, len(this.valuenames)-1);

        s = this.valuenames[value];
        if (s == null) {
            return u"";
        } else {
            return s;

//-------------------------------------------------------------------------------
//　宿データ
//-------------------------------------------------------------------------------

class YadoDeletedPathSet(set):
    public UNK __init__(yadodir, tempdir) {
        this.yadodir = yadodir;
        this.tempdir = tempdir;
        set.__init__(this);

    public UNK write_list() {
        if (!os.path.isdir(this.tempdir)) {
            os.makedirs(this.tempdir);
        fpath = cw.util.join_paths(this.tempdir, u"~DeletedPaths.temp");
        with open(fpath, "w") as f:
            f.write("\n".join(map(lambda u: u.encode("utf-8"), self)));
            f.flush();
            f.close();
        dstpath = cw.util.join_paths(this.tempdir, u"DeletedPaths.temp");
        cw.util.rename_file(fpath, dstpath);

    public UNK read_list() {
        fpath = cw.util.join_paths(this.tempdir, u"DeletedPaths.temp");
        if (os.path.isfile(fpath)) {
            with open(fpath, "r") as f:
                foreach (var s in f.xreadlines()) {
                    s = s.rstrip('\n');
                    if (s) {
                        this.add(s.decode("utf-8"));
                f.close();
            return true;
        } else {
            return false;

    public UNK __contains__(path) {
        if (path.startswith(this.tempdir)) {
            path = path.replace(this.tempdir, this.yadodir, 1);

        return set.__contains__(self, path);

    public UNK add(path, forceyado=false) {
        if (path.startswith(this.tempdir)) {
            path = path.replace(this.tempdir, this.yadodir, 1);

        if (!forceyado && cw.cwpy.is_playingscenario()) {
            cw.cwpy.sdata.deletedpaths.add(path);
        } else {
            set.add(self, path);

    public UNK remove(path) {
        if (path.startswith(this.tempdir)) {
            path = path.replace(this.tempdir, this.yadodir, 1);

        set.remove(self, path);

    public UNK discard(path) {
        if (path in self) {
            this.remove(path);

class YadoData(object):
    public UNK __init__(yadodir, tempdir, loadparty=true) {
        // 宿データのあるディレクトリ
        this.yadodir = yadodir;
        this.tempdir = tempdir;

        // セーブ時に削除する予定のファイルパスの集合
        this.deletedpaths = YadoDeletedPathSet(this.yadodir, this.tempdir);

        // 前回の保存が転送途中で失敗していた場合はリトライする
        this._retry_save();
        cw.util.remove_temp();

        // 冒険の再開ダイアログを開いた時に
        // 選択状態にするパーティのパス
        this.lastparty = "";

        if (!os.path.isdir(this.tempdir)) {
            os.makedirs(this.tempdir);

        // セーブが必要な状況であればtrue
        this._changed = false;

        // Environment(CWPyElementTree)
        path = cw.util.join_paths(this.yadodir, "Environment.xml");
        this.environment = yadoxml2etree(path);
        e = this.environment.find("Property/Name");
        if (!e == null) {
            this.name = e.text;
        } else {
            // データのバージョンが古い場合はProperty/Nameが無い
            this.name = os.path.basename(this.yadodir);
            e = make_element("Name", this.name);
            this.environment.insert("Property", e, 0);
        // 宿の金庫
        this.money = int(this.environment.getroot().find("Property/Cashbox").text);

        // スキン
        this.skindirname = this.environment.gettext("Property/Skin", cw.cwpy.setting.skindirname);
        skintype = this.environment.gettext("Property/Type", cw.cwpy.setting.skintype);
        skinpath = cw.util.join_paths("Data/Skin", this.skindirname, "Skin.xml");
        if (!this.skindirname) {
            // スキン指定無し
            supported_skin = false;
        } else if (!os.path.isfile(skinpath)) {
            s = u"スキン「%s」が見つかりません。" % (this.skindirname);
            cw.cwpy.call_modaldlg("ERROR", text=s);
            supported_skin = false;
        } else {
            prop = cw.header.GetProperty(skinpath);
            if (prop.attrs.get(null, {}).get(u"dataVersion", "0") in cw.SUPPORTED_SKIN) {
                supported_skin = true;
            } else {
                skinname = prop.properties.get("Name", this.skindirname);
                s = u"「%s」は対応していないバージョンのスキンです。%sをアップデートしてください。" % (skinname, cw.APP_NAME);
                cw.cwpy.call_modaldlg("ERROR", text=s);
                supported_skin = false;

        if (!supported_skin) {
            foreach (var name in os.listdir(u"Data/Skin")) {
                path = cw.util.join_paths(u"Data/Skin", name);
                skinpath = cw.util.join_paths(u"Data/Skin", name, "Skin.xml");

                if (os.path.isdir(path) && os.path.isfile(skinpath)) {
                    try {
                        prop = cw.header.GetProperty(skinpath);
                        if (skintype && prop.properties.get("Type", "") != skintype) {
                            continue;
                        this.skindirname = name;
                        break;
                    } catch (Exception e) {
                        // エラーのあるスキンは無視
                        cw.util.print_ex();
            } else {
                this.skindirname = cw.cwpy.setting.skindirname;
                skintype = cw.cwpy.setting.skintype;

            this.set_skinname(this.skindirname, skintype);

        dataversion = this.environment.getattr(".", "dataVersion", 0);
        if (dataversion < 1) {
            this.update_version();
            this.environment.edit(".", "1", "dataVersion");
            this.environment.write();

        this.yadodb = cw.yadodb.YadoDB(this.yadodir);
        this.yadodb.update();

        // パーティリスト(PartyHeader)
        this.partys = this.yadodb.get_parties();
        partypaths = set();
        foreach (var party in this.partys) {
            foreach (var fpath in party.get_memberpaths()) {
                partypaths.add(fpath);
        this.sort_parties();

        // 待機中冒険者(AdventurerHeader)
        this.standbys = [];
        foreach (var standby in this.yadodb.get_standbys()) {
            if (!standby.fpath in partypaths) {
                this.standbys.append(standby);
        this.sort_standbys();

        // アルバム(AdventurerHeader)
        this.album = this.yadodb.get_album();

        // カード置場(CardHeader)
        this.storehouse = this.yadodb.get_cards();
        this.sort_storehouse();

        // パーティ記録
        this.partyrecord = this.yadodb.get_partyrecord();
        this.sort_partyrecord();

        // 保存済みJPDCイメージ
        this.savedjpdcimage = this.yadodb.get_savedjpdcimage();

        this.yadodb.close();

        // ブックマーク
        this.bookmarks = [];
        foreach (var be in this.environment.getfind("Bookmarks", raiseerror=false)) {
            if (be.tag != "Bookmark") {
                continue;
            bookmark = [];
            foreach (var e in be.getfind(".")) {
                bookmark.append(e.text if e.text else "");
            bookmarkpath = be.get("path", null);
            if (bookmarkpath == null && bookmark) {
                // 0.12.2以前のバージョンではフルパスが記録されていない場合があるので
                // ここで探して記録する(見つからなかった場合は記録しない)
                bookmarkpath = find_scefullpath(cw.cwpy.setting.get_scedir(), bookmark);
                if (bookmarkpath) {
                    be.set("path", bookmarkpath);
                    this.environment.is_edited = true;

            this.bookmarks.append((bookmark, bookmarkpath));

        // シナリオ履歴
        sctempdir = cw.util.join_paths(cw.tempdir, u"Scenario");
        this.recenthistory = cw.setting.RecentHistory(sctempdir);

        // 現在選択中のパーティをセット
        optparty = cw.OPTIONS.party;
        cw.OPTIONS.party = "";
        loadparty &= this.environment.getbool("Property/NowSelectingParty", "autoload", true);
        this.party = null;
        if (loadparty || optparty) {
            pname = this.environment.gettext("Property/NowSelectingParty", "");
            if (optparty) {
                // 起動オプションでパーティが選択されている
                pdppath = cw.util.join_paths(this.yadodir, u"Party");
                pdpath = cw.util.join_paths(pdppath, optparty);
                if (os.path.isdir(pdpath)) {
                    pfile = cw.util.join_paths(pdpath, u"Party.xml");
                    if (!os.path.isfile(pfile)) {
                        // 古いデータではParty.xmlでない場合があるのでXMLファイルを探す
                        foreach (var fname in os.listdir(pdpath)) {
                            if (os.path.splitext(fname)[1].lower() == ".xml") {
                                pfile = cw.util.join_paths(pdpath, fname);
                                break;

                    pname = cw.util.relpath(pfile, this.yadodir);

            if (pname) {
                path = cw.util.join_paths(this.yadodir, pname);
                seq = [header for header in this.partys if path == header.fpath];

                if (seq) {
                    this.load_party(seq[0]);
                } else {
                    cw.OPTIONS.scenario = "";
                    this.load_party(null);

            } else {
                cw.OPTIONS.scenario = "";
                this.load_party(null);

    public UNK update_version() {
        """古いバージョンの宿データであれば更新する。;
        """;
        nowparty = this.environment.gettext("Property/NowSelectingParty", "");
        ppath = cw.util.join_paths(this.yadodir, "Party");
        foreach (var fpath in os.listdir(ppath)) {
            fpath = cw.util.join_paths(ppath, fpath);
            if (os.path.isdir(fpath) || !fpath.lower().endswith(".xml")) {
                continue;

            // パーティデータが1つのファイルであれば
            // ディレクトリ方式に変換する

            // 変換後のディレクトリ
            dpath = cw.util.splitext(fpath)[0];
            dpath = cw.binary.util.check_duplicate(dpath);
            os.makedirs(dpath);

            if (nowparty == cw.util.splitext(os.path.basename(fpath))[0]) {
                pname = cw.util.join_paths("Party", os.path.basename(dpath), "Party.xml");
                this.environment.edit("Property/NowSelectingParty", pname);

            // データベース
            carddb = cw.yadodb.YadoDB(dpath, cw.yadodb.PARTY);
            order = 0;

            // シナリオログ
            wslpath = cw.util.splitext(fpath)[0] + ".wsl";
            haswsl = os.path.isfile(wslpath);
            if (haswsl) {
                cw.util.decompress_zip(wslpath, cw.tempdir, "ScenarioLog");

                // 荷物袋内のカード群(ファイルパスのみ)
                files = cw.data.make_element("BackpackFiles");
                party = xml2etree(cw.util.join_paths(cw.tempdir, u"ScenarioLog/Party", os.path.basename(fpath)));
                foreach (var e in party.getfind("Backpack")) {
                    // まだ所持しているカードとシナリオ内で
                    // 失われたカードを判別できないので、
                    // ログの荷物袋のカードは一旦全て削除済みと
                    // マークしておき、現行の荷物袋のカードは
                    // 新規入手状態にする
                    carddata = CWPyElementTree(element=e);
                    name = carddata.gettext("Property/Name", "");
                    carddata.edit("Property", "2", "moved");
                    carddata.fpath = cw.binary.util.check_filename(name + ".xml");
                    carddata.fpath = cw.util.join_paths(dpath, e.tag, carddata.fpath);
                    carddata.fpath = cw.binary.util.check_duplicate(carddata.fpath);
                    carddata.write(path=carddata.fpath);

                    header = cw.header.CardHeader(carddata=e);
                    header.fpath = carddata.fpath;
                    carddb.insert_cardheader(header, commit=false, cardorder=order);
                    order += 1;

                    path = cw.util.relpath(carddata.fpath, dpath);
                    path = cw.util.join_paths(path);
                    files.append(cw.data.make_element("File", path));

                // 新フォーマットの荷物袋ログ
                path = cw.util.join_paths(cw.tempdir, u"ScenarioLog/Backpack.xml");
                etree = CWPyElementTree(element=files);
                etree.write(path);

                party.getroot().remove(party.find("Backpack"));
                party.write();
                shutil.move(party.fpath, cw.util.join_paths(cw.tempdir, u"ScenarioLog/Party/Party.xml"));

                wslpath2 = cw.util.join_paths(dpath, "Party.wsl");
                cw.util.compress_zip(cw.util.join_paths(cw.tempdir, u"ScenarioLog"), wslpath2, unicodefilename=true);
                cw.util.remove(cw.util.join_paths(cw.tempdir, u"ScenarioLog"));

            // 現状のパーティデータ
            data = xml2etree(fpath);
            // Backpack要素を分解してディレクトリに保存
            foreach (var e in data.getfind("Backpack")) {
                carddata = CWPyElementTree(element=e);
                name = carddata.gettext("Property/Name", "");
                carddata.fpath = cw.binary.util.check_filename(name + ".xml");
                carddata.fpath = cw.util.join_paths(dpath, e.tag, carddata.fpath);
                carddata.fpath = cw.binary.util.check_duplicate(carddata.fpath);

                header = cw.header.CardHeader(carddata=e);

                if (haswsl && !carddata.getbool(".", "scenariocard", false)) {
                    // シナリオログ側のコメントを参照
                    carddata.edit(".", "true", "scenariocard");
                    header.scenariocard = true;

                    // 元々scenariocardでない場合は
                    // ImagePathの指す先をバイナリ化しておく
                    foreach (var e2 in carddata.iter()) {
                        if (e2.tag == "ImagePath" && e2.text && !cw.binary.image.path_is_code(e2.text)) {
                            path = cw.util.join_paths(this.yadodir, e2.text);
                            if (os.path.isfile(path)) {
                                with open(path, "rb") as f:
                                    imagedata = f.read();
                                    f.close();
                                e2.text = cw.binary.image.data_to_code(imagedata);
                    header.imgpaths = cw.image.get_imageinfos(carddata.find("Property"));

                carddata.write(path=carddata.fpath);

                header.fpath = carddata.fpath;
                carddb.insert_cardheader(header, commit=false, cardorder=order);
                order += 1;

            carddb.commit();
            carddb.close();

            // パーティの基本データを書き込み
            data.remove(".", data.find("Backpack"));
            data.write(path=cw.util.join_paths(dpath, "Party.xml"));

            // 旧データを除去
            if (haswsl) {
                os.remove(wslpath);
            os.remove(fpath);

    public UNK changed() {
        """データの変化を通知する。""";
        this._changed = true;

    public UNK is_changed() {
        return this._changed;

    public UNK is_empty() {
        return !(this.partys || this.standbys || this.storehouse or\;
                    this.album || this.partyrecord || this.savedjpdcimage or\;
                    this.get_gossips() || this.get_compstamps());

    public UNK set_skinname(skindirname, skintype) {
        this.skindirname = skindirname;
        e = this.environment.find("Property/Skin");
        if (e == null) {
            prop = this.environment.find("Property");
            prop.append(make_element("Skin", skindirname));
        } else {
            e.text = skindirname;

        e = this.environment.find("Property/Type");
        if (e == null) {
            prop = this.environment.find("Property");
            prop.append(make_element("Type", skintype));
        } else {
            e.text = skintype;

        this.environment.is_edited = true;

    public UNK load_party(header=null) {
        """;
        header: PartyHeader;
        引数のパーティー名のデータを読み込む。;
        パーティー名がnullの場合はパーティーデータは空になる;
        """;
        // パーティデータが変更されている場合はxmlをTempに吐き出す
        if (this.party) {
            this.party.write();
            if (this.party.members) {
                this.add_party(this.party);

            if (this.party.members) {
                // 次に冒険の再開ダイアログを開いた時に
                // 選択状態にする
                this.lastparty = this.party.path;
            } else {
                this.lastparty = "";

        if (header) {
            this.party = Party(header);
            if (this.party.lastscenario || this.party.lastscenariopath) {
                cw.cwpy.setting.lastscenario = this.party.lastscenario;
                cw.cwpy.setting.lastscenariopath = this.party.lastscenariopath;
            if (header.fpath.lower().startswith("yado")) {
                name = cw.util.relpath(header.fpath, this.yadodir);
            } else {
                name = cw.util.relpath(header.fpath, this.tempdir);
            name = cw.util.join_paths(name);
            this.environment.edit("Property/NowSelectingParty", name);

            if (header in this.partys) {
                this.partys.remove(header);

        } else {
            this.party = null;
            this.environment.edit("Property/NowSelectingParty", "");

    public UNK add_standbys(path, sort=true) {
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        header = this.create_advheader(path);
        header.order = cw.util.new_order(this.standbys);
        this.standbys.append(header);
        if (sort) {
            this.sort_standbys();
        return header;

    public UNK add_album(path) {
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        header = this.create_advheader(path, true);
        this.album.append(header);
        cw.util.sort_by_attr(this.album, "name");
        return header;

    public UNK add_party(party, sort=true) {
        fpath = party.path;
        header = this.create_partyheader(fpath);
        header.data = party // 保存時まで記憶しておく
        this.partys.append(header);
        header.order = cw.util.new_order(this.partys);
        if (sort) {
            this.sort_parties();
        return header;

    public UNK add_partyrecord(partyrecord) {
        """パーティ記録を追加する。""";
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        fpath = cw.xmlcreater.create_partyrecord(partyrecord);
        partyrecord.fpath = fpath;
        header = cw.header.PartyRecordHeader(partyrecord=partyrecord);
        this.partyrecord.append(header);
        this.sort_partyrecord();
        return header;

    public UNK replace_partyrecord(partyrecord) {
        """partyrecordと同名のパーティ記録を上書きする。;
        同名の情報が無かった場合は、追加する。;
        """;
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        foreach (var i, header in enumerate(this.partyrecord)) {
            if (header.name == partyrecord.name) {
                this.set_partyrecord(i, partyrecord);
                return;
        return this.add_partyrecord(partyrecord);

    public UNK set_partyrecord(index, partyrecord) {
        """this.partyrecord[index]をpartyrecordで上書きする。;
        """;
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        header = this.partyrecord[index];
        this.deletedpaths.add(header.fpath);
        fpath = cw.xmlcreater.create_partyrecord(partyrecord);
        partyrecord.fpath = fpath;
        header = cw.header.PartyRecordHeader(partyrecord=partyrecord);
        this.partyrecord[index] = header;
        return header;

    public UNK remove_partyrecord(header) {
        """パーティ記録を削除する。""";
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        this.partyrecord.remove(header);
        this.deletedpaths.add(header.fpath);

    public UNK remove_emptypartyrecord() {
        """メンバが全滅したパーティ記録を削除する。""";
        foreach (var header in this.partyrecord[:]) {
            foreach (var member in header.members) {
                if (member) {
                    break;
            } else {
                this.partyrecord.remove(header);

    public UNK can_restoreparty(partyrecordheader) {
        """partyrecordheaderが再結成可能であればtrueを返す。;
        """;
        foreach (var member in partyrecordheader.members) {
            if (this.can_restore(member)) {
                return true;
        return false;

    public UNK can_restore(member) {
        """memberがパーティの再結成に応じられるかを返す。;
        アクティブでないパーティに所属しているなど、;
        応じられない場合はfalseを返す。;
        """;
        foreach (var standby in this.standbys) {
            if (os.path.splitext(os.path.basename(standby.fpath))[0] == member) {
                return true;
        if (this.party) {
            // 現在のパーティは再結成の前に解散するため
            // standbysの中にいるのと同様に扱う
            foreach (var m in this.party.members) {
                if (os.path.splitext(os.path.basename(m.fpath))[0] == member) {
                    return true;
        return false;

    public UNK get_restoremembers(partyrecordheader) {
        """partyrecordheaderの再結成で;
        待機メンバでなくなるメンバの一覧を返す。;
        """;
        seq = [];
        foreach (var member in partyrecordheader.members) {
            foreach (var standby in this.standbys) {
                if (os.path.splitext(os.path.basename(standby.fpath))[0] == member) {
                    seq.append(standby);
                    break;
        return seq;

    public UNK restore_party(partyrecordheader) {
        """partyrecordheaderからパーティを再結成する。;
        現在操作中のパーティがいた場合は解散される。;
        結成されたパーティに属するメンバのheaderのlistを返す。;
        所属メンバが宿帳に一人も見つからなかった場合は[]を返す。;
        """;
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        if (cw.cwpy.ydata.party) {
            cw.cwpy.dissolve_party(cleararea=false);
        assert !cw.cwpy.ydata.party;
        chgarea = !cw.cwpy.areaid in (2, cw.AREA_BREAKUP);

        members = [];
        foreach (var member in partyrecordheader.members) {
            foreach (var standby in this.standbys) {
                if (os.path.splitext(os.path.basename(standby.fpath))[0] == member) {
                    members.append(standby);
                    break;
        if (!members) {
            // メンバがいない場合は失敗
            return members;

        foreach (var standby in members) {
            this.standbys.remove(standby);

        name = partyrecordheader.name;
        money = partyrecordheader.money;
        prop = cw.header.GetProperty(partyrecordheader.fpath);
        is_suspendlevelup = cw.util.str2bool(prop.properties["SuspendLevelUp"]);
        if (this.money < money) {
            money = this.money;
        this.set_money(-money);
        path = cw.xmlcreater.create_party(members, moneyamount=money, pname=name,;
                                          is_suspendlevelup=is_suspendlevelup);
        header = this.create_partyheader(cw.util.join_paths(path, "Party.xml"));

        cw.cwpy.load_party(header, chgarea=chgarea, newparty=true);

        // 荷物袋の内容を復元。カード置場にない場合は復元不可。
        // 最初は作者名・シナリオ名・使用回数を使用して検索するが、
        // それで見つからない場合はカード名と解説のみで検索する。
        e = yadoxml2etree(partyrecordheader.fpath, tag="BackpackRecord");
        foreach (var ce in e.getfind(".")) {
            if (ce.tag != "CardRecord") {
                continue;
            get = false;
            name = ce.getattr(".", "name", "");
            desc = ce.getattr(".", "desc", "");
            author = ce.getattr(".", "author", "");
            scenario = ce.getattr(".", "scenario", "");
            uselimit = ce.getint(".", "uselimit", 0);
            foreach (var cheader in this.storehouse) {
                if cheader.name == name and\;
                   cheader.desc == desc and\;
                   cheader.author == author and\;
                   cheader.scenario == scenario and\;
                   cheader.uselimit == uselimit:
                    get = true;
                    cw.cwpy.trade(targettype="BACKPACK", header=cheader, sound=false, sort=false);
                    break;
            if (get) {
                continue;
            foreach (var cheader in this.storehouse) {
                if cheader.name == name and\;
                   cheader.desc == desc:
                    cw.cwpy.trade(targettype="BACKPACK", header=cheader, sound=false, sort=false);
                    break;
        this.party.backpack.reverse();
        foreach (var order, header in enumerate(this.party.backpack)) {
            header.order = order;
        this.party.sort_backpack();

        cw.cwpy.statusbar.change(false);
        cw.cwpy.draw();
        cw.cwpy.ydata.party._loading = false;
        return members;

    public UNK create_advheader(path="", album=false, element=null) {
        """;
        path: xmlのパス。;
        album: trueならアルバム用のAdventurerHeaderを作成。;
        element: PropertyタグのElement。;
        """;
        rootattrs = {};
        if (!element) {
            element = yadoxml2element(path, "Property", rootattrs=rootattrs);

        return cw.header.AdventurerHeader(element, album, rootattrs=rootattrs);

    public UNK create_cardheader(path="", element=null, owner=null) {
        """;
        path: xmlのパス。;
        element: PropertyタグのElement。;
        """;
        if (element == null) {
            element = yadoxml2element(path, "Property");

        return cw.header.CardHeader(element, owner=owner);

    public UNK create_partyheader(path="", element=null) {
        """;
        path: xmlのパス。;
        element: PropertyタグのElement。;
        """;
        if (element == null) {
            element = yadoxml2element(path, "Property");

        return cw.header.PartyHeader(element);

    public UNK create_party(header, chgarea=true) {
        """新しくパーティを作る。;
        header: AdventurerHeader;
        """;
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        initmoneyamount = cw.cwpy.setting.initmoneyamount;
        if (cw.cwpy.setting.initmoneyisinitialcash) {
            initmoneyamount = cw.cwpy.setting.initialcash;

        if (this.money < initmoneyamount) {
            money = this.money;
        } else {
            money = initmoneyamount;
        this.set_money(-money);
        path = cw.xmlcreater.create_party([header], moneyamount=money);
        header = this.create_partyheader(cw.util.join_paths(path, "Party.xml"));
        cw.cwpy.load_party(header, chgarea=chgarea);
        cw.cwpy.statusbar.change(false);
        cw.cwpy.draw();

    public UNK sort_standbys() {
        if (cw.cwpy.setting.sort_standbys == "Level") {
            cw.util.sort_by_attr(this.standbys, "level", "name", "order");
        } else if (cw.cwpy.setting.sort_standbys == "Name") {
            cw.util.sort_by_attr(this.standbys, "name", "level", "order");
        } else {
            cw.util.sort_by_attr(this.standbys, "order");

    public UNK sort_parties() {
        if (cw.cwpy.setting.sort_parties == "HighestLevel") {
            cw.util.sort_by_attr(this.partys, "highest_level", "average_level", "name", "order");
        } else if (cw.cwpy.setting.sort_parties == "AverageLevel") {
            cw.util.sort_by_attr(this.partys, "average_level", "highest_level", "name", "order");
        } else if (cw.cwpy.setting.sort_parties == "Name") {
            cw.util.sort_by_attr(this.partys, "name", "order");
        } else if (cw.cwpy.setting.sort_parties == "Money") {
            cw.util.sort_by_attr(this.partys, "money", "name", "order");
        } else {
            cw.util.sort_by_attr(this.partys, "order");

    public UNK sort_storehouse() {
        sort_cards(this.storehouse, cw.cwpy.setting.sort_cards, cw.cwpy.setting.sort_cardswithstar);

    public UNK sort_partyrecord() {
        cw.util.sort_by_attr(this.partyrecord, "name");

    public UNK save() {
        """宿データをセーブする。""";
        // カード置場の順序を記憶しておく
        cardorder = {};
        cardtable = {};
        foreach (var header in this.storehouse) {
            if (header.fpath.lower().startswith("yado")) {
                fpath = cw.util.relpath(header.fpath, this.yadodir);
            } else {
                fpath = cw.util.relpath(header.fpath, this.tempdir);
                header.fpath = header.fpath.replace(this.tempdir, this.yadodir, 1);
            fpath = cw.util.join_paths(fpath);
            cardorder[fpath] = header.order;
            cardtable[fpath] = header;
        // 宿帳の順序を記憶しておく
        adventurerorder = {};
        adventurertable = {};
        foreach (var header in this.standbys) {
            if (header.fpath.lower().startswith("yado")) {
                fpath = cw.util.relpath(header.fpath, this.yadodir);
            } else {
                fpath = cw.util.relpath(header.fpath, this.tempdir);
                header.fpath = header.fpath.replace(this.tempdir, this.yadodir, 1);
            fpath = cw.util.join_paths(fpath);
            adventurerorder[fpath] = header.order;
            adventurertable[fpath] = header;

        // アルバム(順序情報なし)
        foreach (var header in this.album) {
            if (!header.fpath.lower().startswith("yado")) {
                fpath = cw.util.relpath(header.fpath, this.tempdir);
                header.fpath = header.fpath.replace(this.tempdir, this.yadodir, 1);

        // ScenarioLog更新
        if (cw.cwpy.is_playingscenario()) {
            logfilepath = cw.cwpy.advlog.logfilepath;
            cw.cwpy.sdata.update_log();
            cw.cwpy.advlog.resume_scenario(logfilepath);

        // environment.xml書き出し
        this.environment.write_xml();

        // party.xmlと冒険者のxmlファイル書き出し
        if (this.party) {
            this.party.write();

        this.deletedpaths.write_list();

        this._transfer_temp();

        // 各パーティの荷物袋のデータを保存する
        def update_backpack(party):
            // カード置場の順序を記憶しておく
            cardorder = {};
            ppath = os.path.dirname(party.path);
            yadodir = party.get_yadodir();
            tempdir = party.get_tempdir();
            cardtable = {};
            foreach (var header in party.backpack) {
                header.do_write();
                if (header.fpath.lower().startswith("yado")) {
                    fpath = cw.util.relpath(header.fpath, yadodir);
                } else {
                    fpath = cw.util.relpath(header.fpath, tempdir);
                    header.fpath = header.fpath.replace(this.tempdir, this.yadodir, 1);
                fpath = cw.util.join_paths(fpath);
                cardorder[fpath] = header.order;
                cardtable[fpath] = header;
            carddb = cw.yadodb.YadoDB(ppath, mode=cw.yadodb.PARTY);
            carddb.update(cards=cardtable, cardorder=cardorder);
            carddb.close();
        if (this.party) {
            update_backpack(this.party);
        partyorder = {};
        foreach (var party in this.partys) {
            if (party.data) {
                update_backpack(party.data);
                if (party.fpath.lower().startswith(this.tempdir.lower())) {
                    party.fpath = party.fpath.replace(this.tempdir, this.yadodir, 1);
                party.data = null;
            partyorder[party.fpath] = party.order;

        partyrecord = {};
        foreach (var header in this.partyrecord) {
            if (header.fpath.lower().startswith("yado")) {
                fpath = cw.util.relpath(header.fpath, this.yadodir);
            } else {
                fpath = cw.util.relpath(header.fpath, this.tempdir);
                header.fpath = header.fpath.replace(this.tempdir, this.yadodir, 1);
            partyrecord[fpath] = header;

        savedjpdcimage = {};
        foreach (var header in this.savedjpdcimage.itervalues()) {
            if (header.fpath.lower().startswith("yado")) {
                fpath = cw.util.relpath(header.fpath, this.yadodir);
            } else {
                fpath = cw.util.relpath(header.fpath, this.tempdir);
                header.fpath = header.fpath.replace(this.tempdir, this.yadodir, 1);
            savedjpdcimage[fpath] = header;

        // カードデータベースを更新
        @synclock(_lock);
        def update_database(yadodir):
            yadodb = cw.yadodb.YadoDB(yadodir);
            yadodb.update(cards=cardtable,;
                          adventurers=adventurertable,;
                          cardorder=cardorder,;
                          adventurerorder=adventurerorder,;
                          partyorder=partyorder,;
                          partyrecord=partyrecord,;
                          savedjpdcimage=savedjpdcimage);
            yadodb.close();
        thr = threading.Thread(target=update_database, kwargs={"yadodir": this.yadodir});
        thr.start();

        cw.cwpy.clear_selection();
        cw.cwpy.draw();
        this._changed = false;

    public UNK _retry_save() {
        """TempからYadoへの転送中に失敗した保存処理を再実行する。;
        """;
        if (this.deletedpaths.read_list()) {
            this._transfer_temp();

    public UNK _transfer_temp() {
        // TEMPのファイルを移動
        deltempfpath = cw.util.join_paths(this.deletedpaths.tempdir, u"DeletedPaths.temp");
        foreach (var dpath, _dnames, fnames in os.walk(this.tempdir)) {
            foreach (var fname in fnames) {
                path = cw.util.join_paths(dpath, fname);
                if (path == deltempfpath) {
                    continue;
                if (os.path.isfile(path)) {
                    dstpath = path.replace(this.tempdir, this.yadodir, 1);
                    cw.util.rename_file(path, dstpath);

        // 削除予定のファイル削除
        // Materialディレクトリにある空のフォルダも削除
        materialdir = cw.util.join_paths(this.yadodir, "Material");

        // 安全のためこれらのパスは削除の際に無視する
        ignores = set();
        for ipath in (cw.cwpy.yadodir, cw.cwpy.tempdir,;
                      os.path.join(cw.cwpy.yadodir, "Adventurer"),;
                      os.path.join(cw.cwpy.yadodir, "Party"),;
                      os.path.join(cw.cwpy.yadodir, "Album"),;
                      os.path.join(cw.cwpy.yadodir, "CastCard"),;
                      os.path.join(cw.cwpy.yadodir, "SkillCard"),;
                      os.path.join(cw.cwpy.yadodir, "ItemCard"),;
                      os.path.join(cw.cwpy.yadodir, "BeastCard"),;
                      os.path.join(cw.cwpy.yadodir, "InfoCard"),;
                      os.path.join(cw.cwpy.yadodir, "Material")):
            ignores.add(os.path.normpath(os.path.normcase(ipath)));

        // 削除実行
        delfailurepaths = set();
        foreach (var path in this.deletedpaths) {
            if (os.path.normpath(os.path.normcase(path)) in ignores) {
                continue;
            cw.util.remove(path);
            dpath = os.path.dirname(path);
            if dpath.startswith(materialdir) && os.path.isdir(dpath)\;
                                                    && !os.listdir(dpath):
                cw.util.remove(dpath);

        this.deletedpaths.clear();
        // 宿のtempフォルダを空にする
        cw.util.remove(deltempfpath);
        cw.util.remove(this.tempdir);

        // BUG: 環境によってファイルやフォルダの削除が失敗する事がある
        //      (WindowsError: [Error 5] アクセスが拒否されました)。
        //      そうしたファイルは削除リストに残しておき、後で削除する。
        foreach (var path in delfailurepaths) {
            this.deletedpaths.add(path);

    //---------------------------------------------------------------------------
    // ゴシップ・シナリオ終了印用メソッド
    //---------------------------------------------------------------------------

    public UNK get_gossips() {
        """ゴシップ名をset型で返す。""";
        return set([e.text for e in this.environment.getfind("Gossips") if e.text]);

    public UNK get_compstamps() {
        """冒険済みシナリオ名をset型で返す。""";
        return set([e.text for e in this.environment.getfind("CompleteStamps") if e.text]);

    public UNK get_gossiplist() {
        """ゴシップ名をlist型で返す。""";
        return [e.text for e in this.environment.getfind("Gossips") if e.text];

    public UNK get_compstamplist() {
        """冒険済みシナリオ名をlist型で返す。""";
        return [e.text for e in this.environment.getfind("CompleteStamps") if e.text];

    public UNK has_compstamp(name) {
        """冒険済みシナリオかどうかbool値で返す。;
        name: シナリオ名。;
        """;
        foreach (var e in this.environment.getfind("CompleteStamps")) {
            if (e.text && e.text == name) {
                return true;

        return false;

    public UNK has_gossip(name) {
        """ゴシップを所持しているかどうかbool値で返す。;
        name: ゴシップ名;
        """;
        foreach (var e in this.environment.getfind("Gossips")) {
            if (e.text && e.text == name) {
                return true;

        return false;

    public UNK set_compstamp(name) {
        """冒険済みシナリオ印をセットする。シナリオプレイ中に取得した;
        シナリオ印はScenarioDataのリストに登録する。;
        name: シナリオ名;
        """;
        if (!this.has_compstamp(name)) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            e = make_element("CompleteStamp", name);
            this.environment.append("CompleteStamps", e);

            if (cw.cwpy.is_playingscenario()) {
                if (cw.cwpy.sdata.compstamps.get(name) is false) {
                    cw.cwpy.sdata.compstamps.pop(name);
                } else {
                    cw.cwpy.sdata.compstamps[name] = true;

    public UNK set_gossip(name) {
        """ゴシップをセットする。シナリオプレイ中に取得した;
        ゴシップはScenarioDataのリストに登録する。;
        name: ゴシップ名;
        """;
        if (!this.has_gossip(name)) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            e = make_element("Gossip", name);
            this.environment.append("Gossips", e);

            if (cw.cwpy.is_playingscenario()) {
                if (cw.cwpy.sdata.gossips.get(name) is false) {
                    cw.cwpy.sdata.gossips.pop(name);
                } else {
                    cw.cwpy.sdata.gossips[name] = true;

    public UNK remove_compstamp(name) {
        """冒険済みシナリオ印を削除する。シナリオプレイ中に削除した;
        シナリオ印はScenarioDataのリストから解除する。;
        name: シナリオ名;
        """;
        elements = [e for e in this.environment.getfind("CompleteStamps");
                                                            if e.text == name];

        foreach (var e in elements) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.environment.remove("CompleteStamps", e);

        if (elements && cw.cwpy.is_playingscenario()) {
            if (cw.cwpy.sdata.compstamps.get(name) is true) {
                cw.cwpy.sdata.compstamps.pop(name);
            } else {
                cw.cwpy.sdata.compstamps[name] = false;

    public UNK remove_gossip(name) {
        """ゴシップを削除する。シナリオプレイ中に削除した;
        ゴシップはScenarioDataのリストから解除する。;
        name: ゴシップ名;
        """;
        elements = [e for e in this.environment.getfind("Gossips");
                                                            if e.text == name];

        foreach (var e in elements) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.environment.remove("Gossips", e);

        if (elements && cw.cwpy.is_playingscenario()) {
            if (cw.cwpy.sdata.gossips.get(name) is true) {
                cw.cwpy.sdata.gossips.pop(name);
            } else {
                cw.cwpy.sdata.gossips[name] = false;

    public UNK clear_compstamps() {
        """冒険済みシナリオ印を全て削除する。""";

        foreach (var e in list(this.environment.getfind("CompleteStamps"))) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.environment.remove("CompleteStamps", e);

            if (cw.cwpy.is_playingscenario()) {
                name = e.text;
                if (cw.cwpy.sdata.compstamps.get(name) is true) {
                    cw.cwpy.sdata.compstamps.pop(name);
                } else {
                    cw.cwpy.sdata.compstamps[name] = false;
        assert len(this.environment.getfind("CompleteStamps")) == 0;

    public UNK clear_gossips() {
        """ゴシップを全て削除する。""";

        foreach (var e in list(this.environment.getfind("Gossips"))) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.environment.remove("Gossips", e);

            if (cw.cwpy.is_playingscenario()) {
                name = e.text;
                if (cw.cwpy.sdata.gossips.get(name) is true) {
                    cw.cwpy.sdata.gossips.pop(name);
                } else {
                    cw.cwpy.sdata.gossips[name] = false;
        assert len(this.environment.getfind("Gossips")) == 0;

    public UNK set_money(value, blink=false) {
        """金庫に入っている金額を変更する。;
        現在の所持金にvalue値をプラスするので注意。;
        """;
        if (value != 0) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.money += value;
            this.money = cw.util.numwrap(this.money, 0, 9999999);
            this.environment.edit("Property/Cashbox", str(this.money));
            showbuttons = !cw.cwpy.is_playingscenario() || !cw.cwpy.is_runningevent();
            cw.cwpy.statusbar.change(showbuttons);
            cw.cwpy.has_inputevent = true;
            if (blink) {
                if (cw.cwpy.statusbar.yadomoney) {
                    cw.animation.start_animation(cw.cwpy.statusbar.yadomoney, "blink");

    //---------------------------------------------------------------------------
    // パーティ連れ込み
    //---------------------------------------------------------------------------

    public UNK join_npcs() {
        """;
        シナリオのNPCを宿に連れ込む。;
        """;
        r_gene = re.compile(u"＠Ｇ\d{10}$");
        foreach (var fcard in cw.cwpy.get_fcards()) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            fcard.set_fullrecovery();

            // 必須クーポンを所持していなかったら補填
            if (!fcard.has_age() || !fcard.has_sex()) {
                cw.cwpy.play_sound("signal");
                cw.cwpy.call_modaldlg("DATACOMP", ccard=fcard);

            // システムクーポン
            fcard.set_coupon(u"＿" + fcard.name, fcard.level * (fcard.level-1));
            fcard.set_coupon(u"＠レベル原点", fcard.level);
            fcard.set_coupon(u"＠ＥＰ", 0);
            talent = fcard.get_talent();

            value = 10;
            foreach (var nature in cw.cwpy.setting.natures) {
                if (u"＿" + nature.name == talent) {
                    value = nature.levelmax;
                    break;

            fcard.set_coupon(u"＠本来の上限", value);
            foreach (var coupon in fcard.get_coupons()) {
                if (r_gene.match(coupon)) {
                    break;
            } else {
                gene = cw.header.Gene();
                gene.set_talentbit(talent);
                fcard.set_coupon(u"＠Ｇ" + gene.get_str(), 0);

            data = fcard.data;

            // 所持カードの素材ファイルコピー
            foreach (var cardtype in ("SkillCard", "ItemCard", "BeastCard")) {
                foreach (var e in data.getfind("%ss" % (cardtype))) {
                    // 対象カード名取得
                    name = e.gettext("Property/Name", "noname");
                    name = cw.util.repl_dischar(name);
                    // 素材ファイルコピー
                    dstdir = cw.util.join_paths(this.yadodir,;
                                                    "Material", cardtype, name if name else"noname");
                    dstdir = cw.util.dupcheck_plus(dstdir);
                    can_loaded_scaledimage = e.getbool(".", "scaledimage", false);
                    cw.cwpy.copy_materials(e, dstdir, can_loaded_scaledimage=can_loaded_scaledimage);

            // カード画像コピー
            name = cw.util.repl_dischar(fcard.name) if fcard.name else "noname";
            e = data.getfind("Property");
            dstdir = cw.util.join_paths(this.yadodir,;
                                                "Material", "Adventurer", name);
            dstdir = cw.util.dupcheck_plus(dstdir);
            can_loaded_scaledimage = data.getbool(".", "scaledimage", false);
            cw.cwpy.copy_materials(e, dstdir, can_loaded_scaledimage=can_loaded_scaledimage);
            // xmlファイル書き込み
            data.getroot().tag = "Adventurer";
            path = cw.util.join_paths(this.tempdir, "Adventurer", name + ".xml");
            path = cw.util.dupcheck_plus(path);
            data.write(path);
            // 待機中冒険者のリストに追加
            this.add_standbys(path, sort=false);
        this.sort_standbys();

    //---------------------------------------------------------------------------
    // ここからpathリスト取得用メソッド
    //---------------------------------------------------------------------------

    public UNK get_nowplayingpaths() {
        """wslファイルを読み込んで、;
        現在プレイ中のシナリオパスの集合を返す。;
        """;
        seq = [];

        foreach (var dpath in (this.yadodir, this.tempdir)) {
            dpath = cw.util.join_paths(dpath, u"Party");
            if (!os.path.isdir(dpath)) {
                continue;

            foreach (var dname in os.listdir(dpath)) {
                dpath2 = cw.util.join_paths(dpath, dname);
                if (!os.path.isdir(dpath2)) {
                    continue;

                foreach (var name in os.listdir(dpath2)) {
                    path = cw.util.join_paths(dpath2, name);
                    if name.endswith(".wsl") && os.path.isfile(path)\;
                                        && !path in this.deletedpaths:
                        e = cw.util.get_elementfromzip(path, "ScenarioLog.xml",;
                                                                    "Property");
                        path = e.gettext("WsnPath");
                        path = cw.util.get_linktarget(path);
                        path = os.path.normcase(os.path.normpath(os.path.abspath(path)));
                        seq.append(path);

        return set(seq);

    public UNK get_partypaths() {
        """パーティーのxmlファイルのpathリストを返す。""";
        seq = [];
        dpath = cw.util.join_paths(this.yadodir, "Party");

        foreach (var fname in os.listdir(dpath)) {
            fpath = cw.util.join_paths(dpath, fname);

            if (os.path.isfile(fpath) && fname.endswith(".xml")) {
                seq.append(fpath);

        return seq;

    public UNK get_storehousepaths() {
        """BeastCard, ItemCard, SkillCardのディレクトリにあるカードの;
        xmlのpathリストを返す。;
        """;
        seq = [];

        foreach (var dname in ("BeastCard", "ItemCard", "SkillCard")) {
            foreach (var fname in os.listdir(cw.util.join_paths(this.yadodir, dname))) {
                fpath = cw.util.join_paths(this.yadodir, dname, fname);

                if (os.path.isfile(fpath) && fname.endswith(".xml")) {
                    seq.append(fpath);

        return seq;

    public UNK get_standbypaths() {
        """パーティーに所属していない待機中冒険者のxmlのpathリストを返す。""";
        seq = [];

        foreach (var header in this.partys) {
            paths = header.get_memberpaths();
            seq.extend(paths);

        members = set(seq);
        seq = [];

        foreach (var fname in os.listdir(cw.util.join_paths(this.yadodir, "Adventurer"))) {
            fpath = cw.util.join_paths(this.yadodir, "Adventurer", fname);

            if (os.path.isfile(fpath) && fname.endswith(".xml")) {
                if (!fpath in members) {
                    seq.append(fpath);

        return seq;

    public UNK get_albumpaths() {
        """アルバムにある冒険者のxmlのpathリストを返す。""";
        seq = [];

        foreach (var fname in os.listdir(cw.util.join_paths(this.yadodir, "Album"))) {
            fpath = cw.util.join_paths(this.yadodir, "Album", fname);

            if (os.path.isfile(fpath) && fname.endswith(".xml")) {
                seq.append(fpath);

        return seq;

    //---------------------------------------------------------------------------
    // ブックマーク
    //---------------------------------------------------------------------------

    public UNK add_bookmark(spaths, path) {
        """シナリオのブックマークを追加する。""";
        this.changed();
        this.bookmarks.append((spaths, path));
        be = this.environment.find("Bookmarks");
        if (be == null) {
            be = make_element("Bookmarks");
            this.environment.append(".", be);
        e = make_element("Bookmark");
        foreach (var p in spaths) {
            e2 = make_element("Path", p);
            e.append(e2);
        e.set("path", path);
        this.environment.is_edited = true;
        be.append(e);

    public UNK set_bookmarks(bookmarks) {
        """シナリオのブックマーク群を入れ替える。""";
        this.changed();
        this.bookmarks = bookmarks;

        be = this.environment.find("Bookmarks");
        if (be == null) {
            be = make_element("Bookmarks");
            this.environment.append(".", be);
        } else {
            be.clear();

        foreach (var spaths, path in bookmarks) {
            e = make_element("Bookmark");
            foreach (var p in spaths) {
                e2 = make_element("Path", p);
                e.append(e2);
            e.set("path", path);
            be.append(e);
        this.environment.is_edited = true;

def find_scefullpath(scepath, spaths):
    """開始ディレクトリscepathから経路spathsを;
    辿った結果得られたフルパスを返す。;
    辿れなかった場合は""を返す。;
    """;
    bookmarkpath = u"";
    foreach (var p in spaths) {
        scepath = cw.util.get_linktarget(scepath);
        scepath = cw.util.join_paths(scepath, p);
        if (!os.path.exists(scepath)) {
            break;
    } else {
        bookmarkpath = os.path.abspath(scepath);
        bookmarkpath = os.path.normpath(scepath);
    return bookmarkpath;

class Party(object):
    public UNK __init__(header, partyinfoonly=true) {
        path = header.fpath;

        // true時は、エリア移動中にPlayerCardスプライトを新規作成する
        this._loading = true;

        this.members = [];
        if (!header.data) {
            this.backpack = [];
            this.backpack_moved = [];
        this.path = path;

        // キャンセル可能な対象消去メンバ(互換機能)
        this.vanished_pcards = [];

        // パーティデータ(CWPyElementTree)
        this.data = yadoxml2etree(path);
        // パーティ名
        this.name = this.data.gettext("Property/Name", "");
        // パーティ所持金
        this.money = this.data.getint("Property/Money", 0);

        // 現在プレイ中のシナリオ
        this.lastscenario = [];
        this.lastscenariopath = this.data.getattr("Property/LastScenario", "path", "");
        foreach (var e in this.data.getfind("Property/LastScenario", raiseerror=false)) {
            this.lastscenario.append(e.text);

        // レベルアップ停止中か
        this.is_suspendlevelup = this.data.getbool("Property/SuspendLevelUp", false);

        this.partyinfoonly = partyinfoonly;
        if (partyinfoonly) {
            // 選択中パーティのメンバー(CWPyElementTree)
            paths = this.get_memberpaths();
            this.members = [yadoxml2etree(path) for path in paths];
            // 選択中のパーティの荷物袋(CardHeader)
            if (header.data) {
                // header.dataがある場合は保存前
                this.backpack = header.data.backpack;
                this.backpack_moved = header.data.backpack_moved;
            } else {
                dpath = os.path.dirname(this.path);
                carddb = cw.yadodb.YadoDB(dpath, mode=cw.yadodb.PARTY);
                carddb.update();
                foreach (var header in carddb.get_cards()) {
                    if (header.moved == 0) {
                        this.backpack.append(header);
                    } else {
                        this.backpack_moved.append(header);
                carddb.close();
            this.sort_backpack();

    public UNK sort_backpack() {
        sort_cards(this.backpack, cw.cwpy.setting.sort_cards, cw.cwpy.setting.sort_cardswithstar);

    public UNK get_backpackkeycodes(skill=true, item=true, beast=true) {
        """荷物袋内のキーコード一覧を返す。""";
        s = set();
        foreach (var header in this.backpack) {
            if (!skill && header.type == "SkillCard") {
                continue;
            } else if (!item && header.type == "ItemCard") {
                continue;
            } else if (!beast && header.type == "BeastCard") {
                continue;
            s.update(header.get_keycodes());

        s.discard("");
        return s;

    public UNK has_keycode(keycode, skill=true, item=true, beast=true, hand=true) {
        """指定されたキーコードを所持しているか。""";
        foreach (var header in this.backpack) {
            if (!skill && header.type == "SkillCard") {
                continue;
            } else if (!item && header.type == "ItemCard") {
                continue;
            } else if (!beast && header.type == "BeastCard") {
                continue;

            if (keycode in header.get_keycodes()) {
                return true;

        return false;

    public UNK get_relpath() {
        ppath = os.path.dirname(this.path);
        if (ppath.lower().startswith("yado")) {
            relpath = cw.util.relpath(ppath, cw.cwpy.yadodir);
        } else {
            relpath = cw.util.relpath(ppath, cw.cwpy.tempdir);
        return cw.util.join_paths(relpath);

    public UNK get_yadodir() {
        return cw.util.join_paths(cw.cwpy.yadodir, this.get_relpath());

    public UNK get_tempdir() {
        return cw.util.join_paths(cw.cwpy.tempdir, this.get_relpath());

    public UNK is_loading() {
        """membersのデータを元にPlayerCardインスタンスを;
        生成していなかったら、trueを返す。;
        """;
        return this._loading;

    public UNK reload() {
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        header = cw.header.PartyHeader(data=this.data.find("Property"));
        header.data = self;
        this.__init__(header);

    public UNK add(header, data=null) {
        """;
        メンバーを追加する。引数はAdventurerHeader。;
        """;
        pcardsnum = len(this.members);

        // パーティ人数が6人だったら処理中断
        if (pcardsnum >= 6) {
            return;

        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        s = os.path.basename(header.fpath);
        s = cw.util.splitext(s)[0];
        e = this.data.make_element("Member", s);
        if (!data) {
            data = yadoxml2etree(header.fpath);
        pcards = cw.cwpy.get_pcards();
        if (pcards) {
            // 欠けているindexがあったら隙間に挿入する
            foreach (var i, pcard in enumerate(pcards)) {
                if (i != pcard.index) {
                    index = i;
                    break;
            } else {
                index = pcards[-1].index + 1;
        } else {
            index = 0;
        this.members.insert(index, data);
        this.data.insert("Property/Members", e, index);
        pos_noscale = (9 + 95 * index + 9 * index, 285);
        pcard = cw.sprite.card.PlayerCard(data, pos_noscale=pos_noscale, status="deal", index=index);
        cw.animation.animate_sprite(pcard, "deal");

    public UNK remove(pcard) {
        """;
        メンバーを削除する。引数はPlayerCard。;
        """;
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        pcard.remove_numbercoupon();
        this.members.remove(pcard.data);
        if (cw.cwpy.cardgrp.has(pcard)) {
            cw.cwpy.cardgrp.remove(pcard);
            cw.cwpy.pcards.remove(pcard);
        this.data.getfind("Property/Members").clear();

        foreach (var pcard in cw.cwpy.get_pcards()) {
            s = os.path.basename(pcard.data.fpath);
            s = cw.util.splitext(s)[0];
            e = this.data.make_element("Member", s);
            this.data.append("Property/Members", e);

    public UNK replace_order(index1, index2) {
        """;
        メンバーの位置を入れ替える。;
        """;
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        seq = cw.cwpy.get_pcards();
        assert len(seq) == len(this.members);
        seq[index1], seq[index2] = seq[index2], seq[index1];
        this.members[index1], this.members[index2] = this.members[index2], this.members[index1];
        foreach (var index, pcard in enumerate(seq)) {
            pcard.index = index;
            pcard.layer = (pcard.layer[0], pcard.layer[1], index, pcard.layer[3]);
            cw.cwpy.cardgrp.change_layer(pcard, pcard.layer);
        cw.cwpy.pcards = seq;

        this.data.getfind("Property/Members").clear();
        foreach (var pcard in cw.cwpy.get_pcards()) {
            s = os.path.basename(pcard.data.fpath);
            s = cw.util.splitext(s)[0];
            e = this.data.make_element("Member", s);
            this.data.append("Property/Members", e);

        pcard1 = seq[index1];
        pcard2 = seq[index2];
        cw.animation.animate_sprites([pcard1, pcard2], "hide");
        pos_noscale = seq[index1].get_pos_noscale();
        seq[index1].set_pos_noscale(seq[index2].get_pos_noscale());
        seq[index2].set_pos_noscale(pos_noscale);
        cw.animation.animate_sprites([pcard1, pcard2], "deal");

    public UNK set_name(name) {
        """;
        パーティ名を変更する。;
        """;
        if (this.name != name) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            oldname = this.name;
            this.name = name;
            this.data.edit("Property/Name", name);
            cw.cwpy.advlog.rename_party(this.name, oldname);
            cw.cwpy.background.reload(false, nocheckvisible=true);

    public UNK set_money(value, fromevent=false, blink=false) {
        """;
        パーティの所持金を変更する。;
        """;
        if (value != 0) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.money += value;
            this.money = cw.util.numwrap(this.money, 0, 9999999);
            this.data.edit("Property/Money", str(this.money));
            if (blink) {
                if (cw.cwpy.statusbar.partymoney) {
                    cw.animation.start_animation(cw.cwpy.statusbar.partymoney, "blink");
            if (!fromevent) {
                showbuttons = !cw.cwpy.is_playingscenario() || !cw.cwpy.is_runningevent();
                cw.cwpy.statusbar.change(showbuttons);
                cw.cwpy.has_inputevent = true;

    public UNK suspend_levelup(suspend) {
        """;
        レベルアップの可否を設定する。;
        """;
        if (suspend != this.is_suspendlevelup) {
            if (cw.cwpy.ydata) {
                cw.cwpy.ydata.changed();
            this.is_suspendlevelup = suspend;
            e = this.data.find("Property/SuspendLevelUp");
            if (e == null) {
                pe = this.data.find("Property");
                pe.append(make_element("SuspendLevelUp", str(suspend)));
                this.data.is_edited = true;
            } else {
                this.data.edit("Property/SuspendLevelUp", str(suspend));

    public UNK set_numbercoupon() {
        """;
        番号クーポンを配布する。;
        """;
        names = [cw.cwpy.msgs["number_1_coupon"], u"＿２", u"＿３", u"＿４", u"＿５", u"＿６"];

        foreach (var index, pcard in enumerate(cw.cwpy.get_pcards())) {
            pcard.remove_numbercoupon();
            pcard.set_coupon(names[index], 0);
            pcard.set_coupon(u"＠ＭＰ３", 0) // 1.29

    public UNK remove_numbercoupon() {
        """;
        番号クーポンを除去する。;
        """;
        foreach (var pcard in cw.cwpy.get_pcards()) {
            pcard.remove_numbercoupon();

    public UNK write() {
        this.data.write_xml();

        foreach (var member in this.members) {
            member.write_xml();

    public UNK lost() {
        if (cw.cwpy.ydata) {
            cw.cwpy.ydata.changed();
        foreach (var card in this.backpack[:]) {
            cw.cwpy.trade("TRASHBOX", header=card, from_event=true, sort=false);
        foreach (var pcard in cw.cwpy.get_pcards()) {
            pcard.lost();
        this.members = [];

        cw.cwpy.remove_xml(this);
        cw.cwpy.ydata.deletedpaths.add(os.path.dirname(this.path));

    public UNK get_coupontable() {
        """;
        パーティ全体が所持しているクーポンの;
        所持数テーブルを返す。;
        """;
        d = {};

        foreach (var member in this.members) {
            foreach (var e in member.getfind("Property/Coupons")) {
                if (e.text in d) {
                    d[e.text] += 1;
                } else {
                    d[e.text] = 1;

        return d;

    public UNK get_coupons() {
        """;
        パーティ全体が所持しているクーポンをセット型で返す。;
        """;
        seq = [];

        foreach (var member in this.members) {
            foreach (var e in member.getfind("Property/Coupons")) {
                seq.append(e.text);

        return set(seq);

    public UNK get_allcardheaders() {
        seq = [];
        seq.extend(this.backpack);

        foreach (var pcard in cw.cwpy.get_pcards()) {
            foreach (var headers in pcard.cardpocket) {
                seq.extend(headers);

        return seq;

    public UNK is_adventuring() {
        path = cw.util.splitext(this.data.fpath)[0] + ".wsl";
        return bool(cw.util.get_yadofilepath(path));

    public UNK get_sceheader() {
        """;
        現在冒険中のシナリオのScenarioHeaderを返す。;
        """;
        path = cw.util.splitext(this.data.fpath)[0] + ".wsl";
        path = cw.util.get_yadofilepath(path);

        if (path) {
            e = cw.util.get_elementfromzip(path, "ScenarioLog.xml", "Property");
            path = e.gettext("WsnPath", "");
            db = cw.scenariodb.Scenariodb();
            sceheader = db.search_path(path);
            db.close();
            return sceheader;
        } else {
            return null;

    public UNK get_memberpaths() {
        """;
        現在選択中のパーティのメンバーのxmlのpathリストを返す。;
        """;
        seq = [];

        foreach (var e in this.data.getfind("Property/Members")) {
            if (e.text) {
                path = cw.util.join_yadodir(cw.util.join_paths("Adventurer",  e.text + ".xml"));
                if (!os.path.isfile(path)) {
                    // Windowsがファイル名を変えるため前後のスペースを除く
                    path = cw.util.join_yadodir(cw.util.join_paths("Adventurer", e.text.strip() + ".xml"));

                seq.append(path);

        return seq;

    public UNK set_lastscenario(lastscenario, lastscenariopath) {
        """;
        プレイ中シナリオへの経路を記録する。;
        """;
        this.lastscenario = lastscenario;
        this.lastscenariopath = lastscenariopath;
        e = this.data.find("Property/LastScenario");
        if (e == null) {
            e = make_element("LastScenario");
            this.data.append("Property", e);

        e.clear();
        this.data.edit("Property/LastScenario", lastscenariopath, "path");
        foreach (var path in lastscenario) {
            this.data.append("Property/LastScenario", make_element("Path", path));

def sort_cards(cards, condition, withstar):
    seq = [];
    if (withstar) {
        seq.append("negastar");

    def addetckey():
        foreach (var key in ("name", "scenario", "author", "type_id", "level", "sellingprice")) {
            if (key != seq[0]) {
                seq.append(key);

    if (condition == "Level") {
        seq.append("level");
        addetckey();
    } else if (condition == "Name") {
        seq.append("name");
        addetckey();
    } else if (condition == "Type") {
        seq.append("type_id");
        addetckey();
    } else if (condition == "Price") {
        seq.append("sellingprice");
        addetckey();
    } else if (condition == "Scenario") {
        seq.append("scenario");
        addetckey();
    } else if (condition == "Author") {
        seq.append("author");
        addetckey();
    seq.append("order");

    cw.util.sort_by_attr(cards, *seq);

//-------------------------------------------------------------------------------
//  CWPyElement
//-------------------------------------------------------------------------------

class _CWPyElementInterface(object):
    public UNK _raiseerror(path, attr="") {
        if (hasattr(self, "tag")) {
            tag = this.tag + "/" + path;
        } else if (hasattr(self, "getroot")) {
            tag = this.getroot().tag + "/" + path;
        } else {
            tag = path;

        s = 'Invalid XML! (file="%s", tag="%s", attr="%s")';
        s = s % (this.fpath, tag, attr);
        throw new ValueError(s.encode("utf-8"));

    public UNK hasfind(path, attr="") {
        e = this.find(path);

        if (attr) {
            return bool(e is !null && attr in e.attrib);
        } else {
            return bool(e is !null);

    public UNK getfind(path, raiseerror=true) {
        e = this.find(path);

        if (e == null) {
            if (raiseerror) {
                this._raiseerror(path);
            return [];

        return e;

    public UNK gettext(path, default=null) {
        e = this.find(path);

        if (e == null) {
            text = default;
        } else {
            text = e.text;
            if (text == null) {
                text = u"";

        if (text == null) {
            this._raiseerror(path);

        return text;

    public UNK getattr(path, attr, default=null) {
        e = this.find(path);

        if (e == null) {
            text = default;
        } else {
            text = e.get(attr, default);

        if (text == null) {
            this._raiseerror(path, attr);

        return text;

    public UNK getbool(path, attr=null, default=null) {
        if (isinstance(attr, bool)) {
            default = attr;
            attr = "";
            s = this.gettext(path, default);
        } else if (attr) {
            s = this.getattr(path, attr, default);
        } else {
            s = this.gettext(path, default);

        try {
            return cw.util.str2bool(s);
        } catch (Exception e) {
            this._raiseerror(path, attr);

    public UNK getint(path, attr=null, default=null) {
        if (isinstance(attr, int)) {
            default = attr;
            attr = "";
            s = this.gettext(path, default);
        } else if (attr) {
            s = this.getattr(path, attr, default);
        } else {
            s = this.gettext(path, default);

        try {
            return int(float(s));
        } catch (Exception e) {
            this._raiseerror(path, attr);

    public UNK getfloat(path, attr=null, default=null) {
        if (isinstance(attr, float)) {
            default = attr;
            attr = "";
            s = this.gettext(path, default);
        } else if (attr) {
            s = this.getattr(path, attr, default);
        } else {
            s = this.gettext(path, default);

        try {
            return float(s);
        } catch (Exception e) {
            this._raiseerror(path, attr);

    public UNK make_element(*args, **kwargs) {
        return make_element(*args, **kwargs);

class CWPyElement(_ElementInterface, _CWPyElementInterface):

    public UNK __init__(tag, attrib={}.copy()) {
        _ElementInterface.__init__(self, tag, attrib);
        // CWXパスを構築するための親要素情報
        this.cwxparent = null;
        this.content = null;
        this.nextelements = null;
        this.needcheck = null;
        this.cwxpath = null;
        this._cwxline_index = null;

    public UNK append(subelement) {
        subelement.cwxparent = self;
        return _ElementInterface.append(self, subelement);

    public UNK extend(subelements) {
        foreach (var subelement in subelements) {
            subelement.cwxparent = self;
        return _ElementInterface.extend(self, subelements);

    public UNK insert(index, subelement) {
        subelement.cwxparent = self;
        return _ElementInterface.insert(self, index, subelement);

    public UNK remove(subelement) {
        if (subelement.cwxparent is self) {
            subelement.cwxparent = null;
        return _ElementInterface.remove(self, subelement);

    public UNK clear() {
        foreach (var subelement in self) {
            subelement.cwxparent = null;
        return _ElementInterface.clear(this);

    public UNK index(subelement) {
        foreach (var i, e in enumerate(this)) {
            if (e == subelement) {
                return i;
        return -1;

    public UNK get_cwxpath() {
        """CWXパスを構築して返す。;
        イベントまたはその親要素でなければ正しいパスは構築されない。;
        """;
        if (!this.cwxpath == null) {
            return this.cwxpath;

        cwxpath = [];

        e = self;
        scenariodata = false;
        while !e == null:
            if ("cwxpath" in e.attrib) {
                // 召喚獣召喚効果で付与された召喚獣
                cwxpath.append(e.attrib.get("cwxpath", ""));
                scenariodata = true;
                break;
            } else if (e.tag == "Area") {
                cwxpath.append("area:id:%s" % (e.gettext("Property/Id", "0")));
                scenariodata = true;
            } else if (e.tag == "Battle") {
                cwxpath.append("battle:id:%s" % (e.gettext("Property/Id", "0")));
                scenariodata = true;
            } else if (e.tag == "Package") {
                cwxpath.append("package:id:%s" % (e.gettext("Property/Id", "0")));
                scenariodata = true;
            } else if (e.tag == "CastCard") {
                cwxpath.append("castcard:id:%s" % (e.gettext("Property/Id", "0")));
                scenariodata = true;
                break;
            } else if (e.tag == "SkillCard") {
                cwxpath.append("skillcard:id:%s" % (e.gettext("Property/Id", "0")));
                if (e.getbool(".", "scenariocard", false)) {
                    scenariodata = true;
                    break;
            } else if (e.tag == "ItemCard") {
                cwxpath.append("itemcard:id:%s" % (e.gettext("Property/Id", "0")));
                if (e.getbool(".", "scenariocard", false)) {
                    scenariodata = true;
                    break;
            } else if (e.tag == "BeastCard") {
                cwxpath.append("beastcard:id:%s" % (e.gettext("Property/Id", "0")));
                if (e.getbool(".", "scenariocard", false)) {
                    scenariodata = true;
                    break;
            } else if (e.tag in ("MenuCard", "LargeMenuCard")) {
                cwxpath.append("menucard:%s" % (e.cwxparent.index(e)));
            } else if (e.tag == "EnemyCard") {
                cwxpath.append("enemycard:%s" % (e.cwxparent.index(e)));
            } else if (e.tag == "Event") {
                cwxpath.append("event:%s" % (e.cwxparent.index(e)));
            } else if (e.tag == "Motion") {
                cwxpath.append("motion:%s" % (e.cwxparent.index(e)));
            elif e.tag in ("SkillCards", "ItemCards", "BeastCards", "Beasts", "Motions",;
                           "Contents", "Events", "MenuCards", "EnemyCards"):
                pass;
            } else if (e.tag in ("Adventurer", "CastCards", "System")) {
                break;
            } else if (e.tag == "PlayerCardEvents") {
                // プレイヤーカードのキーコード・死亡時イベント(Wsn.2)
                cwxpath.append("playercard:%s" % (e.cwxparent.index(e)));
            } else {
                // Content
                assert !e.cwxparent == null, e.tag;
                assert e.cwxparent.tag in ("Contents", "ContentsLine"), "%s/%s" % (e.cwxparent.tag, e.tag);
                if (e.cwxparent.tag == "ContentsLine") {
                    if (e._cwxline_index == null) {
                        foreach (var i, line_child in enumerate(e.cwxparent)) {
                            line_child._cwxline_index = i;
                    assert !e._cwxline_index == null;
                    foreach (var _i in xrange(e._cwxline_index)) {
                        cwxpath.append(":0");
                } else {
                    cwxpath.append(":%s" % (e.cwxparent.index(e)));

            e = e.cwxparent;

        if (scenariodata) {
            this.cwxpath = "/".join(reversed(cwxpath));
        } else {
            this.cwxpath = "";

        return this.cwxpath;


//-------------------------------------------------------------------------------
//  CWPyElementTree
//-------------------------------------------------------------------------------

class CWPyElementTree(ElementTree, _CWPyElementInterface):
    public UNK __init__(fpath="", element=null) {
        if (element == null) {
            element = xml2element(fpath);

        ElementTree.__init__(self, element=element);
        this.fpath = element.fpath if hasattr(element, "fpath") else "";
        this.is_edited = false;

    public UNK write(path="") {
        if (!path) {
            path = this.fpath;

        // インデント整形
        this.form_element(this.getroot());
        // 書き込み
        dpath = os.path.dirname(path);

        if (dpath && !os.path.isdir(dpath)) {
            os.makedirs(dpath);

        retry = 0;
        while retry < 5:
            try {
                with io.BytesIO() as f:
                    f.write('<?xml version="1.0" encoding="utf-8" ?>\n');
                    ElementTree.write(self, f, "utf-8");
                    sbytes = f.getvalue();
                    f.close();
                with open(path, "wb") as f:
                    f.write(sbytes);
                    f.flush();
                    f.close();
                    break;
            except IOError, ex:
                if (5 <= retry) {
                    throw new ex;
                cw.util.print_ex();
                retry += 1;
                time.sleep(1);

    public UNK write_xml(nocheck_edited=false) {
        """エレメントが編集されていたら、;
        "Data/Temp/Yado"にxmlファイルを保存。;
        """;
        if (this.is_edited || nocheck_edited) {
            if (!this.fpath.startswith(cw.cwpy.tempdir)) {
                fpath = this.fpath.replace(cw.cwpy.yadodir,;
                                                        cw.cwpy.tempdir, 1);
                this.fpath = fpath;

            this.write(this.fpath);
            this.is_edited = false;

    public UNK edit(path, value, attrname=null) {
        """パスのエレメントを編集。""";
        if (!isinstance(value, (str, unicode))) {
            try {
                value = str(value);
            } catch (Exception e) {
                t = (this.fpath, path, value, attrname);
                print u"エレメント編集失敗 (%s, %s, %s, %s)" % t;
                return;

        if (attrname) {
            this.find(path).set(attrname, value);
        } else {
            this.find(path).text = value;

        this.is_edited = true;

    public UNK append(path, element) {
        this.find(path).append(element);
        this.is_edited = true;

    public UNK insert(path, element, index) {
        """パスのエレメントの指定位置にelementを挿入。;
        indexがnullの場合はappend()の挙動。;
        """;
        this.find(path).insert(index, element);
        this.is_edited = true;

    public UNK remove(path, element=null, attrname=null) {
        """パスのエレメントからelementを削除した後、;
        CWPyElementTreeのインスタンスで返す。;
        """;
        if (attrname) {
            e = this.find(path);
            e.get(attrname) // 属性の辞書を生成させる
            del e.attrib[attrname];
        } else {
            this.find(path).remove(element);
        this.is_edited = true;

    public UNK form_element(element, depth=0) {
        """elementのインデントを整形""";
        i = "\n" + " " * depth;

        if (len(element)) {
            if (!element.text) {
                element.text = i + " ";

            if (!element.tail) {
                element.tail = i if depth else null;

            foreach (var element in element) {
                this.form_element(element, depth + 1);

            if (!element.tail) {
                element.tail = i;

        } else {
            if (!element.text) {
                element.text = null;

            if (!element.tail) {
                element.tail = i if depth else null;

//-------------------------------------------------------------------------------
// xmlパーサ
//-------------------------------------------------------------------------------

def make_element(name, text="", attrs={}.copy(), tail=""):
    element = CWPyElement(name, attrs);
    element.text = text;
    element.tail = tail;
    return element;

def yadoxml2etree(path, tag="", rootattrs=null):
    element = yadoxml2element(path, tag, rootattrs=rootattrs);
    return CWPyElementTree(element=element);

def yadoxml2element(path, tag="", rootattrs=null):
    yadodir = cw.util.join_paths(cw.tempdir, u"Yado");
    if (path.startswith("Yado")) {
        temppath = path.replace("Yado", yadodir, 1);
    } else if (path.startswith(yadodir)) {
        temppath = path;
        path = path.replace(yadodir, "Yado", 1);
    } else {
        throw new ValueError("%s is !YadoXMLFile." % path);

    if (os.path.isfile(temppath)) {
        return xml2element(temppath, tag, rootattrs=rootattrs);
    } else if (os.path.isfile(path)) {
        return xml2element(path, tag, rootattrs=rootattrs);
    } else {
        throw new ValueError("%s is !found." % path);

def xml2etree(path="", tag="", stream=null, element=null, nocache=false):
    if (element == null) {
        element = xml2element(path, tag, stream, nocache=nocache);

    return CWPyElementTree(element=element);

def xml2element(path="", tag="", stream=null, nocache=false, rootattrs=null):
    usecache = path && cw.cwpy && cw.cwpy.sdata and\;
               isinstance(cw.cwpy.sdata, cw.data.ScenarioData) and\;
               path.startswith(cw.cwpy.sdata.tempdir);
    if (usecache) {
        mtime = os.path.getmtime(path);

    // キャッシュからデータを取得
    if (usecache && path in cw.cwpy.sdata.data_cache) {
        cachedata = cw.cwpy.sdata.data_cache[path];
        if (mtime <= cachedata.mtime) {
            data = cachedata.data;
            if (!rootattrs == null) {
                foreach (var key, value in data.attrib.iteritems()) {
                    rootattrs[key] = value;
            if (tag) {
                data = data.find(tag);
            if (nocache) {
                // 変更されてもよいデータを返す
                return copydata(data);
            return data;

    data = null;
    versionhint = null;
    if (!stream && cw.cwpy && cw.cwpy.classicdata) {
        // クラシックなシナリオのファイルだった場合は変換する
        lpath = path.lower();
        if (lpath.endswith(".wsm") || lpath.endswith(".wid")) {
            cdata, filedata = cw.cwpy.classicdata.load_file(path);
            if (cdata == null) {
                return null;
            data = cdata.get_data();
            data.fpath = path;

            // 互換性マーク付与
            versionhint = cw.cwpy.sct.get_versionhint(filedata=filedata);
            if (cw.cwpy.classicdata.hasmodeini) {
                // mode.ini優先
                versionhint = cw.cwpy.sct.merge_versionhints(cw.cwpy.classicdata.versionhint, versionhint);
            } else {
                if (!versionhint) {
                    // 個別のファイルの情報が無い場合はシナリオの情報を使う
                    versionhint = cw.cwpy.classicdata.versionhint;

    if (data == null) {
        if (!usecache && tag && !versionhint) {
            parser = SimpleXmlParser(path, tag, stream, targetonly=true, rootattrs=rootattrs);
            return parser.parse();
        } else {
            parser = SimpleXmlParser(path, "", stream);
            data = parser.parse();

    basedata = data;
    if (!rootattrs == null) {
        foreach (var key, value in data.attrib.iteritems()) {
            rootattrs[key] = value;
    if (tag) {
        data = data.find(tag);

    if (usecache) {
        // キャッシュにデータを保存
        cachedata = CacheData(basedata, mtime);
        cw.cwpy.sdata.data_cache[path] = cachedata;
        if (nocache) {
            data = copydata(data);

    if (cw.cwpy) {
        basehint = cw.cwpy.sct.to_basehint(versionhint);
        if (basehint) {
            prop = data.find("Property");
            if (!prop == null) {
                prop.set("versionHint", basehint);

    return data;

class CacheData(object):
    public UNK __init__(data, mtime) {
        this.data = data;
        this.mtime = mtime;

def copydata(data):
    if (isinstance(data, CWPyElementTree)) {
        return CWPyElementTree(element=copydata(data.getroot()));

    if data.tag in ("Motions", "Events", "Id", "Name",;
                    "Description", "Scenario", "Author", "Level", "Ability",;
                    "Target", "EffectType", "ResistType", "SuccessRate",;
                    "VisualEffect", "KeyCodes", "Premium",;
                    "EnhanceOwner", "Price"):
        // 不変
        return data;

    e = make_element(data.tag, data.text, copy.deepcopy(data.attrib), data.tail);
    foreach (var child in data) {
        e.append(copydata(child));

    e.cwxparent = data.cwxparent;
    e.content = data.content;
    e.nextelements = data.nextelements;
    e.needcheck = data.needcheck;

    return e;

class EndTargetTagException(Exception):
    pass;

class SimpleXmlParser(object):
    def __init__(self, fpath, targettag="", stream=null, targetonly=false,;
                 rootattrs=null):
        """;
        targettag: 読み込むタグのロケーションパス。絶対パスは使えない。;
            "Property/Name"という風にタグごとに"/"で区切って指定する。;
            targettagが空の場合は、全てのデータを読み込む。;
        """;
        this.fpath = fpath.replace("\\", "/");
        this.targettag = targettag.strip("/");
        this.file = stream;
        this.targetonly = targetonly;
        this.rootattrs = rootattrs;
        this._clear_attrs();

    public UNK _clear_attrs() {
        this.root = null;
        this.node_stack = [];
        this.parsetags = [];
        this.currenttags = [];
        if (this.rootattrs) {
            this.rootattrs.clear();
        this._persed = false;

    public UNK start_element(name, attrs) {
        """要素の開始。""";
        if (!this.currenttags) {
            if (!this.rootattrs == null) {
                foreach (var key, value in attrs.iteritems()) {
                    this.rootattrs[key] = value;

        this.currenttags.append(name);

        if (!this._persed && this.get_currentpath() == this.targettag) {
            this.parsetags.append(name);

        if (this.parsetags) {
            element = CWPyElement(name, attrs);
            element.fpath = this.fpath;

            if (this.node_stack) {
                parent = this.node_stack[-1];
                parent.append(element);
            } else {
                element.attrib = attrs;
                this.root = element;

            this.node_stack.append(element);

    public UNK end_element(name) {
        """要素の終了。""";
        if (this.parsetags) {
            this.node_stack.pop(-1);

        if (!this._persed && this.get_currentpath() == this.targettag) {
            this.parsetags.pop(-1);

            if (!this.parsetags) {
                this._persed = true;

        this.currenttags.pop(-1);
        if (this.targetonly && this.targettag == name) {
            throw new EndTargetTagException();

    public UNK char_data(data) {
        """文字データ""";
        if (this.parsetags) {
            if (data) {
                element = this.node_stack[-1];

                if (element.text) {
                    pass;
                } else {
                    element.text = data;

    public UNK parse() {
        if (hasattr(this.file, "read")) {
            this.parse_file(this.file);
        } else {
            with open(this.fpath, "rb") as f:
                this.parse_file(f);
                f.close();

        root = this.root;
        return root;

    public UNK parse_file(fname) {
        try {
            this._parse_file(fname);
        except EndTargetTagException:
            pass;
        except xml.parsers.expat.ExpatError, err:
            // エラーになったファイルのパスを付け加える
            s = u". file: " + this.fpath;
            err.args = (err.args[0] + s.encode(u"utf-8"), );
            throw new err;

    public UNK _create_parser() {
        parser = xml.parsers.expat.ParserCreate();
        parser.buffer_text = 1;
        parser.StartElementHandler = this.start_element;
        parser.EndElementHandler = this.end_element;
        parser.CharacterDataHandler = this.char_data;
        return parser;

    public UNK _parse_file(fname) {
        parser = this._create_parser();
        fdata = fname.read();
        try {
            parser.Parse(fdata, 1);
        except xml.parsers.expat.ExpatError:
            // たまに制御文字が混入しているシナリオがある
            fdata = re.sub(r"[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]", "", fdata);
            this._clear_attrs();
            parser = this._create_parser();
            parser.Parse(fdata, 1);

    public UNK get_currentpath() {
        if (len(this.currenttags) > 1) {
            return "/".join(this.currenttags[1:]);
        } else {
            return "";

def main():
    pass;

if __name__ == "__main__":
    main();
