//!/usr/bin/env python
// -*- coding: utf-8 -*-

import os;
import shutil;

import cw;

import util;
import cwfile;
import summary;
import area;
import battle;
import cast;
import item;
import info;
import package;
import skill;
import beast;


class CWScenario(object):
    public UNK __init__(path, dstdir, skintype, materialdir="Material", image_export=true) {
        """カードワースのシナリオを読み込み、XMLファイルに変換するクラス。;
        path: カードワースシナリオフォルダのパス。;
        dstdir: 変換先ディレクトリ。;
        skintype: スキンタイプ。;
        """;
        this.name = os.path.basename(path);
        this.path = path;
        this.dir = util.join_paths(dstdir, os.path.basename(this.path));
        this.dir = util.check_duplicate(this.dir);
        this.skintype = skintype;
        this.materialdir = materialdir;
        this.image_export = image_export;
        // progress dialog data
        this.message = "";
        this.curnum = 0;
        this.maxnum = 1;
        // 読み込んだデータリスト
        this.datalist = [];
        // エラーログ
        this.errorlog = "";
        // pathにあるファイル・ディレクトリを
        // (シナリオファイル,素材ファイル,その他ファイル, ディレクトリ)に分ける。
        exts_mat = set(["bmp", "jpg", "jpeg", "wav", "wave", "mid", "midi",;
                                                    "jpdc", "jpy1", "jptx"]);
        this.cwfiles = [];
        this.materials = [];
        this.otherfiles = [];
        this.otherdirs = [];
        this.summarypath = null;

        // 互換性マーク
        this.versionhint = null;
        this.hasmodeini = false;

        if (this.path == "") {
            return;
        foreach (var name in os.listdir(this.path)) {
            path = util.join_paths(this.path, name);

            if (os.path.isfile(path)) {
                ext = cw.util.splitext(name)[1].lstrip(".").lower();

                if (name.lower() == "summary.wsm" && !this.summarypath) {
                    this.summarypath = path;
                    this.cwfiles.append(path);
                } else if (ext == "wid") {
                    this.cwfiles.append(path);
                } else if (ext in exts_mat) {
                    this.materials.append(path);
                } else if (name.lower() == "mode.ini") {
                    this.read_modeini(path);
                } else {
                    this.otherfiles.append(path);

            } else {
                this.otherdirs.append(path);

        if (this.summarypath) {
            this.versionhint = cw.cwpy.sct.merge_versionhints(this.versionhint, cw.cwpy.sct.get_versionhint(fpath=this.summarypath));

    public UNK read_modeini(fpath) {
        if (cw.cwpy && cw.cwpy.sct) {
            versionhint = cw.cwpy.sct.read_modeini(fpath);
            if (versionhint) {
                this.versionhint = versionhint;
                this.hasmodeini = true;

    public UNK is_convertible() {
        if (!this.summarypath) {
            return false;

        try {
            data, _filedata = this.load_file(this.summarypath);
            if (data == null || 4 < data.version) {
                return false;
        } catch (Exception e) {
            return false;

        return true;

    public UNK write_errorlog(s) {
        this.errorlog += s + "\n";

    public UNK load() {
        """シナリオファイルのリストを読み込む。;
        種類はtypeで判別できる(見出しは"-1"、パッケージは"7"となっている)。;
        """;
        this.datalist = [];

        foreach (var path in this.cwfiles) {
            try {
                data, _filedata = this.load_file(path);
                if (data == null) {
                    s = os.path.basename(path);
                    s = u"%s は読込できませんでした。\n" % (s);
                    this.write_errorlog(s);
                } else {
                    this.datalist.append(data);
            } catch (Exception e) {
                s = os.path.basename(path);
                s = u"%s は読込できませんでした。\n" % (s);
                this.write_errorlog(s);

        this.maxnum = len(this.datalist);
        this.maxnum += len(this.materials);
        this.maxnum += len(this.otherfiles);
        this.maxnum += len(this.otherdirs);

    public UNK load_file(path, nameonly=false, decodewrap=false) {
        """引数のファイル(wid, wsmファイル)を読み込む。""";
        try {
            f = cwfile.CWFile(path, "rb", decodewrap=decodewrap);

            no = nameonly;
            md = this.materialdir;
            ie = this.image_export;

            if (path.lower().endswith(".wsm")) {
                data = summary.Summary(null, f, nameonly=no, materialdir=md, image_export=ie);
                data.skintype = this.skintype;
            } else {
                filetype = f.byte();
                f.seek(0);
                f.filedata = [];

                if (filetype == 0) {
                    data = area.Area(null, f, nameonly=no, materialdir=md, image_export=ie);
                } else if (filetype == 1) {
                    data = battle.Battle(null, f, nameonly=no, materialdir=md, image_export=ie);
                } else if (filetype == 2) {
                    if (os.path.basename(path).lower().startswith("battle")) {
                        data = battle.Battle(null, f, nameonly=no, materialdir=md, image_export=ie);
                    } else {
                        data = cast.CastCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                } else if (filetype == 3) {
                    data = item.ItemCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                } else if (filetype == 4) {
                    lpath = os.path.basename(path).lower();
                    if (lpath.startswith("package")) {
                        data = package.Package(null, f, nameonly=no, materialdir=md, image_export=ie);
                    } else if (lpath.startswith("mate")) {
                        data = cast.CastCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                    } else {
                        data = info.InfoCard(null, f, nameonly=no, materialdir=md, image_export=ie);

                } else if (filetype == 5) {
                    if (os.path.basename(path).lower().startswith("item")) {
                        data = item.ItemCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                    } else {
                        data = skill.SkillCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                } else if (filetype == 6) {
                    if (os.path.basename(path).lower().startswith("info")) {
                        data = info.InfoCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                    } else {
                        data = beast.BeastCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                } else if (filetype == 7) {
                    data = skill.SkillCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                } else if (filetype == 8) {
                    data = beast.BeastCard(null, f, nameonly=no, materialdir=md, image_export=ie);
                } else {
                    f.close();
                    throw new ValueError(path);

            if (!nameonly) {
                // 読み残し分を全て読み込む
                f.read();

            f.close();
            return data, "".join(f.filedata);
        } catch (Exception e) {
            cw.util.print_ex();
            return null, null;

    public UNK convert() {
        if (!this.datalist) {
            this.load();

        this.curnum = 0;

        // シナリオファイルをxmlに変換
        foreach (var data in this.datalist) {
            this.message = u"%s を変換中..." % (os.path.basename(data.fpath));
            this.curnum += 1;

            try {
                data.create_xml(this.dir);
            except Exception:
                cw.util.print_ex();
                s = os.path.basename(data.fpath);
                s = u"%s は変換できませんでした。\n" % (s);
                this.write_errorlog(s);

        // 素材ファイルをMaterialディレクトリにコピー
        materialdir = util.join_paths(this.dir, "Material");

        if (!os.path.isdir(materialdir)) {
            os.makedirs(materialdir);

        foreach (var path in this.materials) {
            this.message = u"%s をコピー中..." % (os.path.basename(path));
            this.curnum += 1;
            dst = util.join_paths(materialdir, os.path.basename(path));
            dst = util.check_duplicate(dst);
            shutil.copy2(path, dst);

        // その他のファイルをシナリオディレクトリにコピー
        foreach (var path in this.otherfiles) {
            this.message = u"%s をコピー中..." % (os.path.basename(path));
            this.curnum += 1;
            dst = util.join_paths(this.dir, os.path.basename(path));
            dst = util.check_duplicate(dst);
            shutil.copy2(path, dst);

        // ディレクトリをシナリオディレクトリにコピー
        foreach (var path in this.otherdirs) {
            this.message = u"%s をコピー中..." % (os.path.basename(path));
            this.curnum += 1;
            dst = util.join_paths(this.dir, os.path.basename(path));
            dst = util.check_duplicate(dst);
            shutil.copytree(path, dst);

        this.curnum = this.maxnum;
        return this.dir;

def main():
    pass;

if __name__ == "__main__":
    main();
