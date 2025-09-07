using UnityEngine;

public enum SkillType
{
    Damage,
    Heal,
    Buff,
    Debuff
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Battle/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;

    public SkillType skillType;
    public int power;     // Besar damage / heal / buff / debuff
    public int spCost = 0; // Bisa dipakai nanti kalau ada sistem SP

    public void ApplyEffect(Unit user, Unit target)
    {
        switch (skillType)
        {
            case SkillType.Damage:
                Debug.Log($"{user.unitName} uses {skillName} on {target.unitName}");
                target.TakeDamage(power);
                break;

            case SkillType.Heal:
                Debug.Log($"{user.unitName} uses {skillName} to heal {target.unitName}");
                target.Heal(power);
                break;

            case SkillType.Buff:
                Debug.Log($"{user.unitName} buffs {target.unitName} with {skillName}");
                // contoh sederhana: tambah HP max sementara
                target.maxHP += power;
                target.currentHP += power;
                if (target.hpBar != null)
                {
                    target.hpBar.SetMaxHP(target.maxHP);
                    target.hpBar.SetHP(target.currentHP);
                }
                break;

            case SkillType.Debuff:
                Debug.Log($"{user.unitName} debuffs {target.unitName} with {skillName}");
                // contoh sederhana: kurangi HP langsung (debuff)
                target.TakeDamage(power / 2);
                break;
        }
    }
}
