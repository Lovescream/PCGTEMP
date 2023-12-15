using System;

[Serializable]
public class Status {

    public float maxHP;
    public float HP;

    public float maxShield;
    public float shield;

    public float maxSanity;
    public float sanity;

    public float damage;
    public float knockBackPower;

    public float defense;
    public float knockBackResist;

    public float HPratio => HP / maxHP;
    // DamageShield: 보호막에 피해를 입히고, 남은 피해를 리턴.
    public float DamageShield(float damage) {
        shield -= damage;

        float remainDamage;
        if (shield >= 0) remainDamage = 0;
        else {
            remainDamage = -shield;
            shield = 0;
        }

        return remainDamage;
    }

    // DecreaseSanity: 정신력 감소. 정신력이 0 이하로 떨어졌는지 여부를 리턴.
    public bool DecreaseSanity(float amount) {
        sanity -= amount;
        if (sanity <= 0) {
            sanity = 0;
            return true;
        }
        else return false;
    }
}