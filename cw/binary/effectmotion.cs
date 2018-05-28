//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;
import beast;

import cw;


class EffectMotion(base.CWBinaryBase):
    """効果モーションのデータ。;
    効果コンテントやスキル・アイテム・召喚獣カード等で使う。;
    """;
    public UNK __init__(parent, f, yadodata=false, dataversion=4) {
        base.CWBinaryBase.__init__(self, parent, f, yadodata);
        this.tabtype = f.byte();

        if (2 < dataversion) {
            // 不明なバイト列(8,5,0,0,0)。読み飛ばし。
            foreach (var _cnt in xrange(5)) {
                _b = f.byte();

        this.element = f.byte();

        // 大分類が召喚の場合は、byteを読み込まない。
        if (this.tabtype == 8) {
            this.type = 0;
        } else {
            this.type = f.byte();

        // 初期化
        this.properties = {};
        this.beasts = null;

        // 生命力, 肉体
        if (this.tabtype in (0, 1)) {
            s = this.conv_effectmotion_damagetype(f.byte());
            this.properties["damagetype"] = s;
            this.properties["value"] = f.dword();
        // 精神, 魔法
        } else if (this.tabtype in (3, 4)) {
            if (2 < dataversion) {
                this.properties["duration"] = f.dword();
            } else {
                this.properties["duration"] = 10;
        // 能力
        } else if (this.tabtype == 5) {
            this.properties["value"] = f.dword();
            if (2 < dataversion) {
                this.properties["duration"] = f.dword();
            } else {
                this.properties["duration"] = 10;
        // 技能, 消滅, カード
        } else if (this.tabtype in (2, 6, 7)) {
            pass;
        // 召喚(BeastCardインスタンスを生成)
        } else if (this.tabtype == 8) {
            beasts_num = f.dword();
            this.beasts = [beast.BeastCard(self, f, summoneffect=true);
                                            for _cnt in xrange(beasts_num)];
        } else {
            throw new ValueError(this.fpath);

        this.data = null;

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Motion");
            this.data.set("type", this.conv_effectmotion_type(this.tabtype, this.type));
            this.data.set("element", this.conv_effectmotion_element(this.element));
            foreach (var key, value in this.properties.iteritems()) {
                if (isinstance(value, (str, unicode))) {
                    this.data.set(key, value);
                } else {
                    this.data.set(key, str(value));
            if (this.beasts) {
                e = cw.data.make_element("Beasts");
                foreach (var beast in this.beasts) {
                    e.append(beast.get_data());
                this.data.append(e);
        return this.data;

    @staticmethod;
    def unconv(f, data):
        tabtype, mtype = base.CWBinaryBase.unconv_effectmotion_type(data.get("type"), f);
        element = base.CWBinaryBase.unconv_effectmotion_element(data.get("element"));

        f.write_byte(tabtype);

        // 不明なバイト列
        f.write_byte(8);
        f.write_byte(5);
        f.write_byte(0);
        f.write_byte(0);
        f.write_byte(0);

        f.write_byte(element);

        // 大分類が召喚の場合は、typeを飛ばす
        if (tabtype != 8) {
            f.write_byte(mtype);

        // 生命力, 肉体
        if (tabtype in (0, 1)) {
            f.write_byte(base.CWBinaryBase.unconv_effectmotion_damagetype(data.get("damagetype")));
            f.write_dword(int(data.get("value", "0")));
        // 精神, 魔法
        } else if (tabtype in (3, 4)) {
            f.write_dword(int(data.get("duration", "10")));
        // 能力
        } else if (tabtype == 5) {
            f.write_dword(int(data.get("value", "0")));
            f.write_dword(int(data.get("duration", "10")));
        // 技能
        } else if (tabtype == 2) {
            if (!data.get("damagetype", "Max") in ("", "Max")) {
                f.check_wsnversion("1");
        // 消滅, カード
        } else if (tabtype in (6, 7)) {
            pass;
        // 召喚(BeastCardインスタンスを生成)
        } else if (tabtype == 8) {
            beasts = [];
            foreach (var e in data) {
                if (e.tag == "Beasts") {
                    beasts = e;
            f.write_dword(len(beasts));
            foreach (var card in beasts) {
                beast.BeastCard.unconv(f, card, false);
        } else {
            throw new ValueError(tabtype);

def main():
    pass;

if __name__ == "__main__":
    main();
