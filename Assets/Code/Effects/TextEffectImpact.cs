using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Andallfor.TextEngine {
internal class TextEffectSpreadHandler : TextEffectHandler {
    private List<TextEffectSpreadData> activeEffects = new List<TextEffectSpreadData>();
    private Dictionary<int, float> steps = new Dictionary<int, float>(); // ugly, but bypasses csharp1654
    private int lastSeen = -1;
    private float animationTime, timeChange;
    private Vector2 spreadRange, scaleRange;

    public TextEffectSpreadHandler(float animationTime, Vector2 scaleRange, Vector2 spreadRange) {
        this.animationTime = animationTime;
        this.scaleRange = scaleRange;
        this.spreadRange = spreadRange;

        this.timeChange = (1f / animationTime) * (1f / 60f);
    }

    public override bool update(int i, int offset, bool allSent) {
        if (text.textInfo.meshInfo[0].vertexCount == 0) return true;

        if (i != lastSeen) {
            if (text.text[i] != ' ') {
                System.Random r = new System.Random();
                float x = (float) r.NextDouble() * spreadRange.x * 2f - spreadRange.x;
                float y = (float) r.NextDouble() * spreadRange.y * 2f - spreadRange.y;
                float s = scaleRange.x + (scaleRange.y - scaleRange.x) * (float) r.NextDouble();
                var off = new TextEngineCharacterData(new Vector3(x, y, 0));

                TextEngineCharacterData pos = parent.getCharacterPosition(i - offset);
                activeEffects.Add(new TextEffectSpreadData() {
                    textIndex = i, meshIndex = i - offset,
                    targetPosition = pos,
                    initalPosition = pos + off,
                    scale = s
                });

                steps[i - offset] = 0;

                parent.setCharacterPosition(i - offset, (pos + off) + TextEngineCharacterData.scale(pos, s));
            }

            lastSeen = i;
        }

        if (activeEffects.Count == 0) return true;
        bool stillMoving = false;

        foreach (var data in activeEffects) {
            float step = steps[data.meshIndex];
            if (step >= 1) continue;
            else if (step + timeChange >= 1) parent.setCharacterPosition(data.meshIndex, data.targetPosition);
            else {
                TextEngineCharacterData pos = TextEngineCharacterData.lerp(data.initalPosition, data.targetPosition, interpPos(step));
                pos += TextEngineCharacterData.scale(data.targetPosition, data.scale - (data.scale - 1f) * step * step * step);
                parent.setCharacterPosition(data.meshIndex, pos);
                steps[data.meshIndex] = step + timeChange;
                stillMoving = true;
            }
            
        }

        return stillMoving;
    }

    private float interpPos(float x) => (float)
        (-70404137.6188 * Math.Pow(x, 5.93504071248) +
        7968487.36242 * Math.Pow(x, 5.93120866927) +
        62435651.2044 * Math.Pow(x, 5.93552981156) +
        0.0575461897356);
}

internal struct TextEffectSpreadData {
    public int textIndex, meshIndex;
    public float scale;
    public TextEngineCharacterData targetPosition, initalPosition;
}
}
