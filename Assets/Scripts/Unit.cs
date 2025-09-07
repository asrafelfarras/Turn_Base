using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int strikeDamage = 5;

    public int maxHP = 50;
    public int currentHP;

    public CommandMenu commandMenu;
    public UnitHPBar hpBar;

    private BattleSystem battleSystem;

    [Header("Skills (ScriptableObjects)")]
    public SkillData[] skills; // Skill unik tiap Unit

    void Start()
    {
        currentHP = maxHP;

        if (hpBar != null)
        {
            hpBar.SetMaxHP(maxHP);
            hpBar.SetHP(currentHP);
        }

        battleSystem = FindObjectOfType<BattleSystem>();
    }

    void OnMouseDown()
    {
        // Kalau lagi pilih target switch, klik unit akan diproses
        if (battleSystem != null && battleSystem.state == BattleState.WaitingForSwitchTarget)
        {
            battleSystem.OnSwitchTargetSelected(this);
        }
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }

    public void Strike(Unit target)
    {
        Debug.Log(unitName + " uses Strike on " + target.unitName);
        target.TakeDamage(strikeDamage);
    }

    public void UseSkill(int skillIndex, Unit target)
    {
        if (skills == null || skills.Length <= skillIndex)
        {
            Debug.LogWarning(unitName + " tried to use a skill that doesn’t exist!");
            return;
        }

        SkillData skill = skills[skillIndex];
        Debug.Log(unitName + " uses " + skill.skillName + " on " + target.unitName);

        skill.ApplyEffect(this, target); // Jalankan efek skill
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Debug.Log(unitName + " has been defeated!");
        }

        if (hpBar != null)
            hpBar.SetHP(currentHP);
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;

        Debug.Log(unitName + " healed for " + amount + " HP!");

        if (hpBar != null)
            hpBar.SetHP(currentHP);
    }
}
