//!/usr/bin/env python
// -*- coding: utf-8 -*-

import os;
import io;
import weakref;

import util;
import wx;

import cw;


class CWBinaryBase(object):
    public CWBinaryBase(UNK parent, UNK f, bool yadodata=false, UNK materialdir="Material", bool image_export=true) {
        this.set_root(parent); // TODO
        this.xmltype = this.__class__.__name__; // TODO
        if (hasattr(f, "name")) {
            this.fpath = f.name;
        } else {
            this.fpath = "";
        }
        this.materialbasedir = "";
        this.set_materialdir(materialdir);
        this.set_image_export(image_export, false);
        this.yadodb = null;
        this.xmlpath = "";

        if (parent) {
            this.yadodata = parent.yadodata;
        } else {
            this.yadodata = yadodata;
        }
    }

    public UNK set_root(parent) {
        if (parent) {
            this.root = parent.root;
        } else {
            this.root = weakref.ref(this);
        }
    }

    public UNK get_root() {
        return this.root();
    }

    public UNK set_dir(UNK path) {
        this.get_root().dir = path;
    }

    public UNK get_dir() {
        try {
            return this.get_root().dir;
        } catch (Exception e) {
            return "";
        }
    }

    public UNK set_imgdir(UNK path) {
        this.get_root().imgdir = path;
    }

    public UNK get_imgdir() {
        try {
            return this.get_root().imgdir;
        } catch (Exception e) {
            return "";
        }
    }

    public UNK get_fname() {
        fname = os.path.basename(this.fpath);
        return cw.util.splitext(fname)[0];
    }

    public bool is_root() {
        return bool(self == this.get_root());
    }

    public UNK is_yadodata() {
        return this.yadodata;
    }

    public UNK set_materialdir(UNK materialdir) {
        // """materialdirを素材ディレクトリとして登録する。;
        // デフォルト値は"Material"。""";
        this._materialdir = materialdir;
    }

    public UNK get_materialdir() {
        // """素材ディレクトリ名を返す。;
        // 親要素がある場合、親の設定が優先される。""";
        root = this.get_root();
        if (root is self) {
            return this._materialdir;
        } else {
            return root.get_materialdir();
        }
    }

    public UNK set_image_export(UNK image_export, bool force=false) {
        // """XML変換時に格納イメージをエクスポートするか設定する。""";
        this._image_export = image_export;
        this._force_exportsetting = force;
    }

    public UNK get_image_export() {
        // """XML変換時に格納イメージをエクスポートする場合はtrue。;
        // 親要素がある場合、親の設定が優先される。""";
        if (this._force_exportsetting) {
            return this._image_export;
        }

        root = this.get_root();
        if (root is self) {
            return this._image_export;
        } else {
            return root.get_image_export();
        }
    }


