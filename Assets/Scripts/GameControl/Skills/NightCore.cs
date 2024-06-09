using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightCore : PlayerSkill
{
    private float _baseSpeedUp = 1.2F;
    private float _basePitchUp = 1.2F;
    public NightCore(Player _inActivePlayer, bool inIsAvaible) : base(_inActivePlayer, inIsAvaible)
    {
        this._skillId = 1;
        this._cooldown = 5.0F;
        this._duration = 10.0F;
        this._skillName = "NightCore";
        this._skillDescription = "It's time to dance. All notes are bonus notes but speed increased. Have fun";
        this._blockingSkills = new int[] { 0 };
    }

    protected override void ApplySkill()
    {
        this._activePlayer.SetSpeedCoef(this._baseSpeedUp);
        this._activePlayer.SetPitchCoef(this._basePitchUp);
    }

    protected override void RevokeSkill()
    {
        this._activePlayer.SetSpeedCoef(1.0F);
        this._activePlayer.SetPitchCoef(1.0F);
    }
}
