namespace DefaultRotations.Magical;

[Rotation("Standard", CombatType.Both, GameVersion = "6.31")]
[SourceCode(Path = "main/DefaultRotations/Magical/RDM_Default.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/rdm/rdm_ew_opener.png")]
public sealed class RDM_Default : RedMageRotation
{
    private static BaseAction VerthunderStartUp { get; } = new BaseAction(ActionID.VerthunderPvE, false);

    private bool CanStartMeleeCombo
    {
        get
        {
            if (Player.HasStatus(true, StatusID.Manafication, StatusID.Embolden) ||
                             BlackMana == 100 || WhiteMana == 100) return true;

            if (BlackMana == WhiteMana) return false;

            else if (WhiteMana < BlackMana)
            {
                if (Player.HasStatus(true, StatusID.VerstoneReady)) return false;
            }
            else
            {
                if (Player.HasStatus(true, StatusID.VerfireReady)) return false;
            }

            if (Player.HasStatus(true, VercurePvE.Setting.StatusProvide ?? [])) return false;

            //Waiting for embolden.
            if (EmboldenPvE.EnoughLevel && EmboldenPvE.CD.WillHaveOneChargeGCD(5)) return false;

            return true;
        }
    }

    [UI("Use Vercure", Description = "Use Vercure for Dualcast when out of combat.")]
    [RotationConfig(CombatType.PvE)]
    public bool UseVercure { get; set; }

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < VerthunderStartUp.Info.CastTime + CountDownAhead
            && VerthunderStartUp.CanUse(out var act)) return act;

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

        act = null;
        if (ManaStacks == 3) return false;

        if (!VerthunderIiPvESet.CanUse(out _))
        {
            if (VerfirePvE.CanUse(out act)) return true;
            if (VerstonePvE.CanUse(out act)) return true;
        }

        if (ScatterPvE.CanUse(out act)) return true;
        if (WhiteMana < BlackMana)
        {
            if (VeraeroIiPvESet.CanUse(out act) && BlackMana - WhiteMana != 5) return true;
            if (VeraeroPvESet.CanUse(out act) && BlackMana - WhiteMana != 6) return true;
        }
        if (VerthunderIiPvESet.CanUse(out act)) return true;
        if (VerthunderPvESet.CanUse(out act)) return true;

        if (JoltPvE.CanUse(out act)) return true;

        if (UseVercure && NotInCombatDelay && VercurePvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool EmergencyGCD(out IAction? act)
    {
        if (ManaStacks == 3)
        {
            if (BlackMana > WhiteMana)
            {
                if (VerholyPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            if (VerflarePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (ResolutionPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ScorchPvE.CanUse(out act, skipAoeCheck: true)) return true;


        if (IsLastGCD(true, MoulinetPvE) && MoulinetPvESet.CanUse(out act, skipAoeCheck: true)) return true;
        if (ZwerchhauPvESet.CanUse(out act)) return true;
        if (RedoublementPvESet.CanUse(out act)) return true;

        if (!CanStartMeleeCombo) return false;

        if (MoulinetPvESet.CanUse(out act))
        {
            if (BlackMana >= 60 && WhiteMana >= 60) return true;
        }
        else
        {
            if (BlackMana >= 50 && WhiteMana >= 50 && RipostePvESet.CanUse(out act)) return true;
        }
        if (ManaStacks > 0 && RipostePvESet.CanUse(out act)) return true;

        return base.EmergencyGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (CombatElapsedLess(4)) return false;

        if (IsBurst && HasHostilesInRange && EmboldenPvESet.CanUse(out act, skipAoeCheck: true)) return true;

        //Use Manafication after embolden.
        if ((Player.HasStatus(true, StatusID.Embolden) || IsLastAbility(ActionID.EmboldenPvE))
            && ManaficationPvESet.CanUse(out act)) return true;

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

        //Swift
        if (ManaStacks == 0 && (BlackMana < 50 || WhiteMana < 50)
            && (CombatElapsedLess(4) || !ManaficationPvE.EnoughLevel || !ManaficationPvE.CD.WillHaveOneChargeGCD(0, 1)))
        {
            if (InCombat && !Player.HasStatus(true, StatusID.VerfireReady, StatusID.VerstoneReady))
            {
                if (SwiftcastPvE.CanUse(out act)) return true;
                if (AccelerationPvE.CanUse(out act, usedUp: true)) return true;
            }
        }

        if (IsBurst && UseBurstMedicine(out act)) return true;

        //Attack abilities.
        if (ContreSixtePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (FlechePvE.CanUse(out act)) return true;

        if (EngagementPvE.CanUse(out act, usedUp: true)) return true;
        if (CorpsacorpsPvE.CanUse(out act) && !IsMoving) return true;

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

