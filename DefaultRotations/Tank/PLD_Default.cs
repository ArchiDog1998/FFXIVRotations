namespace DefaultRotations.Tank;

[BetaRotation]
[Rotation("Standard", CombatType.Both, GameVersion = "7.05",
    Description = "It is almost there but the burst window is not perfect!")]
[LinkDescription("https://xiv.sleepyshiba.com/pld/img/100open.png")]
[LinkDescription("https://xiv.sleepyshiba.com/pld/img/100burst.png")]
[SourceCode(Path = "main/DefaultRotations/Tank/PLD_Default.cs")]
public class PLD_Default : PaladinRotation
{
    [UI("Use Divine Veil at 15 seconds remaining on Countdown")]
    [RotationConfig(CombatType.PvE)]
    public bool UseDivineVeilPre { get; set; } = false;

    [UI("Use Holy Circle or Holy Spirit when out of melee range")]
    [RotationConfig(CombatType.PvE)]
    public bool UseHolyWhenAway { get; set; } = false;

    [UI("Use Shield Bash when Low Blow is cooling down")]
    [RotationConfig(CombatType.PvE)]
    public bool UseShieldBash { get; set; } = true;

    private static bool InBurstStatus => !Player.WillStatusEnd(0, true, StatusID.FightOrFlight);

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < HolySpiritPvE.Info.CastTime + CountDownAhead
            && HolySpiritPvE.CanUse(out var act)) return act;

        if (remainTime < 15 && UseDivineVeilPre
            && DivineVeilPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (ShieldBashPvP.CanUse(out act)) return true;
        if (IntervenePvP.CanUse(out act)) return true;
        #endregion

        if (!CombatElapsedLess(3) && UseBurstMedicine(out act)) return true;
        if (CombatElapsedLess(6)) return false;
        if (IsBurst)
        {
            if (FightOrFlightPvE.CanUse(out act)) return true;
        }
        if (InBurstStatus)
        {
            if (ImperatorPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (RequiescatPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (BladeOfHonorPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (CircleOfScornPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (SpiritsWithinPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;

        if (!IsMoving && IntervenePvE.CanUse(out act, usedUp: InBurstStatus)) return true;

        if (OathGauge == 100 && UseOath(out act)) return true;

        return base.AttackAbility(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (BladeOfValorPvPCombo.CanUse(out act, skipAoeCheck: true)) return true;

        if (ConfiteorPvP.CanUse(out act, skipAoeCheck: true)) return true;

        if (RoyalAuthorityPvPCombo.CanUse(out act)) return true;
        #endregion

        //Burst
        if (ConfiteorPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        if (!TotalEclipsePvE.CanUse(out _) && GoringBladePvE.CanUse(out act)) return true;
        var cd = FightOrFlightPvE.CD;
        if (cd.IsCoolingDown && cd.WillHaveOneCharge(7))
        {
            if (AtonementPvE.CanUse(out act)) return true;
            if (LastComboAction is ActionID.RiotBladePvE)
            {
                if (AtonementPvEReplace.CanUse(out act)) return true;
            }
        }
        else
        {
            if (AtonementPvEReplace.CanUse(out act)) return true;
            if (!Player.WillStatusEnd(0, true, StatusID.DivineMight))
            {
                if (HolyCirclePvE.CanUse(out act)) return true;
                if (HolySpiritPvE.CanUse(out act)) return true;
            }
        }

        //AOE
        if (ProminencePvECombo.CanUse(out act)) return true;

        //Single
        if (UseShieldBash && ShieldBashPvE.CanUse(out act)) return true;
        if (RageOfHalonePvECombo.CanUse(out act)) return true;

        //Range
        if (UseHolyWhenAway)
        {
            if (HolyCirclePvE.CanUse(out act)) return true;
            if (HolySpiritPvE.CanUse(out act)) return true;
        }
        if (ShieldLobPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool DefenseAreaAbility(out IAction? act)
    {
        if (ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (DivineVeilPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(out act);
    }

    protected override bool HealAreaAbility(out IAction? act)
    {
        if (PassageOfArmsPvE.CanUse(out act)) return true;
        return base.HealAreaAbility(out act);
    }

    protected override bool DefenseSingleAbility(out IAction? act)
    {
        //10
        if (BulwarkPvE.CanUse(out act, true)) return true;
        if (UseOath(out act, true)) return true;
        //30
        if ((!RampartPvE.CD.IsCoolingDown || RampartPvE.CD.ElapsedAfter(60)) && SentinelPvEReplace.CanUse(out act)) return true;

        //20
        if (SentinelPvE.CD.IsCoolingDown && SentinelPvE.CD.ElapsedAfter(60) && RampartPvE.CanUse(out act)) return true;

        if (ReprisalPvE.CanUse(out act)) return true;

        return base.DefenseSingleAbility(out act);
    }

    private bool UseOath(out IAction act, bool onLast = false)
    {
        if (SheltronPvEReplace.CanUse(out act, onLastAbility: onLast)) return true;
        if (InterventionPvE.CanUse(out act, onLastAbility: onLast)) return true;

        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        #region PvP
        if (GuardianPvP.CanUse(out act)) return true;
        #endregion

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool GeneralAbility(out IAction? act)
    {
        #region PvP
        if (HolySheltronPvP.CanUse(out act)) return true;
        #endregion

        return base.GeneralAbility(out act);
    }
}
