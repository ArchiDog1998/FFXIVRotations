namespace DefaultRotations.Magical;

[BetaRotation]
[Rotation("General", CombatType.PvE, GameVersion = "7.01", Description = "This isn't perfect. The logic of it is not clear from the Balance. And I don't like the PCT, so the burst window isn't perfect. If anyone wants to improve it, plz create your own rotation!")]
[SourceCode(Path = "main/DefaultRotations/Magical/PCT_Default.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/pct/pictomancer9spellsinglemuseopener.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/pct/pictomancer8spelltriplemuseburst.png")]
public sealed class PCT_Default : PictomancerRotation
{
    private static bool InBurstStatus => !Player.WillStatusEndGCD(0, 0, true, StatusID.StarryMuse);

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < CountDownAhead 
            && SteelMusePvEReplace.CanUse(out var act, skipAoeCheck: true, usedUp: true)) return act;

        if (remainTime < RainbowDripPvE.Info.CastTime + CountDownAhead 
            && RainbowDripPvE.CanUse(out act, skipAoeCheck: true)) return act;

        if (remainTime > 6)
        {
            if (DrawPictures(out act)) return act;
        }

        return base.CountDownAction(remainTime);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD.IsTheSameTo(ActionID.ClawMotifPvE, ActionID.MawMotifPvE, ActionID.PomMotifPvE, ActionID.WingMotifPvE) 
            && InBurstStatus && Player.WillStatusEndGCD(2, 0, true, StatusID.StarryMuse))
        {
            if (SwiftcastPvE.CanUse(out act)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (CombatElapsedLess(2))
        {
            if (CometInBlackPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (HolyInWhitePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (Player.HasStatus(true, StatusID.StarryMuse)
            && (!CombatElapsedLess(30) || Player.WillStatusEndGCD(2, 0, true, StatusID.StarryMuse)))
        {
            if (StarPrismPvE.CanUse(out act, skipAoeCheck: true)) return true;

            if (!Player.WillStatusEndGCD(0, 0, true, StatusID.RainbowBright))
            {
                if (RainbowDripPvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true)) return true;
            }
        }

        if (!CombatElapsedLess(seperateTime) && !ScenicMusePvE.CD.WillHaveOneCharge(10))
        {
            if (HammerStampPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (DrawPictures(out act)) return true;

        //Aoe
        if (BlizzardIiInCyanPvEReplace.CanUse(out act)) return true;
        if (CometInBlackPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (HolyInWhitePvE.CanUse(out act)) return true;
        if (FireIiInRedPvEReplace.CanUse(out act)) return true;

        //Single
        if (BlizzardInCyanPvEReplace.CanUse(out act)) return true;
        if (FireInRedPvEReplace.CanUse(out act)) return true;

        if (IsMoving) // Moving.
        {
            if (HolyInWhitePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        if (UseBurstMedicine(out act)) return true;

        if (Muse(out act)) return true;

        if (!Player.HasStatus(true, StatusID.HammerTime) 
            && SubtractivePalettePvE.CanUse(out act)) return true;

        return base.AttackAbility(out act);
    }

    private const float seperateTime = 3.5f;

    private bool Muse(out IAction? act)
    {
        if (SteelMusePvEReplace.CanUse(out act, skipAoeCheck: true, usedUp: InBurstStatus)) return true;

        if (InBurstStatus)
        {
            if (MogOfTheAgesPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (!CombatElapsedLess(seperateTime))
        {
            if (ScenicMusePvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (LivingMusePvEReplace.CanUse(out act, skipAoeCheck: true, usedUp: InBurstStatus)) return true;

        return false;
    }

    private bool DrawPictures(out IAction? act)
    {
        act = null;

        if (CombatElapsedLess(seperateTime))
        {
            if (CreatureMotifPvEReplace.CanUse(out act)) return true;
        }
        else
        {
            if (LivingMusePvE.CD.CurrentCharges > 1 ||
                LivingMusePvE.CD.CurrentCharges == 1 
                && Player.WillStatusEndGCD(1, 0, true, StatusID.StarryMuse))
            {
                if (CreatureMotifPvEReplace.CanUse(out act)) return true;
            }
        }

        if (!InCombat || !InBurstStatus && !CombatElapsedLess(20))
            //Don't draw at first.
        {
            if (CreatureMotifPvEReplace.CanUse(out act)) return true;
            if (WeaponMotifPvEReplace.CanUse(out act)) return true;
            if (LandscapeMotifPvEReplace.CanUse(out act)) return true;
        }
        return false;
    }
}
