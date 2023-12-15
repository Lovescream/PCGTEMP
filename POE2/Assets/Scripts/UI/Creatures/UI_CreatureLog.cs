using UnityEngine;
using UnityEngine.UI;

public class UI_CreatureLog : MonoBehaviour {

    public CreatureData creature;

    public string content;

    private Text text;

    void Awake() {
        text = this.GetComponent<Text>();
    }

    public void SetLog(CreatureData creature, int index) {
        this.creature = creature;
        content = creature.logList[index].log;

        UpdateUI();
    }

    public void UpdateUI() {
        text.text = content;

        Vector2 size = text.rectTransform.sizeDelta;
        size.y = text.preferredHeight;
        text.rectTransform.sizeDelta = size;
    }

}