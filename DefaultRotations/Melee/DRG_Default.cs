namespace DefaultRotations.Melee;

[SourceCode(Path = "main/DefaultRotations/Melee/DRG_Default.cs")]
[Rotation("Default", CombatType.PvE, GameVersion = "6.18")]

public sealed class DRG_Default : DragoonRotation
{
    [RotationDesc(ActionID.SpineshatterDivePvE, ActionID.DragonfireDivePvE)]
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
            if (LifeSurgePvE.CanUse(out act, onLastAbility: true, isEmpty: true)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
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

        if (SpineshatterDivePvE.CanUse(out act, isEmpty: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceChargePvE.Cooldown.ElapsedOneChargeAfterGCD(3)) return true;
        }
        if (Player.HasStatus(true, StatusID.PowerSurge) && SpineshatterDivePvE.Cooldown.CurrentCharges != 1 && SpineshatterDivePvE.CanUse(out act)) return true;

        if (MirageDivePvE.CanUse(out act)) return true;

        if (DragonfireDivePvE.CanUse(out act, skipAoeCheck: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceChargePvE.Cooldown.ElapsedOneChargeAfterGCD(3)) return true;
        }

        if (WyrmwindThrustPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return base.AttackAbility(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (CoerthanTormentPvE.CanUse(out act)) return true;
        if (SonicThrustPvE.CanUse(out act)) return true;
        if (DoomSpikePvE.CanUse(out act)) return true;


        if (WheelingThrustPvE.CanUse(out act)) return true;
        if (FangAndClawPvE.CanUse(out act)) return true;


        if (FullThrustPvE.CanUse(out act)) return true;
        if (ChaosThrustPvE.CanUse(out act)) return true;

        if (Player.WillStatusEndGCD(5, 0, true, StatusID.PowerSurge))
        {
            if (DisembowelPvE.CanUse(out act)) return true;
        }

        if (VorpalThrustPvE.CanUse(out act)) return true;
        if (TrueThrustPvE.CanUse(out act)) return true;

        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(out act)) return true;
        if (PiercingTalonPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
}
