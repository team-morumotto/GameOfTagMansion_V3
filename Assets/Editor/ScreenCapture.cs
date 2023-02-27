/*
参考文献
～コードのベース～
[Unity]Unity Editor上でスクリーンショットを撮る方法 - https://nobushiueshi.com/unityunity-editor上でスクリーンショットを撮る方法/

～プラットフォーム判定～
Unity プラットフォーム判別 - https://qiita.com/Ubermensch/items/75072ef89249cb3b30e7
RuntimePlatform - https://docs.unity3d.com/ScriptReference/RuntimePlatform.html

～ファイラーで表示する方法～
【Unity】 セーブファイルのパスを開くメニューを作る - https://www.urablog.xyz/entry/2021/11/05/070000
How to Open a File or Folder in Terminal on Mac - https://www.switchingtomac.com/tutorials/how-to-open-a-file-or-folder-in-terminal-on-mac/
【 xdg-open 】コマンド――ファイルをデフォルトアプリケーションで開く - https://atmarkit.itmedia.co.jp/ait/articles/1906/06/news007.html

～ディレクトリの有無判定～
Unity フォルダやファイルの有無を確認し無ければ生成する - https://hakase0274.hatenablog.com/entry/2019/07/27/223840
[unity]指定されたファイルがどのパスにあるか調べる - https://www.create-forever.games/unity-directory-getfiles/
*/

using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ScreenCapture {
    [MenuItem("Tools/Screen Capture")]

    public static void Capture() {
        //## ｺｺｶﾗ 変数宣言 ##//
        Camera camera = Camera.main;                                    // カメラオブジェクトの取得
        DateTime now = DateTime.Now;                                    // 現在時刻を取得
        string folderName = "Screenshot";                               // 格納先フォルダ名
        string headerName = "ScreenShot";                               // ファイル名の先頭
        string productName = Application.productName;                   // プロジェクト名を取得
        string fileName = "ScreenShot_1920x1080_2019-01-01_000000.png"; // ファイル名の例 本当はここに入れなくてもいい（後で上書きするから....）
        var size = new Vector2Int((int)Handles.GetMainGameViewSize().x, (int)Handles.GetMainGameViewSize().y); // Game 画面のサイズを取得
        var render = new RenderTexture(size.x, size.y, 0);                        // RenderTextureの生成。最後の0だけは理解できない。ﾅﾆｺﾚ?
        var texture = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false); // RGB24 -> ARGB32 に変えることで透明度を保持
        //## ｺｺﾏﾃﾞ変数宣言 ##//

        // 取得した情報でファイル名を生成 ヘッダー名にプロジェクト名を入れる場合はproductNameを使う
        fileName = string.Format("{0}_{1}x{2}_{3}-{4:D2}-{5:D2}_{6:D2}{7:D2}{8:D2}.png", headerName, Screen.width, Screen.height, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        try {
            // カメラ画像を RenderTexture に描画
            camera.targetTexture = render;
            camera.Render();

            // RenderTexture の画像を読み取って出力する
            RenderTexture.active = render;
            texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            texture.Apply();
        }
        finally {
            // 処理後にデータを空っぽにする
            camera.targetTexture = null;
            RenderTexture.active = null;
        }

        // ディレクトリなかったらフォルダを作る
        if (!System.IO.Directory.Exists(folderName)) {
            // ディレクトリ作成
            System.IO.Directory.CreateDirectory(folderName);
        }

        // PNG 画像としてファイル保存
        File.WriteAllBytes(
            $"{folderName}/{fileName}", texture.EncodeToPNG()
        );

        // 保存したファイルパスをデバッグログに表示
        Debug.Log("Saved: " + $"{folderName}/{fileName}");

        // 保存したやつをOSごとのファイラーで表示
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            // Windows
            string path = ($"{Application.dataPath}/../{folderName}/{fileName}").Replace('/', '\\');
            System.Diagnostics.Process.Start("explorer.exe", $"/select,{path}");
        }
        else if (Application.platform == RuntimePlatform.OSXEditor) {
            // MacOS
            System.Diagnostics.Process.Start("open", $"{folderName}/{fileName}");
        }
        else if (Application.platform == RuntimePlatform.LinuxEditor) {
            // Linux
            System.Diagnostics.Process.Start("xdg-open", $"{folderName}/{fileName}");
        }
        else {
            // その他
            Debug.Log("対応外のため開けませんでした。自分で開いてください。ごめんね。");
        }
    }
}