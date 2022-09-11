using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Andallfor.TextEngine {
public class TECharacter {
    public readonly int textIndex, meshIndex;
    public TECPosition position {get; private set;}
    public TECPosition originalPosition {get; private set;}
    public TECColor32 color {get; private set;}
    public TECColor32 originalColor {get; private set;}
    public char representation {get; private set;}
    private TextEngineInstanceDrawer parent;

    internal TECharacter(int textIndex, int meshIndex, char representation, TECPosition initPos, TECColor32 initColor, TextEngineInstanceDrawer parent) {
        this.textIndex = textIndex;
        this.meshIndex = meshIndex;
        this.parent = parent;
        this.representation = representation;
        this.originalPosition = initPos;
        this.originalColor = initColor;
        this.color = originalColor;
    }

    public void setCharacterPosition(TECPosition tecd, bool silent = false) {
        position = tecd;
        if (!silent) parent.markDirty(this);
    }

    public void setCharacterColor(TECColor32 tecc, bool silent = false) {
        color = tecc;
        if (!silent) parent.markDirty(this);
    }

    public void markDirty() {parent.markDirty(this);}
}


public struct TECPosition {
    public Vector3 tl, tr, bl, br;

    public TECPosition(Vector3 all) {
        tl = all;
        tr = all;
        bl = all;
        br = all;
    }

    public TECPosition(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br) {
        this.bl = bl;
        this.tl = tl;
        this.tr = tr;
        this.br = br;
    }

    public TECPosition(TECPosition tecd) {
        this.bl = tecd.bl;
        this.br = tecd.br;
        this.tl = tecd.tl;
        this.tr = tecd.tr;
    }

    public Vector3 center() => (tl + tr + bl + br) / 4f;

    public static TECPosition scale(TECPosition t, float s) {
        Vector3 c = t.center();
        TECPosition _t = new TECPosition(t);

        _t.tl = (t.tl - c) * (s - 1f);
        _t.tr = (t.tr - c) * (s - 1f);
        _t.bl = (t.bl - c) * (s - 1f);
        _t.br = (t.br - c) * (s - 1f);

        return _t;
    }

    public static TECPosition lerp(TECPosition a, TECPosition b, float time) => new TECPosition(
        a.bl + (b.bl - a.bl) * time, a.tl + (b.tl - a.tl) * time, a.tr + (b.tr - a.tr) * time, a.br + (b.br - a.br) * time);

    public static TECPosition operator +(TECPosition a, TECPosition b) => new TECPosition(
        a.bl + b.bl, a.tl + b.tl, a.tr + b.tr, a.br + b.br);
    public static TECPosition operator -(TECPosition a, TECPosition b) => new TECPosition(
        a.bl - b.bl, a.tl - b.tl, a.tr - b.tr, a.br - b.br);
    
    public static TECPosition operator +(TECPosition a, Vector3 b) => new TECPosition(
        a.bl + b, a.tl + b, a.tr + b, a.br + b);
    public static TECPosition operator -(TECPosition a, Vector3 b) => new TECPosition(
        a.bl - b, a.tl - b, a.tr - b, a.br - b);
    
    public override string ToString() => $"tl: {tl}, tr: {tr}, bl: {bl}, br: {br}";
}

public struct TECColor32{
    public Color32 tl, tr, bl, br;

    public TECColor32(Color32 all) {
        tl = all;
        tr = all;
        bl = all;
        br = all;
    }

    public TECColor32(Color32 bl, Color32 tl, Color32 tr, Color32 br) {
        this.bl = bl;
        this.tl = tl;
        this.tr = tr;
        this.br = br;
    }

    public TECColor32(TECColor32 tecd) {
        this.bl = tecd.bl;
        this.br = tecd.br;
        this.tl = tecd.tl;
        this.tr = tecd.tr;
    }

    public static TECColor32 lerp(TECColor32 a, TECColor32 b, float time) => new TECColor32(
        Color32.Lerp(a.bl, b.bl, time), Color32.Lerp(a.tl, b.tl, time), Color32.Lerp(a.tr, b.tr, time), Color32.Lerp(a.br, b.br, time));
    
    public override string ToString() => $"tl: {tl}, tr: {tr}, bl: {bl}, br: {br}";
}
}
