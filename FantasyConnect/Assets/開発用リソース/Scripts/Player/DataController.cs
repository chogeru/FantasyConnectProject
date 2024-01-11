using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class DataController : MonoBehaviour
{
    [SerializeField,Header("ランキング表示用のText")]
    Text viewText;
    [SerializeField] 
    Button dataButton;　
    [SerializeField] 
    InputField nameField, commentField;                   //スプレットシートID URL　　　　　　　　　　　　　　　                    //シート名(画面右下参照)
    string url = "https://docs.google.com/spreadsheets/d/1ETYf6iIiRoeGH4gqHe6vv5UjS2563xMhhNoO5BspwY0/gviz/tq?tqx=out:csv&sheet=RankData";
                     //AppsScriptのデプロイURL
    string gasUrl = "https://script.google.com/macros/s/AKfycbx1-Slce8jqs9cv39lEK0w1ykyrVQKGeWyaSbTBh4N5oFIY_vgmfMB0Qoc6PDasfoLW/exec";
    //文字列のリスト
    List<string> datas = new List<string>();

    void Start()
    {
        //データ所得
        StartCoroutine(GetData());
        dataButton.onClick.AddListener(() => StartCoroutine(PostData()));
    }

    IEnumerator PostData()
    {
        //WWWForm型のインスタンスを生成
        WWWForm form = new WWWForm();

        //それぞれのInputFieldから情報を取得
        string nameText = nameField.text;
        string commentText = commentField.text;

        //値が空の場合は処理を中断
        if (string.IsNullOrEmpty(nameText) 
            //|| string.IsNullOrEmpty(commentText)
            )
        {
            Debug.Log("入力がない");
            yield break;
        }

        //それぞれの値をカンマ区切りでcombinedText変数に代入
        string combinedText = string.Join(",", nameText
            //, commentText
            );

        //formにPostする情報をvalというキー、値はcombinedTextで追加する
        form.AddField("val", combinedText);

        //UnityWebRequestを使ってGoogle Apps Script用URLにform情報をPost送信する
        using (UnityWebRequest req = UnityWebRequest.Post(gasUrl, form))
        {
            //情報を送信
            yield return req.SendWebRequest();

            //リクエストが成功したかどうかの判定
            if (IsWebRequestSuccessful(req))
            {

                Debug.Log("success");
            }
            else
            {
                Debug.Log("error");
            }
        }
    }
    IEnumerator GetData()
    {
        //UnityWebRequest型オブジェクト
        using (UnityWebRequest req = UnityWebRequest.Get(url)) 
        {
            //URLにリクエストを送る
            yield return req.SendWebRequest();
            //成功した場合
            if (IsWebRequestSuccessful(req)) 
            {
                //受け取ったデータを整形する関数に情報を渡す
                ParseData(req.downloadHandler.text);
                //データを表示する
                DisplayText();
            }
            else                            
            {
                //所得出来なかった場合
                Debug.Log("データ所得に失敗");
            }
        }
    }

    //データを整形する関数
    void ParseData(string csvData)
    {
        //スプレッドシートを1行ずつ配列に格納
        string[] rows = csvData.Split(new[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries); 
        foreach (string row in rows)
        {
            //一行ずつの情報を1セルずつ配列に格納
            string[] cells = row.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string cell in cells)
            {
                //セルの文字列からダブルクォーテーションを除去
                string trimCell = cell.Trim('"');
                //除去した文字列が空白でなければdatasに追加していく
                if (!string.IsNullOrEmpty(trimCell)) 
                {
                    datas.Add(trimCell);
                }
            }
        }
    }

    //文字を表示させる関数
    void DisplayText()
    {
        foreach (string data in datas)
        {
            viewText.text += data + "\n";
        }
    }

    //リクエストが成功したかどうか判定する関数
    bool IsWebRequestSuccessful(UnityWebRequest req)
    {
        /*プロトコルエラーとコネクトエラーではない場合はtrueを返す*/
        return req.result != UnityWebRequest.Result.ProtocolError &&
               req.result != UnityWebRequest.Result.ConnectionError;
    }
}
