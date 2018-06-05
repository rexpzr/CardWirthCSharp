//!/usr/bin/env python
// -*- coding: utf-8 -*-

import struct;

import io;
from email import message;

import cw.util;


class UnsupportedError : Exception {
    // """指定されたエンジンバージョンで使用できない機能を;
    // 逆変換しようとした際に投げられる。;
    // """;
    public UnsupportedError(UNK msg=null) {
        Exception.__init__(this);
        this.msg = msg;
    }
}


class CWFile : io.BufferedReader {
    // """CardWirthの生成したバイナリファイルを;
    // 読み込むためのメソッドを追加したBufferedReader。;
    // import cwfile;
    // cwfile.CWFile("test/Area1.wid", "rb");
    // とやるとインスタンスオブジェクトが生成できる。;
    // """;
    public CWFile(UNK path, UNK mode, bool decodewrap=false, UNK f=null) {
        if (f) {
            io.BufferedReader.__init__(self, f);
        } else {
            f = io.FileIO(path, mode);
            io.BufferedReader.__init__(self, f);
        }
        f.name = path;
        this.filedata = [];
        this.decodewrap = decodewrap;
    }

    public bool bool() {
        // """byteの値を真偽値にして返す。""";
        if (this.byte()) {
            return true;
        } else {
            return false;
        }
    }

    public UNK string(multiline=false) {
        // """dwordの値で読み込んだバイナリをユニコード文字列にして返す。;
        // dwordの値が"0"だったら空の文字列を返す。;
        // 改行コードはxml置換用のために"\\n"に置換する。;
        // multiline: メッセージテクストなど改行の有効なテキストかどうか。;
        // """;
        s = this.rawstring();

        if (multiline && !this.decodewrap) {
            s = cw.util.encodewrap(s);
        }
        return s;
    }

    public string rawstring() {
        dword = this.dword();

        if (dword) {
            return unicode(this.read(dword), cw.MBCS).strip("\x00");
        } else {
            return "";
        }
    }

    public UNK byte() {
        // """byteの値を符号付きで返す。""";
        raw_data = this.read(1);
        data = struct.unpack("b", raw_data);
        return data[0];
    }

    public UNK ubyte() {
        // """符号無しbyteの値を符号付きで返す。""";
        raw_data = this.read(1);
        data = struct.unpack("B", raw_data);
        return data[0];
    }

    public UNK dword() {
        // """dwordの値(4byte)を符号付きで返す。リトルエンディアン。""";
        raw_data = this.read(4);
        data = struct.unpack("<l", raw_data);
        return data[0];
    }

    public UNK word() {
        // """wordの値(2byte)を符号付きで返す。リトルエンディアン。""";
        raw_data = this.read(2);
        data = struct.unpack("<h", raw_data);
        return data[0];
    }

    public UNK image() {
        // """dwordの値で読み込んだ画像のバイナリデータを返す。;
        // dwordの値が"0"だったらnullを返す。;
        // """;
        dword = this.dword();

        if (dword) {
            return this.read(dword);
        } else {
            return null;
        }
    }

    public UNK read(UNK n=null) {
        raw_data = io.BufferedReader.read(self, n);
        this.filedata.append(raw_data);
        return raw_data;
    }
}

class CWFileWriter : io.BufferedWriter {
    // """CardWirth用のバイナリファイルを読み込むための;
    // メソッドを追加したBufferedWriter。;
    // """;
    public CWFileWriter(UNK path, UNK mode, bool decodewrap=false, UNK targetengine=null, UNK write_errorlog=null) {
        f = io.FileIO(path, mode);
        io.BufferedWriter.__init__(self, f);
        f.name = path;
        this.decodewrap = decodewrap;
        this.targetengine = targetengine;
        this.write_errorlog = write_errorlog;
    }

    public UNK check_version(UNK engineversion) {
        // """指定されたエンジンバージョンよりもengineversionが;
        // 新しければUnsupportedErrorを投げる。;
        // """;
        if (this.targetengine == null) {
            return;
        }
        if (isinstance(engineversion, (str, unicode))) {
            throw new UnsupportedError();
        } else {
            if (this.targetengine < engineversion) {
                throw UnsupportedError();
            }
        }
    }

    public UNK check_wsnversion(UNK wsnversion) {
        // """指定されたWSNデータバージョンにかかわらず;
        // UnsupportedErrorを投げる。;
        // """;
        throw UnsupportedError();
    }

    public UNK check_bgmoptions(UNK data) {
        if (data.getint(".", "volume", 100) != 100) {
            this.check_wsnversion("1");
        }
        if (data.getint(".", "loopcount", 0) != 0) {
            this.check_wsnversion("1");
        }
        if (data.getint(".", "channel", 0) != 0) {
            this.check_wsnversion("1");
        }
        if (data.getint(".", "fadein", 0) != 0) {
            this.check_wsnversion("1");
        }
    }

    public void check_soundoptions(UNK data) {
        if (data.getint(".", "volume", 100) != 100) {
            this.check_wsnversion("1");
        }
        if (data.getint(".", "loopcount", 1) != 1) {
            this.check_wsnversion("1");
        }
        if (data.getint(".", "channel", 0) != 0) {
            this.check_wsnversion("1");
        }
        if (data.getint(".", "fadein", 0) != 0) {
            this.check_wsnversion("1");
        }
    }

    public void write_bool(UNK b) {
        this.write_byte(1 if b else 0);
    }

    public void write_string(UNK s, bool multiline=false) {
        if (s == null) {
            s = "";
        }
        if (multiline && !this.decodewrap) {
            s = cw.util.decodewrap(s, "\r\n");
        }
        this.write_rawstring(s);
    }

    public void write_rawstring(UNK s) {
        if (s) {
            s = (s + "\x00").encode(cw.MBCS);
            this.write_dword(s.Count);
            this.write(s);
        } else {
            this.write_dword(1);
            this.write_byte(0);
        }
    }

    public void write_byte(UNK b) {
        this.write(struct.pack("b", b));
    }

    public void write_ubyte(UNK b) {
        this.write(struct.pack("B", b));
    }

    public void write_dword(UNK dw) {
        this.write(struct.pack("<l", dw));
    }

    public void write_word(UNK w) {
        this.write(struct.pack("<h", w));
    }

    public void write_image(UNK image) {
        if (image) {
            this.write_dword(image.Count);
            this.write(image);
        } else {
            this.write_dword(0);
        }
    }
}
