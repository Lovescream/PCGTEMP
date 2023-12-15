using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {

    public DataContainer[] serializeTarget;

    private Dictionary<string, object> dataDic = new Dictionary<string, object>();

    public object this[string dataName] {
        get {
            if (dataDic.ContainsKey(dataName))
                return dataDic[dataName];
            else
                return default;
        }
        set {
            dataDic[dataName] = value;
        }
    }
    public bool HasKey(string dataName) { return dataDic.ContainsKey(dataName); }
    public void SetData(string dataName, object dataValue) {
        dataDic[dataName] = dataValue;
    }
    public T LoadData<T>(string dataName, T defaultValue) {
        if (HasKey(dataName)) return (T)dataDic[dataName];
        else return defaultValue;
    }

    public void Load() {
        this.dataDic = SaveManager.Instance.Load("TestSave").dataDic;
    }
    public void ReadyToSave() {
        List<DataContainer> dataContainerList = new List<DataContainer>();
        foreach (string key in dataDic.Keys) {
            object dataValue = dataDic[key];
            dataContainerList.Add(new DataContainer(key, dataValue));
        }
        serializeTarget = dataContainerList.ToArray();
    }
}

[Serializable]
public class DataContainer {
    public string dataName;
    public object dataValue;

    protected DataContainer() { }

    public DataContainer(string dataName, object dataValue) {
        this.dataName = dataName;
        this.dataValue = dataValue;
    }
}