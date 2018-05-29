public static class F
{
    public const BINARY_HEADER = "binaryimage://";
    public static bool path_is_code(UNK path) {
        // """pathがバイナリイメージのテキスト表現であればtrue。""";
        return path.startswith(BINARY_HEADER);
    }
    
    public static string code_to_data(UNK code) {
        // """テキスト表現codeをバイナリイメージへ変換する。""";
        if (path_is_code(code)) {
            return base64.b64decode(code[len(BINARY_HEADER):]); // TODO
        }
        return "";
    }
    
    public static string data_to_code(UNK data) {
        """バイナリイメージdataをテキスト表現へ変換する。""";
        if (len(data)) {
            return BINARY_HEADER + base64.b64encode(data);
        }
        return "";
    }
}
