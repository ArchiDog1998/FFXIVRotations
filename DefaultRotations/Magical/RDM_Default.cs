namespace DefaultRotations.Magical;

[BetaRotation]
[Rotation("Standard", CombatType.Both, GameVersion = "7.05")]
[SourceCode(Path = "main/DefaultRotations/Magical/RDM_Default.cs")]
[LinkDescription("https://i.imgur.com/5TW44kN.png")]
[LinkDescription("https://i.imgur.com/LGRfOzV.jpeg")]
public sealed class RDM_Default : RedMageRotation
{
    private bool CanStartMeleeCombo
    {
        get
        {
            //TODO: better way about start melee combo.
            if (Player.HasStatus(true, StatusID.MagickedSwordplay)) return true;

            if (BlackMana < 50 || WhiteMana < 50) return false;

            if (AverageTimeToKill < 20) return true;

            if (EmboldenPvE.EnoughLevel && EmboldenPvE.CD.IsCoolingDown && !EmboldenPvE.CD.ElapsedAfter(25)) return true;

            if (!Player.WillStatusEndGCD(0, 0, true, StatusID.Embolden)
                || BlackMana == 100 || WhiteMana == 100) return true;

            if (EmboldenPvE.EnoughLevel && EmboldenPvE.CD.ElapsedAfter(60) 
                && !EmboldenPvE.CD.ElapsedAfter(70)) return true;

            return false;
        }
    }

    [UI("Use Vercure", Description = "Use Vercure for Dualcast when out of combat.")]
    [RotationConfig(CombatType.PvE)]
    public bool UseVercure { get; set; }

    public RDM_Default()
    {
        SwiftcastPvE.RotationCheck = () =>
        {
            if (CanStartMeleeCombo) return false;

            if (ManaficationPvE.Setting.ComboIdsNot.Union([ActionID.RedoublementPvE, ActionID.EnchantedRedoublementPvE])
                .Contains(LastComboAction)) return false;

            return true;
        };

        unsafe
        {
            VeraeroIiiPvE.RotationCheck = () => Countdown.Instance->Active != 0 || !CombatElapsedLess(3);
        }

        //TODO: 
        //AccelerationPvE.RotationCheck = () =>
    }

    protected override IAction? CountDownAction(float remainTime)
    {
        if (SingleLong(out var act) && act is IBaseAction action
            && remainTime < action.Info.CastTime + CountDownAhead) return act;

        //Remove Swift
        StatusHelper.StatusOff(StatusID.Dualcast);
        StatusHelper.StatusOff(StatusID.Acceleration);
        StatusHelper.StatusOff(StatusID.Swiftcast);

        return base.CountDownAction(remainTime);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (Player.HasStatus(true, StatusID.WhiteShift))
        {
            if (VerstonePvP.CanUse(out act, skipComboCheck: true)) return true;
            if (VeraeroIiiPvP.CanUse(out act)) return true;
            if (EnchantedRipostePvP.CanUse(out act)) return true;
            if (EnchantedZwerchhauPvP.CanUse(out act)) return true;
            if (EnchantedRedoublementPvP.CanUse(out act)) return true;
            if (VerholyPvP.CanUse(out act, skipAoeCheck: true)) return true;
        }
        else if (Player.HasStatus(true, StatusID.BlackShift))
        {
            if (VerfirePvP.CanUse(out act, skipComboCheck: true)) return true;
            if (VerthunderIiiPvP.CanUse(out act)) return true;
            if (EnchantedRipostePvP_29692.CanUse(out act)) return true;
            if (EnchantedZwerchhauPvP_29693.CanUse(out act)) return true;
            if (EnchantedRedoublementPvP_29694.CanUse(out act)) return true;
            if (VerflarePvP.CanUse(out act, skipAoeCheck: true)) return true;
        }
        #endregion

        if (HasSwift)
        {
            if (AoeLong(out act)) return true;
            if (SingleLong(out act)) return true;
        }
        else
        {
            if (AoeShort(out act)) return true;
            if (SingleShort(out act)) return true;
            if (UseVercure && VercurePvE.CanUse(out act)) return true;
        }

        return base.GeneralGCD(out act);
    }

    protected override bool EmergencyGCD(out IAction? act)
    {
        if (FinishManaUsage(out act)) return true;

        if (CanStartMeleeCombo)
        {
            if (EnchantedMoulinetPvE.CanUse(out act)) return true;
            if (MoulinetPvE.CanUse(out act)) return true;
            if (RipostePvEReplace.CanUse(out act)) return true;
        }

        if (!CombatElapsedLess(8))
        {
            if (GrandImpactPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        return base.EmergencyGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (CombatElapsedLess(6)) return false;

        if (PrefulgencePvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (IsBurst && HasHostilesInRange && EmboldenPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((Player.HasStatus(true, StatusID.Embolden) || IsLastAbility(ActionID.EmboldenPvE))
            && ManaficationPvE.CanUse(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (Player.HasStatus(true, StatusID.BlackShift))
        {
            if (ResolutionPvP_29696.CanUse(out act)) return true;
        }
        else if (Player.HasStatus(true, StatusID.WhiteShift))
        {
            if (ResolutionPvP.CanUse(out act)) return true;
        }

        if (DisplacementPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (CorpsacorpsPvP.CanUse(out act, skipAoeCheck: true)) return true;
        #endregion

        if (SwiftcastPvE.CanUse(out act)) return true;
        if (IsBurst && UseBurstMedicine(out act)) return true;
        if (FlechePvE.CanUse(out act)) return true;

        var inburst = Player.HasStatus(true, StatusID.Embolden);

        if (AccelerationPvE.CanUse(out act, usedUp: IsMoving || inburst)) return true;

        if (ContreSixtePvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (EngagementPvE.CanUse(out act)) return true;
        if (CorpsacorpsPvE.CanUse(out act) && !IsMoving) return true;
        if (EngagementPvE.CanUse(out act, usedUp: true)) return true;
        if (CorpsacorpsPvE.CanUse(out act, usedUp: inburst)
            && !IsMoving) return true;

        if (ViceOfThornsPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return base.AttackAbility(out act);
    }

    protected override bool DefenseAreaAbility(out IAction? act)
    {
        #region PvP
        if (MagickBarrierPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (FrazzlePvP.CanUse(out act, skipAoeCheck: true)) return true;
        #endregion
        return base.DefenseAreaAbility(out act);
    }
}