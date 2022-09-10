using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System;
using System.Threading.Tasks;

namespace Andallfor.TextEngine {
public class TextEngine
{
    private Queue<TextEngineInstance> internalTextQueue = new Queue<TextEngineInstance>();

    public TextEngine(List<TextEngineInstance> instances) {
        internalTextQueue = new Queue<TextEngineInstance>(instances);
    }

    public async Task<bool> drawNext() {
        if (internalTextQueue.Count == 0) return false;

        await (new TextEngineInstanceDrawer(internalTextQueue.Dequeue())).start();

        return true;
    }
}

internal class TextEngineInstanceDrawer {
    public TextEngineInstance tei;
    public TextMeshProUGUI t;
    public Task updateLoop;
    public int waitTimer, currentReadingIndex = -1, invalidOffset = 0;
    public List<TextEffectHandler> effects = new List<TextEffectHandler>();
    public Queue<TextEngineCharacterData> charData = new Queue<TextEngineCharacterData>();
    public Vector3[] characterVerts;
    public bool isDirty;

    public TextEngineInstanceDrawer(TextEngineInstance tei) {
        this.tei = tei;
        this.t = tei.options.textRepresentation;
        this.waitTimer = Mathf.RoundToInt(1000f * (60f / tei.options.lettersPerMinute));

        t.text = "";

        effects.Add(new TextEffectSpreadHandler(0.3f, new Vector2(0.85f, 1.25f), new Vector2(50, -60)));
        //effects.Add(new TextEffectWaveHandler(1, 1));
        
        
        updateLoop = new Task(async () => {
            while (true) {
                if (currentReadingIndex == -1) continue;
                if (effects.Count > 0) {
                    List<TextEffectHandler> _effects = new List<TextEffectHandler>();
                    bool allSent = currentReadingIndex == tei.text.Length - 1;
                    foreach (var effect in effects) {
                        bool response = effect.update(currentReadingIndex, invalidOffset, allSent);
                        if (response || !allSent) _effects.Add(effect);
                    }

                    effects = _effects;
                }

                if (isDirty) {
                    isDirty = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => {
                        t.textInfo.meshInfo[0].mesh.vertices = characterVerts;
                        t.UpdateGeometry(t.textInfo.meshInfo[0].mesh, 0);
                    });
                }
                
                await Task.Delay(17); // 1/60th of a second
            }
        });

        // clear/save textint
        t.text = tei.text;
        t.ForceMeshUpdate(true, true);
        Vector3[] verts = t.textInfo.meshInfo[0].vertices;
        int offset = 0;
        for (int i = 0; i < tei.text.Length; i++) {
            if (tei.text[i] == ' ') {offset++; continue;}

            int index = (i - offset) * 4;

            charData.Enqueue(new TextEngineCharacterData(verts[index], verts[index + 1], verts[index + 2], verts[index + 3]));
        }

        t.textInfo.meshInfo[0].mesh.vertices = new Vector3[t.textInfo.meshInfo[0].mesh.vertices.Length];
        t.UpdateGeometry(t.textInfo.meshInfo[0].mesh, 0);

        characterVerts = new Vector3[t.textInfo.meshInfo[0].vertices.Length];
    }

    public async Task<bool> start() {
        foreach (TextEffectHandler teh in effects) teh.init(t, this);
        updateLoop.Start();
        currentReadingIndex = -1;

        for (int i = 0; i < tei.text.Length; i++) {
            currentReadingIndex = i;
            if (tei.text[i] == ' ') {invalidOffset++; await Task.Delay(waitTimer); continue;}

            setCharacterPosition(i - invalidOffset, charData.Dequeue());
            
            await Task.Delay(waitTimer);
        }

        await updateLoop;

        return true;
    }

    public void setCharacterPosition(int meshIndex, TextEngineCharacterData tecd) {
        characterVerts[meshIndex * 4] = tecd.bl;
        characterVerts[meshIndex * 4 + 1] = tecd.tl;
        characterVerts[meshIndex * 4 + 2] = tecd.tr;
        characterVerts[meshIndex * 4 + 3] = tecd.br;

        isDirty = true;
    }

    public void moveCharacterPosition(int meshIndex, TextEngineCharacterData tecd) {
        characterVerts[meshIndex * 4] += tecd.bl;
        characterVerts[meshIndex * 4 + 1] += tecd.tl;
        characterVerts[meshIndex * 4 + 2] += tecd.tr;
        characterVerts[meshIndex * 4 + 3] += tecd.br;

        isDirty = true;
    }

    public TextEngineCharacterData getCharacterPosition(int meshIndex) => new TextEngineCharacterData(
        characterVerts[meshIndex * 4],
        characterVerts[meshIndex * 4 + 1],
        characterVerts[meshIndex * 4 + 2],
        characterVerts[meshIndex * 4 + 3]);
}

internal struct TextEngineCharacterData {
    public Vector3 tl, tr, bl, br;

    public TextEngineCharacterData(Vector3 all) {
        tl = all;
        tr = all;
        bl = all;
        br = all;
    }

    public TextEngineCharacterData(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br) {
        this.bl = bl;
        this.tl = tl;
        this.tr = tr;
        this.br = br;
    }

    public TextEngineCharacterData(TextEngineCharacterData tecd) {
        this.bl = tecd.bl;
        this.br = tecd.br;
        this.tl = tecd.tl;
        this.tr = tecd.tr;
    }

    public Vector3 center() => (tl + tr + bl + br) / 4f;

    public static TextEngineCharacterData scale(TextEngineCharacterData t, float s) {
        Vector3 c = t.center();
        TextEngineCharacterData _t = new TextEngineCharacterData(t);

        _t.tl = (t.tl - c) * (s - 1f);
        _t.tr = (t.tr - c) * (s - 1f);
        _t.bl = (t.bl - c) * (s - 1f);
        _t.br = (t.br - c) * (s - 1f);

        return _t;
    }

    public static TextEngineCharacterData lerp(TextEngineCharacterData a, TextEngineCharacterData b, float time) => new TextEngineCharacterData(
        a.bl + (b.bl - a.bl) * time, a.tl + (b.tl - a.tl) * time, a.tr + (b.tr - a.tr) * time, a.br + (b.br - a.br) * time);

    public static TextEngineCharacterData operator +(TextEngineCharacterData a, TextEngineCharacterData b) => new TextEngineCharacterData(
        a.bl + b.bl, a.tl + b.tl, a.tr + b.tr, a.br + b.br);
    public static TextEngineCharacterData operator -(TextEngineCharacterData a, TextEngineCharacterData b) => new TextEngineCharacterData(
        a.bl - b.bl, a.tl - b.tl, a.tr - b.tr, a.br - b.br);
    
    public static TextEngineCharacterData operator +(TextEngineCharacterData a, Vector3 b) => new TextEngineCharacterData(
        a.bl + b, a.tl + b, a.tr + b, a.br + b);
    public static TextEngineCharacterData operator -(TextEngineCharacterData a, Vector3 b) => new TextEngineCharacterData(
        a.bl - b, a.tl - b, a.tr - b, a.br - b);
    
    public override string ToString() => $"tl: {tl}, tr: {tr}, bl: {bl}, br: {br}";
}
}
