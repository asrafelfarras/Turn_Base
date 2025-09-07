using System;
using UnityEngine;
using UnityEngine.UI;

public class UnitHPBar : MonoBehaviour
{
    public Image hpFillImage;
    public float maxHP = 100f;
    public float currentHP = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMaxHP(float hp)
    {
        maxHP = hp;
        currentHP = hp;
        UpdateBar();
    }

    public void SetHP(float hp)
    {
        currentHP = Mathf.Clamp(hp, 0, maxHP);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = currentHP / maxHP;
        }
    }
}
