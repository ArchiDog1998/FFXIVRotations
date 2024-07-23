using System.ComponentModel;

namespace DefaultRotations.Tank;

[BetaRotation]
[Rotation("General", CombatType.Both, GameVersion = "7.0")]
[SourceCode(Path = "main/DefaultRotations/Tank/WAR_Default.cs")]
[LinkDescription("https://raw.githubusercontent.com/Moxfi/XIV/main/Dawntrail/Resources/Openers/Warrior_Dawntrail_2.5_GCD_Warrior_Opener.png")]
[Description("I don't know why the action Primal Wrath was used AFTER Fell Cleave, so I make it earilier.")]
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

        if (StormsPathPvPCombo.CanUse(out act)) return true;
        #endregion

        if (!Player.WillStatusEndGCD(3, 0, true, StatusID.SurgingTempest))
        {
            if (ChaoticCyclonePvE.CanUse(out act)) return true;
            if (InnerChaosPvE.CanUse(out act)) return true;

            if (IsBurstStatus)
            {
                if (!IsMoving && PrimalRendPvEReplace.CanUse(out act, skipAoeCheck: true)
                    && PrimalRendPvEReplace.ChosenAction?.Target.Target?.DistanceToPlayer() < 1) return true;
            }
            if (IsBurstStatus || BeastGauge > 80 || !Player.WillStatusEndGCD(0, 0, false, StatusID.InnerRelease))
            {
                if (SteelCyclonePvEReplace.CanUse(out act)) return true;
                if (InnerBeastPvEReplace.CanUse(out act)) return true;
            }
        }

        if (MythrilTempestPvECombo.CanUse(out act)) return true;

        if (StormsEyePvE.CanUse(out act)) return true;
        if (StormsPathPvECombo.CanUse(out act)) return true;

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

        if (PrimalWrathPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (Player.WillStatusEndGCD(0, 0, false, StatusID.InnerRelease))
        {
            if (InfuriatePvE.CanUse(out act, usedUp: true)) return true;
        }

        if (!Player.WillStatusEndGCD(6, 0, true, StatusID.SurgingTempest)
            || !MythrilTempestPvE.EnoughLevel)
        {
            if (InnerReleasePvE.EnoughLevel)
            {
                if (InnerReleasePvE.CanUse(out act)) return true;
            }
            else
            {
                if (BerserkPvE.CanUse(out act)) return true;
            }
            if (UseBurstMedicine(out act)) return true;
        }

        if (!Player.WillStatusEndGCD(0, 0, false, StatusID.SurgingTempest))
        {
            if (OrogenyPvE.CanUse(out act)) return true;
            if (UpheavalPvE.CanUse(out act)) return true;
            if (OnslaughtPvE.CanUse(out act, usedUp: IsBurstStatus) && !IsMoving) return true;
        }

        return base.AttackAbility(out act);
    }

    protected override bool GeneralAbility(out IAction? act)
    {
        #region PvP
        if (BloodwhettingPvP.CanUse(out act)) return true;
        #endregion

        return base.GeneralAbility(out act);
    }

    protected override bool HealSingleAbility(out IAction? act)
    {
        if (Player.GetHealthRatio() < HealthSingleAbility)
        {
            if (ThrillOfBattlePvE.CanUse(out act)) return true;
            if (EquilibriumPvE.CanUse(out act)) return true;
        }

        if (!HasTankStance && NascentFlashPvE.CanUse(out act)) return true;
        return base.HealSingleAbility(out act);
    }

    protected override bool DefenseSingleAbility(out IAction? act)
    {
        if (Player.HasStatus(true, StatusID.Holmgang_409) && Player.GetHealthRatio() < 0.3f)
        {
            act = null;
            return false;
        }

        if (MobsTime)
        {
            //10
            if (RawIntuitionPvEReplace.CanUse(out act, onLastAbility: true) && NumberOfHostilesInRange > 2) return true;
            //if (RawIntuitionPvE.CanUse(out act)) return false;

            if (!Player.WillStatusEndGCD(0, 0, true, StatusID.Bloodwhetting, StatusID.RawIntuition)) return false;

            if (HighDefense(out act)) return true;
            if (ReprisalPvE.CanUse(out act)) return true;
        }
        else
        {
            if (HighDefense(out act)) return true;
            //10
            if (RawIntuitionPvEReplace.CanUse(out act, onLastAbility: true)) return true;
        }

        return false;
    }

    private bool HighDefense(out IAction? act)
    {
        //40 30
        if (DamnationPvE.CD.JustUsedAfter(60) && VengeancePvEReplace.CanUse(out act)) return true;

        //20
        if ((VengeancePvEReplace.ChosenAction?.CD.JustUsedAfter(60) ?? false) && RampartPvE.CanUse(out act)) return true;

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
