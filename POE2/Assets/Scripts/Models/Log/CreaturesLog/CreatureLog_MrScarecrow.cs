using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureLog_MrScarecrow : CreatureLog {

    private InteractiveCreatureAI_MrScareCrow creatureAI;

    public override void Initialize() {
        base.Initialize();

        creatureAI = this.GetComponent<InteractiveCreatureAI_MrScareCrow>();

        AddLogAction("EndTutorial", new Ref<Action>(() => creatureAI.cbOnClearAllTutorial, x => creatureAI.cbOnClearAllTutorial = x));
    }

}