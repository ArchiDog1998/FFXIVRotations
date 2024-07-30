namespace DefaultRotations.Melee;

[BetaRotation]
[Rotation("General", CombatType.PvE, GameVersion = "7.01", Description ="It has a perfect opener, but a not perfect burst windows. It may waste some Dreadwinder.")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/vpr/viper-tentative-standard-opener.png")]
[SourceCode(Path = "main/DefaultRotations/Melee/VPR_Default.cs")]
public sealed class VPR_Default : ViperRotation
{
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 0.2)
        {
            if (SlitherPvE.CanUse(out var act, usedUp: true)) return act;
        }
        return base.CountDownAction(remainTime);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        var willDie = HostileTarget?.IsDying() ?? false;
        var cd = SerpentsIrePvE.CD;
        var inLongBurst = cd.IsCoolingDown && !cd.ElapsedAfter(35) || willDie;
        var burst = cd.IsCoolingDown && (!cd.ElapsedAfter(20)
            || cd.ElapsedAfter(60) && !cd.ElapsedAfter(75)) || willDie;

        if (RattlingCoilStacks >= 3)
        {
            if (FinishCoil(out act)) return true;
            if (UncoiledFuryPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }
        if (Player.HasStatus(true, StatusID.Reawakened) && !SerpentsIrePvE.CD.IsCoolingDown)
        {
            if (UncoiledFuryPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (FinishReawaken(out act)) return true;

        //Start the status.
        if (CombatElapsedLess(2))
        {
            if (BasicCombo(out act)) return true;
        }
        if (CombatElapsedLess(10))
        {
            if (FinishCoil(out act)) return true;
            if (StartCoil(out act, true)) return true;
        }

        if (SerpentOffering == 100 || burst)
        {
            if (HostileTarget?.WillStatusEnd(10, true, StatusID.NoxiousGnash) ?? false)
            {
                if (StartCoil(out act, true)) return true;
            }
            if (FinishCoil(out act)) return true;
            if (ReawakenPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (BasicSingleCombo3(out act)) return true;

        var willNoDebuff = HostileTarget?.WillStatusEndGCD(1, 0, true, StatusID.NoxiousGnash) ?? false;
        if (!willNoDebuff)
        {
            if ((burst || inLongBurst) && UncoiledFuryPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }
        if (FinishCoil(out act)) return true;
        if (StartCoil(out act, inLongBurst || willNoDebuff || DreadwinderPvE.CD.CurrentCharges == DreadwinderPvE.CD.MaxCharges
            || cd.WillHaveOneCharge(9) && (HostileTarget?.WillStatusEnd(17, true, StatusID.NoxiousGnash) ?? false))) return true;

        //TODO: It may lose a Combo3 buff!
        if (BasicCombo(out act)) return true;

        //Range
        if (UncoiledFuryPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (WrithingSnapPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    private bool StartCoil(out IAction? act, bool usedUp)
    {
        if (PitOfDreadPvE.CanUse(out act, usedUp: usedUp)) return true;
        if (DreadwinderPvE.CanUse(out act, usedUp: usedUp)) return true;
        return false;
    }

    private bool FinishCoil(out IAction? act)
    {
        if (HuntersCoilPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (SwiftskinsCoilPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (HuntersDenPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (SwiftskinsDenPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return false;
    }

    private bool FinishReawaken(out IAction? act)
    {
        if (FirstGenerationPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (SecondGenerationPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ThirdGenerationPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (FourthGenerationPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (OuroborosPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return false;
    }

    private bool BasicSingleCombo3(out IAction? act)
    {
        //Single
        if (FlankstingStrikePvE.CanUse(out act)) return true;
        if (FlanksbaneFangPvE.CanUse(out act)) return true;
        if (HindstingStrikePvE.CanUse(out act)) return true;
        if (HindsbaneFangPvE.CanUse(out act)) return true;
        return false;
    }

    private bool BasicCombo(out IAction? act)
    {
        //Aoe
        if (JaggedMawPvE.CanUse(out act)) return true;
        if (BloodiedMawPvE.CanUse(out act)) return true;

        if (Player.StatusTime(false, StatusID.HuntersInstinct) > Player.StatusTime(false, StatusID.Swiftscaled))
        {
            if (SwiftskinsBitePvE.CanUse(out act)) return true;
        }
        else
        {
            if (HuntersBitePvE.CanUse(out act)) return true;
        }

        if (DreadMawPvE.CanUse(out act)) return true;
        if (SteelMawPvE.CanUse(out act)) return true;

        //Single
        if (!Player.WillStatusEndGCD(0, 0, true, StatusID.FlankstungVenom, StatusID.FlanksbaneVenom))
        {
            if (HuntersStingPvE.CanUse(out act)) return true;
        }
        else
        {
            if (SwiftskinsStingPvE.CanUse(out act)) return true;
        }

        if (DreadFangsPvE.CanUse(out act)) return true;
        if (SteelFangsPvE.CanUse(out act)) return true;

        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (SerpentsTailPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        if (TwinbloodPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        if (TwinfangPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;

        //Burst
        if (SerpentsIrePvE.CanUse(out act)) return true;

        if (!CombatElapsedLess(4))
        {
            if (UseBurstMedicine(out act)) return true;
        }

        return base.AttackAbility(out act);
    }
}