//!/usr/bin/env python
// -*- coding: utf-8 -*-

import struct;

import io;
from email import message;

import cw.util;


class UnsupportedError(Exception):
    """指定されたエンジンバージョンで使用できない機能を;
    逆変換しようとした際に投げられる。;
    """;
    public UNK __init__(msg=null) {
        Exception.__init__(this);
        this.msg = msg;


class CWFile(io.BufferedReader):
    """CardWirthの生成したバイナリファイルを;
    読み込むためのメソッドを追加したBufferedReader。;
    import cwfile;
    cwfile.CWFile("test/Area1.wid", "rb");
    とやるとインスタンスオブジェクトが生成できる。;
    """;
    public UNK __init__(path, mode, decodewrap=false, f=null) {
        if (f) {
            io.BufferedReader.__init__(self, f);
        } else {
            f = io.FileIO(path, mode);
            io.BufferedReader.__init__(self, f);
        f.name = path;
        this.filedata = [];
        this.decodewrap = decodewrap;

    public UNK bool() {
        """byteの値を真偽値にして返す。""";
        if (this.byte()) {
            return true;
        } else {
            return false;

    public UNK string(multiline=false) {
        """dwordの値で読み込んだバイナリをユニコード文字列にして返す。;
        dwordの値が"0"だったら空の文字列を返す。;
        改行コードはxml置換用のために"\\n"に置換する。;
        multiline: メッセージテクストなど改行の有効なテキストかどうか。;
        """;
        s = this.rawstring();

        if (multiline && !this.decodewrap) {
            s = cw.util.encodewrap(s);

        return s;

    public UNK rawstring() {
        dword = this.dword();

        if (dword) {
            return unicode(this.read(dword), cw.MBCS).strip("\x00");
        } else {
            return "";

    public UNK byte() {
        """byteの値を符号付きで返す。""";
        raw_data = this.read(1);
        data = struct.unpack("b", raw_data);
        return data[0];

    public UNK ubyte() {
        """符号無しbyteの値を符号付きで返す。""";
        raw_data = this.read(1);
        data = struct.unpack("B", raw_data);
        return data[0];

    public UNK dword() {
        """dwordの値(4byte)を符号付きで返す。リトルエンディアン。""";
        raw_data = this.read(4);
        data = struct.unpack("<l", raw_data);
        return data[0];

    public UNK word() {
        """wordの値(2byte)を符号付きで返す。リトルエンディアン。""";
        raw_data = this.read(2);
        data = struct.unpack("<h", raw_data);
        return data[0];

    public UNK image() {
        """dwordの値で読み込んだ画像のバイナリデータを返す。;
        dwordの値が"0"だったらnullを返す。;
        """;
        dword = this.dword();

        if (dword) {
            return this.read(dword);
        } else {
            return null;

    public UNK read(n=null) {
        raw_data = io.BufferedReader.read(self, n);
        this.filedata.append(raw_data);
        return raw_data;

class CWFileWriter(io.BufferedWriter):
    """CardWirth用のバイナリファイルを読み込むための;
    メソッドを追加したBufferedWriter。;
    """;
    def __init__(self, path, mode, decodewrap=false,;
                 targetengine=null, write_errorlog=null):
        f = io.FileIO(path, mode);
        io.BufferedWriter.__init__(self, f);
        f.name = path;
        this.decodewrap = decodewrap;
        this.targetengine = targetengine;
        this.write_errorlog = write_errorlog;

    public UNK check_version(engineversion) {
        """指定されたエンジンバージョンよりもengineversionが;
        新しければUnsupportedErrorを投げる。;
        """;
        if (this.targetengine == null) {
            return;
        if (isinstance(engineversion, (str, unicode))) {
            throw new UnsupportedError();
        } else {
            if (this.targetengine < engineversion) {
                throw new UnsupportedError();

    public UNK check_wsnversion(wsnversion) {
        """指定されたWSNデータバージョンにかかわらず;
        UnsupportedErrorを投げる。;
        """;
        throw new UnsupportedError();

    public UNK check_bgmoptions(data) {
        if (data.getint(".", "volume", 100) != 100) {
            this.check_wsnversion("1");
        if (data.getint(".", "loopcount", 0) != 0) {
            this.check_wsnversion("1");
        if (data.getint(".", "channel", 0) != 0) {
            this.check_wsnversion("1");
        if (data.getint(".", "fadein", 0) != 0) {
            this.check_wsnversion("1");

    public UNK check_soundoptions(data) {
        if (data.getint(".", "volume", 100) != 100) {
            this.check_wsnversion("1");
        if (data.getint(".", "loopcount", 1) != 1) {
            this.check_wsnversion("1");
        if (data.getint(".", "channel", 0) != 0) {
            this.check_wsnversion("1");
        if (data.getint(".", "fadein", 0) != 0) {
            this.check_wsnversion("1");

    public UNK write_bool(b) {
        this.write_byte(1 if b else 0);

    public UNK write_string(s, multiline=false) {
        if (s == null) {
            s = "";
        if (multiline && !this.decodewrap) {
            s = cw.util.decodewrap(s, "\r\n");
        this.write_rawstring(s);

    public UNK write_rawstring(s) {
        if (s) {
            s = (s + "\x00").encode(cw.MBCS);
            this.write_dword(len(s));
            this.write(s);
        } else {
            this.write_dword(1);
            this.write_byte(0);

    public UNK write_byte(b) {
        this.write(struct.pack("b", b));

    public UNK write_ubyte(b) {
        this.write(struct.pack("B", b));

    public UNK write_dword(dw) {
        this.write(struct.pack("<l", dw));

    public UNK write_word(w) {
        this.write(struct.pack("<h", w));

    public UNK write_image(image) {
        if (image) {
            this.write_dword(len(image));
            this.write(image);
        } else {
            this.write_dword(0);

def main():
    pass;

if __name__ == "__main__":
    main();
