using UnityEngine;
using PixelBattleText;

public static class TextEffectHelper
{
    public static void ShowQualityAtWorldPos(Transform target, float offsetY, QualityType quality)
    {
        if (PixelBattleTextController.singleton == null) return;

        Vector3 worldPos = target.position + new Vector3(0f, offsetY, 0f);
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);

        if (viewportPos.z < 0) return;

        var effect = GetTextAnimByQuality(quality);
        PixelBattleTextController.DisplayText(effect.text, effect.anim, (Vector2)viewportPos);
    }

    public static void ShowInOutAtWorldPos(Transform target, float offsetY, bool isIn)
    {
        if (PixelBattleTextController.singleton == null) return;

        Vector3 worldPos = target.position + new Vector3(0f, offsetY, 0f);
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);

        if (viewportPos.z < 0) return;

        var effect = GetTextAnimByInOut(isIn);
        PixelBattleTextController.DisplayText(effect.text, effect.anim, (Vector2)viewportPos);
    }

    private static (TextAnimation anim, string text) GetTextAnimByQuality(QualityType quality)
    {
        switch (quality)
        {
            case QualityType.Perfect:
                return (Resources.Load<TextAnimation>("VFX/textAnim_perfect"), "PERFECT");

            case QualityType.Good:
                return (Resources.Load<TextAnimation>("VFX/textAnim_good"), "GOOD");

            case QualityType.Bad:
                return (Resources.Load<TextAnimation>("VFX/textAnim_bad"), "BAD");

            case QualityType.Miss:
                return (Resources.Load<TextAnimation>("VFX/textAnim_miss"), "MISS");

            default:
                return (null, string.Empty);
        }
    }

    private static (TextAnimation anim, string text) GetTextAnimByInOut(bool isIn)
    {
        if(isIn)
            return (Resources.Load<TextAnimation>("VFX/textAnim_good"), "IN");
        
        else 
            return (Resources.Load<TextAnimation>("VFX/textAnim_miss"), "OUT");
    }
}