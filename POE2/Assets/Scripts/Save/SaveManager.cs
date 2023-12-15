using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager {
    private static SaveManager instance;
    public static SaveManager Instance {
        get {
            if (instance == null) instance = new SaveManager();
            return instance;
        }
    }

    private string extension = ".dat";

    public SaveData saveData;

    // GetCurrent: saveData를 반환. 없다면 Initialize("Test") 하여 반환.
    public static SaveData CurrentData() {
        if (Instance.saveData == null) Instance.Initialize("Test");
        return Instance.saveData;
    }

    public string GetFilePath(string fileName) => GetStreamingAssetsPath() + "/" + fileName + extension;
    // IsExist: StreamingAssets 경로에 해당 fileName이 있는지 확인.
    public bool IsExist(string fileName) {
        string path = GetFilePath(fileName);
        return File.Exists(path);
    }

    // Initialize: 해당 파일 초기화. (로드 or 생성)
    // StreamingAssets 안의 fileName 파일을 saveData에 로드. 실패시 새 세이브 파일 생성.
    public void Initialize(string fileName) {
        // StreamingAssets 안에 해당 파일이 있다면, 로드하여 saveData에 넣는다.
        if (IsExist(fileName)) {
            saveData = Load(fileName);
        }
        // 파일이 없다면 새 세이브 파일 생성.
        else {
            saveData = new SaveData();
            saveData["SaveFileName"] = fileName;
            Save(saveData, fileName);
        }
    }




    // SaveData: 현재 세이브 데이터에 'dataName' 데이터를 'dataValue'로 설정하여 파일을 저장.
    public void SaveData(string dataName, object dataValue) {
        // #1. 현재 세이브 데이터 받아오기.
        SaveData currentSaveData = CurrentData();

        // #2. 해당 세이브 데이터에 해당 데이터를 저장.
        currentSaveData.SetData(dataName, dataValue);

        // #3. 파일 저장.
        Save();
    }

    public void Save() {
        SaveData saveData = CurrentData();
        string fileName = saveData.LoadData("SaveFileName", string.Empty);
        Save(saveData, fileName);
    }

    public void Save(SaveData saveData, string fileName) {
        // #1. 파일 경로 정하기.
        string path = GetStreamingAssetsPath();
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        path += ("/" + fileName + extension);

        // #2. 세이브 파일 준비.
        saveData.ReadyToSave();

        // #3. Serialize.
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Create(path);
        binaryFormatter.Serialize(fileStream, saveData);
        fileStream.Close();
    }
    public SaveData Load(string fileName) {
        // #1. 파일 경로 정하기.
        string path = GetFilePath(fileName);
        if (!File.Exists(path)) {
            Debug.LogError("File does not exist:" + path);
            return null;
        }

        // #2. Deserialize.
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Open(path, FileMode.Open);
        SaveData saveData;
        if (fileStream != null && fileStream.Length > 0) saveData = (SaveData)binaryFormatter.Deserialize(fileStream);
        else saveData = default;
        fileStream.Close();

        // #3. DataDic에 넣기.
        foreach (DataContainer dataContainer in saveData.serializeTarget)
            saveData[dataContainer.dataName] = dataContainer.dataValue;
        saveData.serializeTarget = null;


        return saveData;
    }







    // StreamingAssets 폴더 받기.
    public static string GetStreamingAssetsPath() { return Application.streamingAssetsPath.Replace('/', Path.DirectorySeparatorChar); }

}