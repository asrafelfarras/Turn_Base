using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    PlayerTurn,
    EnemyTurn,
    WaitingForSwitchTarget,
    BattleOver
}

public class BattleSystem : MonoBehaviour
{
    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;

    public BattleState state;
    private int currentUnitIndex = 0;

    [Header("Turn Arrow")]
    public GameObject turnArrowPrefab;
    private GameObject currentArrow;

    [Header("SP (Skill Points)")]
    public SPUI spUI;          // Assign di Inspector (Map Canvas)
    public int currentSP = 3;  // nilai awal sesuai selera
    public int maxSP = 5;      // pastikan jumlah square di SPUI = maxSP (atau >= maxSP)

    // Switch
    private Unit unitWaitingToSwitch = null; // unit yang menekan tombol Switch

    void Start()
    {
        state = BattleState.Start;
        SetupBattle();
    }

    void SetupBattle()
    {
        // Init HP Bars
        foreach (var unit in playerUnits)
        {
            if (unit != null && unit.hpBar != null)
            {
                unit.hpBar.SetMaxHP(unit.maxHP);
                unit.hpBar.SetHP(unit.currentHP);
            }
        }

        foreach (var unit in enemyUnits)
        {
            if (unit != null && unit.hpBar != null)
            {
                unit.hpBar.SetMaxHP(unit.maxHP);
                unit.hpBar.SetHP(unit.currentHP);
            }
        }

        // Init SP UI
        ClampSPToMaxSquares();
        UpdateSPUI();

        state = BattleState.PlayerTurn;
        currentUnitIndex = 0;
        StartPlayerTurn();
    }

    // ---------- TURN ARROW ----------
    void HideTurnArrow()
    {
        if (currentArrow != null)
        {
            Destroy(currentArrow);
            currentArrow = null;
        }
    }

    void UpdateTurnArrow(Unit unit)
    {
        HideTurnArrow();

        if (turnArrowPrefab != null && unit != null)
        {
            currentArrow = Instantiate(
                turnArrowPrefab,
                unit.transform.position + Vector3.up * 1f,
                Quaternion.identity
            );
            currentArrow.transform.SetParent(unit.transform);
        }
    }

    // ---------- PLAYER TURN ----------
    void StartPlayerTurn()
    {
        state = BattleState.PlayerTurn;

        HideTurnArrow();

        if (playerUnits.Count == 0)
        {
            Debug.Log("Semua player mati!");
            state = BattleState.BattleOver;
            return;
        }

        if (currentUnitIndex >= playerUnits.Count)
            currentUnitIndex = 0;

        // Cari unit hidup (safety loop)
        int safety = 0;
        while (playerUnits[currentUnitIndex] == null || playerUnits[currentUnitIndex].IsDead())
        {
            currentUnitIndex++;
            if (currentUnitIndex >= playerUnits.Count)
                currentUnitIndex = 0;

            safety++;
            if (safety > playerUnits.Count)
            {
                Debug.Log("Semua unit player mati!");
                state = BattleState.BattleOver;
                return;
            }
        }

        var currentUnit = playerUnits[currentUnitIndex];

        // Tampilkan hanya menu unit aktif
        foreach (var unit in playerUnits)
        {
            if (unit != null && unit.commandMenu != null)
                unit.commandMenu.ShowMenu(false);
        }

        if (currentUnit != null && currentUnit.commandMenu != null)
            currentUnit.commandMenu.ShowMenu(true);

        UpdateTurnArrow(currentUnit);
    }

    public void OnCommandSelected(string command)
    {
        if (state != BattleState.PlayerTurn)
            return;

        var currentUnit = playerUnits[currentUnitIndex];
        var target = enemyUnits.Count > 0 ? enemyUnits[0] : null;

        switch (command)
        {
            case "Strike":
                currentUnit.Strike(target);
                GainSP(1); // Strike menambah SP
                AfterAttack(target);
                break;

            case "Skill":
                if (currentSP > 0)
                {
                    currentUnit.UseSkill(0, target);
                    UseSP(1); // Skill mengurangi SP
                    AfterAttack(target);
                }
                else
                {
                    Debug.Log("Not enough SP!");
                    // Jangan end turn; biarkan pemain pilih aksi lain
                }
                break;

            case "Switch":
                Debug.Log(currentUnit.unitName + " wants to switch!");
                unitWaitingToSwitch = currentUnit;
                state = BattleState.WaitingForSwitchTarget;
                // Kamu bisa tampilkan UI petunjuk di sini kalau perlu
                break;
        }
    }

