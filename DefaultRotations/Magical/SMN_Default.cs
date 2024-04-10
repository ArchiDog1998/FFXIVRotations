using System.ComponentModel;

namespace DefaultRotations.Magical;

[BetaRotation]
[RotationDesc(ActionID.SearingLightPvE)]
[Rotation("General purpose", CombatType.PvE, GameVersion = "6.38")]
[SourceCode(Path = "main/DefaultRotations/Magical/SMN_Default.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/smn/6.png")]
public sealed class SMN_Default : SummonerRotation
{
    public enum SwiftType : byte
    {
        No,
        Emerald,
        Ruby,
        All,
    }

    public enum SummonOrderType : byte
    {
        [Description("Topaz-Emerald-Ruby")]
        TopazEmeraldRuby,

        [Description("Topaz-Ruby-Emerald")]
        TopazRubyEmerald,

        [Description("Emerald-Topaz-Ruby")]
        EmeraldTopazRuby,
    }

    [UI("Order")]
    [RotationConfig(CombatType.PvE)]
    public SummonOrderType SummonOrder { get; set; } = SummonOrderType.EmeraldTopazRuby;


    [UI("Use Swiftcast")]
    [RotationConfig(CombatType.PvE)]
    public SwiftType AddSwiftcast { get; set; } = SwiftType.No;

    [UI("Use Crimson Cyclone")]
    [RotationConfig(CombatType.PvE)]
    public bool AddCrimsonCyclone { get; set; } = true;

    public override bool CanHealSingleSpell => false;

    [RotationDesc(ActionID.CrimsonCyclonePvE)]
    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (CrimsonCyclonePvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.MoveForwardGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (SummonCarbunclePvE.CanUse(out act)) return true;

        if (SlipstreamPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (CrimsonStrikePvE.CanUse(out act, skipAoeCheck: true)) return true;

        //AOE
        if (PreciousBrilliancePvE.CanUse(out act)) return true;
        //Single
        if (GemshinePvE.CanUse(out act)) return true;

        if (!IsMoving && AddCrimsonCyclone && CrimsonCyclonePvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((Player.HasStatus(false, StatusID.SearingLight) || SearingLightPvE.Cooldown.IsCoolingDown) && SummonBahamutPvE.CanUse(out act)) return true;
        if (!SummonBahamutPvE.EnoughLevel && HasHostilesInRange && AetherchargePvE.CanUse(out act)) return true;

        if (IsMoving && (Player.HasStatus(true, StatusID.GarudasFavor) || InIfrit)
            && !Player.HasStatus(true, StatusID.Swiftcast) && !InBahamut && !InPhoenix
            && RuinIvPvE.CanUse(out act, skipAoeCheck: true)) return true;

        switch (SummonOrder)
        {
            case SummonOrderType.TopazEmeraldRuby:
            default:
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                break;

            case  SummonOrderType.TopazRubyEmerald:
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                break;

            case  SummonOrderType.EmeraldTopazRuby:
                if (SummonEmeraldPvE.CanUse(out act)) return true;
                if (SummonTopazPvE.CanUse(out act)) return true;
                if (SummonRubyPvE.CanUse(out act)) return true;
                break;
        }


        if (SummonTimeEndAfterGCD() && AttunmentTimeEndAfterGCD() &&
            !Player.HasStatus(true, StatusID.Swiftcast) && !InBahamut && !InPhoenix &&
            RuinIvPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (OutburstPvE.CanUse(out act)) return true;

        //毁123
        if (RuinPvE.CanUse(out act)) return true;
        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        if (IsBurst && !Player.HasStatus(false, StatusID.SearingLight))
        {
            if (SearingLightPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        if ((InBahamut && SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || InPhoenix || 
            IsTargetBoss && IsTargetDying) && EnkindleBahamutPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || IsTargetBoss && IsTargetDying) && DeathflarePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (RekindlePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (MountainBusterPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((Player.HasStatus(false, StatusID.SearingLight) && InBahamut && (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || !EnergyDrainPvE.Cooldown.IsCoolingDown) ||
            !SearingLightPvE.EnoughLevel || IsTargetBoss && IsTargetDying) && PainflarePvE.CanUse(out act)) return true;
        
        if ((Player.HasStatus(false, StatusID.SearingLight) && InBahamut && (SummonBahamutPvE.Cooldown.ElapsedOneChargeAfterGCD(3) || !EnergyDrainPvE.Cooldown.IsCoolingDown) ||
            !SearingLightPvE.EnoughLevel || IsTargetBoss && IsTargetDying) && FesterPvE.CanUse(out act)) return true;

        if (EnergySiphonPvE.CanUse(out act)) return true;
        if (EnergyDrainPvE.CanUse(out act)) return true;

        return base.AttackAbility(out act);
    }
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        switch (AddSwiftcast)
        {
            case SwiftType.No:
            default:
                break;
            case SwiftType.Emerald:
                if (nextGCD.IsTheSameTo(true, SlipstreamPvE) || Attunement == 0 && Player.HasStatus(true, StatusID.GarudasFavor))
                {
                    if (SwiftcastPvE.CanUse(out act)) return true;
                }
                break;
            case SwiftType.Ruby:
                if (InIfrit && (nextGCD.IsTheSameTo(true, GemshinePvE, PreciousBrilliancePvE) || IsMoving))
                {
                    if (SwiftcastPvE.CanUse(out act)) return true;
                }
                break;

            case SwiftType.All:
                if (nextGCD.IsTheSameTo(true, SlipstreamPvE) || Attunement == 0 && Player.HasStatus(true, StatusID.GarudasFavor) ||
                   InIfrit && (nextGCD.IsTheSameTo(true, GemshinePvE, PreciousBrilliancePvE) || IsMoving))
                {
                    if (SwiftcastPvE.CanUse(out act)) return true;
                }
                break;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override IAction? CountDownAction(float remainTime)
    {
        if (SummonCarbunclePvE.CanUse(out var act)) return act;

        if (remainTime <= RuinPvE.Info.CastTime + CountDownAhead
            && RuinPvE.CanUse(out act)) return act;
        return base.CountDownAction(remainTime);
    }
}
