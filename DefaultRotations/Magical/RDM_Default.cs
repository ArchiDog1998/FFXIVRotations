namespace DefaultRotations.Magical;

[Rotation("Standard", CombatType.PvE, GameVersion = "6.31")]
[RotationDesc(ActionID.EmboldenPvE)]
[SourceCode(Path = "main/DefaultRotations/Magical/RDM_Default.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/rdm/rdm_ew_opener.png")]
public sealed class RDM_Default : RedMageRotation
{
    static IBaseAction VerthunderStartUp { get; } = new BaseAction(ActionID.VerthunderPvE, false);

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
            if (EmboldenPvE.EnoughLevel && EmboldenPvE.Cooldown.WillHaveOneChargeGCD(5)) return false;

            return true;
        }
    }

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetBool(CombatType.PvE, "UseVercure", false, "Use Vercure for Dualcast when out of combat.");
    }

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
        act = null;
        if (ManaStacks == 3) return false;

        if (!VerthunderIiPvE.CanUse(out _))
        {
            if (VerfirePvE.CanUse(out act)) return true;
            if (VerstonePvE.CanUse(out act)) return true;
        }

        if (ScatterPvE.CanUse(out act)) return true;
        if (WhiteMana < BlackMana)
        {
            if (VeraeroIiPvE.CanUse(out act) && BlackMana - WhiteMana != 5) return true;
            if (VeraeroPvE.CanUse(out act) && BlackMana - WhiteMana != 6) return true;
        }
        if (VerthunderIiPvE.CanUse(out act)) return true;
        if (VerthunderPvE.CanUse(out act)) return true;

        if (JoltPvE.CanUse(out act)) return true;

        if (Configs.GetBool("UseVercure") && NotInCombatDelay && VercurePvE.CanUse(out act)) return true;

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


        if (IsLastGCD(true, MoulinetPvE) && MoulinetPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ZwerchhauPvE.CanUse(out act)) return true;
        if (RedoublementPvE.CanUse(out act)) return true;

        if (!CanStartMeleeCombo) return false;

        if (MoulinetPvE.CanUse(out act))
        {
            if (BlackMana >= 60 && WhiteMana >= 60) return true;
        }
        else
        {
            if (BlackMana >= 50 && WhiteMana >= 50 && RipostePvE.CanUse(out act)) return true;
        }
        if (ManaStacks > 0 && RipostePvE.CanUse(out act)) return true;

        return base.EmergencyGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (CombatElapsedLess(4)) return false;

        if (IsBurst && HasHostilesInRange && EmboldenPvE.CanUse(out act, skipAoeCheck: true)) return true;

        //Use Manafication after embolden.
        if ((Player.HasStatus(true, StatusID.Embolden) || IsLastAbility(ActionID.EmboldenPvE))
            && ManaficationPvE.CanUse(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        //Swift
        if (ManaStacks == 0 && (BlackMana < 50 || WhiteMana < 50)
            && (CombatElapsedLess(4) || !ManaficationPvE.EnoughLevel || !ManaficationPvE.Cooldown.WillHaveOneChargeGCD(0, 1)))
        {
            if (!Player.HasStatus(true, StatusID.VerfireReady, StatusID.VerstoneReady))
            {
                if (SwiftcastPvE.CanUse(out act)) return true;
                if (InCombat && AccelerationPvE.CanUse(out act, isEmpty: true)) return true;
            }
        }

        if (IsBurst && UseBurstMedicine(out act)) return true;

        //Attack abilities.
        if (ContreSixtePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (FlechePvE.CanUse(out act)) return true;

        if (EngagementPvE.CanUse(out act, isEmpty: true)) return true;
        if (CorpsacorpsPvE.CanUse(out act) && !IsMoving) return true;

        return base.AttackAbility(out act);
    }
}

