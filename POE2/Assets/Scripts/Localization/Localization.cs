using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[Serializable]
public class Localization {
    private Dictionary<string, Font> fontDic = new Dictionary<string, Font>();
    // key: 언어 이름.
    // value: 언어팩.
    [SerializeField]
    private Dictionary<string, Dictionary<string, string>> packages;
    [SerializeField]
    private string currentLanguage;
    [SerializeField]
    public Action cbOnSetLanguage;
    [SerializeField]
    private static Localization instance;
    public static Localization Instance {
        get {
            if (instance == null) instance = new Localization();
            return instance;
        }
    }

    private Localization() {
        // 패키지 초기화.
        packages = new Dictionary<string, Dictionary<string, string>>();
        packages["korean"] = new Dictionary<string, string>();
        packages["english"] = new Dictionary<string, string>();
        packages["japanese"] = new Dictionary<string, string>();
        packages["simplified_chinese"] = new Dictionary<string, string>();
        packages["traditional_chinese"] = new Dictionary<string, string>();
        // 현재 언어 설정 불러오기.
        string language = PlayerPrefs.GetString("Language", string.Empty);
        // 언어 설정이 되어있지 않으면, 시스템 언어로 언어 설정.
        if (language == string.Empty) {
            switch (Application.systemLanguage) {
                case SystemLanguage.Japanese:
                    language = "japanese";
                    break;
                case SystemLanguage.Korean:
                    language = "korean";
                    break;
                case SystemLanguage.ChineseSimplified:
                    language = "simplified_chinese";
                    break;
                case SystemLanguage.ChineseTraditional:
                    language = "traditional_chinese";
                    break;
                default:
                    language = "english";
                    break;
            }
            // 언어 설정 저장.
            PlayerPrefs.SetString("Language", language);
            PlayerPrefs.Save();
        }
        // 언어 설정.
        SetLanguage(language);

        LoadXML();
        LoadFont();
    }

    public Font GetFont() {
        if (fontDic.ContainsKey(currentLanguage))
            return fontDic[currentLanguage];
        else return null;
    }

    public string GetCurrentLanguage() {
        return currentLanguage;
    }

    // LoadXML: Xml File Load.
    private void LoadXML() {
        XmlDocument xmlDocument = new XmlDocument();
        TextAsset textAsset = Resources.Load<TextAsset>("Localization/texts");
        if (!textAsset) Debug.Log("XML File Load Failed.");
        else {
            xmlDocument.LoadXml(textAsset.text);
            if (xmlDocument == null) return;
            XmlNodeList keyNodes = xmlDocument.DocumentElement.ChildNodes;
            for (int i = 0; i < keyNodes.Count; ++i) {
                string key = keyNodes[i].Attributes["name"].Value;
                packages["korean"]["key"] = string.Empty;
                packages["english"]["key"] = string.Empty;
                packages["japanese"]["key"] = string.Empty;
                packages["simplified_chinese"]["key"] = string.Empty;
                packages["traditional_chinese"]["key"] = string.Empty;
                XmlNodeList languageNodes = keyNodes[i].ChildNodes;
                for (int j = 0; j < languageNodes.Count; ++j) {
                    switch (languageNodes[j].Name) {
                        case "korean":
                            packages["korean"][key] = languageNodes[j].InnerText;
                            break;
                        case "japanese":
                            packages["japanese"][key] = languageNodes[j].InnerText;
                            break;
                        case "english":
                            packages["english"][key] = languageNodes[j].InnerText;
                            break;
                        case "simplified_chinese":
                            packages["simplified_chinese"][key] = languageNodes[j].InnerText;
                            break;
                        case "traditional_chinese":
                            packages["traditional_chinese"][key] = languageNodes[j].InnerText;
                            break;
                    }
                }
            }
        }
    }

    // LoadFont: Font Load.
    public void LoadFont() {
        Font[] fonts = Resources.LoadAll<Font>("Fonts");
        for (int i = 0; i < fonts.Length; ++i) {
            if (fonts[i]) {
                //fonts[i].material.mainTexture.filterMode = FilterMode.Point;
                fontDic[fonts[i].name] = fonts[i];
            }
        }
    }
    
    // SetLanguage: 해당 언어로 교체.
    public void SetLanguage(string language) {
        currentLanguage = language;
        PlayerPrefs.SetString("Language", currentLanguage);
        PlayerPrefs.Save();
        cbOnSetLanguage?.Invoke();
    }

    public Sprite GetSprite(string key) {
        string str = "." + currentLanguage;
        Sprite sprite = Resources.Load<Sprite>("Localization/Sprites/" + key + str);
        if (!sprite) sprite = Resources.Load<Sprite>("Localization/Sprites/" + key + ".english");
        return sprite;
    }

    public string GetText(string key) {
        string text = string.Empty;
        if (packages.ContainsKey(currentLanguage) && key != null && packages[currentLanguage].ContainsKey(key.Trim()))
            text = packages[currentLanguage][key.Trim()];
        return text;
    }
}
