using UnityEngine;

public class Gem : Collectable
{
    protected override void OnCollect()
    {
        ScoreManager.Instance.AddGems(value);
        base.OnCollect();
    }
}
