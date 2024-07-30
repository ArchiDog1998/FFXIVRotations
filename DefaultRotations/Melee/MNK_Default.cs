namespace DefaultRotations.Melee;

[Rotation("Double Lunar", CombatType.Both, GameVersion = "7.01")]
[SourceCode(Path = "main/DefaultRotations/Melee/MNK_Default.cs")]
[LinkDescription("https://files.catbox.moe/cy23wy.png", "Double Lunar 5s Buffs DK")]
[LinkDescription("https://files.catbox.moe/pebjzp.png", "Even window")]
[LinkDescription("https://files.catbox.moe/erzg5q.png", "Odd window")]
public sealed class MNK_Default : MonkRotation
{
    [UI("Use Form Shift")]
    [RotationConfig(CombatType.PvE)]
    public bool AutoFormShift { get; set; } = true;

    [RotationConfig(CombatType.PvE)]
    [Range(0, 3, ConfigUnitType.None)]
    [UI("The GCD Count of Using Perfect Balance Before Brotherhood",
        Description = "It may miss one Masterful Blitz if it is set too big or small.")]
    public int GcdCountPB { get; set; } = 3;

    private static bool HasPerfectBalance => Player.HasStatus(true, StatusID.PerfectBalance);

    public bool In120s
    {
        get
        {
            if (HostileTarget?.IsDying() ?? false) return true;
            var cd = BrotherhoodPvE.CD;
            if (!cd.IsCoolingDown) return false;
            return !cd.ElapsedAfter(20);
        }
    }

    public bool In60s 
    {
        get
        {
            if (HostileTarget?.IsDying() ?? false) return true;

            var cd = BrotherhoodPvE.CD;
            if (!cd.IsCoolingDown) return false;
            return cd.ElapsedAfter(60) && !cd.ElapsedAfter(75);
        }
    }

    public MNK_Default()
    {
        PerfectBalancePvE.Setting.RotationCheck = () => HasRaptorForm() && !Player.HasStatus(true, StatusID.FiresRumination, StatusID.WindsRumination);
        FiresReplyPvE.Setting.RotationCheck = () => HasRaptorForm() && !HasPerfectBalance;
    }

    private static bool HasRaptorForm() => Player.HasStatus(true, StatusID.RaptorForm);

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 0.2)
        {
            if (ThunderclapPvE.CanUse(out var act)) return act;
        }
        if (remainTime < 15)
        {
            if (Chakra < 5 && DoMediationPvE(out var act)) return act;
            if (FormShiftPvE.CanUse(out act)) return act;
        }

        return base.CountDownAction(remainTime);
    }

    private bool OpoOpoForm(out IAction? act)
    {
        if (ArmOfTheDestroyerPvEReplace.CanUse(out act)) return true;
        if (DragonKickPvE.CanUse(out act)) return true;
        if (BootshinePvEReplace.CanUse(out act)) return true;
        return false;
    }

    private bool RaptorForm(out IAction? act)
    {
        if (FourpointFuryPvE.CanUse(out act)) return true;
        if (TwinSnakesPvE.CanUse(out act)) return true;
        if (TrueStrikePvEReplace.CanUse(out act)) return true;
        return false;
    }

    private bool CoerlForm(out IAction? act)
    {
        if (RockbreakerPvE.CanUse(out act)) return true;
        if (DemolishPvE.CanUse(out act)) return true;
        if (SnapPunchPvEReplace.CanUse(out act)) return true;
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

        if (!HasPerfectBalance)
        {
            if (FiresReplyPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (WindsReplyPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (Player.HasStatus(true, StatusID.CoeurlForm))
        {
            if (CoerlForm(out act)) return true;
        }
        if (Player.HasStatus(true, StatusID.RaptorForm))
        {
            if (RaptorForm(out act)) return true;
        }
        if (OpoOpoForm(out act)) return true;

        if (Chakra < 5 && DoMediationPvE(out act)) return true;
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
            if (MasterfulBlitzPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        }
        else if (HasPerfectBalance && EnhancedPerfectBalanceTrait.EnoughLevel)
        {
            if (In120s)
            {
                if (LunarNadi(out act)) return true;
            }
            else if (In60s)
            {
                if (SolarNadi(out act)) return true;
            }
            else
            {
                if (LunarNadi(out act)) return true;
            }
        }

        act = null;
        return false;
    }

    bool LunarNadi(out IAction? act)
    {
        return OpoOpoForm(out act);
    }

    bool SolarNadi(out IAction? act)
    {
        if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.OPOOPO))
        {
            if (OpoOpoForm(out act)) return true;
        }
        else if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.RAPTOR))
        {
            if (RaptorForm(out act)) return true;
        }
        else if (!BeastChakra.Contains(Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.COEURL))
        {
            if (CoerlForm(out act)) return true;
        }

        return OpoOpoForm(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (In60s || In120s || CombatElapsedLessGCD(3) || BrotherhoodPvE.CD.WillHaveOneChargeGCD((uint)GcdCountPB))
        {
            if (PerfectBalancePvE.CanUse(out act, usedUp: true)) return true;
        }

        if (CombatElapsedLessGCD(1)) return false;
        if (UseBurstMedicine(out act)) return true;

        if (CombatElapsedLessGCD(2)) return false;

        if (BrotherhoodPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (RiddleOfFirePvEReplace.CanUse(out act)) return true;

        if (RiddleOfWindPvEReplace.CanUse(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        if (HowlingFistPvEReplace.CanUse(out act)) return true;
        if (SteelPeakPvEReplace.CanUse(out act)) return true;

        return base.AttackAbility(out act);
    }
}
