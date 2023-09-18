using System;
using UnityEngine;

public class Target : MonoBehaviour, IEquatable<Target>
{
    public TargetManager.TargetType Type;

    public Vector3 Position { get { return transform.position; } }
    public bool IsOccupied { private set; get; }

    public bool Occupy()
    {
        IsOccupied = true;

        return true;
    }

    public void Free()
    {
        IsOccupied = false;
    }

    public override string ToString()
    {
        return string.Format("{0} : {1}", Type.ToString(), Position.ToString());
    }

    public bool Equals(Target other)
    {
        return Position == other.Position;
    }
}