using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TextPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    public string text = "";
    public float wordSpeed = 20f;
    public bool autoSpeedFromClipDuration = true;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TextPlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.text = text;
        behaviour.wordSpeed = wordSpeed;
        behaviour.autoSpeedFromClipDuration = autoSpeedFromClipDuration;
        return playable;
    }
}

[Serializable]
public class TextPlayableBehaviour : PlayableBehaviour
{
    public string text = "";
    public float wordSpeed = 20f;
    public bool autoSpeedFromClipDuration = true;
    public float holdDurationAfterTyping = 3f; // Minimum time to keep text visible after typing completes

    // internal flag to ensure we only trigger once per clip instance
    private bool triggered = false;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // playerData is the bound object on the track (we expect a CutsceneTxt)
        var cutscene = playerData as CutsceneTxt;
        if (cutscene == null)
            return;

        float weight = (float)info.weight;
        if (weight > 0.5f && !triggered)
        {
            // Calculate effective word speed
            double clipDuration = playable.GetDuration();
            float effectiveSpeed = wordSpeed;

            if (autoSpeedFromClipDuration && clipDuration > 0)
            {
                // Compute speed so text types to completion within (clipDuration - holdDurationAfterTyping)
                // This leaves room for the hold time at the end while the clip is still active
                // availableTypingTime = clipDuration - holdDurationAfterTyping
                // speed = charCount / availableTypingTime
                int charCount = text.Length;
                float availableTypingTime = Mathf.Max(0.1f, (float)clipDuration - holdDurationAfterTyping);
                effectiveSpeed = charCount > 0 ? charCount / availableTypingTime : wordSpeed;
            }

            // Start typing with computed speed and hold duration
            cutscene.PlayTextLineWithHold(text, effectiveSpeed, holdDurationAfterTyping);
            triggered = true;
        }

        if (weight <= 0.5f && triggered)
        {
            // Reset for next time the clip plays
            triggered = false;
        }
    }
}

// Note: `TextPlayableTrack` must live in its own file named `TextPlayableTrack.cs`.
// Track class was moved to its own file to satisfy Unity's requirement.
