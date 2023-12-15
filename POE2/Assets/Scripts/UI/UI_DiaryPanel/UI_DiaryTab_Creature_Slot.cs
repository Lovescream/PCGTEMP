using UnityEngine;
using UnityEngine.UI;

public class UI_DiaryTab_Creature_Slot : MonoBehaviour {

    public Image creatureSpriteBorder;
    public Image creatureSprite;
    public Text creatureName;

    private CreatureData creature;
    private UI_Panel_Diary diaryPanel;

    public void Initialize(CreatureData creature) {
        diaryPanel = UIManager.Instance.diaryPanel;
        this.creature = creature;
        if (creature == null) {
            this.gameObject.SetActive(false);
        }
        else {
            this.gameObject.SetActive(true);
            creatureSprite.sprite = creature.sprite;
            creatureName.text = creature.creatureName;
        }
    }

    public void OnBtn() {
        diaryPanel.creatureInfoTab.SetCreature(creature);
        diaryPanel.ActivateTab(diaryPanel.creatureInfoTab);
    }
}