//-------------------------------------------------------------------------------
// XML作成用
//-------------------------------------------------------------------------------

    public UNK create_xml(UNK dpath) {
        // """XMLファイルを作成する。;
        // dpath: XMLを作成するディレクトリ;
        // """;
        // 保存ディレクトリ設定
        this.set_dir(dpath);

        // xmlファイルパス
        if (this.xmltype in ("Summary", "Environment")) {
            path = util.join_paths(this.get_dir(), this.xmltype + ".xml");
            path = util.check_duplicate(path);
        } else if (this.xmltype == "Party") {
            name = util.check_filename(this.name);
            path = util.join_paths(this.get_dir(), "Party", name);
            path = util.check_duplicate(path);
            path = util.join_paths(path, "Party.xml");
        } else {
            name = util.check_filename(this.name) + ".xml";

            // シナリオデータは先頭にidを付与
            if (!this.is_yadodata()) {
                name = str(this.id).zfill(2) + "_" + name;
            }

            path = util.join_paths(this.get_dir(), this.xmltype, name);
            path = util.check_duplicate(path);
        }

        // xml出力

        if (!os.path.isdir(os.path.dirname(path))) {
            os.makedirs(os.path.dirname(path));
        }

        data = this.get_data();
        cw.data.CWPyElementTree(element=data).write(path);
        this.xmlpath = path;
        return path;
    }

    public string export_image() {
        // """内部画像を出力する""";
        if (!hasattr(self, "image")) {
            return "";
        }

        if (!this.image) {
            return "";
        }

        if (!this.get_image_export()) {
            // パスの代わりにバイナリイメージを使用する
            return cw.binary.image.data_to_code(this.image);
        }

        basedir = this.materialbasedir;
        if (!basedir) {
            basedir = this.get_dir();
        }

        // 画像保存ディレクトリ
        if (this.xmltype == "Summary") {
            imgdir = basedir;
        } else if (this.xmltype == "BeastCard" && this.summoneffect) {
            imgdir = this.get_imgdir();
            if (!basedir) {
                basedir = this.get_root().materialbasedir;
            }

            if (!imgdir) {
                root = this.get_root();
                name = util.check_filename(root.name);
                mdir = this.get_materialdir();
                if (mdir == "") {
                    imgdir = util.join_paths(basedir, root.xmltype, name);
                } else {
                    imgdir = util.join_paths(basedir, mdir, root.xmltype, name);
                }
                imgdir = util.check_duplicate(imgdir);
                this.set_imgdir(imgdir);
            }

        } else if (this.xmltype in ("Adventurer", "SkillCard", "ItemCard", "BeastCard", "CastCard") {
            name = util.check_filename(this.name);
            mdir = this.get_materialdir();
            if (mdir == "") {
                imgdir = util.join_paths(basedir, this.xmltype, name);
            } else {
                imgdir = util.join_paths(basedir, mdir, this.xmltype, name);
            }
            imgdir = util.check_duplicate(imgdir);
            this.set_imgdir(imgdir);
        } else {
            mdir = this.get_materialdir();
            if (mdir == "") {
                imgdir = util.join_paths(basedir, this.xmltype);
            } else {
                imgdir = util.join_paths(basedir, mdir, this.xmltype);
            }
        }

        // 画像保存
        // 画像パス
        if (this.xmltype == "Summary") {
            path = util.join_paths(imgdir, this.xmltype + ".bmp");
        } else {
            name = util.check_filename(this.name) + ".bmp";
            path = util.join_paths(imgdir, name);
        }

        // 画像出力
        path = util.check_duplicate(path);

        if (!os.path.isdir(imgdir)) {
            os.makedirs(imgdir);
        }

        with open(path, "wb") as f: // TODO
            f.write(this.image);
            f.flush();
            f.close();

        // 最後に参照パスを返す
        path = path.replace(basedir + "/", "", 1);
        return util.repl_escapechar(path);
    }

    public static void check_imgpath(UNK f, UNK e_imgpath, UNK defpostype) {
        // """ImagePath要素のWSNバージョンをチェックする。""";
        if (e_imgpath == null) {
            return;
        }
        assert e_imgpath.tag in ("ImagePath", "Talk"), e_imgpath.tag;
        postype = e_imgpath.getattr(".", "positiontype", "Default");
        if (postype in (defpostype, "Default")) {
            return;
        }
        f.check_wsnversion("2");
    }

    public static void check_coupon(UNK f, UNK coupon) {
        // """称号名couponがシステムクーポンであればWSNバージョンをチェックする。;
        // """;
        if (coupon in (u"＠効果対象", u"＠効果対象外", u"＠イベント対象", u"＠使用者")) {
            f.check_wsnversion("2");
        }
    }

    public static UNK import_image(UNK f, UNK imagepath, bool convertbitmap=true, bool fullpath=false, string defpostype="TopLeft") {
        // """imagepathの画像を読み込み、バイナリデータとして返す。;
        // ビットマップ以外であればビットマップに変換する。;
        // """;
        if (isinstance(imagepath, cw.data.CWPyElement)) {
            e = imagepath;
            if (e.tag == "ImagePath") {
                CWBinaryBase.check_imgpath(f, e, defpostype);
                imagepath = e.text;
            } else if (e.tag == "ImagePaths") {
                if (1 < len(e)) {
                    f.check_wsnversion("1");
                }
                CWBinaryBase.check_imgpath(f, e.find("ImagePath"), defpostype);
                imagepath = e.gettext("ImagePath", "");
            } else {
                imagepath = "";
            }
        }

        if (!imagepath) {
            return null;
        }

        if (cw.binary.image.path_is_code(imagepath)) {
            image = cw.binary.image.code_to_data(imagepath);
        } else {
            if (fullpath) {
                fpath = imagepath;
            } else {
                fpath = cw.util.join_paths(cw.cwpy.sdata.tempdir, imagepath);
                if (!os.path.isfile(fpath)) {
                    fpath = cw.util.join_paths(cw.cwpy.tempdir, imagepath);
                    if (!os.path.isfile(fpath)) {
                        fpath = cw.util.join_paths(cw.cwpy.yadodir, imagepath);
                        if (!os.path.isfile(fpath)) {
                            return null;
                        }
                    }
                }
            }

            with open(fpath, "rb") as f: // TODO
                image = f.read();
                f.close();
        }

        if (convertbitmap && cw.util.get_imageext(image) != ".bmp") {
            with io.BytesIO(image) as f: // TODO
                data = wx.ImageFromStream(f);
                f.close();
            with io.BytesIO() as f: // TODO
                data.SaveStream(f, wx.BITMAP_TYPE_BMP);
                image = f.getvalue();
                f.close();
        }

        return image;
    }

    public UNK get_data() {
        // """CWPyElementのインスタンスを返す。""";
        return null;
    }

    public string get_materialpath(UNK path) {
        // """引数のパスを素材ディレクトリに関連づける。;
        // dpath: 素材ファイルのパス。;
        // """;
        if (path == u"（なし）") {
            return "";
        }
        mdir = this.get_materialdir();
        if (mdir == "") {
            return path;
        } else if (path) {
            return util.join_paths(mdir, path);
        } else {
            return "";
        }
    }

    public static string materialpath(string path) {
        // """素材パスを逆変換する。""";
        if (!path) {
            return "";
        }

        if (path.startswith("Material/")) {
            return path[9:];
        } else {
            return path;
        }
    }

    public string get_indent(int indent) {
        // """インデントの文字列を返す。スペース一個分。""";
        return " " * indent;
    }

    public string get_propertiestext(UNK d) {
        // """XMLエレメントのプロパティ文字列を返す。""";
        s = "";

        foreach (var key, value in d.iteritems()) {
            s += ' %s="%s"' % (key, value);

        return s;
    }

//-------------------------------------------------------------------------------
// コンテント
//-------------------------------------------------------------------------------

    public UNK conv_contenttype(int n) {
        // """引数の値から、コンテントの種類を返す。""";
        if (n == 0) {
            return "Start", "";                // スタート
        } else if (n == 1) {
            return "Link", "Start";            // スタートへのリンク
        } else if (n == 2) {
            return "Start", "Battle";          // バトル開始
        } else if (n == 3) {
            return "End", ""                  // シナリオクリア
        } else if (n == 4) {
            return "End", "BadEnd";            // ゲームオーバー
        } else if (n == 5) {
            return "Change", "Area";           // エリア移動
        } else if (n == 6) {
            return "Talk", "Message";          // メッセージ
        } else if (n == 7) {
            return "Play", "Bgm";              // BGM変更
        } else if (n == 8) {
            return "Change", "BgImage";        // 背景変更
        } else if (n == 9) {
            return "Play", "Sound";            // 効果音
        } else if (n == 10) {
            return "Wait", "";                 // 空白時間
        } else if (n == 11) {
            return "Effect", "";               // 効果
        } else if (n == 12) {
            return "Branch", "Select";         // メンバ選択分岐
        } else if (n == 13) {
            return "Branch", "Ability";        // 能力判定分岐
        } else if (n == 14) {
            return "Branch", "Random";         // ランダム分岐
        } else if (n == 15) {
            return "Branch", "Flag";           // フラグ分岐
        } else if (n == 16) {
            return "Set", "Flag";              // フラグ変更
        } else if (n == 17) {
            return "Branch", "MultiStep";      // ステップ多岐分岐
        } else if (n == 18) {
            return "Set", "Step";              // ステップ変更
        } else if (n == 19) {
            return "Branch", "Cast";           // キャスト存在分岐
        } else if (n == 20) {
            return "Branch", "Item";           // アイテム所持分岐
        } else if (n == 21) {
            return "Branch", "Skill";          // スキル所持分岐
        } else if (n == 22) {
            return "Branch", "Info";           // 情報所持分岐
        } else if (n == 23) {
            return "Branch", "Beast";          // 召喚獣存在分岐
        } else if (n == 24) {
            return "Branch", "Money";          // 所持金分岐
        } else if (n == 25) {
            return "Branch", "Coupon";         // 称号分岐
        } else if (n == 26) {
            return "Get", "Cast";              // キャスト加入
        } else if (n == 27) {
            return "Get", "Item";              // アイテム入手
        } else if (n == 28) {
            return "Get", "Skill";             // スキル入手
        } else if (n == 29) {
            return "Get", "Info";              // 情報入手
        } else if (n == 30) {
            return "Get", "Beast";             // 召喚獣獲得
        } else if (n == 31) {
            return "Get", "Money";             // 所持金増加
        } else if (n == 32) {
            return "Get", "Coupon";            // 称号付与
        } else if (n == 33) {
            return "Lose", "Cast";             // キャスト離脱
        } else if (n == 34) {
            return "Lose", "Item";             // アイテム喪失
        } else if (n == 35) {
            return "Lose", "Skill";            // スキル喪失
        } else if (n == 36) {
            return "Lose", "Info";             // 情報喪失
        } else if (n == 37) {
            return "Lose", "Beast";            // 召喚獣喪失
        } else if (n == 38) {
            return "Lose", "Money";            // 所持金減少
        } else if (n == 39) {
            return "Lose", "Coupon";           // 称号剥奪
        } else if (n == 40) {
            return "Talk", "Dialog";           // セリフ
        } else if (n == 41) {
            return "Set", "StepUp";            // ステップ増加
        } else if (n == 42) {
            return "Set", "StepDown";          // ステップ減少
        } else if (n == 43) {
            return "Reverse", "Flag";          // フラグ反転
        } else if (n == 44) {
            return "Branch", "Step";           // ステップ上下分岐
        } else if (n == 45) {
            return "Elapse", "Time";           // 時間経過
        } else if (n == 46) {
            return "Branch", "Level";          // レベル分岐
        } else if (n == 47) {
            return "Branch", "Status";         // 状態分岐
        } else if (n == 48) {
            return "Branch", "PartyNumber";    // 人数判定分岐
        } else if (n == 49) {
            return "Show", "Party";            // パーティ表示
        } else if (n == 50) {
            return "Hide", "Party";            // パーティ隠蔽
        } else if (n == 51) {
            return "Effect", "Break";          // 効果中断
        } else if (n == 52) {
            return "Call", "Start";            // スタートのコール
        } else if (n == 53) {
            return "Link", "Package";          // パッケージへのリンク
        } else if (n == 54) {
            return "Call", "Package";          // パッケージのコール
        } else if (n == 55) {
            return "Branch", "Area";           // エリア分岐
        } else if (n == 56) {
            return "Branch", "Battle";         // バトル分岐
        } else if (n == 57) {
            return "Branch", "CompleteStamp";  // 終了シナリオ分岐
        } else if (n == 58) {
            return "Get", "CompleteStamp";     // 終了シナリオ設定
        } else if (n == 59) {
            return "Lose", "CompleteStamp";    // 終了シナリオ削除
        } else if (n == 60) {
            return "Branch", "Gossip";         // ゴシップ分岐
        } else if (n == 61) {
            return "Get", "Gossip";            // ゴシップ追加
        } else if (n == 62) {
            return "Lose", "Gossip";           // ゴシップ削除
        } else if (n == 63) {
            return "Branch", "IsBattle";       // バトル判定分岐
        } else if (n == 64) {
            return "Redisplay", "";            // 画面の再構築
        } else if (n == 65) {
            return "Check", "Flag";            // フラグ判定
        } else if (n == 66) {
            return "Substitute", "Step";       // ステップ代入(1.30)
        } else if (n == 67) {
            return "Substitute", "Flag";       // フラグ代入(1.30)
        } else if (n == 68) {
            return "Branch", "StepValue";      // ステップ比較(1.30)
        } else if (n == 69) {
            return "Branch", "FlagValue";      // フラグ比較(1.30)
        } else if (n == 70) {
            return "Branch", "RandomSelect";   // ランダム選択(1.30)
        } else if (n == 71) {
            return "Branch", "KeyCode";        // キーコード所持分岐(1.50)
        } else if (n == 72) {
            return "Check", "Step";            // ステップ判定(1.50)
        } else if (n == 73) {
            return "Branch", "Round";          // ラウンド分岐(1.50)
        } else {
            throw new ValueError(this.fpath);
        }
    }

    public static int unconv_contenttype(string ctype, string n) {
        if (ctype == "Start" && n == "") {
            return 0;
        } else if (ctype == "Link" && n == "Start") {
            return 1;
        } else if (ctype == "Start" && n == "Battle") {
            return 2;
        } else if (ctype == "End" && n == "") {
            return 3;
        } else if (ctype == "End" && n == "BadEnd") {
            return 4;
        } else if (ctype == "Change" && n == "Area") {
            return 5;
        } else if (ctype == "Talk" && n == "Message") {
            return 6;
        } else if (ctype == "Play" && n == "Bgm") {
            return 7;
        } else if (ctype == "Change" && n == "BgImage") {
            return 8;
        } else if (ctype == "Play" && n == "Sound") {
            return 9;
        } else if (ctype == "Wait" && n == "") {
            return 10;
        } else if (ctype == "Effect" && n == "") {
            return 11;
        } else if (ctype == "Branch" && n == "Select") {
            return 12;
        } else if (ctype == "Branch" && n == "Ability") {
            return 13;
        } else if (ctype == "Branch" && n == "Random") {
            return 14;
        } else if (ctype == "Branch" && n == "Flag") {
            return 15;
        } else if (ctype == "Set" && n == "Flag") {
            return 16;
        } else if (ctype == "Branch" && n == "MultiStep") {
            return 17;
        } else if (ctype == "Set" && n == "Step") {
            return 18;
        } else if (ctype == "Branch" && n == "Cast") {
            return 19;
        } else if (ctype == "Branch" && n == "Item") {
            return 20;
        } else if (ctype == "Branch" && n == "Skill") {
            return 21;
        } else if (ctype == "Branch" && n == "Info") {
            return 22;
        } else if (ctype == "Branch" && n == "Beast") {
            return 23;
        } else if (ctype == "Branch" && n == "Money") {
            return 24;
        } else if (ctype == "Branch" && n == "Coupon") {
            return 25;
        } else if (ctype == "Get" && n == "Cast") {
            return 26;
        } else if (ctype == "Get" && n == "Item") {
            return 27;
        } else if (ctype == "Get" && n == "Skill") {
            return 28;
        } else if (ctype == "Get" && n == "Info") {
            return 29;
        } else if (ctype == "Get" && n == "Beast") {
            return 30;
        } else if (ctype == "Get" && n == "Money") {
            return 31;
        } else if (ctype == "Get" && n == "Coupon") {
            return 32;
        } else if (ctype == "Lose" && n == "Cast") {
            return 33;
        } else if (ctype == "Lose" && n == "Item") {
            return 34;
        } else if (ctype == "Lose" && n == "Skill") {
            return 35;
        } else if (ctype == "Lose" && n == "Info") {
            return 36;
        } else if (ctype == "Lose" && n == "Beast") {
            return 37;
        } else if (ctype == "Lose" && n == "Money") {
            return 38;
        } else if (ctype == "Lose" && n == "Coupon") {
            return 39;
        } else if (ctype == "Talk" && n == "Dialog") {
            return 40;
        } else if (ctype == "Set" && n == "StepUp") {
            return 41;
        } else if (ctype == "Set" && n == "StepDown") {
            return 42;
        } else if (ctype == "Reverse" && n == "Flag") {
            return 43;
        } else if (ctype == "Branch" && n == "Step") {
            return 44;
        } else if (ctype == "Elapse" && n == "Time") {
            return 45;
        } else if (ctype == "Branch" && n == "Level") {
            return 46;
        } else if (ctype == "Branch" && n == "Status") {
            return 47;
        } else if (ctype == "Branch" && n == "PartyNumber") {
            return 48;
        } else if (ctype == "Show" && n == "Party") {
            return 49;
        } else if (ctype == "Hide" && n == "Party") {
            return 50;
        } else if (ctype == "Effect" && n == "Break") {
            return 51;
        } else if (ctype == "Call" && n == "Start") {
            return 52;
        } else if (ctype == "Link" && n == "Package") {
            return 53;
        } else if (ctype == "Call" && n == "Package") {
            return 54;
        } else if (ctype == "Branch" && n == "Area") {
            return 55;
        } else if (ctype == "Branch" && n == "Battle") {
            return 56;
        } else if (ctype == "Branch" && n == "CompleteStamp") {
            return 57;
        } else if (ctype == "Get" && n == "CompleteStamp") {
            return 58;
        } else if (ctype == "Lose" && n == "CompleteStamp") {
            return 59;
        } else if (ctype == "Branch" && n == "Gossip") {
            return 60;
        } else if (ctype == "Get" && n == "Gossip") {
            return 61;
        } else if (ctype == "Lose" && n == "Gossip") {
            return 62;
        } else if (ctype == "Branch" && n == "IsBattle") {
            return 63;
        } else if (ctype == "Redisplay" && n == "") {
            return 64;
        } else if (ctype == "Check" && n == "Flag") {
            return 65;
        } else if (ctype == "Substitute" && n == "Step") { // 1.30
            return 66;
        } else if (ctype == "Substitute" && n == "Flag") { // 1.30
            return 67;
        } else if (ctype == "Branch" && n == "StepValue") { // 1.30
            return 68;
        } else if (ctype == "Branch" && n == "FlagValue") { // 1.30
            return 69;
        } else if (ctype == "Branch" && n == "RandomSelect") { // 1.30
            return 70;
        } else if (ctype == "Branch" && n == "KeyCode") { // 1.50
            return 71;
        } else if (ctype == "Check" && n == "Step") { // 1.50
            return 72;
        } else if (ctype == "Branch" && n == "Round") { // 1.50
            return 73;
        } else {
            throw new ValueError(ctype + ", " + n);
        }
    }

//-------------------------------------------------------------------------------
// 適用メンバ・適用範囲
//-------------------------------------------------------------------------------

    public string conv_target_member(n) {
        // """引数の値から、「適用メンバ」の種類を返す。;
        // 0:Selected(現在選択中のメンバ), 1:Random(ランダムメンバ),;
        // 2:Party(現在選択中以外のメンバ);
        // 睡眠者有効ならば＋3で、返り値の文字列の後ろに"Sleep"を付ける。;
        // さらに6:Party(パーティの全員。効果コンテントの時に使う);
        // """;
        if (n == 0) {
            return "Selected";
        } else if (n == 1) {
            return "Random";
        } else if (n == 2) {
            return "Party";
        } else if (n == 3) {
            return "SelectedSleep";
        } else if (n == 4) {
            return "RandomSleep";
        } else if (n == 5) {
            return "PartySleep";
        } else if (n == 6) { // 存在するか不明だが残しておく
            return "Party";
        } else {
            throw new ValueError(this.fpath);
        }
    }

    public static int unconv_target_member(string n) {
        if (n == "Selected") {
            return 0;
        } else if (n == "Random") {
            return 1;
        } else if (n == "Unselected") { // 存在するか不明だが残しておく
            return 2;
        } else if (n == "SelectedSleep") {
            return 3;
        } else if (n == "RandomSleep") {
            return 4;
        } else if (n == "PartySleep") {
            return 5;
        } else if (n == "Party") {
            return 2;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();
        }
    }

    public UNK conv_target_member_dialog(n) {
        // """引数の値から、台詞コンテントの話者を返す。;
        // 0:Selected(現在選択中のメンバ), 1:Random(ランダムメンバ),;
        // 2:Unselected(現在選択中以外のメンバ);
        // 以降は1.50～;
        // 3:Valued(評価メンバ);
        // """;
        if n in (-1, 0): // 稀に-1になっている事がある
            return "Selected";
        } else if (n == 1) {
            return "Random";
        } else if (n == 2) {
            return "Unselected";
        } else if (n == 3) {
            return "Valued";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_target_member_dialog(n, f):
        if (n == "Selected") {
            return 0;
        } else if (n == "Random") {
            return 1;
        } else if (n == "Unselected") {
            return 2;
        } else if (n == "Valued") {
            f.check_version(1.50);
            return 3;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_target_scope(n) {
        """引数の値から、「適用範囲」の種類を返す。;
        0:Selected(現在選択中のメンバ), 1:Random(パーティの誰か一人),;
        2:Party(パーティの全員), 3:Backpack(荷物袋),;
        4:PartyAndBackpack(全体(荷物袋含む)) 5:Field(フィールド全体);
        """;
        if (n == 0) {
            return "Selected";
        } else if (n == 1) {
            return "Random";
        } else if (n == 2) {
            return "Party";
        } else if (n == 3) {
            return "Backpack";
        } else if (n == 4) {
            return "PartyAndBackpack";
        } else if (n == 5) {
            return "Field";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_target_scope(n):
        if (n == "Selected") {
            return 0;
        } else if (n == "Random") {
            return 1;
        } else if (n == "Party") {
            return 2;
        } else if (n == "Backpack") {
            return 3;
        } else if (n == "PartyAndBackpack") {
            return 4;
        } else if (n == "Field") {
            return 5;
        } else if (n in ("CouponHolder", "CardTarget")) {
            f.check_wsnversion("2");
            return 0;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_target_scope_coupon(n) {
        """引数の値から、「適用範囲」の種類を返す(1.30～のクーポン分岐)。;
        0:Selected(現在選択中のメンバ), 1:Random(パーティの誰か一人),;
        2:Party(パーティの全員), 3:Field(フィールド全体);
        """;
        if (n == 0) {
            return "Selected";
        } else if (n == 1) {
            return "Random";
        } else if (n == 2) {
            return "Party";
        } else if (n == 3) {
            return "Field";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_target_scope_coupon(n, f):
        if (n == "Selected") {
            return 0;
        } else if (n == "Random") {
            return 1;
        } else if (n == "Party") {
            return 2;
        } else if (n == "Field") {
            f.check_version(1.30);
            return 3;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_castranges(n) {
        """引数の値から、「適用メンバ」の種類をsetで返す(1.30)。;
        0b0001:パーティ;
        0b0010:エネミー;
        0b0100:同行NPC;
        """;
        s = set();
        if ((n & 0b0001) != 0) {
            s.add("Party");
        if ((n & 0b0010) != 0) {
            s.add("Enemy");
        if ((n & 0b0100) != 0) {
            s.add("Npc");
        return s;

    @staticmethod;
    def unconv_castranges(data):
        value = 0;
        foreach (var n in data) {
            if (n.text == "Party") {
                value |= 0b0001;
            } else if (n.text == "Enemy") {
                value |= 0b0010;
            } else if (n.text == "Npc") {
                value |= 0b0100;
            } else {
                throw new cw.binary.cwfile.UnsupportedError();
        return value;

    public UNK conv_keycoderange(n) {
        """引数の値から、「キーコード取得範囲」の種類を返す(1.50～のキーコード所持分岐)。;
        0:Selected(現在選択中のメンバ), 1:Random(パーティの誰か一人),;
        2:Backpack(荷物袋), 3:PartyAndBackpack(全体(荷物袋含む));
        """;
        if (n == 0) {
            return "Selected";
        } else if (n == 1) {
            return "Random";
        } else if (n == 2) {
            return "Backpack";
        } else if (n == 3) {
            return "PartyAndBackpack";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_keycoderange(n):
        if (n == "Selected") {
            return 0;
        } else if (n == "Random") {
            return 1;
        } else if (n == "Backpack") {
            return 2;
        } else if (n == "PartyAndBackpack") {
            return 3;
        } else if (n in ("CouponHolder", "CardTarget")) {
            f.check_wsnversion("2");
            return 0;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

//-------------------------------------------------------------------------------
// コンテント系
//-------------------------------------------------------------------------------

    public UNK conv_spreadtype(n) {
        """引数の値から、カードの並べ方を返す。""";
        if (n == 0) {
            return "Auto";
        } else if (n == 1) {
            return "Custom";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_spreadtype(n):
        if (n == "Auto") {
            return 0;
        } else if (n == "Custom") {
            return 1;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_statustype(n) {
        """引数の値から、状態を返す。;
        0:Active(行動可能), 1:Inactive(行動不可), 2:Alive(生存), 3:Dead(非生存),;
        4:Fine(健康), 5:Injured(負傷), 6:Heavy-Injured(重傷),;
        7:Unconscious(意識不明), 8:Poison(中毒), 9:Sleep(眠り),;
        10:Bind(呪縛), 11:Paralyze(麻痺・石化);
        以降は1.30～;
        12:Confuse(混乱), 13:Overheat(激昂), 14:Brave(勇敢), 15:Panic(恐慌);
        以降は1.50～;
        16:Silence(沈黙), 17:FaceUp(暴露), 18:AntiMagic(魔法無効化),;
        19:UpAction(行動力上昇), 20:UpAvoid(回避力上昇),;
        21:UpResist(抵抗力上昇), 22:UpDefense(防御力上昇),;
        23:DownAction(行動力低下), 24:DownAvoid(回避力低下),;
        25:DownResist(抵抗力低下), 26:DownDefense(防御力低下);
        """;
        if (n == 0) {
            return "Active";
        } else if (n == 1) {
            return "Inactive";
        } else if (n == 2) {
            return "Alive";
        } else if (n == 3) {
            return "Dead";
        } else if (n == 4) {
            return "Fine";
        } else if (n == 5) {
            return "Injured";
        } else if (n == 6) {
            return "HeavyInjured";
        } else if (n == 7) {
            return "Unconscious";
        } else if (n == 8) {
            return "Poison";
        } else if (n == 9) {
            return "Sleep";
        } else if (n == 10) {
            return "Bind";
        } else if (n == 11) {
            return "Paralyze";
        } else if (n == 12) {
            return "Confuse";
        } else if (n == 13) {
            return "Overheat";
        } else if (n == 14) {
            return "Brave";
        } else if (n == 15) {
            return "Panic";
        } else if (n == 16) {
            return "Silence";
        } else if (n == 17) {
            return "FaceUp";
        } else if (n == 18) {
            return "AntiMagic";
        } else if (n == 19) {
            return "UpAction";
        } else if (n == 20) {
            return "UpAvoid";
        } else if (n == 21) {
            return "UpResist";
        } else if (n == 22) {
            return "UpDefense";
        } else if (n == 23) {
            return "DownAction";
        } else if (n == 24) {
            return "DownAvoid";
        } else if (n == 25) {
            return "DownResist";
        } else if (n == 26) {
            return "DownDefense";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_statustype(n, f):
        if (n == "Active") {
            return 0;
        } else if (n == "Inactive") {
            return 1;
        } else if (n == "Alive") {
            return 2;
        } else if (n == "Dead") {
            return 3;
        } else if (n == "Fine") {
            return 4;
        } else if (n == "Injured") {
            return 5;
        } else if (n == "HeavyInjured") {
            return 6;
        } else if (n == "Unconscious") {
            return 7;
        } else if (n == "Poison") {
            return 8;
        } else if (n == "Sleep") {
            return 9;
        } else if (n == "Bind") {
            return 10;
        } else if (n == "Paralyze") {
            return 11;
        } else if (n == "Confuse") {
            f.check_version(1.30);
            return 12;
        } else if (n == "Overheat") {
            f.check_version(1.30);
            return 13;
        } else if (n == "Brave") {
            f.check_version(1.30);
            return 14;
        } else if (n == "Panic") {
            f.check_version(1.30);
            return 15;
        } else if (n == "Silence") {
            f.check_version(1.50);
            return 16;
        } else if (n == "FaceUp") {
            f.check_version(1.50);
            return 17;
        } else if (n == "AntiMagic") {
            f.check_version(1.50);
            return 18;
        } else if (n == "UpAction") {
            f.check_version(1.50);
            return 19;
        } else if (n == "UpAvoid") {
            f.check_version(1.50);
            return 20;
        } else if (n == "UpResist") {
            f.check_version(1.50);
            return 21;
        } else if (n == "UpDefense") {
            f.check_version(1.50);
            return 22;
        } else if (n == "DownAction") {
            f.check_version(1.50);
            return 23;
        } else if (n == "DownAvoid") {
            f.check_version(1.50);
            return 24;
        } else if (n == "DownResist") {
            f.check_version(1.50);
            return 25;
        } else if (n == "DownDefense") {
            f.check_version(1.50);
            return 26;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_effectcardtype(n) {
        """引数の値から、「カード種別」を返す(1.50～のキーコード所持分岐)。;
        0:All(全種類), 1:Skill(特殊技能), 2:Item(アイテム), 3:Beast(召喚獣);
        """;
        if (n == 0) {
            return "All";
        } else if (n == 1) {
            return "Skill";
        } else if (n == 2) {
            return "Item";
        } else if (n == 3) {
            return "Beast";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_effectcardtype(n):
        if (n == "All") {
            return 0;
        } else if (n == "Skill") {
            return 1;
        } else if (n == "Item") {
            return 2;
        } else if (n == "Beast") {
            return 3;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_comparison4(n) {
        """引数の値から、「4路選択条件」を返す(1.50～のステップ判定)。;
        0:=(条件値と一致), 1:<>(条件値と不一致),;
        2:<(条件値より大きい), 3:>(条件値より小さい);
        """;
        if (n == 0) {
            return "=";
        } else if (n == 1) {
            return "<>";
        } else if (n == 2) {
            return "<";
        } else if (n == 3) {
            return ">";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_comparison4(n):
        if (n == "=") {
            return 0;
        } else if (n == "<>") {
            return 1;
        } else if (n == "<") {
            return 2;
        } else if (n == ">") {
            return 3;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_comparison3(n) {
        """引数の値から、「3路選択条件」を返す(1.50～のラウンド判定)。;
        0:=(条件値と一致), 1:<(条件値より大きい), 2:>(条件値より小さい);
        """;
        if (n == 0) {
            return "=";
        } else if (n == 1) {
            return "<";
        } else if (n == 2) {
            return ">";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_comparison3(n):
        if (n == "=") {
            return 0;
        } else if (n == "<") {
            return 1;
        } else if (n == ">") {
            return 2;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

//-------------------------------------------------------------------------------
// 効果モーション関連
//-------------------------------------------------------------------------------

    public UNK conv_effectmotion_element(n) {
        """引数の値から、効果モーションの「属性」を返す。;
        0:All(全), 1:Health(肉体), 2:Mind(精神), 3:Miracle(神聖),;
        4:Magic(魔力), 5:Fire(炎), 6:Ice(冷);
        """;
        if (n == 0) {
            return "All";
        } else if (n == 1) {
            return "Health";
        } else if (n == 2) {
            return "Mind";
        } else if (n == 3) {
            return "Miracle";
        } else if (n == 4) {
            return "Magic";
        } else if (n == 5) {
            return "Fire";
        } else if (n == 6) {
            return "Ice";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_effectmotion_element(n):
        if (n == "All") {
            return 0;
        } else if (n == "Health") {
            return 1;
        } else if (n == "Mind") {
            return 2;
        } else if (n == "Miracle") {
            return 3;
        } else if (n == "Magic") {
            return 4;
        } else if (n == "Fire") {
            return 5;
        } else if (n == "Ice") {
            return 6;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_effectmotion_type(tabn, n) {
        """引数の値から、効果モーションの「種類」を返す。;
        tabn: 大分類。;
        n: 小分類。;
        """;
        if (tabn == 0) {
            if (n == 0) {
                return "Heal"                     // 回復
            } else if (n == 1) {
                return "Damage"                   // ダメージ
            } else if (n == 2) {
                return "Absorb"                   // 吸収
            } else {
                throw new ValueError(this.fpath);

        } else if (tabn == 1) {
            if (n == 0) {
                return "Paralyze"                 // 麻痺状態
            } else if (n == 1) {
                return "DisParalyze"              // 麻痺解除
            } else if (n == 2) {
                return "Poison"                   // 中毒状態
            } else if (n == 3) {
                return "DisPoison"                // 中毒解除
            } else {
                throw new ValueError(this.fpath);

        } else if (tabn == 2) {
            if (n == 0) {
                return "GetSkillPower"            // 精神力回復
            } else if (n == 1) {
                return "LoseSkillPower"           // 精神力不能
            } else {
                throw new ValueError(this.fpath);

        } else if (tabn == 3) {
            if (n == 0) {
                return "Sleep"                    // 睡眠状態
            } else if (n == 1) {
                return "Confuse"                  // 混乱状態
            } else if (n == 2) {
                return "Overheat"                 // 激昂状態
            } else if (n == 3) {
                return "Brave"                    // 勇敢状態
            } else if (n == 4) {
                return "Panic"                    // 恐慌状態
            } else if (n == 5) {
                return "Normal"                   // 正常状態
            } else {
                throw new ValueError(this.fpath);

        } else if (tabn == 4) {
            if (n == 0) {
                return "Bind"                     // 束縛状態
            } else if (n == 1) {
                return "DisBind"                  // 束縛解除
            } else if (n == 2) {
                return "Silence"                  // 沈黙状態
            } else if (n == 3) {
                return "DisSilence"               // 沈黙解除
            } else if (n == 4) {
                return "FaceUp"                   // 暴露状態
            } else if (n == 5) {
                return "FaceDown"                 // 暴露解除
            } else if (n == 6) {
                return "AntiMagic"                // 魔法無効化状態
            } else if (n == 7) {
                return "DisAntiMagic"             // 魔法無効化解除
            } else {
                throw new ValueError(this.fpath);

        } else if (tabn == 5) {
            if (n == 0) {
                return "EnhanceAction"            // 行動力変化
            } else if (n == 1) {
                return "EnhanceAvoid"             // 回避力変化
            } else if (n == 2) {
                return "EnhanceResist"            // 抵抗力変化
            } else if (n == 3) {
                return "EnhanceDefense"           // 防御力変化
            } else {
                throw new ValueError(this.fpath);

        } else if (tabn == 6) {
            if (n == 0) {
                return "VanishTarget"             // 対象消去
            } else if (n == 1) {
                return "VanishCard"               // カード消去
            } else if (n == 2) {
                return "VanishBeast"              // 召喚獣消去
            } else {
                throw new ValueError(this.fpath);

        } else if (tabn == 7) {
            if (n == 0) {
                return "DealAttackCard"           // 通常攻撃
            } else if (n == 1) {
                return "DealPowerfulAttackCard"   // 渾身の一撃
            } else if (n == 2) {
                return "DealCriticalAttackCard"   // 会心の一撃
            } else if (n == 3) {
                return "DealFeintCard"            // フェイント
            } else if (n == 4) {
                return "DealDefenseCard"          // 防御
            } else if (n == 5) {
                return "DealDistanceCard"         // 見切り
            } else if (n == 6) {
                return "DealConfuseCard"          // 混乱
            } else if (n == 7) {
                return "DealSkillCard"            // 特殊技能
            } else if (n == 8) {
                return "CancelAction"            // 行動キャンセル(1.50)
            } else {
                throw new ValueError(this.fpath);

        } else if (tabn == 8) {
            if (n == 0) {
                return "SummonBeast"              // 召喚獣召喚
            } else {
                throw new ValueError(this.fpath);

        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_effectmotion_type(n, f):
        if (n == "Heal") {
            return 0, 0;
        } else if (n == "Damage") {
            return 0, 1;
        } else if (n == "Absorb") {
            return 0, 2;

        } else if (n == "Paralyze") {
            return 1, 0;
        } else if (n == "DisParalyze") {
            return 1, 1;
        } else if (n == "Poison") {
            return 1, 2;
        } else if (n == "DisPoison") {
            return 1, 3;

        } else if (n == "GetSkillPower") {
            return 2, 0;
        } else if (n == "LoseSkillPower") {
            return 2, 1;

        } else if (n == "Sleep") {
            return 3, 0;
        } else if (n == "Confuse") {
            return 3, 1;
        } else if (n == "Overheat") {
            return 3, 2;
        } else if (n == "Brave") {
            return 3, 3;
        } else if (n == "Panic") {
            return 3, 4;
        } else if (n == "Normal") {
            return 3, 5;

        } else if (n == "Bind") {
            return 4, 0;
        } else if (n == "DisBind") {
            return 4, 1;
        } else if (n == "Silence") {
            return 4, 2;
        } else if (n == "DisSilence") {
            return 4, 3;
        } else if (n == "FaceUp") {
            return 4, 4;
        } else if (n == "FaceDown") {
            return 4, 5;
        } else if (n == "AntiMagic") {
            return 4, 6;
        } else if (n == "DisAntiMagic") {
            return 4, 7;

        } else if (n == "EnhanceAction") {
            return 5, 0;
        } else if (n == "EnhanceAvoid") {
            return 5, 1;
        } else if (n == "EnhanceResist") {
            return 5, 2;
        } else if (n == "EnhanceDefense") {
            return 5, 3;

        } else if (n == "VanishTarget") {
            return 6, 0;
        } else if (n == "VanishCard") {
            return 6, 1;
        } else if (n == "VanishBeast") {
            return 6, 2;

        } else if (n == "DealAttackCard") {
            return 7, 0;
        } else if (n == "DealPowerfulAttackCard") {
            return 7, 1;
        } else if (n == "DealCriticalAttackCard") {
            return 7, 2;
        } else if (n == "DealFeintCard") {
            return 7, 3;
        } else if (n == "DealDefenseCard") {
            return 7, 4;
        } else if (n == "DealDistanceCard") {
            return 7, 5;
        } else if (n == "DealConfuseCard") {
            return 7, 6;
        } else if (n == "DealSkillCard") {
            return 7, 7;
        elif n == "CancelAction": // 1.50
            f.check_version(1.50);
            return 7, 8;

        } else if (n == "SummonBeast") {
            return 8, 0;

        } else if (n == "NoEffect") {
            f.check_wsnversion("2");
            return 0, 1;

        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_effectmotion_damagetype(n) {
        """引数の値から、効果モーションの「属性」を返す。;
        0:levelratio(レベル比), 1:normal(効果値), 2:max(最大値);
        """;
        if (n == 0) {
            return "LevelRatio";
        } else if (n == 1) {
            return "Normal";
        } else if (n == 2) {
            return "Max";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_effectmotion_damagetype(n):
        if (n == "LevelRatio") {
            return 0;
        } else if (n == "Normal") {
            return 1;
        } else if (n == "Max") {
            return 2;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

//-------------------------------------------------------------------------------
// スキル・アイテム・召喚獣関連
//-------------------------------------------------------------------------------

    public UNK conv_card_effecttype(n) {
        """引数の値から、「効果属性」の種類を返す。;
        0:Physic(物理属性), 1:Magic(魔法属性), 2:MagicalPhysic(魔法的物理属性),;
        3:PhysicalMagic(物理的魔法属性), 4:null(無属性);
        """;
        if (n == 0) {
            return "Physic";
        } else if (n == 1) {
            return "Magic";
        } else if (n == 2) {
            return "MagicalPhysic";
        } else if (n == 3) {
            return "PhysicalMagic";
        } else if (n == 4) {
            return "None";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_card_effecttype(n):
        if (n == "Physic") {
            return 0;
        } else if (n == "Magic") {
            return 1;
        } else if (n == "MagicalPhysic") {
            return 2;
        } else if (n == "PhysicalMagic") {
            return 3;
        } else if (n == "None") {
            return 4;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_card_resisttype(n) {
        """引数の値から、「抵抗属性」の種類を返す。;
        0:Avoid(物理属性), 1:Resist(抵抗属性), 3:Unfail(必中属性);
        """;
        if (n == 0) {
            return "Avoid";
        } else if (n == 1) {
            return "Resist";
        } else if (n == 2) {
            return "Unfail";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_card_resisttype(n):
        if (n == "Avoid") {
            return 0;
        } else if (n == "Resist") {
            return 1;
        } else if (n == "Unfail") {
            return 2;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_card_visualeffect(n) {
        """引数の値から、「視覚的効果」の種類を返す。;
        0:null(無し), 1:Reverse(反転),;
        2:Horizontal(横), 3:Vertical(縦);
        """;
        if (n == 0) {
            return "None";
        } else if (n == 1) {
            return "Reverse";
        } else if (n == 2) {
            return "Horizontal";
        } else if (n == 3) {
            return "Vertical";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_card_visualeffect(n):
        if (n == "None") {
            return 0;
        } else if (n == "Reverse") {
            return 1;
        } else if (n == "Horizontal") {
            return 2;
        } else if (n == "Vertical") {
            return 3;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_card_physicalability(n) {
        """引数の値から、身体的要素の種類を返す。;
        0:Dex(器用), 1:Agl(素早さ), 2:Int(知力);
        3:Str(筋力), 4:Vit(生命), 5:Min(精神);
        """;
        if (n == 0) {
            return "Dex";
        } else if (n == 1) {
            return "Agl";
        } else if (n == 2) {
            return "Int";
        } else if (n == 3) {
            return "Str";
        } else if (n == 4) {
            return "Vit";
        } else if (n == 5) {
            return "Min";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_card_physicalability(n):
        if (n == "Dex") {
            return 0;
        } else if (n == "Agl") {
            return 1;
        } else if (n == "Int") {
            return 2;
        } else if (n == "Str") {
            return 3;
        } else if (n == "Vit") {
            return 4;
        } else if (n == "Min") {
            return 5;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_card_mentalability(n) {
        """引数の値から、精神的要素の種類を返す。;
        1:Aggressive(好戦), -1:Unaggressive(平和), 2:Cheerful(社交),;
        -2:Uncheerful(内向), 3:Brave(勇敢), -3:Unbrave(臆病), 4:Cautious(慎重),;
        -4:Uncautious(大胆), 5:Trickish(狡猾), -5:Untrickish(正直);
        """;
        if (n == 1) {
            return "Aggressive";
        } else if (n == -1) {
            return "Unaggressive";
        } else if (n == 2) {
            return "Cheerful";
        } else if (n == -2) {
            return "Uncheerful";
        } else if (n == 3) {
            return "Brave";
        } else if (n == -3) {
            return "Unbrave";
        } else if (n == 4) {
            return "Cautious";
        } else if (n == -4) {
            return "Uncautious";
        } else if (n == 5) {
            return "Trickish";
        } else if (n == -5) {
            return "Untrickish";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_card_mentalability(n):
        if (n == "Aggressive") {
            return 1;
        } else if (n == "Unaggressive") {
            return -1;
        } else if (n == "Cheerful") {
            return 2;
        } else if (n == "Uncheerful") {
            return -2;
        } else if (n == "Brave") {
            return 3;
        } else if (n == "Unbrave") {
            return -3;
        } else if (n == "Cautious") {
            return 4;
        } else if (n == "Uncautious") {
            return -4;
        } else if (n == "Trickish") {
            return 5;
        } else if (n == "Untrickish") {
            return -5;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_card_target(n) {
        """引数の値から、効果目標の種類を返す。;
        0:null(対象無し), 1:User(使用者), 2:Party(味方),;
        3:Enemy(敵方) ,4:Both(双方);
        """;
        if (n == 0) {
            return "None";
        } else if (n == 1) {
            return "User";
        } else if (n == 2) {
            return "Party";
        } else if (n == 3) {
            return "Enemy";
        } else if (n == 4) {
            return "Both";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_card_target(n):
        if (n == "None") {
            return 0;
        } else if (n == "User") {
            return 1;
        } else if (n == "Party") {
            return 2;
        } else if (n == "Enemy") {
            return 3;
        } else if (n == "Both") {
            return 4;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_card_premium(n) {
        """引数の値から、希少度の種類を返す。;
        一時的に所持しているだけのF9でなくなるカードの場合は+3されている。;
        0:Normal, 2:Rare, 1:Premium;
        """;
        if (n == 0) {
            return "Normal";
        } else if (n == 1) {
            return "Rare";
        } else if (n == 2) {
            return "Premium";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_card_premium(n):
        if (n == "Normal") {
            return 0;
        } else if (n == "Rare") {
            return 1;
        } else if (n == "Premium") {
            return 2;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

//-------------------------------------------------------------------------------
//　キャラクター関連
//-------------------------------------------------------------------------------

    public UNK conv_mentality(n) {
        """引数の値から、精神状態の種類を返す。;
        ここでは「"0"=正常状態」以外の判別は適当。;
        """;
        if (n == 0) {
            return "Normal"            // 正常状態
        } else if (n == 1) {
            return "Sleep"             // 睡眠状態
        } else if (n == 2) {
            return "Confuse"           // 混乱状態
        } else if (n == 3) {
            return "Overheat"          // 激昂状態
        } else if (n == 4) {
            return "Brave"             // 勇敢状態
        } else if (n == 5) {
            return "Panic"             // 恐慌状態
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_mentality(n):
        if (n == "Normal") {
            return 0;
        } else if (n == "Sleep") {
            return 1;
        } else if (n == "Confuse") {
            return 2;
        } else if (n == "Overheat") {
            return 3;
        } else if (n == "Brave") {
            return 4;
        } else if (n == "Panic") {
            return 5;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

//-------------------------------------------------------------------------------
//　宿データ関連
//-------------------------------------------------------------------------------

    public UNK conv_yadotype(n) {
        """引数の値から、宿の種類を返す。;
        1:Normal(ノーマル宿), 2:Debug(デバッグ宿);
        """;
        if (n == 1) {
            return "Normal";
        } else if (n == 2) {
            return "Debug";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_yadotype(n):
        if (n == "Normal") {
            return 1;
        } else if (n == "Debug") {
            return 2;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_yado_summaryview(n) {
        """引数の値から、張り紙の表示の種類を返す。;
        0:隠蔽シナリオ、終了済シナリオを表示しない, 1:隠蔽シナリオを表示しない,;
        2:全てのシナリオを表示, 3:適応レベルのシナリオのみを表示;
        """;
        if (n == 0) {
            return "HideHiddenAndCompleteScenario";
        } else if (n == 1) {
            return "HideHiddenScenario";
        } else if (n == 2) {
            return "ShowAll";
        } else if (n == 3) {
            return "ShowFittingScenario";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_yado_summaryview(n):
        if (n == "HideHiddenAndCompleteScenario") {
            return 0;
        } else if (n == "HideHiddenScenario") {
            return 1;
        } else if (n == "ShowAll") {
            return 2;
        } else if (n == "ShowFittingScenario") {
            return 3;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_yado_bgchange(n) {
        """引数の値から、背景の切り替え方式の種類を返す。;
        0:アニメーションなし, 1:短冊式,;
        2:色変換式, 3:ドット置換式;
        """;
        if (n == 0) {
            return "NoAnimation";
        } else if (n == 1) {
            return "ReedShape";
        } else if (n == 2) {
            return "ColorShade";
        } else if (n == 3) {
            return "ReplaceDot";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_yado_bgchange(n):
        if (n == "NoAnimation") {
            return 0;
        } else if (n == "ReedShape") {
            return 1;
        } else if (n == "ColorShade") {
            return 2;
        } else if (n == "ReplaceDot") {
            return 3;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

//-------------------------------------------------------------------------------
//　特殊セル関連(1.50～)
//-------------------------------------------------------------------------------

    public UNK conv_borderingtype(n) {
        """引数の値から、テキストセルの縁取り方式を返す。;
        0:縁取り形式1, 1:縁取り形式2;
        """;
        if (n == 0) {
            return "Outline";
        } else if (n == 1) {
            return "Inline";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_borderingtype(n):
        if (n == "Outline") {
            return 0;
        } else if (n == "Inline") {
            return 1;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_blendmode(n) {
        """引数の値から、カラーセルの合成方法を返す。;
        0,1:上書き, 2:加算, 3:減算, 4:乗算;
        """;
        if (n in (0, 1)) {
            return "Normal";
        } else if (n == 2) {
            return "Add";
        } else if (n == 3) {
            return "Subtract";
        } else if (n == 4) {
            return "Multiply";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_blendmode(n):
        if (n == "Normal") {
            return 0;
        } else if (n == "Add") {
            return 2;
        } else if (n == "Subtract") {
            return 3;
        } else if (n == "Multiply") {
            return 4;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

    public UNK conv_gradientdir(n) {
        """引数の値から、グラデーション方向を返す。;
        0: グラデーション無し, 1:左から右, 2: 上から下;
        """;
        if (n == 0) {
            return "None";
        } else if (n == 1) {
            return "LeftToRight";
        } else if (n == 2) {
            return "TopToBottom";
        } else {
            throw new ValueError(this.fpath);

    @staticmethod;
    def unconv_gradientdir(n):
        if (n == "None") {
            return 0;
        } else if (n == "LeftToRight") {
            return 1;
        } else if (n == "TopToBottom") {
            return 2;
        } else {
            throw new cw.binary.cwfile.UnsupportedError();

def main():
    pass;

if __name__ == "__main__":
    main();

