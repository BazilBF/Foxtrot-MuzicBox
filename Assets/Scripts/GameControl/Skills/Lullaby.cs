using System.Collections;
using System.Collections.Generic;

public class Lullaby : PlayerSkill
{
    private float _baseSlowDown = 0.8F;
    public Lullaby(Player _inActivePlayer, bool inIsAvaible) : base(_inActivePlayer, inIsAvaible)
    {
        this._skillId = 0;
        this._cooldown = 5.0F;
        this._duration = 10.0F;
        this._skillName = "Lullaby";
        this._skillDescription = "Slows down music, don't fall asleep";
        this._blockingSkills = new int[] { 1 };
        this._netColors = new UnityEngine.Color[] { new UnityEngine.Color(1.0F, 0.9F, 0.0F), new UnityEngine.Color(1.0F, 0.6F, 0.0F) };
    }

    protected override void ApplySkill()
    {
        this._activePlayer.SetSpeedCoef(this._baseSlowDown);
    }

    protected override void RevokeSkill()
    {
        this._activePlayer.SetSpeedCoef(1.0F);
    }
}
