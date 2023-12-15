using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DiaryTab_CreatureInfo : UI_DiaryTab {

    [Header("크리쳐 기본 정보")]
    public Image creatureSprite;
    public Text creatureName;
    [Header("*로그 정보")]
    public Text logText;
    public int maxLogLineCount;
    [Header("*페이지 버튼")]
    public GameObject btnPrevPage;
    public GameObject btnNextPage;

    private int maxPage;
    private int currentPage;
    private int logCount;

    private Dictionary<int, int> pageDictionary;

    private CreatureData currentCreature;
    private TextGenerator textGenerator;

    public override void Initialize() {
        base.Initialize();

        textGenerator = logText.cachedTextGenerator;

        // #1. 크리쳐의 로그를 바탕으로 페이지를 세팅한다.
        logCount = currentCreature.logList.Count;
        PageInitialize();

        // #2. 크리쳐 기본 정보 설정.
        creatureSprite.sprite = currentCreature.sprite;
        creatureName.text = currentCreature.creatureName;
        // #3. 초기 로그 페이지 설정.
        currentPage = 0;
        SetPage(0);
    }

    public bool IsEmpty() => logText.text.Length <= 0;

    public void SetCreature(CreatureData creature) => currentCreature = creature;

    #region Page

    // PageInitialize: 페이지 정보 초기화.
    private void PageInitialize() {
        pageDictionary = new Dictionary<int, int>();

        ResetLog();

        bool isFirst = true;

        int page = 0;
        for (int i = 0; i < logCount; i++) {
            // 페이지의 첫 번째인 경우, pageDictionary에 추가.
            if (isFirst) pageDictionary.Add(page, i);

            // 페이지에 로그 추가 성공시,
            if (AddLog(i, isFirst)) {
                isFirst = false;

            }
            // 페이지에 로그 추가 실패시,
            else {
                i--;
                isFirst = true;
                ResetLog();
            }
        }

        maxPage = page;
    }

    private void NextPage() {
        // #1. 이미 최대 페이지에 도달했다면 아무것도 하지 않음.
        if (currentPage >= maxPage) return;

        // #2. 페이지를 넘기고 페이지 세팅.
        currentPage++;
        SetPage();
    }
    private void PrevPage() {
        // #1. 이미 처음 페이지에 도달했다면 아무것도 하지 않음.
        if (currentPage <= 0) return;

        // #2. 페이지를 넘기고 페이지 세팅.
        currentPage--;
        SetPage();
    }

    // SetPage: 해당 페이지로 설정. page = -1이면, currentPage로 설정.
    private void SetPage(int page = -1) {
        // #1. 페이지 로그 초기화.
        ResetLog();

        // #2. page = -1 이면, page를 현재page로 설정한다.
        if (page == -1) page = currentPage;

        // #3. 이 page의 시작과 끝 index를 정한다.
        int startIndex = pageDictionary[page];
        int endIndex = (page + 1 <= maxPage) ? pageDictionary[page] - 1 : -1;

        // #4. 로그 추가.
        AddLog(startIndex, endIndex);

        // #5. 버튼 세팅.
        btnPrevPage.gameObject.SetActive(currentPage > 0);
        btnNextPage.gameObject.SetActive(currentPage < maxPage);
    }

    #endregion

    #region Log

    // AddLog: logText에 해당 내용을 추가한다.
    //        최대 라인을 넘기게 되면 추가하지 않고 false를 리턴한다.
    //        changeLine가 false이면 줄바꿈을 하지 않는다.
    public bool AddLog(string log, bool changeLine = true) {
        // #1. 현재 로그 내용을 originContent 복사.
        string originContent = logText.text;

        // #2. changeLine 여부에 따라 줄바꿈 결정.
        if (changeLine) logText.text += "\n\n";

        // #3. 해당 내용 추가.
        logText.text += log;

        // #4. UI에 구현.
        TextGenerationSettings settings = logText.GetGenerationSettings(logText.rectTransform.rect.size);
        textGenerator.Populate(logText.text, settings);

        // #5. 만일 최대 줄 수를 넘기면 originContent로 되돌린 후 false 반환.
        if (textGenerator.lineCount > maxLogLineCount) {
            logText.text = originContent;
            return false;
        }
        return true;
    }
    // AddLog: 현재 크리쳐의 기록된 i번째 로그를 추가한다.
    public bool AddLog(int index, bool changeLine = true) {
        return AddLog(currentCreature.logList[index].log, changeLine);
    }
    // AddLog: 현재 크리쳐의 기록된 로그를 startIndex 번째부터 endIndex 번째까지 추가한다.
    //        endIndex = -1 이면 끝까지 추가한다.
    private void AddLog(int startIndex, int endIndex) {
        if (endIndex == -1) endIndex = logCount - 1;
        for (int i = startIndex; i <= endIndex; i++) {
            if (!AddLog(i, !IsEmpty())) return;
        }
    }

    // ResetLog: 로그 텍스트를 모두 지운다.
    public void ResetLog() => logText.text = "";

    #endregion

    #region OnButton

    public void onBtnNextPage() => NextPage();
    public void onBtnPrevPage() => PrevPage();
    public void onBtnBack() => UIManager.Instance.diaryPanel.ActivateTab(UIManager.Instance.diaryPanel.creatureTab);

    #endregion
}