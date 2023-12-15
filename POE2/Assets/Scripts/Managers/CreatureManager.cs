using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreatureManager : MonoBehaviour {

    public static CreatureManager Instance { get; set; }

    public Dictionary<int, CreatureData> creatureDic;
    
    // 만난 크리쳐 목록.
    public List<CreatureData> encounteredCreatureList;

    void Awake() {
        if (!Instance) Instance = this;

        Initialize();
    }

    void Start() {
        
    }

    private void Initialize() {
        LoadCreatures();
    }
    private void LoadCreatures() {
        // 크리쳐 정보 불러오기.
        creatureDic = new Dictionary<int, CreatureData>();
        CreatureData[] creatures = Resources.LoadAll<CreatureData>("Prefabs/Creatures/Data");
        for (int i = 0; i < creatures.Length; i++) {
            if (creatures[i].id == 0) Debug.LogError("Invaild Creature!");
            else {
                if (!creatureDic.ContainsKey(creatures[i].id)) {
                    CreatureData data = creatures[i].Clone() as CreatureData;
                    data.Initialize();
                    creatureDic.Add(creatures[i].id, data);
                }
            }
        }

        // 마주친 크리쳐 정보.
        encounteredCreatureList = new List<CreatureData>();
    }

    // AddEncounterCreature: 해당 크리쳐를 만난 크리쳐 목록에 추가.
    public void AddEncounterCreature(Creature creature) {
        CreatureData data = GetCreatureData(creature.id);

        if (!encounteredCreatureList.Contains(data)) encounteredCreatureList.Add(data);
    }

    public void AllCurrentCreatureInitialize() {
        Creature[] creatures = FindObjectsOfType<Creature>();
        for (int i = 0; i < creatures.Length; i++) {
            creatures[i].Initialize();
        }
    }
    public void AllCurrentCreatureAIInitialize() {
        CreatureAI[] ais = FindObjectsOfType<CreatureAI>();
        for (int i = 0; i < ais.Length; i++) {
            ais[i].Initialize();
        }
    }

    // GetRecordedCreatureList: 기록된 크리쳐 리스트를 불러온다. 모든 CreatureData를 탐색해, logList가 비어있지 않은 것들을 불러오는 것.
    public List<CreatureData> GetRecordedCreatureList() {
        List<CreatureData> list = new List<CreatureData>();
        List<int> keyList = creatureDic.Keys.ToList();
        for(int i = 0; i < keyList.Count; i++) {
            if (creatureDic[keyList[i]].logList.Count > 0) list.Add(creatureDic[keyList[i]]);
        }
        return list;
    }

    public CreatureData GetCreatureData(int id) => creatureDic.ContainsKey(id) ? creatureDic[id] : null;
    public Creature CreateCreature2(int id) {
        CreatureData data = GetCreatureData(id);
        if (data == null) {
            Debug.LogError("[CreatureManager] CreateCreature: 없는 Creatre ID를 참조하였음.");
            return null;
        }
        return data.CreateCreature();
    }
}