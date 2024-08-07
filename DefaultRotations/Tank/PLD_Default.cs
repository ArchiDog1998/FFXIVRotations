﻿namespace DefaultRotations.Tank;

[Rotation("Tentative v1.2", CombatType.Both, GameVersion = "6.31")]
[LinkDescription("https://xiv.sleepyshiba.com/pld/img/63-60stentative2.png")]
[SourceCode(Path = "main/DefaultRotations/Tank/PLD_Default.cs")]
public class PLD_Default : PaladinRotation
{
    [UI("Use Divine Veil at 15 seconds remaining on Countdown")]
    [RotationConfig(CombatType.PvE)]
    public bool UseDivineVeilPre { get; set; } = false;

    [UI("Use Holy Circle or Holy Spirit when out of melee range")]
    [RotationConfig(CombatType.PvE)]
    public bool UseHolyWhenAway { get; set; } = true;

    [UI("Use Shield Bash when Low Blow is cooling down")]
    [RotationConfig(CombatType.PvE)]
    public bool UseShieldBash { get; set; } = true;

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

        if (InCombat)
        {
            if (UseBurstMedicine(out act)) return true;
            if (IsBurst && !CombatElapsedLess(5) && FightOrFlightPvEReplace.CanUse(out act, onLastAbility: true)) return true;
        }
        if (CombatElapsedLess(8)) return false;

        if (CircleOfScornPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (SpiritsWithinPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;

        if (Player.WillStatusEndGCD(6, 0, true, StatusID.FightOrFlight)
            && RequiescatPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;

        if (!IsMoving && IntervenePvE.CanUse(out act, skipAoeCheck: true, usedUp: HasFightOrFlight)) return true;

        if (HasTankStance && OathGauge == 100 && UseOath(out act)) return true;

        return base.AttackAbility(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (BladeOfValorPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (BladeOfTruthPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (BladeOfFaithPvP.CanUse(out act, skipAoeCheck: true) && Player.HasStatus(true, StatusID.BladeOfFaithReady)) return true;

        if (ConfiteorPvP.CanUse(out act, skipAoeCheck: true)) return true;

        if (RoyalAuthorityPvP.CanUse(out act)) return true;
        if (RiotBladePvP.CanUse(out act)) return true;
        if (FastBladePvP.CanUse(out act)) return true;
        #endregion

        if (Player.HasStatus(true, StatusID.Requiescat))
        {
            if (ConfiteorPvEReplace.CanUse(out act, skipAoeCheck: true))
            {
                if (Player.HasStatus(true, StatusID.ConfiteorReady)) return true;
            }
            if (HolyCirclePvE.CanUse(out act)) return true;
            if (HolySpiritPvE.CanUse(out act)) return true;
        }

        //AOE
        if (HasDivineMight && HolyCirclePvE.CanUse(out act)) return true;
        if (ProminencePvE.CanUse(out act)) return true;
        if (TotalEclipsePvE.CanUse(out act)) return true;

        //Single
        if (!CombatElapsedLess(8) && HasFightOrFlight && GoringBladePvE.CanUse(out act)) return true; // Dot
        if (!FightOrFlightPvE.CD.WillHaveOneChargeGCD(2))
        {
            if (!FightOrFlightPvE.CD.WillHaveOneChargeGCD(6) &&
                HasDivineMight && HolySpiritPvE.CanUse(out act)) return true;
            if (RageOfHalonePvEReplace.CanUse(out act)) return true;
            if (AtonementPvEReplace.CanUse(out act)) return true;
        }
        //123
        if (UseShieldBash && ShieldBashPvE.CanUse(out act)) return true;

        if (RageOfHalonePvEReplace.CanUse(out act)) return true;
        if (RiotBladePvE.CanUse(out act)) return true;
        if (FastBladePvE.CanUse(out act)) return true;

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
