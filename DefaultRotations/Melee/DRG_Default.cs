namespace DefaultRotations.Melee;

[SourceCode(Path = "main/DefaultRotations/Melee/DRG_Default.cs")]
[Rotation("Default", CombatType.Both, GameVersion = "6.18")]
public sealed class DRG_Default : DragoonRotation
{
    protected override bool MoveForwardAbility(out IAction act)
    {
        if (SpineshatterDivePvE.CanUse(out act)) return true;
        if (DragonfireDivePvE.CanUse(out act, skipAoeCheck: true)) return true;
        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD.IsTheSameTo(true, FullThrustPvE, CoerthanTormentPvE)
            || Player.HasStatus(true, StatusID.LanceCharge) && nextGCD.IsTheSameTo(false, FangAndClawPvE))
        {
            if (LifeSurgePvE.CanUse(out act, onLastAbility: true, usedUp: true)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (GeirskogulPvP.CanUse(out act)) return true;
        if (NastrondPvP.CanUse(out act)) return true;

        if (HighJumpPvP.CanUse(out act)) return true;

        if (HorridRoarPvP.CanUse(out act)) return true;
        #endregion

        if (IsBurst && InCombat)
        {
            if (LanceChargePvE.CanUse(out act, skipAoeCheck: true) && Player.HasStatus(true, StatusID.PowerSurge)) return true;
            if (LanceChargePvE.CanUse(out act, skipAoeCheck: true, onLastAbility: true) && !Player.HasStatus(true, StatusID.PowerSurge)) return true;

            if (DragonSightPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (BattleLitanyPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (NastrondPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (StardiverPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (HighJumpPvE.EnoughLevel)
        {
            if (HighJumpPvE.CanUse(out act)) return true;
        }
        else
        {
            if (JumpPvE.CanUse(out act)) return true;
        }

        if (GeirskogulPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (SpineshatterDivePvE.CanUse(out act, usedUp: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceChargePvE.CD.ElapsedOneChargeAfterGCD(3)) return true;
        }
        if (Player.HasStatus(true, StatusID.PowerSurge) && SpineshatterDivePvE.CD.CurrentCharges != 1 && SpineshatterDivePvE.CanUse(out act)) return true;

        if (MirageDivePvE.CanUse(out act)) return true;

        if (DragonfireDivePvE.CanUse(out act, skipAoeCheck: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceChargePvE.CD.ElapsedOneChargeAfterGCD(3)) return true;
        }

        if (WyrmwindThrustPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return base.AttackAbility(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (WyrmwindThrustPvP.CanUse(out act)) return true;

        if (ChaoticSpringPvP.CanUse(out act)) return true;

        if (WheelingThrustPvP.CanUse(out act)) return true;
        if (FangAndClawPvP.CanUse(out act)) return true;
        if (RaidenThrustPvP.CanUse(out act)) return true;
        #endregion

        if (CoerthanTormentPvE.CanUse(out act)) return true;
        if (SonicThrustPvE.CanUse(out act)) return true;
        if (DoomSpikePvE.CanUse(out act)) return true;


        if (WheelingThrustPvE.CanUse(out act)) return true;
        if (FangAndClawPvE.CanUse(out act)) return true;


        if (FullThrustPvE.CanUse(out act)) return true;
        if (ChaosThrustPvE.CanUse(out act)) return true;

        if (Player.WillStatusEndGCD(5, 0, true, StatusID.PowerSurge_2720))
        {
            if (DisembowelPvE.CanUse(out act)) return true;
        }

        if (VorpalThrustPvE.CanUse(out act)) return true;
        if (TrueThrustPvE.CanUse(out act)) return true;

        if (PiercingTalonPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (MoveForwardAbility(out act)) return true;
        return base.MoveForwardGCD(out act);
    }
}
