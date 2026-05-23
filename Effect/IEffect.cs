using System;

public interface IEffect
{
    void Play(Action onComplete = null);

    void Stop();

    bool IsPlaying { get; }
}
