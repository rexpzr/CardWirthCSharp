class Package : base.CWBinaryBase {
    // """widファイルの情報カードのデータ。;
    // type:InfoCardと区別が付くように、Packageは暫定的に"7"とする。;
    // """;
    public Package(UNK parent, UNK f, bool yadodata=false, bool nameonly=false, string materialdir="Material", bool image_export=true) : base(parent, f, yadodata, materialdir, image_export) {
        this.type = 7;
        f.dword() // 不明
        this.name = f.string();
        this.id = f.dword();
        if (nameonly) {
            return;
        }
        events_num = f.dword();
        this.events = [event.SimpleEvent(self, f) for _cnt in xrange(events_num)];

        this.data = null;
    }

    public UNK get_data() {
        if (this.data == null) {
            this.data = cw.data.make_element("Package");
            prop = cw.data.make_element("Property");
            e = cw.data.make_element("Id", str(this.id));
            prop.append(e);
            e = cw.data.make_element("Name", this.name);
            prop.append(e);
            this.data.append(prop);
            e = cw.data.make_element("Events");
            foreach (var event in this.events) {
                e.append(event.get_data());
            }
            this.data.append(e);
        }
        return this.data;
    }

    public static void unconv(UNK f, UNKdata) {
        name = "";
        resid = 0;
        events = [];

        foreach (var e in data) {
            if (e.tag == "Property") {
                foreach (var prop in e) {
                    if (prop.tag == "Id") {
                        resid = int(prop.text);
                    } else if (prop.tag == "Name") {
                        name = prop.text;
                    }
                }
            } else if (e.tag == "Events") {
                events = e;
            }
        }

        f.write_dword(0) // 不明
        f.write_string(name);
        f.write_dword(resid);
        f.write_dword(len(events));
        foreach (var evt in events) {
            event.SimpleEvent.unconv(f, evt);
        }
    }
}
