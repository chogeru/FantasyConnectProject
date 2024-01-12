using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    // UIのCanvasオブジェクトへの参照
    [SerializeField,Header("開始時のセレクトボタン")]
    private GameObject m_TitleCanvas;
    [SerializeField,Header("開始時のText用Canvas")]
    private GameObject m_TextCanvas;
    [SerializeField]
    private GameObject m_Fade;
    [SerializeField]
    CurrencySystem m_CurrencySystem;
    [SerializeField]
    InventorySystem m_InventorySystem;
    [SerializeField]
    SceneSave m_SceneData;
    // フェードインの速度調整用パラメータ
    [SerializeField]
    private float m_EaseSpeed = 0.01f;

    // 現在のアルファ値を保持する変数
    private float m_CurrentAlpha = 0.0f;

    // フレームごとに呼び出されるUnityのUpdateメソッド
    private void Update()
    {
        // 任意のキーが押された場合、フェードイン処理を開始する
        if (Input.anyKeyDown)
        {
            m_TextCanvas.SetActive(false);
            m_TitleCanvas.SetActive(true);
            StartCoroutine(FadeInCanvas());
        }
    }
    public void ResetStart()
    {
        m_CurrencySystem.ResetCurrency();
        m_InventorySystem.ResetItem();
        m_SceneData.ResetSceneData();
        SceneController.SceneConinstance.LoadSceneWithLoadingScreen("TutorialScene");
    }
    // タイトル画面のCanvasをフェードインさせるコルーチン
    private IEnumerator FadeInCanvas()
    {
        // アルファ値が1.0未満の間、フェードイン処理を実行
        while (m_CurrentAlpha < 1.0f)
        {
            // アルファ値を時間経過と速度に基づいて増加させ、0.0から1.0にクランプする
            m_CurrentAlpha += Time.deltaTime * m_EaseSpeed;
            m_CurrentAlpha = Mathf.Clamp01(m_CurrentAlpha);

            // CanvasオブジェクトがCanvasGroupを持っている場合、alphaプロパティに現在のアルファ値を適用
            CanvasGroup canvasGroup = m_TitleCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = m_CurrentAlpha;
            }
            // CanvasオブジェクトがCanvasGroupを持っていない場合、Imageコンポーネントのcolorのalphaに現在のアルファ値を適用
            else
            {
                Image image = m_TitleCanvas.GetComponent<Image>();
                if (image != null)
                {
                    Color newColor = image.color;
                    newColor.a = m_CurrentAlpha;
                    image.color = newColor;
                }
            }

            yield return null;
        }

        // ループを抜けた後、最終的にアルファ値を1.0に設定して、完全にフェードインが終了したことを確認
        CanvasGroup finalCanvasGroup = m_TitleCanvas.GetComponent<CanvasGroup>();
        if (finalCanvasGroup != null)
        {
            finalCanvasGroup.alpha = 1.0f;
            m_Fade.SetActive(false);
        }
        else
        {
            Image finalImage = m_TitleCanvas.GetComponent<Image>();
            if (finalImage != null)
            {
                Color finalColor = finalImage.color;
                finalColor.a = 1.0f;
                finalImage.color = finalColor;
            }
        }
    }
}