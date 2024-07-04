namespace DefaultRotations.Melee;

[Rotation("Lunar Solar", CombatType.Both, GameVersion = "6.35")]
[SourceCode(Path = "main/DefaultRotations/Melee/MNK_Default.cs")]
[LinkDescription("https://i.imgur.com/C5lQhpe.png")]

public sealed class MNK_Default : MonkRotation
{
    [UI("Use Form Shift")]
    [RotationConfig(CombatType.PvE)]
    public bool AutoFormShift { get; set; } = true;

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 0.2)
        {
            if (ThunderclapPvE.CanUse(out var act)) return act;
        }
        if (remainTime < 15)
        {
            //if (Chakra < 5 && MeditationPvE.CanUse(out var act)) return act;
            if (FormShiftPvE.CanUse(out var act)) return act;
        }

        return base.CountDownAction(remainTime);
    }

    private bool OpoOpoForm(out IAction? act)
    {
        if (ArmOfTheDestroyerPvESet.CanUse(out act)) return true;
        if (DragonKickPvE.CanUse(out act)) return true;
        if (BootshinePvESet.CanUse(out act)) return true;
        return false;
    }

    private bool UseLunarPerfectBalance => (HasSolar || Player.HasStatus(false, StatusID.PerfectBalance))
        && (!Player.WillStatusEndGCD(0, 0, false, StatusID.RiddleOfFire) || Player.HasStatus(false, StatusID.RiddleOfFire) || RiddleOfFirePvE.CD.WillHaveOneChargeGCD(2)) && PerfectBalancePvE.CD.WillHaveOneChargeGCD(3);

    private bool RaptorForm(out IAction? act)
    {
        if (FourpointFuryPvE.CanUse(out act)) return true;
        if ((Player.WillStatusEndGCD(3, 0, true, StatusID.DisciplinedFist)
            || Player.WillStatusEndGCD(7, 0, true, StatusID.DisciplinedFist)
            && UseLunarPerfectBalance) && TwinSnakesPvE.CanUse(out act)) return true;
        if (TrueStrikePvESet.CanUse(out act)) return true;
        return false;
    }

    private bool CoerlForm(out IAction? act)
    {
        if (RockbreakerPvE.CanUse(out act)) return true;
        if (UseLunarPerfectBalance && DemolishPvE.CanUse(out act, skipStatusProvideCheck: true)
            && (DemolishPvE.Target.Target?.WillStatusEndGCD(7, 0, true, StatusID.Demolish) ?? false)) return true;
        if (DemolishPvE.CanUse(out act)) return true;
        if (SnapPunchPvESet.CanUse(out act)) return true;
        return false;
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (RisingPhoenixPvP.CanUse(out act)) return true;
        if (EnlightenmentPvP.CanUse(out act)) return true;
        if (RisingPhoenixPvP.CanUse(out act)) return true;
        if (PhantomRushPvP.CanUse(out act)) return true;
        if (SixsidedStarPvP.CanUse(out act)) return true;
        if (EnlightenmentPvP.CanUse(out act, usedUp: true)) return true;

        if (InCombat)
        {
            if (RisingPhoenixPvP.CanUse(out act)) return true;
            if (ThunderclapPvP.CanUse(out act)) return true;
            if (RiddleOfEarthPvP.CanUse(out act)) return true;
        }

        if (Player.HasStatus(true, StatusID.EarthResonance))
        {
            if (EarthsReplyPvP.CanUse(out act)) return true;
        }

        if (PhantomRushPvP.CanUse(out act)) return true;
        if (DemolishPvP.CanUse(out act)) return true;
        if (TwinSnakesPvP.CanUse(out act)) return true;
        if (DragonKickPvP.CanUse(out act)) return true;
        if (SnapPunchPvP.CanUse(out act)) return true;
        if (TrueStrikePvP.CanUse(out act)) return true;
        if (BootshinePvP.CanUse(out act)) return true;
        #endregion

        if (PerfectBalanceActions(out act)) return true;

        if (Player.HasStatus(true, StatusID.CoeurlForm))
        {
            if (CoerlForm(out act)) return true;
        }

        if (Player.HasStatus(true, StatusID.RiddleOfFire)
            && !RiddleOfFirePvE.CD.ElapsedAfterGCD(2) && (PerfectBalancePvE.CD.ElapsedAfter(60) || !PerfectBalancePvE.CD.IsCoolingDown))
        {
            if (OpoOpoForm(out act)) return true;
        }
        if (Player.HasStatus(true, StatusID.RaptorForm))
        {
            if (RaptorForm(out act)) return true;
        }
        if (OpoOpoForm(out act)) return true;

        //if (Chakra < 5 && MeditationPvE.CanUse(out act)) return true;
        if (AutoFormShift && FormShiftPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (MoveForwardAbility(out act)) return true;
        return base.MoveForwardGCD(out act);
    }

    private bool PerfectBalanceActions(out IAction? act)
    {
       
        if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE))
        {
            if (HasSolar && HasLunar)
            {
                if (PhantomRushPvE.CanUse(out act, skipAoeCheck: true)) return true;
                if (TornadoKickPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            if (BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR))
            {
                if (RisingPhoenixPvE.CanUse(out act, skipAoeCheck: true)) return true;
                if (FlintStrikePvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            else
            {
                if (ElixirFieldPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
        }
        else if (Player.HasStatus(true, StatusID.PerfectBalance) && ElixirFieldPvE.EnoughLevel)
        {
            //Sometimes, no choice
            if (HasSolar || BeastChakra.Count(c => c == Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.OPOOPO) > 1)
            {
                if (LunarNadi(out act)) return true;
            }
            else if (BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.COEURL) || BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR))
            {
                if (SolarNadi(out act)) return true;
            }

            //Add status when solar.
            if (Player.WillStatusEndGCD(3, 0, true, StatusID.DisciplinedFist)
                || (HostileTarget?.WillStatusEndGCD(3, 0, true, StatusID.Demolish) ?? false))
            {
                if (SolarNadi(out act)) return true;
            }
            if (LunarNadi(out act)) return true;
        }

        act = null;
        return false;
    }

    bool LunarNadi(out IAction? act)
    {
        if (OpoOpoForm(out act)) return true;
        return false;
    }

    bool SolarNadi(out IAction? act)
    {
        //Emergency usage of status.
        if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR)
            && HasLunar
            && Player.WillStatusEndGCD(1, 0, true, StatusID.DisciplinedFist))
        {
            if (RaptorForm(out act)) return true;
        }
        if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.COEURL)
            && (HostileTarget?.WillStatusEndGCD(1, 0, true, StatusID.Demolish) ?? false))
        {
            if (CoerlForm(out act)) return true;
        }

        if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.OPOOPO))
        {
            if (OpoOpoForm(out act)) return true;
        }
        if (HasLunar && !BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR))
        {
            if (RaptorForm(out act)) return true;
        }
        if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.COEURL))
        {
            if (CoerlForm(out act)) return true;
        }
        if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR))
        {
            if (RaptorForm(out act)) return true;
        }

        return CoerlForm(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat)
        {
            if (UseBurstMedicine(out act)) return true;
            if (IsBurst && !CombatElapsedLessGCD(2) && RiddleOfFirePvESet.CanUse(out act, onLastAbility: true)) return true;
        }

        act = null;
        if (CombatElapsedLessGCD(3)) return false;

        if (BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE) && Player.HasStatus(true, StatusID.RaptorForm)
            && (!RiddleOfFirePvE.EnoughLevel || Player.HasStatus(false, StatusID.RiddleOfFire) && !Player.WillStatusEndGCD(3, 0, false, StatusID.RiddleOfFire)
            || RiddleOfFirePvE.CD.WillHaveOneChargeGCD(1) && (PerfectBalancePvE.CD.ElapsedAfter(60) || !PerfectBalancePvE.CD.IsCoolingDown)))
        {
            if (PerfectBalancePvE.CanUse(out act, usedUp: true)) return true;
        }

        if (BrotherhoodPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        act = null;

        if (CombatElapsedLessGCD(3)) return false;

        if (HowlingFistPvESet.CanUse(out act)) return true;
        if (SteelPeakPvESet.CanUse(out act)) return true;
        if (HowlingFistPvESet.CanUse(out act, skipAoeCheck: true)) return true;

        if (RiddleOfWindPvESet.CanUse(out act)) return true;

        return base.AttackAbility(out act);
    }
}
