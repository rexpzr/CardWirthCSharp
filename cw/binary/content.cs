//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;
import bgimage;
import dialog;
import effectmotion;

import cw;


class Content(base.CWBinaryBase):
    public UNK __init__(parent, f, stratum) {
        base.CWBinaryBase.__init__(self, parent, f);
        this.xmltype = "Content";

        this.children = [];

        if (f == null) {
            return;

        eventstack = [];
        children = [];

        if (255 < stratum) {
            // ある程度以上ツリー階層が深くなったら
            // 再帰を回避する方向に切り替える
            while true:
                tag, ctype = this.conv_contenttype(f.byte());
                name = f.string();
                children_num = f.dword();
                if (children_num <= 39999) {
                    version = 2;
                } else if (children_num <= 49999) {
                    version = 4;
                    children_num -= 40000;
                } else {
                    version = 5;
                    children_num -= 50000;

                eventstack.append((tag, ctype, name, version));

                if (children_num == 0) {
                    break;
                } else if (children_num == 1) {
                    // 子コンテントが1件だけの時は情報をスタックにためて再帰回避
                    continue;
                } else {
                    // stratumはすでにmaxであるため加算しない
                    children = [Content(self, f, stratum) for _cnt in xrange(children_num)];
                    break;

            foreach (var i, (tag, ctype, name, version) in enumerate(reversed(eventstack))) {
                if (i+1 == len(eventstack)) {
                    e = self;
                } else {
                    e = Content(self, null, stratum);
                e._read_properties(f, tag, ctype, name, version);
                foreach (var child in children) {
                    e.children.append(child);
                children = [e];
        } else {
            tag, ctype = this.conv_contenttype(f.byte());
            name = f.string();
            children_num = f.dword();
            if (children_num <= 39999) {
                version = 2;
            } else if (children_num <= 49999) {
                version = 4;
                children_num -= 40000;
            } else {
                version = 5;
                children_num -= 50000;

            this.children = [Content(self, f, stratum+1) for _cnt in xrange(children_num)];

            this._read_properties(f, tag, ctype, name, version);

    public UNK _read_properties(f, tag, ctype, name, version) {
        this.tag = tag;
        this.type = ctype;
        this.name = name;
        this.version = version;

        // 宿データの埋め込みカードのコンテントは
        // 子コンテントデータの後ろに"dword()"(4)が埋め込まれている。
        if (5 <= this.version) {
            f.dword();

        this.properties = {};

        if (this.tag == "Start" && this.type == "") {
            pass;
        } else if (this.tag == "Link" && this.type == "Start") {
            this.properties["link"] = f.string();
        } else if (this.tag == "Start" && this.type == "Battle") {
            this.properties["id"] = f.dword();
        } else if (this.tag == "End" && this.type == "") {
            this.properties["complete"] = f.bool();
        } else if (this.tag == "End" && this.type == "BadEnd") {
            pass;
        } else if (this.tag == "Change" && this.type == "Area") {
            this.properties["id"] = f.dword();
        } else if (this.tag == "Talk" && this.type == "Message") {
            this.properties["path"] = this.get_materialpath(f.string());
            this.text = f.string(true);
        } else if (this.tag == "Play" && this.type == "Bgm") {
            this.properties["path"] = this.get_materialpath(f.string());
        } else if (this.tag == "Change" && this.type == "BgImage") {
            bgimgs_num = f.dword();
            this.bgimgs = [bgimage.BgImage(self, f) for _cnt in xrange(bgimgs_num)];
        } else if (this.tag == "Play" && this.type == "Sound") {
            this.properties["path"] = this.get_materialpath(f.string());
        } else if (this.tag == "Wait" && this.type == "") {
            this.properties["value"] = f.dword();
        } else if (this.tag == "Effect" && this.type == "") {
            this.properties["level"] = f.dword();
            targetm = f.byte();
            this.properties["targetm"] = this.conv_target_member(targetm);
            this.properties["effecttype"] = this.conv_card_effecttype(f.byte());
            this.properties["resisttype"] = this.conv_card_resisttype(f.byte());
            this.properties["successrate"] = f.dword();
            this.properties["sound"] = this.get_materialpath(f.string());
            this.properties["visual"] = this.conv_card_visualeffect(f.byte());
            this.properties["ignite"] = false;
            motions_num = f.dword();
            this.motions = [effectmotion.EffectMotion(self, f, dataversion=this.version);
                                            for _cnt in xrange(motions_num)];
        } else if (this.tag == "Branch" && this.type == "Select") {
            this.properties["targetall"] = f.bool();
            if (f.bool()) {
                this.properties["method"] = "Random";
            } else {
                this.properties["method"] = "Manual";
        } else if (this.tag == "Branch" && this.type == "Ability") {
            this.properties["value"] = f.dword();
            targetm = f.byte();
            this.properties["targetm"] = this.conv_target_member(targetm);
            this.properties["physical"] = this.conv_card_physicalability(f.dword());
            this.properties["mental"] = this.conv_card_mentalability(f.dword());
        } else if (this.tag == "Branch" && this.type == "Random") {
            this.properties["value"] = f.dword();
        } else if (this.tag == "Branch" && this.type == "Flag") {
            this.properties["flag"] = f.string();
        } else if (this.tag == "Set" && this.type == "Flag") {
            this.properties["flag"] = f.string();
            this.properties["value"] = f.bool();
        } else if (this.tag == "Branch" && this.type == "MultiStep") {
            this.properties["step"] = f.string();
        } else if (this.tag == "Set" && this.type == "Step") {
            this.properties["step"] = f.string();
            this.properties["value"] = f.dword();
        } else if (this.tag == "Branch" && this.type == "Cast") {
            this.properties["id"] = f.dword();
        } else if (this.tag == "Branch" && this.type == "Item") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Branch" && this.type == "Skill") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Branch" && this.type == "Info") {
            this.properties["id"] = f.dword();
        } else if (this.tag == "Branch" && this.type == "Beast") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Branch" && this.type == "Money") {
            this.properties["value"] = f.dword();
        } else if (this.tag == "Branch" && this.type == "Coupon") {
            this.properties["coupon"] = f.string();
            f.dword() // 得点(不使用)
            this.properties["targets"] = this.conv_target_scope_coupon(f.byte());
        } else if (this.tag == "Get" && this.type == "Cast") {
            this.properties["id"] = f.dword();
            this.properties["startaction"] = "NextRound";
        } else if (this.tag == "Get" && this.type == "Item") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Get" && this.type == "Skill") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Get" && this.type == "Info") {
            this.properties["id"] = f.dword();
        } else if (this.tag == "Get" && this.type == "Beast") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Get" && this.type == "Money") {
            this.properties["value"] = f.dword();
        } else if (this.tag == "Get" && this.type == "Coupon") {
            this.properties["coupon"] = f.string();
            this.properties["value"] = f.dword();
            this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Lose" && this.type == "Cast") {
            this.properties["id"] = f.dword();
        } else if (this.tag == "Lose" && this.type == "Item") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Lose" && this.type == "Skill") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Lose" && this.type == "Info") {
            this.properties["id"] = f.dword();
        } else if (this.tag == "Lose" && this.type == "Beast") {
            this.properties["id"] = f.dword();
            if (this.version <= 2) {
                this.properties["number"] = 1;
                this.properties["targets"] = this.conv_target_scope(4);
            } else {
                this.properties["number"] = f.dword();
                this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Lose" && this.type == "Money") {
            this.properties["value"] = f.dword();
        } else if (this.tag == "Lose" && this.type == "Coupon") {
            this.properties["coupon"] = f.string();
            f.dword() // 得点(不使用)
            this.properties["targets"] = this.conv_target_scope(f.byte());
        } else if (this.tag == "Talk" && this.type == "Dialog") {
            member = this.conv_target_member_dialog(f.byte());
            this.properties["targetm"] = member;
            if (member == "Valued") {
                coupons_num = f.dword();
                this.coupons = [cw.binary.coupon.Coupon(self, f) for _cnt in xrange(coupons_num)];
                if (this.coupons && this.coupons[0].name == "") {
                    this.properties["initialValue"] = this.coupons[0].value;
                    this.coupons = this.coupons[1:];
                } else {
                    this.properties["initialValue"] = 0;
            dialogs_num = f.dword();
            this.dialogs = [cw.binary.dialog.Dialog(self, f) for _cnt in xrange(dialogs_num)];
        } else if (this.tag == "Set" && this.type == "StepUp") {
            this.properties["step"] = f.string();
        } else if (this.tag == "Set" && this.type == "StepDown") {
            this.properties["step"] = f.string();
        } else if (this.tag == "Reverse" && this.type == "Flag") {
            this.properties["flag"] = f.string();
        } else if (this.tag == "Branch" && this.type == "Step") {
            this.properties["step"] = f.string();
            this.properties["value"] = f.dword();
        } else if (this.tag == "Elapse" && this.type == "Time") {
            pass;
        } else if (this.tag == "Branch" && this.type == "Level") {
            this.properties["average"] = f.bool();
            this.properties["value"] = f.dword();
        } else if (this.tag == "Branch" && this.type == "Status") {
            this.properties["status"] = this.conv_statustype(f.byte());
            targetm = f.byte();
            this.properties["targetm"] = this.conv_target_member(targetm);
        } else if (this.tag == "Branch" && this.type == "PartyNumber") {
            this.properties["value"] = f.dword();
        } else if (this.tag == "Show" && this.type == "Party") {
            pass;
        } else if (this.tag == "Hide" && this.type == "Party") {
            pass;
        } else if (this.tag == "Effect" && this.type == "Break") {
            pass;
        } else if (this.tag == "Call" && this.type == "Start") {
            this.properties["call"] = f.string();
        } else if (this.tag == "Link" && this.type == "Package") {
            this.properties["link"] = f.dword();
        } else if (this.tag == "Call" && this.type == "Package") {
            this.properties["call"] = f.dword();
        } else if (this.tag == "Branch" && this.type == "Area") {
            pass;
        } else if (this.tag == "Branch" && this.type == "Battle") {
            pass;
        } else if (this.tag == "Branch" && this.type == "CompleteStamp") {
            this.properties["scenario"] = f.string();
        } else if (this.tag == "Get" && this.type == "CompleteStamp") {
            this.properties["scenario"] = f.string();
        } else if (this.tag == "Lose" && this.type == "CompleteStamp") {
            this.properties["scenario"] = f.string();
        } else if (this.tag == "Branch" && this.type == "Gossip") {
            this.properties["gossip"] = f.string();
        } else if (this.tag == "Get" && this.type == "Gossip") {
            this.properties["gossip"] = f.string();
        } else if (this.tag == "Lose" && this.type == "Gossip") {
            this.properties["gossip"] = f.string();
        } else if (this.tag == "Branch" && this.type == "IsBattle") {
            pass;
        } else if (this.tag == "Redisplay" && this.type == "") {
            pass;
        } else if (this.tag == "Check" && this.type == "Flag") {
            this.properties["flag"] = f.string();
        elif this.tag == "Substitute" && this.type == "Step": // 1.30
            this.properties["from"] = f.string();
            this.properties["to"] = f.string();
        elif this.tag == "Substitute" && this.type == "Flag": // 1.30
            this.properties["from"] = f.string();
            this.properties["to"] = f.string();
        elif this.tag == "Branch" && this.type == "StepValue": // 1.30
            this.properties["from"] = f.string();
            this.properties["to"] = f.string();
        elif this.tag == "Branch" && this.type == "FlagValue": // 1.30
            this.properties["from"] = f.string();
            this.properties["to"] = f.string();
        elif this.tag == "Branch" && this.type == "RandomSelect": // 1.30
            this.castranges = this.conv_castranges(f.byte());
            style = f.byte();
            if ((style & 0b01) != 0) {
                this.properties["minLevel"] = f.dword();
                this.properties["maxLevel"] = f.dword();
            if ((style & 0b10) != 0) {
                this.properties["status"] = this.conv_statustype(f.byte());
        elif this.tag == "Branch" && this.type == "KeyCode": // 1.50
            this.properties["targetkc"] = this.conv_keycoderange(f.byte());
            ect = this.conv_effectcardtype(f.byte());
            if (ect == "All") {
                this.properties["skill"] = true;
                this.properties["item"] = true;
                this.properties["beast"] = true;
                this.properties["hand"] = true // BUG: CardWirth 1.50ではアイテムが対象にあると手札も検索される
            } else if (ect == "Skill") {
                this.properties["skill"] = true;
                this.properties["item"] = false;
                this.properties["beast"] = false;
                this.properties["hand"] = false;
            } else if (ect == "Item") {
                this.properties["skill"] = false;
                this.properties["item"] = true;
                this.properties["beast"] = false;
                this.properties["hand"] = true // BUG: CardWirth 1.50ではアイテムが対象にあると手札も検索される
            } else if (ect == "Beast") {
                this.properties["skill"] = false;
                this.properties["item"] = false;
                this.properties["beast"] = true;
                this.properties["hand"] = false;
            this.properties["keyCode"] = f.string();
        elif this.tag == "Check" && this.type == "Step": // 1.50
            this.properties["step"] = f.string();
            this.properties["value"] = f.dword();
            this.properties["comparison"] = this.conv_comparison4(f.byte());
        elif this.tag == "Branch" && this.type == "Round": // 1.50
            this.properties["comparison"] = this.conv_comparison3(f.byte());
            this.properties["round"] = f.dword();
        } else {
            throw new ValueError(this.tag + ", " + this.type);

        this.data = null;

    public UNK get_data() {
        if (!this.data == null) {
            return this.data;

        contentsline = null;
        child = self;
        while true:
            child.data = cw.data.make_element(child.tag);
            if (child.type) {
                child.data.set("type", child.type);
            child.data.set("name", child.name);

            foreach (var key, value in child.properties.iteritems()) {
                if (isinstance(value, (str, unicode))) {
                    child.data.set(key, value);
                } else {
                    child.data.set(key, str(value));

            if (child.tag == "Talk" && child.type == "Message") {
                child.data.append(cw.data.make_element("Text", child.text));
            } else if (child.tag == "Change" && child.type == "BgImage") {
                e = cw.data.make_element("BgImages");
                foreach (var bgimg in child.bgimgs) {
                    e.append(bgimg.get_data());
                child.data.append(e);
            } else if (child.tag == "Effect" && child.type == "") {
                e = cw.data.make_element("Motions");
                foreach (var motion in child.motions) {
                    e.append(motion.get_data());
                child.data.append(e);
            } else if (child.tag == "Talk" && child.type == "Dialog") {
                if (child.properties["targetm"] == "Valued") {
                    e = cw.data.make_element("Coupons");
                    foreach (var coupon in child.coupons) {
                        e.append(coupon.get_data());
                    child.data.append(e);
                e = cw.data.make_element("Dialogs");
                foreach (var dialog in child.dialogs) {
                    e.append(dialog.get_data());
                child.data.append(e);
            elif child.tag == "Branch" && child.type == "RandomSelect": // 1.30
                e = cw.data.make_element("CastRanges");
                foreach (var castrange in child.castranges) {
                    e.append(cw.data.make_element("CastRange", castrange));
                child.data.append(e);

            if (!contentsline == null) {
                contentsline.append(child.data);

            if (len(child.children) == 1) {
                if (contentsline == null) {
                    contentsline = cw.data.make_element("ContentsLine");
                    contentsline.append(child.data);
                child = child.children[0];
                continue // 再帰回避
            } else {
                if (child.children) {
                    e = cw.data.make_element("Contents");
                    foreach (var child2 in child.children) {
                        e.append(child2.get_data());
                    child.data.append(e);
                break;

        if (contentsline == null) {
            return this.data;
        } else {
            return contentsline;

    @staticmethod;
    def unconv(f, data):
        if (data.tag == "ContentsLine") {
            foreach (var child in data[:-1]) {
                Content._unconv_header(f, child);
                f.write_dword(1 + 50000);
            Content.unconv(f, data[-1]);
            foreach (var child in reversed(data[:-1])) {
                Content._unconv_properties(f, child);

        } else {
            Content._unconv_header(f, data);

            children = ();
            foreach (var e in data) {
                if (e.tag == "Contents") {
                    children = e;
            f.write_dword(len(children) + 50000);
            foreach (var child in children) {
                Content.unconv(f, child);

            Content._unconv_properties(f, data);

    @staticmethod;
    def _unconv_header(f, data):
        tag = data.tag;
        ctype = data.get("type", "");
        name = data.get("name", "");
        f.write_byte(base.CWBinaryBase.unconv_contenttype(tag, ctype));
        f.write_string(name);

    @staticmethod;
    def _unconv_properties(f, data):
        // 宿データの埋め込みカードのコンテントは
        // 子コンテントデータの後ろに"dword()"(4)が埋め込まれている。
        f.write_dword(4);

        tag = data.tag;
        ctype = data.get("type", "");

        if (tag == "Start" && ctype == "") {
            pass;
        } else if (tag == "Link" && ctype == "Start") {
            f.write_string(data.get("link"));
        } else if (tag == "Start" && ctype == "Battle") {
            f.write_dword(int(data.get("id")));
        } else if (tag == "End" && ctype == "") {
            f.write_bool(cw.util.str2bool(data.get("complete")));
        } else if (tag == "End" && ctype == "BadEnd") {
            pass;
        } else if (tag == "Change" && ctype == "Area") {
            if (data.get("transition", "Default") != "Default") {
                f.check_wsnversion("");
            f.write_dword(int(data.get("id")));
        } else if (tag == "Talk" && ctype == "Message") {
            if (data.getint(".", "columns", 1) != 1) {
                f.check_wsnversion("1");
            if (data.getbool(".", "centeringx", false)) {
                f.check_wsnversion("2");
            if (data.getbool(".", "centeringy", false)) {
                f.check_wsnversion("2");
            text = "";
            foreach (var e in data) {
                if (e.tag == "Text") {
                    text= e.text;
                    break;

            e_imgpaths = data.find("ImagePaths");
            if (!e_imgpaths == null) {
                if (1 < len(e_imgpaths)) {
                    f.check_wsnversion("1");
                base.CWBinaryBase.check_imgpath(f, e_imgpaths.find("ImagePath"), "TopLeft");
                imgpath2 = prop.gettext("ImagePath", "");
                if (imgpath2) {
                    imgpath = base.CWBinaryBase.materialpath(imgpath2);
                } else {
                    imgpath = u"";
            } else {
                base.CWBinaryBase.check_imgpath(f, data, "TopLeft");
                imgpath = data.get("path");
            f.write_string(base.CWBinaryBase.materialpath(imgpath));
            f.write_string(text, true);
        } else if (tag == "Play" && ctype == "Bgm") {
            f.write_string(base.CWBinaryBase.materialpath(data.get("path")));
            f.check_bgmoptions(data);
        } else if (tag == "Change" && ctype == "BgImage") {
            if (data.get("transition", "Default") != "Default") {
                f.check_wsnversion("");
            bgimgs = [];
            foreach (var e in data) {
                if (e.tag == "BgImages") {
                    bgimgs = e;
                    break;
            f.write_dword(len(bgimgs));
            foreach (var bgimg in bgimgs) {
                bgimage.BgImage.unconv(f, bgimg);
        } else if (tag == "Play" && ctype == "Sound") {
            f.write_string(base.CWBinaryBase.materialpath(data.get("path")));
            f.check_soundoptions(data);
        } else if (tag == "Wait" && ctype == "") {
            f.write_dword(int(data.get("value")));
        } else if (tag == "Effect" && ctype == "") {
            if (data.getbool(".", "refability", false)) {
                f.check_wsnversion("2");
            if (data.getbool(".", "ignite", false)) {
                f.check_wsnversion("2");
            f.write_dword(int(data.get("level")));
            f.write_byte(base.CWBinaryBase.unconv_target_member(data.get("targetm")));
            f.write_byte(base.CWBinaryBase.unconv_card_effecttype(data.get("effecttype")));
            f.write_byte(base.CWBinaryBase.unconv_card_resisttype(data.get("resisttype")));
            f.write_dword(int(data.get("successrate")));
            f.write_string(base.CWBinaryBase.materialpath(data.get("sound")));
            f.write_byte(base.CWBinaryBase.unconv_card_visualeffect(data.get("visual")));
            f.check_soundoptions(data);
            motions = [];
            foreach (var e in data) {
                if (e.tag == "Motions") {
                    motions = e;
                    break;
            f.write_dword(len(motions));
            foreach (var motion in motions) {
                effectmotion.EffectMotion.unconv(f, motion);
        } else if (tag == "Branch" && ctype == "Select") {
            f.write_bool(cw.util.str2bool(data.get("targetall")));
            if ("method" in data.attrib) {
                smethod = data.get("method");
                if (!smethod in ("Manual", "Random")) {
                    f.check_wsnversion("1");
                f.write_bool(smethod == "Random");
            } else {
                f.write_bool(cw.util.str2bool(data.get("random")));
        } else if (tag == "Branch" && ctype == "Ability") {
            f.write_dword(int(data.get("value")));
            f.write_byte(base.CWBinaryBase.unconv_target_member(data.get("targetm")));
            f.write_dword(base.CWBinaryBase.unconv_card_physicalability(data.get("physical")));
            f.write_dword(base.CWBinaryBase.unconv_card_mentalability(data.get("mental")));
        } else if (tag == "Branch" && ctype == "Random") {
            f.write_dword(int(data.get("value")));
        } else if (tag == "Branch" && ctype == "Flag") {
            f.write_string(data.get("flag"));
        } else if (tag == "Set" && ctype == "Flag") {
            f.write_string(data.get("flag"));
            f.write_bool(cw.util.str2bool(data.get("value")));
        } else if (tag == "Branch" && ctype == "MultiStep") {
            f.write_string(data.get("step"));
        } else if (tag == "Set" && ctype == "Step") {
            f.write_string(data.get("step"));
            f.write_dword(int(data.get("value")));
        } else if (tag == "Branch" && ctype == "Cast") {
            f.write_dword(int(data.get("id")));
        } else if (tag == "Branch" && ctype == "Item") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Branch" && ctype == "Skill") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Branch" && ctype == "Info") {
            f.write_dword(int(data.get("id")));
        } else if (tag == "Branch" && ctype == "Beast") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Branch" && ctype == "Money") {
            f.write_dword(int(data.get("value")));
        } else if (tag == "Branch" && ctype == "Coupon") {
            // Wsn.1方式
            coupon = data.get("coupon", "");
            // Wsn.2方式(couponnames)
            names =[coupon] if coupon else [];
            foreach (var e in data.getfind("Coupons", raiseerror=false)) {
                names.append(e.text);
            if (len(names) > 1) {
                f.check_wsnversion("2");
            } else if (len(names) == 1) {
                coupon = names[0];
            base.CWBinaryBase.check_coupon(f, coupon);
            f.write_string(coupon);
            f.write_dword(0);
            f.write_byte(base.CWBinaryBase.unconv_target_scope_coupon(data.get("targets"), f));
        } else if (tag == "Get" && ctype == "Cast") {
            if (data.getattr(".", "startaction", "NextRound") != "NextRound") {
                f.check_wsnversion("2");
            f.write_dword(int(data.get("id")));
        } else if (tag == "Get" && ctype == "Item") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Get" && ctype == "Skill") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Get" && ctype == "Info") {
            f.write_dword(int(data.get("id")));
        } else if (tag == "Get" && ctype == "Beast") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Get" && ctype == "Money") {
            f.write_dword(int(data.get("value")));
        } else if (tag == "Get" && ctype == "Coupon") {
            base.CWBinaryBase.check_coupon(f, data.get("coupon"));
            f.write_string(data.get("coupon"));
            f.write_dword(int(data.get("value")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Lose" && ctype == "Cast") {
            f.write_dword(int(data.get("id")));
        } else if (tag == "Lose" && ctype == "Item") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Lose" && ctype == "Skill") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Lose" && ctype == "Info") {
            f.write_dword(int(data.get("id")));
        } else if (tag == "Lose" && ctype == "Beast") {
            f.write_dword(int(data.get("id")));
            f.write_dword(int(data.get("number")));
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Lose" && ctype == "Money") {
            f.write_dword(int(data.get("value")));
        } else if (tag == "Lose" && ctype == "Coupon") {
            base.CWBinaryBase.check_coupon(f, data.get("coupon"));
            f.write_string(data.get("coupon"));
            f.write_dword(0);
            f.write_byte(base.CWBinaryBase.unconv_target_scope(data.get("targets")));
        } else if (tag == "Talk" && ctype == "Dialog") {
            if (data.getint(".", "columns", 1) != 1) {
                f.check_wsnversion("1");
            if (data.getbool(".", "centeringx", false)) {
                f.check_wsnversion("2");
            if (data.getbool(".", "centeringy", false)) {
                f.check_wsnversion("2");
            targetm = data.get("targetm");
            f.write_byte(base.CWBinaryBase.unconv_target_member_dialog(targetm, f));
            if (targetm == "Valued") {
                coupons = [];
                initvalue = data.get("initialValue", "0");
                coupons.append(cw.data.make_element("Coupon", "", attrs={"value": initvalue}));
                foreach (var e in data) {
                    if (e.tag == "Coupons") {
                        foreach (var e_coupon in e) {
                            coupons.append(e_coupon);
                f.write_dword(len(coupons));
                foreach (var coupon in coupons) {
                    cw.binary.coupon.Coupon.unconv(f, coupon);
            dialogs = [];
            foreach (var e in data) {
                if (e.tag == "Dialogs") {
                    dialogs = e;
                    break;
            f.write_dword(len(dialogs));
            foreach (var dialog in dialogs) {
                cw.binary.dialog.Dialog.unconv(f, dialog);
        } else if (tag == "Set" && ctype == "StepUp") {
            f.write_string(data.get("step"));
        } else if (tag == "Set" && ctype == "StepDown") {
            f.write_string(data.get("step"));
        } else if (tag == "Reverse" && ctype == "Flag") {
            f.write_string(data.get("flag"));
        } else if (tag == "Branch" && ctype == "Step") {
            f.write_string(data.get("step"));
            f.write_dword(int(data.get("value")));
        } else if (tag == "Elapse" && ctype == "Time") {
            pass;
        } else if (tag == "Branch" && ctype == "Level") {
            f.write_bool(cw.util.str2bool(data.get("average")));
            f.write_dword(int(data.get("value")));
        } else if (tag == "Branch" && ctype == "Status") {
            f.write_byte(base.CWBinaryBase.unconv_statustype(data.get("status"), f));
            f.write_byte(base.CWBinaryBase.unconv_target_member(data.get("targetm")));
        } else if (tag == "Branch" && ctype == "PartyNumber") {
            f.write_dword(int(data.get("value")));
        } else if (tag == "Show" && ctype == "Party") {
            pass;
        } else if (tag == "Hide" && ctype == "Party") {
            pass;
        } else if (tag == "Effect" && ctype == "Break") {
            pass;
        } else if (tag == "Call" && ctype == "Start") {
            f.write_string(data.get("call"));
        } else if (tag == "Link" && ctype == "Package") {
            f.write_dword(int(data.get("link")));
        } else if (tag == "Call" && ctype == "Package") {
            f.write_dword(int(data.get("call")));
        } else if (tag == "Branch" && ctype == "Area") {
            pass;
        } else if (tag == "Branch" && ctype == "Battle") {
            pass;
        } else if (tag == "Branch" && ctype == "CompleteStamp") {
            f.write_string(data.get("scenario"));
        } else if (tag == "Get" && ctype == "CompleteStamp") {
            f.write_string(data.get("scenario"));
        } else if (tag == "Lose" && ctype == "CompleteStamp") {
            f.write_string(data.get("scenario"));
        } else if (tag == "Branch" && ctype == "Gossip") {
            f.write_string(data.get("gossip"));
        } else if (tag == "Get" && ctype == "Gossip") {
            f.write_string(data.get("gossip"));
        } else if (tag == "Lose" && ctype == "Gossip") {
            f.write_string(data.get("gossip"));
        } else if (tag == "Branch" && ctype == "IsBattle") {
            pass;
        } else if (tag == "Redisplay" && ctype == "") {
            if (data.get("transition", "Default") != "Default") {
                f.check_version("CardWirthPy 0.12");
        } else if (tag == "Check" && ctype == "Flag") {
            f.write_string(data.get("flag"));
        elif tag == "Substitute" && ctype == "Step": // 1.30
            f.check_version(1.30);
            if (data.get("from", "").lower() == "??selectedplayer") {
                f.check_wsnversion("2");
            f.write_string(data.get("from"));
            f.write_string(data.get("to"));
        elif tag == "Substitute" && ctype == "Flag": // 1.30
            f.check_version(1.30);
            f.write_string(data.get("from"));
            f.write_string(data.get("to"));
        elif tag == "Branch" && ctype == "StepValue": // 1.30
            f.check_version(1.30);
            f.write_string(data.get("from"));
            f.write_string(data.get("to"));
        elif tag == "Branch" && ctype == "FlagValue": // 1.30
            f.check_version(1.30);
            f.write_string(data.get("from"));
            f.write_string(data.get("to"));
        elif tag == "Branch" && ctype == "RandomSelect": // 1.30
            f.check_version(1.30);
            f.write_byte(base.CWBinaryBase.unconv_castranges(data.find("CastRanges")));
            levelmin = data.get("levelmin", null);
            levelmax = data.get("levelmax", null);
            status = data.get("status", null);
            style = 0;
            if (!(levelmin == null && levelmax == null)) {
                style |= 0b01;
            if (!status == null) {
                style |= 0b10;
            f.write_byte(style);
            if ((style & 0b01) != 0) {
                f.write_dword(levelmin);
                f.write_dword(levelmax);
            if ((style & 0b10) != 0) {
                f.write_byte(base.CWBinaryBase.unconv_statustype(status, f));
        elif tag == "Branch" && ctype == "KeyCode": // 1.50
            f.check_version(1.50);
            f.write_byte(base.CWBinaryBase.unconv_keycoderange(data.get("targetkc")));
            // Wsn.1方式
            etype = data.get("effectCardType", "All");
            skill = false;
            item = false;
            beast = false;
            hand = false;
            if (etype == "All") {
                skill = true;
                item = true;
                beast = true;
            } else if (etype == "Skill") {
                skill = true;
            } else if (etype == "Item") {
                item = true;
            } else if (etype == "Beast") {
                beast = true;
            // Wsn.2方式(任意の組み合わせ)
            if ("skill" in data.attrib) {
                skill = data.getbool(".", "skill");
            if ("item" in data.attrib) {
                item = data.getbool(".", "item");
            if ("beast" in data.attrib) {
                beast = data.getbool(".", "beast");
            if ("hand" in data.attrib) {
                hand = data.getbool(".", "hand");

            if (skill && item && beast && hand) {
                etype = "All";
            } else if (skill && !item && !beast && !hand) {
                etype = "Skill";
            } else if (!skill && item && !beast && hand) {
                etype = "Item";
            } else if (!skill && !item && beast && !hand) {
                etype = "Beast";
            } else {
                f.check_wsnversion("2");
            f.write_byte(base.CWBinaryBase.unconv_effectcardtype(etype));
            f.write_string(data.get("keyCode"));
        elif tag == "Check" && ctype == "Step": // 1.50
            f.check_version(1.50);
            f.write_string(data.get("step"));
            f.write_dword(int(data.get("value")));
            f.write_byte(base.CWBinaryBase.unconv_comparison4(data.get("comparison")));
        elif tag == "Branch" && ctype == "Round": // 1.50
            f.check_version(1.50);
            f.write_byte(base.CWBinaryBase.unconv_comparison3(data.get("comparison")));
            f.write_dword(int(data.get("round")));
        elif tag == "Replace" && ctype == "BgImage": // Wsn.1
            f.check_wsnversion("1");
        elif tag == "Lose" && ctype == "BgImage": // Wsn.1
            f.check_wsnversion("1");
        elif tag == "Move" && ctype == "BgImage": // Wsn.1
            f.check_wsnversion("1");
        elif tag == "Branch" && ctype == "MultiCoupon":  // Wsn.2
            f.check_wsnversion("2");
        elif tag == "Branch" && ctype == "MultiRandom":  // Wsn.2
            f.check_wsnversion("2");
        } else {
            throw new ValueError(tag + ", " + ctype);

def main():
    pass;

if __name__ == "__main__":
    main();
