using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class DataController : MonoBehaviour
{
    [SerializeField,Header("�����L���O�\���p��Text")]
    Text viewText;
    [SerializeField] 
    Button dataButton;�@
    [SerializeField] 
    InputField nameField, commentField;                   //�X�v���b�g�V�[�gID URL�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@                    //�V�[�g��(��ʉE���Q��)
    string url = "https://docs.google.com/spreadsheets/d/1ETYf6iIiRoeGH4gqHe6vv5UjS2563xMhhNoO5BspwY0/gviz/tq?tqx=out:csv&sheet=RankData";
                     //AppsScript�̃f�v���CURL
    string gasUrl = "https://script.google.com/macros/s/AKfycbx1-Slce8jqs9cv39lEK0w1ykyrVQKGeWyaSbTBh4N5oFIY_vgmfMB0Qoc6PDasfoLW/exec";
    //������̃��X�g
    List<string> datas = new List<string>();

    void Start()
    {
        //�f�[�^����
        StartCoroutine(GetData());
        dataButton.onClick.AddListener(() => StartCoroutine(PostData()));
    }

    IEnumerator PostData()
    {
        //WWWForm�^�̃C���X�^���X�𐶐�
        WWWForm form = new WWWForm();

        //���ꂼ���InputField��������擾
        string nameText = nameField.text;
        string commentText = commentField.text;

        //�l����̏ꍇ�͏����𒆒f
        if (string.IsNullOrEmpty(nameText) 
            //|| string.IsNullOrEmpty(commentText)
            )
        {
            Debug.Log("���͂��Ȃ�");
            yield break;
        }

        //���ꂼ��̒l���J���}��؂��combinedText�ϐ��ɑ��
        string combinedText = string.Join(",", nameText
            //, commentText
            );

        //form��Post�������val�Ƃ����L�[�A�l��combinedText�Œǉ�����
        form.AddField("val", combinedText);

        //UnityWebRequest���g����Google Apps Script�pURL��form����Post���M����
        using (UnityWebRequest req = UnityWebRequest.Post(gasUrl, form))
        {
            //���𑗐M
            yield return req.SendWebRequest();

            //���N�G�X�g�������������ǂ����̔���
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
        //UnityWebRequest�^�I�u�W�F�N�g
        using (UnityWebRequest req = UnityWebRequest.Get(url)) 
        {
            //URL�Ƀ��N�G�X�g�𑗂�
            yield return req.SendWebRequest();
            //���������ꍇ
            if (IsWebRequestSuccessful(req)) 
            {
                //�󂯎�����f�[�^�𐮌`����֐��ɏ���n��
                ParseData(req.downloadHandler.text);
                //�f�[�^��\������
                DisplayText();
            }
            else                            
            {
                //�����o���Ȃ������ꍇ
                Debug.Log("�f�[�^�����Ɏ��s");
            }
        }
    }

    //�f�[�^�𐮌`����֐�
    void ParseData(string csvData)
    {
        //�X�v���b�h�V�[�g��1�s���z��Ɋi�[
        string[] rows = csvData.Split(new[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries); 
        foreach (string row in rows)
        {
            //��s���̏���1�Z�����z��Ɋi�[
            string[] cells = row.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string cell in cells)
            {
                //�Z���̕����񂩂�_�u���N�H�[�e�[�V����������
                string trimCell = cell.Trim('"');
                //�������������񂪋󔒂łȂ����datas�ɒǉ����Ă���
                if (!string.IsNullOrEmpty(trimCell)) 
                {
                    datas.Add(trimCell);
                }
            }
        }
    }

    //������\��������֐�
    void DisplayText()
    {
        foreach (string data in datas)
        {
            viewText.text += data + "\n";
        }
    }

    //���N�G�X�g�������������ǂ������肷��֐�
    bool IsWebRequestSuccessful(UnityWebRequest req)
    {
        /*�v���g�R���G���[�ƃR�l�N�g�G���[�ł͂Ȃ��ꍇ��true��Ԃ�*/
        return req.result != UnityWebRequest.Result.ProtocolError &&
               req.result != UnityWebRequest.Result.ConnectionError;
    }
}
