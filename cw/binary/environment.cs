//!/usr/bin/env python
// -*- coding: utf-8 -*-

import os;
import base;

import cw;


class Environment(base.CWBinaryBase):
    """Environment.wyd(type=-1);
    システム設定とかゴシップとか終了印とかいろいろまとめているデータ。;
    """;
    public UNK __init__(parent, f, yadodata=false, versiononly=false) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        this.name = os.path.basename(os.path.dirname(this.fpath));
        this.type = -1;
        this.dataversion = f.string();
        if (this.dataversion.startswith("DATAVERSION_")) {
            this.dataversion_int = int(this.dataversion[len("DATAVERSION_"):]);
        } else {
            this.dataversion_int = 0;

        if (versiononly) {
            return;

        this.yadotype = f.byte() // 宿タイプ(1:通常, 2:デバッグ)
        this.drawcard_speed = f.dword() // カード速度
        this.drawbg_speed = f.dword() // 背景速度
        this.message_speed = f.dword() // メッセージ速度
        this.play_bgm = f.bool() // BGM再生
        this.play_sound = f.bool() // 効果音再生
        if (10 <= this.dataversion_int) {
            this.correct_scaledown = f.bool() // カードのスムージング(縮小)
            this.correct_scaleup = f.bool() // カードのスムージング(拡大)
        } else {
            _b = f.bool() // レアリティのないカードも買い戻せるようにする
            _b = f.bool() // 売却・破棄時に確認メッセージの表示
        this.autoselect_party = f.bool() // 宿を開いた時に最後のパーティを選択
        this.clickcancel = f.bool() // 背景右クリックでキャンセル
        if (10 <= this.dataversion_int) {
            this.effect_getmoney = f.bool() // 所持金増減時に点滅させる
            this.clickjump = f.bool() // 右クリックで待機時間を飛ばす
            this.keep_levelmax = f.bool() // レベルを最大値に維持する
        if (11 <= this.dataversion_int) {
            this.bgeffectatselmode = f.bool() // 選択モードでカーテンをかける
        } else {
            this.bgeffectatselmode = true;
        this.viewtype_poster = f.byte() // 貼紙の表示条件
        this.bgcolor_message = f.dword() // メッセージ背景濃度
        this.use_decofont = f.bool() // 装飾フォントの使用
        this.changetype_bg = f.byte() // 背景切替方式
        this.compstamps = f.string(true) // 終了印のリスト
        this.scenarioname = f.string() // 選択中パーティのいるシナリオ名(用途不明)
        this.gossips = f.string(true) // ゴシップのリスト
        if (10 <= this.dataversion_int) {
            // 1.28以降
            // カード置場のカードデータ
            unusedcards_num = f.dword();
            this.unusedcards = [UnusedCard(self, f);
                                        for _cnt in xrange(unusedcards_num)];
            // カード置場と荷物袋のカードヘッダ
            yadocards_num = f.dword();
            this.yadocards = [YadoCard(self, f) for _cnt in xrange(yadocards_num)];
            // 宿の資金
            this.money = f.dword();
        } else {
            // 1.20
            this.unusedcards = [];
            this.yadocards = [];
            this.money = 0;
        // 選択中のパーティ名
        this.partyname = f.string();

        // CardWirthPyにおける選択中パーティ
        // パーティ変換後に操作する
        this.cwpypartyname = "";
        // スキンタイプ。読み込み後に操作する
        this.skintype = "";
        // スキンディレクトリ。現在の設定を使用
        this.skinname = cw.cwpy.setting.skindirname;
        // データの取得に失敗したカード。変換時に追加する
        this.errorcards = [];

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Environment");

            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            e = cw.data.make_element("Type", this.skintype);
            prop.append(e);
            e = cw.data.make_element("Skin", this.skinname);
            prop.append(e);
            e = cw.data.make_element("Cashbox", str(this.money));
            prop.append(e);
            e = cw.data.make_element("NowSelectingParty", this.cwpypartyname);
            prop.append(e);
            this.data.append(prop);

            e = cw.data.make_element("CompleteStamps");
            foreach (var compstamp in cw.util.decodetextlist(this.compstamps)) {
                if (compstamp) {
                    e.append(cw.data.make_element("CompleteStamp", compstamp));
            this.data.append(e);

            e = cw.data.make_element("Gossips");
            foreach (var gossip in cw.util.decodetextlist(this.gossips)) {
                if (gossip) {
                    e.append(cw.data.make_element("Gossip", gossip));
            this.data.append(e);

            // 保管庫のカードのxml出力
            this.errorcards = [];
            foreach (var i, unusedcard in enumerate(this.unusedcards)) {
                if (unusedcard.data) {
                    try {
                        unusedcard.create_xml2(this.get_dir(), cardorder=i);
                    except Exception:
                        cw.util.print_ex();
                        this.errorcards.append(unusedcard);
                } else {
                    this.errorcards.append(unusedcard);

        return this.data;

    public UNK get_cardtypedict() {
        d = {};

        foreach (var card in this.yadocards) {
            d[card.fname] = card.type;

        return d;

    @staticmethod;
    def unconv(f, data, table):
        yadotype = 1 // 常に通常宿とする
        play_bgm = true;
        play_sound = true;
        correct_scaledown = true;
        correct_scaleup = true;
        autoselect_party = true;
        clickcancel = true;
        effect_getmoney = true;
        clickjump = true;
        keep_levelmax = true;
        viewtype_poster = 1;
        bgcolor_message = 3;
        use_decofont = false;
        changetype_bg = 1;
        compstamps = "";
        scenarioname = "";
        gossips = "";
        money = 0;
        partyname = "";

        // 設定を可能なだけ反映
        if (cw.cwpy.setting.transition == "None") {
            changetype_bg = 0;
        } else if (cw.cwpy.setting.transition == "Blinds") {
            changetype_bg = 1;
        } else if (cw.cwpy.setting.transition == "Fade") {
            changetype_bg = 2;
        } else if (cw.cwpy.setting.transition == "PixelDissolve") {
            changetype_bg = 3;

        def roundval(value):
            return int(round((value-5) / 10.0 * 8.0)) + 4;
        drawcard_speed = roundval(cw.cwpy.setting.get_dealspeed(false));
        drawbg_speed = roundval(cw.cwpy.setting.transitionspeed);
        message_speed = roundval(cw.cwpy.setting.messagespeed);

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Cashbox") {
                        money = int(prop.text);
                        money = cw.util.numwrap(money, 0, 999999);
                    } else if (prop.tag == "NowSelectingParty") {
                        partyname = table["party"].get(prop.text, "");
            } else if (e.tag == "CompleteStamps") {
                seq = [];
                foreach (var cse in e) {
                    if (cse.text) {
                        seq.append(cse.text);
                compstamps = cw.util.encodetextlist(seq);
            } else if (e.tag == "Gossips") {
                seq = [];
                foreach (var ge in e) {
                    if (ge.text) {
                        seq.append(ge.text);
                gossips = cw.util.encodetextlist(seq);

        f.write_string("DATAVERSION_10");
        f.write_byte(yadotype);
        f.write_dword(drawcard_speed);
        f.write_dword(drawbg_speed);
        f.write_dword(message_speed);
        f.write_bool(play_bgm);
        f.write_bool(play_sound);
        f.write_bool(correct_scaledown);
        f.write_bool(correct_scaleup);
        f.write_bool(autoselect_party);
        f.write_bool(clickcancel);
        f.write_bool(effect_getmoney);
        f.write_bool(clickjump);
        f.write_bool(keep_levelmax);
        f.write_byte(viewtype_poster);
        f.write_dword(bgcolor_message);
        f.write_bool(use_decofont);
        f.write_byte(changetype_bg);
        f.write_string(compstamps, true);
        f.write_string(scenarioname);
        f.write_string(gossips, true);
        unusedcards = table["unusedcards"];
        f.write_dword(len(unusedcards));
        foreach (var fname, card in unusedcards) {
            UnusedCard.unconv(f, card, fname);
        yadocards = table["yadocards"];
        f.write_dword(len(yadocards));
        foreach (var fname, card in yadocards.values()) {
            YadoCard.unconv(f, card, fname);
        f.write_dword(money);
        f.write_string(partyname);

class UnusedCard(base.CWBinaryBase):
    """カード置き場のカードのデータ。;
    this.dataにwidファイルから読み込んだカードデータがある。;
    """;
    public UNK __init__(parent, f, yadodata=false) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        if (f) {
            this.fname = f.rawstring();
            this.uselimit = f.dword();
            f.byte();
        } else {
            this.fname = "";
            this.uselimit = 0;
        this.data = null;

    public UNK set_data(data) {
        """widファイルから読み込んだカードデータを関連づける""";
        this.data = data;

    public UNK get_data() {
        return this.data.get_data();

    public UNK create_xml(dpath) {
        return this.create_xml2(dpath, -1);

    public UNK create_xml2(dpath, cardorder) {
        """this.data.create_xml()""";
        this.data.limit = this.uselimit;
        path = this.data.create_xml(dpath);
        yadodb = this.get_root().yadodb;
        if (yadodb) {
            yadodb.insert_card(path, commit=false, cardorder=cardorder);
        return path;

    @staticmethod;
    def unconv(f, data, fname):
        f.write_rawstring(cw.util.splitext(fname)[0]);
        f.write_dword(data.getint("Property/UseLimit", 0));
        f.write_byte(0);

class YadoCard(base.CWBinaryBase):
    """カード置き場のカードと荷物袋のカードのデータ。;
    ここのtypeで宿にあるカードのタイプ(技能・アイテム・召喚獣)を判別できる。;
    """;
    public UNK __init__(parent, f, yadodata=false) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        f.byte();
        f.byte();
        this.name = f.string();
        this.description = f.string();
        this.type = f.byte();
        this.fname = f.rawstring();
        this.number = f.dword() // 個数

    @staticmethod;
    def unconv(f, data, fname):
        name = data.gettext("Property/Name", "");
        description = data.gettext("Property/Description", "");
        if (data.tag == "SkillCard") {
            restype = 1;
        } else if (data.tag == "ItemCard") {
            restype = 2;
        } else if (data.tag == "BeastCard") {
            restype = 3;
        number = 1;

        f.write_byte(0);
        f.write_byte(0);
        f.write_string(name);
        f.write_string(description, true);
        f.write_byte(restype);
        f.write_rawstring(cw.util.splitext(fname)[0]);
        f.write_dword(number);

def main():
    pass;

if __name__ == "__main__":
    main();
