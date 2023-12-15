using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour {

    private static TitleManager instance;
    public static TitleManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<TitleManager>();
            }
            return instance;
        }
        set {
            instance = value;
        }
    }

    public UI_Panel_SaveSlotPanel slotPanel;

    void Start() {

    }

    public void OnBtnStart() {
        slotPanel.OpenPanel();
    }

    public void OnBtnExit() {
        QuitGame();
    }


    //public void DeleteSaveSlotData(string saveFileName) {
    //    if (SaveManager.Instance.IsExist(saveFileName)) {

    //    }
    //    if (File.Exists(Application.streamingAssetsPath + "\\" + slotName + SaveData.extension))
    //        TitleUIManager.Instance.OpenPopup(I._("UI_Main_DeleteMessage"), (UnityAction)(() =>
    //        {
    //            int num = 1;
    //            while (true) {
    //                if (File.Exists(Application.streamingAssetsPath + "\\BackUps\\" + slotName + SaveData.backExtension + (object)num)) {
    //                    Debug.Log((object)("백업본 " + (object)num + " 삭제"));
    //                    File.Delete(Application.streamingAssetsPath + "\\BackUps\\" + slotName + SaveData.backExtension + (object)num);
    //                    ++num;
    //                }
    //                else
    //                    break;
    //            }
    //            Debug.Log((object)((num - 1).ToString() + " : 마지막 백업 인덱스"));
    //            File.Delete(Application.streamingAssetsPath + "\\" + slotName + SaveData.extension);
    //            if (!(bool)(UnityEngine.Object)this.slotPanel)
    //                return;
    //            this.slotPanel.ClosePanel();
    //            this.slotPanel.OpenPanel();
    //        }), (UnityAction)null);
    //    else
    //        Debug.Log((object)"삭제할 데이터가 존재하지 않음");
    //}

    public void GameStart(SaveData saveData) {
        SaveManager.Instance.Initialize(saveData.LoadData("SaveFileName", string.Empty));
        UI_Effect_ScreenFade.Instance.FadeOut(LoadMainWorld);
    }

    private void LoadMainWorld() {
        SceneManager.LoadScene("MainWorld");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
