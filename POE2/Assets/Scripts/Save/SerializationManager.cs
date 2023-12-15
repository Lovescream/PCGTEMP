using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializationManager {

    public static bool Save(string saveName, object saveData) {
        // BianryFormatter.
        BinaryFormatter formatter = GetBinaryFormatter();
        // Directory Check.
        if (!Directory.Exists(Application.persistentDataPath + "/saves"))
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        // File Path.
        string path = Application.persistentDataPath + "/saves/" + saveName + ".save";
        // Create File.
        FileStream file = File.Create(path);

        // Serialize.
        formatter.Serialize(file, saveData);

        // Serialize End.
        file.Close();

        return true;
    }

    public static object Load(string path) {
        // File Path Check.
        if (!File.Exists(path)) return null;
        // BianryFormatter.
        BinaryFormatter formatter = GetBinaryFormatter();
        // File Open.
        FileStream file = File.Open(path, FileMode.Open);
        
        // Deserialize.
        try {
            object save = formatter.Deserialize(file);
            file.Close();
            return save;
        }
        catch {
            Debug.LogErrorFormat("Failed to load file at {0}", path);
            file.Close();
            return null;
        }
    }

    public static BinaryFormatter GetBinaryFormatter() {
        BinaryFormatter formatter = new BinaryFormatter();
        //SurrogateSelector selector = new SurrogateSelector();
        //Vector3SerializationSurrogate vector3Surrogate = new Vector3SerializationSurrogate();
        //QuaternionSerializationSurrogate quaternionSurrogate = new QuaternionSerializationSurrogate();

        //selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
        //selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSurrogate);

        //formatter.SurrogateSelector = selector;

        return formatter;
    }

}