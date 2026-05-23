using UnityEngine;

public class PlayerAnimHandler
{
    private Animator _anim;
    private readonly float _trasitionDuiration = 0f;

    public PlayerAnimHandler(Animator anim)
    {
        _anim = anim;
    }

    public void PlayAnim(PlayerAnimType type)
    {
        switch (type)
        {
            case PlayerAnimType.Idle:
                PlayIdleAnim();
                break;

            case PlayerAnimType.Receive:
                PlayReceiveAnim();
                break;

            case PlayerAnimType.Serve:
                PlayServeAnim();
                break;

            case PlayerAnimType.Toss:
                PlayTossAnim();
                break;

            case PlayerAnimType.Spike_Start:
                PlaySpikeStartAnim();
                break;

            case PlayerAnimType.Spike_End:
                PlaySpikeEndAnim();
                break;
        }
    }

    private void PlayIdleAnim() => _anim?.CrossFade(PlayerAnimHashes.Idle, _trasitionDuiration);
    private void PlayReceiveAnim() => _anim?.CrossFade(PlayerAnimHashes.Receive, _trasitionDuiration);
    private void PlayServeAnim() => _anim?.CrossFade(PlayerAnimHashes.Serve, _trasitionDuiration);
    private void PlayTossAnim() => _anim?.CrossFade(PlayerAnimHashes.Toss, _trasitionDuiration);
    private void PlaySpikeStartAnim() => _anim?.CrossFade(PlayerAnimHashes.Spike_Start, _trasitionDuiration);
    private void PlaySpikeEndAnim() => _anim?.CrossFade(PlayerAnimHashes.Spike_End, _trasitionDuiration);
}