    void AfterAttack(Unit target)
    {
        // Bereskan enemy mati
        if (target != null && target.IsDead())
        {
            enemyUnits.Remove(target);
            Destroy(target.gameObject);

            if (enemyUnits.Count == 0)
            {
                Debug.Log("All enemies defeated!");
                state = BattleState.BattleOver;
                return;
            }
        }

        EndPlayerTurn();
    }

    // Dipanggil ketika klik unit target untuk switch (via OnMouseDown di Unit atau via UI)
    public void OnSwitchTargetSelected(Unit targetUnit)
    {
        if (state != BattleState.WaitingForSwitchTarget) return;
        if (unitWaitingToSwitch == null || targetUnit == null) return;
        if (unitWaitingToSwitch == targetUnit)
        {
            Debug.Log("Tidak bisa tukar dengan diri sendiri!");
            return;
        }

        int indexA = playerUnits.IndexOf(unitWaitingToSwitch);
        int indexB = playerUnits.IndexOf(targetUnit);
        if (indexA < 0 || indexB < 0) return;

        // Swap list
        playerUnits[indexA] = targetUnit;
        playerUnits[indexB] = unitWaitingToSwitch;

        // Swap posisi world
        Vector3 tempPos = unitWaitingToSwitch.transform.position;
        unitWaitingToSwitch.transform.position = targetUnit.transform.position;
        targetUnit.transform.position = tempPos;

        Debug.Log(unitWaitingToSwitch.unitName + " switched with " + targetUnit.unitName);

        unitWaitingToSwitch = null;

        EndPlayerTurn();
    }

    void EndPlayerTurn()
    {
        // Tutup menu unit aktif
        if (playerUnits.Count > 0 && currentUnitIndex < playerUnits.Count)
        {
            var u = playerUnits[currentUnitIndex];
            if (u != null && u.commandMenu != null)
                u.commandMenu.ShowMenu(false);
        }

        currentUnitIndex++;
        if (currentUnitIndex >= playerUnits.Count)
        {
            state = BattleState.EnemyTurn;
            EnemyAction();
        }
        else
        {
            StartPlayerTurn();
        }
    }

    // ---------- ENEMY TURN ----------
    void EnemyAction()
    {
        if (enemyUnits.Count == 0)
        {
            Debug.Log("All enemies defeated!");
            state = BattleState.BattleOver;
            return;
        }

        // Musuh sangat simpel: musuh pertama menyerang player pertama yang hidup
        var enemy = enemyUnits[0];

        Unit target = null;
        foreach (var p in playerUnits)
        {
            if (p != null && !p.IsDead())
            {
                target = p;
                break;
            }
        }

        if (enemy != null && target != null)
        {
            enemy.Strike(target);

            if (target.IsDead())
            {
                playerUnits.Remove(target);
                Destroy(target.gameObject);

                if (playerUnits.Count == 0)
                {
                    Debug.Log("All players defeated!");
                    state = BattleState.BattleOver;
                    return;
                }
            }
        }

        currentUnitIndex = 0;
        StartPlayerTurn();
    }

    // ---------- SP HELPERS ----------
    private void GainSP(int amount)
    {
        currentSP = Mathf.Clamp(currentSP + amount, 0, maxSP);
        UpdateSPUI();
    }

    private void UseSP(int amount)
    {
        currentSP = Mathf.Clamp(currentSP - amount, 0, maxSP);
        UpdateSPUI();
    }

    private void ClampSPToMaxSquares()
    {
        if (spUI != null && spUI.spSquares != null && spUI.spSquares.Length > 0)
        {
            // optional: sesuaikan maxSP ke jumlah square
            maxSP = Mathf.Min(maxSP, spUI.spSquares.Length);
            currentSP = Mathf.Clamp(currentSP, 0, maxSP);
        }
    }

    private void UpdateSPUI()
    {
        if (spUI != null)
        {
            spUI.UpdateSP(currentSP);
        }
        else
        {
            Debug.LogWarning("SPUI belum di-assign di BattleSystem.");
        }
    }
}
