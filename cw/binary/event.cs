//!/usr/bin/env python
// -*- coding: utf-8 -*-

import base;
import content;

import cw;


class Event : base.CWBinaryBase {
    // """イベント発火条件付のイベントデータのクラス。""";
    public Event(UNK parent, UNK f, bool yadodata=false) : base(parent, f, yadodata) {
        contents_num = f.dword();
        this.contents = [content.Content(self, f, 0) for _cnt in xrange(contents_num)];
        ignitions_num = f.dword();
        this.ignitions = [f.dword() for _cnt in xrange(ignitions_num)];
        this.keycodes = f.string(true);

        this.data = null;
    }

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Event");
            e = cw.data.make_element("Ignitions");
            e.append(cw.data.make_element("Number", cw.util.encodetextlist([str(i) for i in this.ignitions]) if this.ignitions else ""));
            keycodes = cw.util.decodetextlist(this.keycodes);
            if (keycodes && keycodes[0] == "MatchingType=All") {
                // 1.50
                matching = "And";
                keycodes = keycodes[1:];
            } else {
                matching = "Or";
            }
            e.set("keyCodeMatchingType", matching);
            e.append(cw.data.make_element("KeyCodes", cw.util.encodetextlist(keycodes)));
            this.data.append(e);
            e = cw.data.make_element("Contents");
            foreach (var content in this.contents) {
                e.append(content.get_data());
            }
            this.data.append(e);
        }
        return this.data;
    }

    public static UNK unconv(UNK f, UNK data) {
        contents = [];
        ignitions = [];
        keycodes = "";

        foreach (var e in data) {
            if (e.tag == "Ignitions") {
                matching = e.get("keyCodeMatchingType", "Or");
                foreach (var ig in e) {
                    if (ig.tag == "Number") {
                        foreach (var num in cw.util.decodetextlist(ig.text)) {
                            ignitionnum = int(num);
                            if (ignitionnum in (4, 5)) {
                                f.check_version(1.50);
                            }
                            ignitions.append(ignitionnum);
                        }
                    } else if (ig.tag == "KeyCodes") {
                        if (matching == "And") {
                            f.check_version(1.50);
                            array = ["MatchingType=All"];
                            array.extend(cw.util.decodetextlist(ig.text));
                            keycodes = cw.util.encodetextlist(array);
                        } else {
                            keycodes = ig.text;
                        }
                    }
                }
            } else if (e.tag == "Contents") {
                contents = e;
            }
        }

        f.write_dword(len(contents));
        foreach (var content in contents) {
            content.Content.unconv(f, content);
        }
        f.write_dword(len(ignitions));
        foreach (var ignition in ignitions) {
            f.write_dword(ignition);
        }
        f.write_string(keycodes, true);
    }
}

class SimpleEvent : base.CWBinaryBase {
    // """イベント発火条件なしのイベントデータのクラス。;
    // カードイベント・パッケージ等で使う。;
    // """;
    public SimpleEvent(UNK parent, UNK f, bool yadodata=false) : base(parent, f, yadodata) {
        contents_num = f.dword();
        this.contents = [content.Content(self, f, 0) for _cnt in xrange(contents_num)];

        this.data = null;
    }

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Event");
            e = cw.data.make_element("Contents");
            foreach (var content in this.contents) {
                e.append(content.get_data());
            }
            this.data.append(e);
        }
        return this.data;
    }

    public static UNK unconv(UNK f, UNK data) {
        contents = [];

        foreach (var e in data) {
            if (e.tag == "Contents") {
                contents = e;
            }
        }

        f.write_dword(len(contents));
        foreach (var ct in contents) {
            content.Content.unconv(f, ct);
        }
    }
}