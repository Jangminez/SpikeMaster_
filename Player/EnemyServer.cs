using UnityEngine;

public class EnemyServer : BasePlayer
{
    protected override void OnInit()
    {
    }

    public override void ResetPlayer()
    {
        base.ResetPlayer();
        transform.position = new Vector3(Random.Range(-2f, 2f), 6f, 0f);
    }
    
    public override void ExecuteAction(QualityType quality)
    {
        PlayAnim(PlayerAnimType.Serve);
    }
}
