namespace DefaultRotations.Magical;

[Rotation("Fire III Opener", CombatType.Both, GameVersion = "7.05", Description = "Aoe isn't ready. It is unfinished.")]
[SourceCode(Path = "main/DefaultRotations/Magical/BLM_Default.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/blm/black-mage-fire-iii-opener.png")]
//[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/blm/black-mage-aoe-with-transpose.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/blm/black-mage-single-target-rotation.png")]
public class BLM_Default : BlackMageRotation 
{
    private static bool HasFire => !Player.WillStatusEnd(0, true, StatusID.Firestarter);

    private bool NeedToGoIce
    {
        get
        {
            //Can use Despair.
            if (DespairPvE.EnoughLevel && CurrentMp >= DespairPvE.Info.MPNeed) return false;

            //Can use Fire1
            if (FirePvE.EnoughLevel && CurrentMp >= FirePvE.Info.MPNeed) return false;

            //Can use Flare Star
            if (AstralSoulStacks == 6) return false;

            return true;
        }
    }

    [UI("Use Transpose to Astral Fire before Paradox")]
    [RotationConfig(CombatType.PvE)]
    public bool UseTransposeForParadox { get; set; } = true;

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < FireIiiPvE.Info.CastTime + CountDownAhead)
        {
            if (FireIiiPvE.CanUse(out var act)) return act;
        }
        return base.CountDownAction(remainTime);
    }

    public BLM_Default()
    {
        FireIiiPvE.RotationCheck = () => !IsLastGCD(ActionID.FireIiiPvE);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (NightWingPvP.CanUse(out act)) return true;
        if (SuperflarePvP.CanUse(out act)) return true;
        #endregion

        if (InAstralFire)
        {
            if (CurrentMp == 0 && ManafontPvE.CanUse(out act)) return true;
            if (SwiftcastPvE.CanUse(out act)) return true;
            if (!CombatElapsedLess(6) && TriplecastPvE.CanUse(out act, usedUp: AstralSoulStacks >= 4)) return true;
        }

        if (!IsPolyglotStacksMaxed && AmplifierPvE.CanUse(out act)) return true;
        if (UseBurstMedicine(out act)) return true;

        if (CombatElapsedLess(20) && Player.HasStatus(true, StatusID.Triplecast))
        {
            if (LeyLinesPvE.CanUse(out act)) return true;
        }

        return base.AttackAbility(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        //To Fire
        if (InUmbralIce && nextGCD.IsTheSameTo(ActionID.FireIiiPvE, ActionID.FireIiPvE) && HasFire)
        {
            if (TransposePvE.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (BurstPvP.CanUse(out act)) return true;
        if (ParadoxPvP.CanUse(out act)) return true;
        if (FirePvP.CanUse(out act)) return true;
        if (BlizzardPvP.CanUse(out act)) return true;
        #endregion

        if (InUmbralIce)
        {
            if (GoFire(out act)) return true;
            if (MaintainIce(out act)) return true;
            if (UsePolyglot(out act)) return true;
            if (DoIce(out act)) return true;
        }
        if (InAstralFire)
        {
            if (GoIce(out act)) return true;
            if (MaintainFire(out act)) return true;
            if (!UsePolyglot(out var attackAct))
            {
                DoFire(out attackAct);
            }
            if (KeepFire(out act, attackAct)) return true;
            else
            {
                act = attackAct;
                return true;
            }
        }

        if (AddElementBase(out act)) return true;
        if (ScathePvE.CanUse(out act)) return true;
        if (MaintainStatus(out act)) return true;

        return base.GeneralGCD(out act);
    }

    private bool GoIce(out IAction? act)
    {
        act = null;

        if (!NeedToGoIce) return false;

        //Go to Ice.
        if (BlizzardIiPvEReplace.CanUse(out act)) return true;
        if (BlizzardIiiPvE.CanUse(out act)) return true;
        if (TransposePvE.CanUse(out act)) return true;
        if (BlizzardPvE.CanUse(out act)) return true;
        return false;
    }

    private bool MaintainIce(out IAction? act)
    {
        act = null;

        if (UmbralIceStacks != 3)
        {
            if (BlizzardIiPvEReplace.CanUse(out act)) return true;
            if (BlizzardIiiPvE.CanUse(out act)) return true;
        }
        return false;
    }

    private bool DoIce(out IAction? act)
    {
        //Add Hearts
        if (UmbralIceStacks == 3 &&
            UmbralHeartTrait.EnoughLevel && UmbralHearts < 3 && !IsLastGCD
            (ActionID.BlizzardIvPvE, ActionID.FreezePvE))
        {
            if (GetUmbralHearts(out act)) return true;
        }

        if (ParadoxPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;

        if (UseThunder(out act)) return true;

        if (BlizzardIiPvEReplace.CanUse(out act)) return true;
        if (BlizzardIvPvE.CanUse(out act)) return true;
        if (BlizzardPvE.CanUse(out act)) return true;
        return false;
    }

    private bool GoFire(out IAction? act)
    {
        act = null;

        //Transpose line
        if (UmbralIceStacks < 3) return false;

        //Need more MP
        if (CurrentMp < 9600) return false;

        if (ParadoxPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;

        //Go to Fire.
        if (FireIiPvEReplace.CanUse(out act)) return true;
        if (FireIiiPvE.CanUse(out act)) return true;
        if (TransposePvE.CanUse(out act)) return true;
        if (FirePvEReplace.CanUse(out act)) return true;

        return false;
    }

    private bool MaintainFire(out IAction? act)
    {
        switch (AstralFireStacks)
        {
            case 1:
                if (FireIiPvEReplace.CanUse(out act)) return true;
                if (FireIiiPvE.CanUse(out act)) return true;
                break;
            case 2:
                if (FireIiPvEReplace.CanUse(out act)) return true;
                if (FirePvEReplace.CanUse(out act)) return true;
                break;
        }

        act = null;
        return false;
    }

    private bool KeepFire(out IAction? act, IAction? nextAct)
    {
        act = null;

        if (nextAct == null && (CurrentMp >= 800 || AstralSoulStacks == 6) || ElementTimeRemaining < GCDTime())
        {
            if (ParadoxPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;
            if (HasFire && FireIiiPvE.CanUse(out act)) return true;
            if (FirePvE.CanUse(out act)) return true;
            return false;
        }

        if (nextAct is not IBaseAction action) return false;
        if (nextAct.IsTheSameTo(ActionID.DespairPvE, ActionID.FlarePvE)) return false;
        var actTime = MathF.Max(action.CD.RecastTime, action.Info.CastTime + 0.1f) + Ping + 0.1f;

        if (ParadoxPvE.CanUse(out act, skipStatusProvideCheck: true)) //Paradox
        {
            if (ElementTimeRemaining < actTime) return true;
        }
        else if (HasFire) //FireIII
        {
            if (ElementTimeRemaining < actTime
                && FireIiiPvE.CanUse(out act)) return true;
        }
        else if (CurrentMp >= FirePvE.Info.MPNeed * 2 + 800) //Fire
        {
            if (ElementTimeRemaining < actTime + fire1CastTime)
            {
                if (FirePvE.CanUse(out act)) return true;
            }
        }
        else
        {
            if (FlarePvE.CanUse(out act)) return true;
            if (DespairPvE.CanUse(out act)) return true;
        }
        return false;
    }

    private float fire1CastTime = 0;
    protected override void UpdateInfo()
    {
        fire1CastTime = MathF.Max(fire1CastTime, FirePvE.Info.CastTime + 0.1f);
        base.UpdateInfo();
    }

    public override void DisplayStatus()
    {
        ImGui.Text("Fire1 CastTime: " + fire1CastTime);
        base.DisplayStatus();
    }

    private bool DoFire(out IAction? act)
    {
        if (UseThunder(out act)) return true;

        if (FlarePvE.CanUse(out act)) return true;

        if (CurrentMp >= FirePvE.Info.MPNeed + 800)
        {
            if (FlareStarPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (FireIvPvE.EnoughLevel)
            {
                if (FireIvPvE.CanUse(out act)) return true;
            }
            else if (!Player.WillStatusEnd(0, true, StatusID.Firestarter))
            {
                if (FireIiiPvE.CanUse(out act)) return true;
            }
            if (FirePvEReplace.CanUse(out act)) return true;
        }

        if (DespairPvE.CanUse(out act)) return true;
        if (FlareStarPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return false;
    }

    private bool AddElementBase(out IAction act)
    {
        if (CurrentMp >= 7200)
        {
            if (FireIiPvEReplace.CanUse(out act)) return true;
            if (FireIiiPvE.CanUse(out act)) return true;
            if (FirePvEReplace.CanUse(out act)) return true;
        }
        else
        {
            if (BlizzardIiPvEReplace.CanUse(out act)) return true;
            if (BlizzardIiiPvE.CanUse(out act)) return true;
            if (BlizzardPvE.CanUse(out act)) return true;
        }
        return false;
    }

    private bool UsePolyglot(out IAction? act)
    {
        if (IsMoving || IsPolyglotStacksMaxed && EnochianTimer < GCDTime(4))
        {
            if (FoulPvE.CanUse(out act)) return true;
            if (XenoglossyPvE.CanUse(out act)) return true;
        }

        act = null;
        return false;
    }

    private bool MaintainStatus(out IAction? act)
    {
        act = null;
        if (CombatElapsedLess(6)) return false;
        if (UmbralSoulPvE.CanUse(out act)) return true;
        if (InAstralFire && TransposePvE.CanUse(out act)) return true;
        if (UseTransposeForParadox &&
            InUmbralIce && !IsParadoxActive && UmbralIceStacks == 3
            && TransposePvE.CanUse(out act)) return true;

        return false;
    }

    protected override bool HealSingleAbility(out IAction? act)
    {
        if (LeyLinesPvE.CanUse(out act)) return true;
        if (!Player.HasStatus(false, StatusID.CircleOfPower))
        {
            if (RetracePvE.CanUse(out act)) return true;
        }
        return base.HealSingleAbility(out act);
    }

    protected override bool MoveBackAbility(out IAction? act)
    {
        if (BetweenTheLinesPvE.CanUse(out act)) return true;
        return base.MoveBackAbility(out act);
    }

    protected override bool GeneralAbility(out IAction? act)
    {
        #region PvP
        if (AetherialManipulationPvP.CanUse(out act)) return true;
        #endregion
        return base.GeneralAbility(out act);
    }
}
