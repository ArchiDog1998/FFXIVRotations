namespace DefaultRotations.Tank;

[Rotation("All-Around", CombatType.Both, GameVersion = "6.35")]
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
        #region PvP
        if (PrimalRendPvP.CanUse(out act)) return true;
        if (ChaoticCyclonePvP.CanUse(out act)) return true;

        if (StormsPathPvP.CanUse(out act)) return true;
        if (MaimPvP.CanUse(out act)) return true;
        if (HeavySwingPvP.CanUse(out act)) return true;
        #endregion

        if (!Player.WillStatusEndGCD(3, 0, true, StatusID.SurgingTempest))
        {
            if (!IsMoving && IsBurstStatus && PrimalRendPvE.CanUse(out act, skipAoeCheck: true))
            {
                if (PrimalRendPvE.Target.Target?.DistanceToPlayer() < 1) return true;
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

        if (TomahawkPvE.CanUse(out act)) return true;

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
        if (OrogenyPvP.CanUse(out act)) return true;
        if (OnslaughtPvP.CanUse(out act)) return true;
        #endregion

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
            if (InfuriatePvE.CanUse(out act, usedUp: true)) return true;
        }

        if (CombatElapsedLessGCD(4)) return false;

        if (OrogenyPvE.CanUse(out act)) return true;
        if (UpheavalPvE.CanUse(out act)) return true;

        if (OnslaughtPvE.CanUse(out act, usedUp: IsBurstStatus) && !IsMoving) return true;

        return base.AttackAbility(out act);
    }

    protected override bool GeneralAbility(out IAction? act)
    {
        #region PvP
        if (BloodwhettingPvP.CanUse(out act)) return true;
        #endregion

        //Auto healing
        if (Player.GetHealthRatio() < 0.6f)
        {
            if (ThrillOfBattlePvE.CanUse(out act)) return true;
            if (EquilibriumPvE.CanUse(out act)) return true;
        }

        if (!HasTankStance && NascentFlashPvE.CanUse(out act)) return true;

        return base.GeneralAbility(out act);
    }

    protected override bool DefenseSingleAbility(out IAction? act)
    {
        if (MobsTime)
        {
            //10
            if (RawIntuitionPvE.CanUse(out act, onLastAbility: true) && NumberOfHostilesInRange > 2) return true;
            //if (RawIntuitionPvE.CanUse(out act)) return false;

            if (!Player.WillStatusEndGCD(0, 0, true, StatusID.Bloodwhetting, StatusID.RawIntuition)) return false;

            if (HighDefense(out act)) return true;
            if (ReprisalPvE.CanUse(out act)) return true;
        }
        else
        {
            if (HighDefense(out act)) return true;
            //10
            if (RawIntuitionPvE.CanUse(out act, onLastAbility: true)) return true;
        }

        return false;
    }

    private bool HighDefense(out IAction? act)
    {
        //30
        if ((!RampartPvE.CD.IsCoolingDown || RampartPvE.CD.ElapsedAfter(60)) && VengeancePvE.CanUse(out act)) return true;

        //20
        if (VengeancePvE.CD.IsCoolingDown && VengeancePvE.CD.ElapsedAfter(60) && RampartPvE.CanUse(out act)) return true;

        act = null;
        return false;
    }

    protected override bool DefenseAreaAbility(out IAction? act)
    {
        act = null;
        if (ShakeItOffPvE.CD.IsCoolingDown && !ShakeItOffPvE.CD.WillHaveOneCharge(60)
            || ReprisalPvE.CD.IsCoolingDown && !ReprisalPvE.CD.WillHaveOneCharge(50)) return false;

        if (ShakeItOffPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.DefenseAreaAbility(out act);
    }
}
