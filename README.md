# CardWirthCSharp
CardWirthPyをC#で書き直し、Unityでも動作可能にするプロジェクト

## 書き換え方針
* ロジック部のみを書き換えていく（UI系はUnityで作成するため無視する)
* \*.py をコピーして \*.cs を作成し、Python のコード部をコメントアウトする

```
class AAA(BBB):
    def __init__():
        pass
```
なら
```
//class AAA(BBB):
//    def __init__():
//        pass
```

とする

* PythonコードをC#のコードに書き換えたら、そのコメントアウトしたコードを削除する
* Pythonにしか無いような記法の部分は、行の最後に```// TODO```をつけておく
```
//    self.seq = [(a + 1, a + 2) for a in self.raw]
```
なら
```
    this.seq = [(a + 1, a + 2) for a in self.raw]; // TODO
```

とする

* モジュールに直接定義した関数は、static の class F を作成して、そのクラスメソッドとする
```
//def hoge(x):
```
なら
```
public static class F
{
    public static UNK hoge(UNK x)
    {
```
とする

* 型が分からない変数は、型を```UNK```とする

## Python C# 対応表

| 名前 | Python | C# |
----|----|---- 
| コメント | ```#``` | ```//``` |
| 関数 | ```def hoge(self, aaa, bbb):``` | ```public UNK hoge(UNK aaa, UNK bbb) {``` |
| なし | ```None``` | ```null``` |
| and (ブール演算) | ```hoge and fuge``` | ```hoge && fuge``` |
| or (ブール演算) | ```hoge or fuge``` | ```hoge || fuge``` |
| if | ```if aaa > bbb:``` | ```if (aaa > bbb) {``` |
| cast int | ```int(hoge)``` | ```(int)hoge``` |
| 行末 | なし | ```;``` （セミコロン） |
| class | ```class AAA(BBB):``` | ```class AAA : BBB {```|
| インスタンスメソッド呼び出し | ```self.hoge()``` | ```this.hoge();```|
----|----|---- 
