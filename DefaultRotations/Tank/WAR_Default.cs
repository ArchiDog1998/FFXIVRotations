namespace DefaultRotations.Tank;

[Rotation("All-Around", CombatType.PvE, GameVersion = "6.35")]
[SourceCode(Path = "main/DefaultRotations/Tank/WAR_Default.cs")]
[LinkDescription("https://cdn.discordapp.com/attachments/277962807813865472/963548326433796116/unknown.png")]
public sealed class WAR_Default : WarriorRotation
{
    private static bool IsBurstStatus => !Player.WillStatusEndGCD(0, 0, false, StatusID.InnerStrength);

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= CountDownAhead)
        {
            if (HasTankStance)
            {
                if (ProvokePvE.CanUse(out var act)) return act;
            }
            else
            {
                if (TomahawkPvE.CanUse(out var act)) return act;
            }
        }
        return base.CountDownAction(remainTime);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (!Player.WillStatusEndGCD(3, 0, true, StatusID.SurgingTempest))
        {
            if (!IsMoving && IsBurstStatus && PrimalRendPvE.CanUse(out act, skipAoeCheck: true))
            {
                if (PrimalRendPvE.Target?.Target?.DistanceToPlayer() < 1) return true;
            }
            if (IsBurstStatus || !Player.HasStatus(false, StatusID.NascentChaos) || BeastGauge > 80)
            {
                if (SteelCyclonePvE.CanUse(out act)) return true;
                if (InnerBeastPvE.CanUse(out act)) return true;
            }
        }

        if (MythrilTempestPvE.CanUse(out act)) return true;
        if (OverpowerPvE.CanUse(out act)) return true;

        if (StormsEyePvE.CanUse(out act)) return true;
        if (StormsPathPvE.CanUse(out act)) return true;
        if (MaimPvE.CanUse(out act)) return true;
        if (HeavySwingPvE.CanUse(out act)) return true;

        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(out act)) return true;
        
        if (TomahawkPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        if (InfuriatePvE.CanUse(out act, gcdCountForAbility: 3)) return true;

        if (CombatElapsedLessGCD(1)) return false;

        if (UseBurstMedicine(out act)) return true;
        if (Player.HasStatus(false, StatusID.SurgingTempest)
            && !Player.WillStatusEndGCD(6, 0, true, StatusID.SurgingTempest)
            || !MythrilTempestPvE.EnoughLevel)
        {
            if (BerserkPvE.CanUse(out act, onLastAbility: true)) return true;
        }

        if (IsBurstStatus)
        {
            if (InfuriatePvE.CanUse(out act, isEmpty: true)) return true;
        }

        if (CombatElapsedLessGCD(4)) return false;

        if (OrogenyPvE.CanUse(out act)) return true;
        if (UpheavalPvE.CanUse(out act)) return true;

        if (OnslaughtPvE.CanUse(out act, isEmpty: IsBurstStatus) && !IsMoving) return true;

        return base.AttackAbility(out act);
    }

    protected override bool GeneralAbility(out IAction? act)
    {
        //Auto healing
        if (Player.GetHealthRatio() < 0.6f)
        {
            if (ThrillOfBattlePvE.CanUse(out act)) return true;
            if (EquilibriumPvE.CanUse(out act)) return true;
        }

        if (!HasTankStance && NascentFlashPvE.CanUse(out act)) return true;

        return base.GeneralAbility(out act);
    }

    [RotationDesc(ActionID.RawIntuitionPvE, ActionID.VengeancePvE, ActionID.RampartPvE, ActionID.RawIntuitionPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseSingleAbility(out IAction act)
    {
        //10
        if (RawIntuitionPvE.CanUse(out act, onLastAbility: true)) return true;

        //30
        if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && VengeancePvE.CanUse(out act)) return true;

        //20
        if (VengeancePvE.Cooldown.IsCoolingDown && VengeancePvE.Cooldown.ElapsedAfter(60) && RampartPvE.CanUse(out act)) return true;

        if (ReprisalPvE.CanUse(out act)) return true;

        return false;
    }

    [RotationDesc(ActionID.ShakeItOffPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseAreaAbility(out IAction? act)
    {
        if (ShakeItOffPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.DefenseAreaAbility(out act);
    }
}
