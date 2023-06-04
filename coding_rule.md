# チームSmile_waya コーディング規約 for C# & Unity

## 参考文献

[ベストなコーディング規約の作り方](https://qiita.com/tadnakam/items/5d1280559eb75b29847c)

## 命名規則

原則として下記のサイトを参考にする。
- [C# のコーディング規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [C# 識別子の名前付け規則と表記規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [真偽値を返す関数のネーミング](https://qiita.com/yskszk/items/5a7f99c974773f03a82a)

### 命名規則に関する個別規約

- 原則として変数名にはキャメルケースを使用すること。  
- クラス、モジュールにはパスカルケースを使用すること。
- 定数には半角大文字を使用すること。
- ファイル名はパスカルケースを使用すること。
- 名前をつける際は「使用目的」に応じたわかりやすい名前にすること。
- 英語、日本語を変数名に使用しないこと。
  - 英単語に自信がない場合は必ず第三者に確認すること。
- 1文字、もしくは2文字などの短い変数名を使用する場合は下記の原則に従うこと。
  - 短い変数名はメソッド内のみで使用すること。
  - `For`や`While`ループでのインデックス(カウンタ)として使う変数は、`i, j, k`を使う。その際、一番外側のループから順に`i, j, k`を使用すること。
  - 座標等指定には`x, y`を使う、もしくは入れること。
- 2バイト文字をクラス名、変数名、ファイル名には原則使用しないこと。(文字化け防止)
- `Enum`には2バイト文字を使用してはならない

## コーディングスタイル

原則として下記のサイトを参考にする。
- [C# のコーディング規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [C# 識別子の名前付け規則と表記規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/identifier-names)

また、拡張機能等を使用した整形等は行わないこと。

### コーディングスタイルに関する個別規約

- 行数、列数を意識して書くこと。あまりに長い場合は分散を視野に入れる (見やすさの為)
- 入れ子は最大3つ、推奨は2つまで (見やすさの為)
- 関数（ブロック）の長さは長くても50行程度で収める (見やすさの為)
- 変数を置く際、`Public`、`Private`、`Protected`、`Unity`などを分けた上でその中で`int`、`float`、`string`、`bool`などを分けること (見やすさの為)
- マジックナンバーは使用しないこと。必ず定数を使用するか、変数に格納すること。また、それぞれに使用用途をコメントで記載すること。
- 独自例外は原則使用しないこと。 (保守管理の容易性)

### コメント

- コメントは原則日本語で記述すること (見やすさの為)
- 関数には必ず`summary`で機能などのコメントを記載すること (保守管理の容易性)
- 既存のコードを修正、置き換えをした際、変更が必要なほかのコメントも同時に変更すること (該当箇所以外もだいたい挙動が変わってくるので)
- コードに関係ないコメントは原則不要とする。また一時的なものはリリースまでに削除する。
- 意味がなくなった、使わなくなったコメントを削除すること (GitHubなどで確認することができるため)
- 変更履歴などはファイルの先頭にまとめる
- ただし**コメントが多すぎる**と可読性を下げるため、コメントは**必要最低限**に留めること

### リソース

- リソース (ここでは画像、音声、モデルデータなど) は原則`Resources`フォルダに格納すること。
- アプリケーションのバージョンは必ずリリースしていなくても「ビルドした回数」に応じて上げること。
  - バージョンの付け方に関しては [このサイト](https://future-architect.github.io/articles/20220426a/) を参考にすること。

### 禁止事項

- ネットや書物からコピーしたプログラムのソースを使用しないこと (信頼性の欠如)
- ネットや書物からコピーしたプログラムのソースを使用する場合には必ず参考文献を明記し、自分の言葉で書き直したり理解をすること
- デバッグのためのコードは必ず使用後に削除すること (保守管理の容易性)

### 制限事項

- 演算子の前後には空白を入れること。
- カンマの後には空白を入れること。
- セミコロンの前には空白を入れないこと。
- セミコロンの後には空白を入れること。
- カッコの前には空白を入れないこと。
- カッコの中には空白を入れないこと。
- カッコの後には空白を入れること。

---

- カンマの後には改行を入れること。
- セミコロンの後には改行を入れること。
- カッコの後には改行を入れること。
- カッコの中には改行を入れないこと。

---

`{` は改行せず書くこと
```csharp
void Start() {}
void Update() {
    // ...
}
```

上記を踏まえて....  
変数宣言の際、同じような動きをするものは、1行にまとめて書くことを推奨する。

```csharp
int timeStamp = Time.deltaTime;
int timeStamp = Time.deltaTime + 1;
int timeStamp = Time.deltaTime + 1, timeStamp = Time.deltaTime + 2;
int timeStamp = Time.deltaTime; int timeStamp = Time.deltaTime + 1;
void test() {
    if(timeStamp_minuet > 0) {
        timeStamp_minuet = 0;
    }
}
```

### 推奨事項

現在の所ありません。