namespace DefaultRotations.Tank;

[Rotation("Balance", CombatType.Both, GameVersion = "6.38")]
[SourceCode(Path = "main/DefaultRotations/Tank/DRK_Balance.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/drk/drk_standard_6.2_v1.png")]
public sealed class DRK_Default : DarkKnightRotation
{
    public override bool CanHealSingleAbility => false;

    private bool InTwoMinsBurst()
    {
        if ((BloodWeaponPvE.CD.IsCoolingDown && DeliriumPvE.CD.IsCoolingDown && ((LivingShadowPvE.CD.IsCoolingDown && !(LivingShadowPvE.CD.ElapsedAfter(15))) || !LivingShadowPvE.EnoughLevel))) return true;
        else return false;
    }

    private static bool CombatLess => CombatElapsedLess(3);

    private bool CheckDarkSide
    {
        get
        {
            if (DarksideTimeRemaining < 3) return true;

            if (CombatLess) return false;

            if ((InTwoMinsBurst() && HasDarkArts) || (HasDarkArts && Player.HasStatus(true, StatusID.BlackestNight)) || (HasDarkArts && DarksideTimeRemaining < 3)) return true;

            if ((InTwoMinsBurst() && BloodWeaponPvE.CD.IsCoolingDown && LivingShadowPvE.CD.IsCoolingDown && SaltedEarthPvE.CD.IsCoolingDown && ShadowbringerPvE.CD.CurrentCharges == 0 && CarveAndSpitPvE.CD.IsCoolingDown)) return true;

            if (TheBlackestNight && CurrentMp < 6000) return false;

            return CurrentMp >= 8500;
        }
    }

    private bool UseBlood
    {
        get
        {
            if (!DeliriumPvE.EnoughLevel) return true;

            if (Player.HasStatus(true, StatusID.Delirium) && LivingShadowPvE.CD.IsCoolingDown) return true;

            if ((DeliriumPvE.CD.WillHaveOneChargeGCD(1) && !LivingShadowPvE.CD.WillHaveOneChargeGCD(3)) || Blood >= 90 && !LivingShadowPvE.CD.WillHaveOneChargeGCD(1)) return true;

            return false;
        }
    }

    [UI("Keep at least 3000 MP")]
    [RotationConfig(CombatType.PvE)]
    public bool TheBlackestNight { get; set; } = true;

    protected override IAction? CountDownAction(float remainTime)
    {
        //Provoke when has Shield.
        if (remainTime <= CountDownAhead)
        {
            if (HasTankStance)
            {
                if (ProvokePvE.CanUse(out _)) return ProvokePvE;
            }
            //else
            //{
            //    if (Unmend.CanUse(out var act1)) return act1;
            //}
        }
        if (remainTime <= 2 && UseBurstMedicine(out var act)) return act;
        if (remainTime <= 3 && TheBlackestNightPvE.CanUse(out act)) return act;
        if (remainTime <= 4 && BloodWeaponPvE.CanUse(out act)) return act;
        return base.CountDownAction(remainTime);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        #region PvP
        if (TheBlackestNightPvP.CanUse(out act)) return true;
        #endregion

        if (base.EmergencyAbility(nextGCD, out act)) return true;

        if ((InCombat && CombatElapsedLess(2) || TimeSinceLastAction.TotalSeconds >= 10))
        {
            if (BloodWeaponPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool HealSingleAbility(out IAction? act)
    {
        if (TheBlackestNightPvE.CanUse(out act)) return true;
        return base.HealSingleAbility(out act);
    }

    protected override bool DefenseAreaAbility(out IAction? act)
    {
        if (!InTwoMinsBurst() && DarkMissionaryPvE.CanUse(out act)) return true;
        if (!InTwoMinsBurst() && ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return base.DefenseAreaAbility(out act);
    }

    protected override bool DefenseSingleAbility(out IAction? act)
    {
        act = null;

        if (Player.HasStatus(true, StatusID.BlackestNight)) return false;

        //10
        if (OblationPvE.CanUse(out act, usedUp: true, onLastAbility: true)) return true;

        if (ReprisalPvE.CanUse(out act, skipAoeCheck: true, onLastAbility: true)) return true;

        if (TheBlackestNightPvE.CanUse(out act, onLastAbility: true)) return true;
        //30
        if ((!RampartPvE.CD.IsCoolingDown || RampartPvE.CD.ElapsedAfter(60)) && ShadowWallPvE.CanUse(out act)) return true;

        //20
        if (ShadowWallPvE.CD.IsCoolingDown && ShadowWallPvE.CD.ElapsedAfter(60) && RampartPvE.CanUse(out act)) return true;
        if (DarkMindPvE.CanUse(out act)) return true;

        return base.DefenseAreaAbility(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (QuietusPvP.CanUse(out act)) return true;

        if (SouleaterPvP.CanUse(out act)) return true;
        if (SyphonStrikePvP.CanUse(out act)) return true;
        if (HardSlashPvP.CanUse(out act)) return true;
        #endregion

        //Use Blood
        if (UseBlood)
        {
            if (QuietusPvE.CanUse(out act)) return true;
            if (BloodspillerPvE.CanUse(out act)) return true;
        }

        //AOE
        if (StalwartSoulPvE.CanUse(out act)) return true;
        if (UnleashPvE.CanUse(out act)) return true;

        //单体
        if (SouleaterPvE.CanUse(out act)) return true;
        if (SyphonStrikePvE.CanUse(out act)) return true;
        if (HardSlashPvE.CanUse(out act)) return true;

        if (BloodWeaponPvE.CD.IsCoolingDown && !Player.HasStatus(true, StatusID.BloodWeapon) && UnmendPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (MoveForwardAbility(out act)) return true;
        return base.MoveForwardGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (SaltedEarthPvP.CanUse(out act)) return true;
        if (SaltAndDarknessPvP.CanUse(out act)) return true;
        if (PlungePvP.CanUse(out act)) return true;
        #endregion

        //if (InCombat && CombatElapsedLess(2) && BloodWeapon.CanUse(out act)) return true;
        if (CheckDarkSide)
        {
            if (FloodOfDarknessPvE.CanUse(out act)) return true;
            if (EdgeOfDarknessPvE.CanUse(out act)) return true;
        }

        if (IsBurst)
        {
            if (UseBurstMedicine(out act)) return true;
            if (InCombat && DeliriumPvE.CanUse(out act)) return true;
            if (DeliriumPvE.CD.ElapsedAfterGCD(1) && !DeliriumPvE.CD.ElapsedAfterGCD(3) 
                && BloodWeaponPvE.CanUse(out act)) return true;
            if (LivingShadowPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (CombatLess)
        {
            act = null;
            return false;
        }

        if (!IsMoving && SaltedEarthPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (ShadowbringerPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (NumberOfHostilesInRange >= 3 && AbyssalDrainPvE.CanUse(out act)) return true;
        if (CarveAndSpitPvE.CanUse(out act)) return true;

        if (InTwoMinsBurst())
        {
            if (ShadowbringerPvE.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

        }

        //if (PlungePvE.CanUse(out act, skipAoeCheck: true) && !IsMoving) return true;

        if (SaltAndDarknessPvE.CanUse(out act)) return true;

        if (InTwoMinsBurst())
        {
            //if (PlungePvE.CanUse(out act, usedUp: true, skipAoeCheck: true) && !IsMoving) return true;
        }

        return base.AttackAbility(out act);
    }
}
