namespace DefaultRotations.Magical;

[BetaRotation]
[Rotation("General", CombatType.PvE, GameVersion = "7.0", Description = "This haven't finished yet! Archi doesn't have enough time!")]
[SourceCode(Path = "main/DefaultRotations/Magical/PCT_Default.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/pct/pictomancer9spellsinglemuseopener.png")]
//[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/pct/1000091140.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/pct/pictomancer8spelltriplemuseburst.png")]
internal class PCT_Default : PictomancerRotation
{
    protected static bool InBurstStatus => !Player.WillStatusEndGCD(0, 0, true, StatusID.StarryMuse);

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 0.4f && SteelMusePvEReplace.CanUse(out var act)) return act;
        if (remainTime < 4.5f && RainbowDripPvE.CanUse(out act, skipAoeCheck: true)) return act;

        if(remainTime > 6)
        {
            if (DrawPictures(out act)) return act;
        }

        return base.CountDownAction(remainTime);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (CombatElapsedLess(3))
        {
            if (HolyInWhitePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (IsMoving) // Moving.
        {
            if (CometInBlackPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (HolyInWhitePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (StarPrismPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (!Player.WillStatusEndGCD(0, 0, true, StatusID.RainbowBright))
        {
            if(RainbowDripPvE.CanUse(out act, skipCastingCheck: true, skipAoeCheck: true)) return true;
        }

        if (HammerStampPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;

        if (DrawPictures(out act)) return true;

        //Aoe
        if (BlizzardIiInCyanPvEReplace.CanUse(out act)) return true;
        if (CometInBlackPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (HolyInWhitePvE.CanUse(out act)) return true;
        if (FireIiInRedPvEReplace.CanUse(out act)) return true;

        //Single
        if (BlizzardInCyanPvEReplace.CanUse(out act)) return true;
        if (FireInRedPvEReplace.CanUse(out act)) return true; 

        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        if (UseBurstMedicine(out act)) return true;

        if (Muse(out act)) return true;

        if (SubtractivePalettePvE.CanUse(out act)) return true;

        return base.AttackAbility(out act);
    }

    private bool Muse(out IAction? act)
    {
        if (SteelMusePvEReplace.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;

        if (CombatElapsedLess(30) || InBurstStatus)
        {
            if (MogOfTheAgesPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        }
        if (LivingMusePvEReplace.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;
        if (ScenicMusePvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        return false;
    }

    private bool DrawPictures(out IAction? act)
    {
        act = null;
        if (!InCombat || !InBurstStatus)
        {
            if (CreatureMotifPvEReplace.CanUse(out act)) return true;
            if (WeaponMotifPvEReplace.CanUse(out act)) return true;
            if (LandscapeMotifPvEReplace.CanUse(out act)) return true;
        }
        return false;
    }
}
