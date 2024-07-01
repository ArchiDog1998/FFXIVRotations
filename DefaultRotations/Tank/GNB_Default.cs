namespace DefaultRotations.Tank;

[Rotation("Default", CombatType.Both, GameVersion = "6.38")]
[SourceCode(Path = "main/DefaultRotations/Tank/GNB_Default.cs")]
public sealed class GNB_Default : GunbreakerRotation
{
    public override bool CanHealSingleSpell => false;

    public override bool CanHealAreaSpell => false;

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= 0.7 && LightningShotPvE.CanUse(out var act)) return act;
        if (remainTime <= 1.2 && UseBurstMedicine(out act)) return act;
        return base.CountDownAction(remainTime);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (base.EmergencyAbility(nextGCD, out act)) return true;

        if (InCombat && CombatElapsedLess(30))
        {
            if (!CombatElapsedLessGCD(2) && NoMercyPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (Player.HasStatus(true, StatusID.NoMercy) && BloodfestPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (SolidBarrelPvP.CanUse(out act)) return true;
        if (BrutalShellPvP.CanUse(out act)) return true;
        if (KeenEdgePvP.CanUse(out act)) return true;
        #endregion

        if (FatedCirclePvE.CanUse(out act)) return true;
        if (CanUseGnashingFang(out act)) return true;

        if (DemonSlaughterPvE.CanUse(out act)) return true;
        if (DemonSlicePvE.CanUse(out act)) return true;

        if (Player.HasStatus(true, StatusID.NoMercy) && CanUseSonicBreak(out act)) return true;

        if (Player.HasStatus(true, StatusID.NoMercy) && CanUseDoubleDown(out act)) return true;

        if (SavageClawPvE.CanUse(out act, skipComboCheck: true)) return true;
        if (WickedTalonPvE.CanUse(out act, skipComboCheck: true)) return true;

        if (CanUseBurstStrike(out act)) return true;

        if (SolidBarrelPvE.CanUse(out act)) return true;
        if (BrutalShellPvE.CanUse(out act)) return true;
        if (KeenEdgePvE.CanUse(out act)) return true;

        if (LightningShotPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (MoveForwardAbility(out act)) return true;
        return base.MoveForwardGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        //if (IsBurst && CanUseNoMercy(out act)) return true;

        if (!CombatElapsedLessGCD(5) && NoMercyPvE.CanUse(out act, skipAoeCheck: true, skipClippingCheck: true)) return true;

        if (JugularRipPvE.CanUse(out act)) return true;

        if (DangerZonePvE.CanUse(out act))
        {
            if (!IsFullParty && !(DangerZonePvE.Target.Target?.IsBossFromTTK() ?? false)) return true;

            if (!GnashingFangPvE.EnoughLevel && (Player.HasStatus(true, StatusID.NoMercy) || !NoMercyPvE.CD.WillHaveOneCharge(15))) return true;

            if (Player.HasStatus(true, StatusID.NoMercy) && GnashingFangPvE.CD.IsCoolingDown) return true;

            if (!Player.HasStatus(true, StatusID.NoMercy) && !GnashingFangPvE.CD.WillHaveOneCharge(20)) return true;
        }

        if (Player.HasStatus(true, StatusID.NoMercy) && CanUseBowShock(out act)) return true;

        //if (RoughDividePvE.CanUse(out act) && !IsMoving) return true;
        if (GnashingFangPvE.CD.IsCoolingDown && DoubleDownPvE.CD.IsCoolingDown && Ammo == 0 && BloodfestPvE.CanUse(out act)) return true;

        if (AbdomenTearPvE.CanUse(out act)) return true;

        //if (Player.HasStatus(true, StatusID.NoMercy))
        //{
        //    if (RoughDividePvE.CanUse(out act, usedUp: true) && !IsMoving) return true;
        //}

        if (EyeGougePvE.CanUse(out act)) return true;
        if (HypervelocityPvE.CanUse(out act)) return true;

        return base.AttackAbility(out act);
    }

    protected override bool DefenseAreaAbility(out IAction? act)
    {
        if (!Player.HasStatus(true, StatusID.NoMercy) && HeartOfLightPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (!Player.HasStatus(true, StatusID.NoMercy) && ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.DefenseAreaAbility(out act);
    }

    protected override bool DefenseSingleAbility(out IAction? act)
    {
        //10
        if (CamouflagePvE.CanUse(out act, onLastAbility: true)) return true;
        //10
        if (HeartOfStonePvE.CanUse(out act, onLastAbility: true)) return true;

        //30
        if ((!RampartPvE.CD.IsCoolingDown || RampartPvE.CD.ElapsedAfter(60)) && NebulaPvE.CanUse(out act)) return true;
        //20
        if (NebulaPvE.CD.IsCoolingDown && NebulaPvE.CD.ElapsedAfter(60) && RampartPvE.CanUse(out act)) return true;

        if (ReprisalPvE.CanUse(out act)) return true;

        return base.DefenseSingleAbility(out act);
    }

    protected override bool HealSingleAbility(out IAction? act)
    {
        if (AuroraPvE.CanUse(out act, usedUp: true, onLastAbility: true)) return true;
        return base.HealSingleAbility(out act);
    }

    //private bool CanUseNoMercy(out IAction act)
    //{
    //    if (!NoMercy.CanUse(out act, CanUseOption.OnLastAbility)) return false;

    //    if (!IsFullParty && !IsTargetBoss && !IsMoving && DemonSlice.CanUse(out _)) return true;

    //    if (!BurstStrike.EnoughLevel) return true;

    //    if (BurstStrike.EnoughLevel)
    //    {
    //        if (IsLastGCD((ActionID)KeenEdge.ID) && Ammo == 1 && !GnashingFang.IsCoolingDown && !BloodFest.IsCoolingDown) return true;
    //        else if (Ammo == (Level >= 88 ? 3 : 2)) return true;
    //        else if (Ammo == 2 && GnashingFang.IsCoolingDown) return true;
    //    }

    //    act = null;
    //    return false;
    //}

    private bool CanUseGnashingFang(out IAction? act)
    {
        if (GnashingFangPvE.CanUse(out act))
        {
            if (DemonSlicePvE.CanUse(out _)) return false;

            if (Ammo == MaxAmmo && (Player.HasStatus(true, StatusID.NoMercy) || !NoMercyPvE.CD.WillHaveOneCharge(55))) return true;

            if (Ammo > 0 && !NoMercyPvE.CD.WillHaveOneCharge(17) && NoMercyPvE.CD.WillHaveOneCharge(35)) return true;

            if (Ammo == 3 && IsLastGCD((ActionID)BrutalShellPvE.ID) && NoMercyPvE.CD.WillHaveOneCharge(3)) return true;

            if (Ammo == 1 && !NoMercyPvE.CD.WillHaveOneCharge(55) && BloodfestPvE.CD.WillHaveOneCharge(5)) return true;

            if (Ammo == 1 && !NoMercyPvE.CD.WillHaveOneCharge(55) && (!BloodfestPvE.CD.IsCoolingDown && BloodfestPvE.EnoughLevel || !BloodfestPvE.EnoughLevel)) return true;
        }
        return false;
    }

    private bool CanUseSonicBreak(out IAction act)
    {
        if (SonicBreakPvE.CanUse(out act))
        {
            if (DemonSlicePvE.CanUse(out _)) return false;

            //if (!IsFullParty && !SonicBreak.IsTargetBoss) return false;

            if (!GnashingFangPvE.EnoughLevel && Player.HasStatus(true, StatusID.NoMercy)) return true;

            if (GnashingFangPvE.CD.IsCoolingDown && Player.HasStatus(true, StatusID.NoMercy)) return true;

            if (!DoubleDownPvE.EnoughLevel && Player.HasStatus(true, StatusID.ReadyToRip)
                && GnashingFangPvE.CD.IsCoolingDown) return true;

        }
        return false;
    }

    private bool CanUseDoubleDown(out IAction act)
    {
        if (DoubleDownPvE.CanUse(out act, skipAoeCheck: true))
        {
            if (DemonSlicePvE.CanUse(out _) && Player.HasStatus(true, StatusID.NoMercy)) return true;

            if (SonicBreakPvE.CD.IsCoolingDown && Player.HasStatus(true, StatusID.NoMercy)) return true;

            if (Player.HasStatus(true, StatusID.NoMercy) && !NoMercyPvE.CD.WillHaveOneCharge(55) && BloodfestPvE.CD.WillHaveOneCharge(5)) return true;

        }
        return false;
    }

    private bool CanUseBurstStrike(out IAction act)
    {
        if (BurstStrikePvE.CanUse(out act))
        {
            if (DemonSlicePvE.CanUse(out _)) return false;

            if (SonicBreakPvE.CD.IsCoolingDown && SonicBreakPvE.CD.WillHaveOneCharge(0.5f) && GnashingFangPvE.EnoughLevel) return false;

            if (Player.HasStatus(true, StatusID.NoMercy) &&
            AmmoComboStep == 0 &&
                !GnashingFangPvE.CD.WillHaveOneCharge(1)) return true;
            
            if (!CartridgeChargeIiTrait.EnoughLevel && Ammo == 2) return true;

            if (IsLastGCD((ActionID)BrutalShellPvE.ID) &&
                (Ammo == MaxAmmo ||
                BloodfestPvE.CD.WillHaveOneCharge(6) && Ammo <= 2 && !NoMercyPvE.CD.WillHaveOneCharge(10) && BloodfestPvE.EnoughLevel)) return true;

        }
        return false;
    }

    private bool CanUseBowShock(out IAction act)
    {
        if (BowShockPvE.CanUse(out act, skipAoeCheck: true))
        {
            if (DemonSlicePvE.CanUse(out _) && !IsFullParty) return true;

            if (!SonicBreakPvE.EnoughLevel && Player.HasStatus(true, StatusID.NoMercy)) return true;

            if (Player.HasStatus(true, StatusID.NoMercy) && SonicBreakPvE.CD.IsCoolingDown) return true;
        }
        return false;
    }
}