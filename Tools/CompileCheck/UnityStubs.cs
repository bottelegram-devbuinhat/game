using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeField : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string header) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string tooltip) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RangeAttribute : Attribute
    {
        public RangeAttribute(float min, float max) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponent : Attribute
    {
        public RequireComponent(Type requiredComponent) { }
        public RequireComponent(Type requiredComponent, Type requiredComponent2) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string fileName { get; set; }
        public string menuName { get; set; }
        public int order { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DisallowMultipleComponent : Attribute { }

    [Serializable]
    public struct Vector2 : IEquatable<Vector2>
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 zero => new Vector2(0f, 0f);
        public static Vector2 one => new Vector2(1f, 1f);
        public static Vector2 up => new Vector2(0f, 1f);
        public static Vector2 right => new Vector2(1f, 0f);

        public float sqrMagnitude => x * x + y * y;
        public float magnitude => MathF.Sqrt(sqrMagnitude);

        public Vector2 normalized
        {
            get
            {
                float mag = magnitude;
                return mag > 1e-5f ? new Vector2(x / mag, y / mag) : zero;
            }
        }

        public static float Dot(Vector2 lhs, Vector2 rhs) => lhs.x * rhs.x + lhs.y * rhs.y;
        public static float Distance(Vector2 a, Vector2 b) => (a - b).magnitude;

        public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
        {
            float sqrMag = vector.sqrMagnitude;
            if (sqrMag > maxLength * maxLength && sqrMag > 0f)
            {
                float mag = MathF.Sqrt(sqrMag);
                return new Vector2(vector.x / mag * maxLength, vector.y / mag * maxLength);
            }
            return vector;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y);
        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.x * d, a.y * d);
        public static Vector2 operator *(float d, Vector2 a) => new Vector2(a.x * d, a.y * d);
        public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.x / d, a.y / d);
        public static bool operator ==(Vector2 lhs, Vector2 rhs) => (lhs - rhs).sqrMagnitude < 1e-10f;
        public static bool operator !=(Vector2 lhs, Vector2 rhs) => !(lhs == rhs);

        public bool Equals(Vector2 other) => x.Equals(other.x) && y.Equals(other.y);
        public override bool Equals(object obj) => obj is Vector2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y);
        public override string ToString() => $"({x:F3}, {y:F3})";
    }

    [Serializable]
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2Int zero => new Vector2Int(0, 0);
        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.x + b.x, a.y + b.y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.x - b.x, a.y - b.y);
        public static bool operator ==(Vector2Int a, Vector2Int b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Vector2Int a, Vector2Int b) => !(a == b);

        public bool Equals(Vector2Int other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is Vector2Int other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y);
        public override string ToString() => $"({x}, {y})";
    }

    [Serializable]
    public struct Vector3 : IEquatable<Vector3>
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0f;
        }

        public static Vector3 zero => new Vector3(0f, 0f, 0f);
        public static Vector3 one => new Vector3(1f, 1f, 1f);
        public static Vector3 up => new Vector3(0f, 1f, 0f);
        public static Vector3 down => new Vector3(0f, -1f, 0f);
        public static Vector3 forward => new Vector3(0f, 0f, 1f);
        public static Vector3 back => new Vector3(0f, 0f, -1f);
        public static Vector3 right => new Vector3(1f, 0f, 0f);
        public static Vector3 left => new Vector3(-1f, 0f, 0f);

        public float sqrMagnitude => x * x + y * y + z * z;
        public float magnitude => MathF.Sqrt(sqrMagnitude);

        public Vector3 normalized
        {
            get
            {
                float mag = magnitude;
                return mag > 1e-5f ? new Vector3(x / mag, y / mag, z / mag) : zero;
            }
        }

        public void Normalize()
        {
            this = normalized;
        }

        public static float Dot(Vector3 lhs, Vector3 rhs) => lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;

        public static Vector3 Cross(Vector3 lhs, Vector3 rhs) => new Vector3(
            lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.x * rhs.y - lhs.y * rhs.x);

        public static float Distance(Vector3 a, Vector3 b) => (a - b).magnitude;

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            Vector3 diff = target - current;
            float dist = diff.magnitude;
            if (dist <= maxDistanceDelta || dist < 1e-6f)
                return target;
            return current + diff / dist * maxDistanceDelta;
        }

        public static Vector3 SmoothDamp(
            Vector3 current,
            Vector3 target,
            ref Vector3 currentVelocity,
            float smoothTime,
            float maxSpeed = float.PositiveInfinity,
            float deltaTime = 0.016f)
        {
            smoothTime = MathF.Max(0.0001f, smoothTime);
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            Vector3 change = current - target;
            Vector3 originalTo = target;
            float maxChange = maxSpeed * smoothTime;
            float sqrMag = change.sqrMagnitude;
            if (sqrMag > maxChange * maxChange && !float.IsInfinity(maxChange))
            {
                change = change.normalized * maxChange;
            }
            target = current - change;
            Vector3 temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            Vector3 output = target + (change + temp) * exp;
            if (Dot(originalTo - current, output - originalTo) > 0f)
            {
                output = originalTo;
                currentVelocity = deltaTime > 1e-6f ? (output - originalTo) / deltaTime : zero;
            }
            return output;
        }

        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
        {
            float sqrMag = Dot(planeNormal, planeNormal);
            if (sqrMag < 1e-6f) return vector;
            float dot = Dot(vector, planeNormal);
            return vector - planeNormal * (dot / sqrMag);
        }

        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
            float sqrMag = vector.sqrMagnitude;
            if (sqrMag > maxLength * maxLength && sqrMag > 0f)
            {
                float mag = MathF.Sqrt(sqrMag);
                return vector * (maxLength / mag);
            }
            return vector;
        }

        public static float Angle(Vector3 from, Vector3 to)
        {
            float denom = MathF.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denom < 1e-15f) return 0f;
            float dot = Mathf.Clamp(Dot(from, to) / denom, -1f, 1f);
            return MathF.Acos(dot) * Mathf.Rad2Deg;
        }

        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float unsignedAngle = Angle(from, to);
            Vector3 cross = Cross(from, to);
            float sign = Dot(axis, cross) >= 0f ? 1f : -1f;
            return unsignedAngle * sign;
        }

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator *(float d, Vector3 a) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);
        public static bool operator ==(Vector3 lhs, Vector3 rhs) => (lhs - rhs).sqrMagnitude < 1e-10f;
        public static bool operator !=(Vector3 lhs, Vector3 rhs) => !(lhs == rhs);

        public bool Equals(Vector3 other) => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        public override bool Equals(object obj) => obj is Vector3 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y, z);
        public override string ToString() => $"({x:F3}, {y:F3}, {z:F3})";
    }

    [Serializable]
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion identity => new Quaternion(0f, 0f, 0f, 1f);

        public static Quaternion Euler(float x, float y, float z)
        {
            float cx = MathF.Cos(x * Mathf.Deg2Rad * 0.5f);
            float sx = MathF.Sin(x * Mathf.Deg2Rad * 0.5f);
            float cy = MathF.Cos(y * Mathf.Deg2Rad * 0.5f);
            float sy = MathF.Sin(y * Mathf.Deg2Rad * 0.5f);
            float cz = MathF.Cos(z * Mathf.Deg2Rad * 0.5f);
            float sz = MathF.Sin(z * Mathf.Deg2Rad * 0.5f);

            return new Quaternion(
                cy * sx * cz + sy * cx * sz,
                sy * cx * cz - cy * sx * sz,
                cy * cx * sz - sy * sx * cz,
                cy * cx * cz + sy * sx * sz);
        }

        public static Quaternion Euler(Vector3 euler) => Euler(euler.x, euler.y, euler.z);

        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            Vector3 n = axis.normalized;
            float half = angle * Mathf.Deg2Rad * 0.5f;
            float s = MathF.Sin(half);
            return new Quaternion(n.x * s, n.y * s, n.z * s, MathF.Cos(half));
        }

        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards = default)
        {
            Vector3 f = forward.normalized;
            if (f.sqrMagnitude < 1e-6f) return identity;
            float yaw = MathF.Atan2(f.x, f.z) * Mathf.Rad2Deg;
            float pitch = -MathF.Asin(Mathf.Clamp(f.y, -1f, 1f)) * Mathf.Rad2Deg;
            return Euler(pitch, yaw, 0f);
        }

        public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
        {
            t = Mathf.Clamp01(t);
            float dot = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
            if (dot < 0f)
            {
                b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
            }
            Quaternion res = new Quaternion(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t,
                a.w + (b.w - a.w) * t);
            float mag = MathF.Sqrt(res.x * res.x + res.y * res.y + res.z * res.z + res.w * res.w);
            return mag > 1e-6f ? new Quaternion(res.x / mag, res.y / mag, res.z / mag, res.w / mag) : identity;
        }

        public Vector3 eulerAngles
        {
            get
            {
                float sinp = 2f * (w * x - z * y);
                float pitch = MathF.Abs(sinp) >= 1f
                    ? MathF.CopySign(MathF.PI / 2f, sinp)
                    : MathF.Asin(sinp);
                float yaw = MathF.Atan2(2f * (w * y + x * z), 1f - 2f * (x * x + y * y));
                float roll = MathF.Atan2(2f * (w * z + x * y), 1f - 2f * (z * z + x * x));
                return new Vector3(
                    NormalizeAngleDeg(pitch * Mathf.Rad2Deg),
                    NormalizeAngleDeg(yaw * Mathf.Rad2Deg),
                    NormalizeAngleDeg(roll * Mathf.Rad2Deg));
            }
        }

        private static float NormalizeAngleDeg(float a)
        {
            a %= 360f;
            if (a < 0f) a += 360f;
            return a;
        }

        public static Quaternion operator *(Quaternion lhs, Quaternion rhs) => new Quaternion(
            lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
            lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);

        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float x = rotation.x * 2f;
            float y = rotation.y * 2f;
            float z = rotation.z * 2f;
            float xx = rotation.x * x;
            float yy = rotation.y * y;
            float zz = rotation.z * z;
            float xy = rotation.x * y;
            float xz = rotation.x * z;
            float yz = rotation.y * z;
            float wx = rotation.w * x;
            float wy = rotation.w * y;
            float wz = rotation.w * z;

            return new Vector3(
                (1f - (yy + zz)) * point.x + (xy - wz) * point.y + (xz + wy) * point.z,
                (xy + wz) * point.x + (1f - (xx + zz)) * point.y + (yz - wx) * point.z,
                (xz - wy) * point.x + (yz + wx) * point.y + (1f - (xx + yy)) * point.z);
        }

        public bool Equals(Quaternion other) => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
        public override bool Equals(object obj) => obj is Quaternion other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y, z, w);
    }

    [Serializable]
    public struct Color : IEquatable<Color>
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static Color white => new Color(1f, 1f, 1f, 1f);
        public static Color black => new Color(0f, 0f, 0f, 1f);
        public static Color red => new Color(1f, 0f, 0f, 1f);
        public static Color green => new Color(0f, 1f, 0f, 1f);
        public static Color blue => new Color(0f, 0f, 1f, 1f);
        public static Color yellow => new Color(1f, 0.92f, 0.016f, 1f);
        public static Color cyan => new Color(0f, 1f, 1f, 1f);
        public static Color magenta => new Color(1f, 0f, 1f, 1f);
        public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);
        public static Color clear => new Color(0f, 0f, 0f, 0f);

        public static Color Lerp(Color a, Color b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color(
                a.r + (b.r - a.r) * t,
                a.g + (b.g - a.g) * t,
                a.b + (b.b - a.b) * t,
                a.a + (b.a - a.a) * t);
        }

        public static Color operator *(Color a, float b) => new Color(a.r * b, a.g * b, a.b * b, a.a * b);
        public static Color operator +(Color a, Color b) => new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
        public static bool operator ==(Color lhs, Color rhs) =>
            Mathf.Abs(lhs.r - rhs.r) < 1e-5f &&
            Mathf.Abs(lhs.g - rhs.g) < 1e-5f &&
            Mathf.Abs(lhs.b - rhs.b) < 1e-5f &&
            Mathf.Abs(lhs.a - rhs.a) < 1e-5f;
        public static bool operator !=(Color lhs, Color rhs) => !(lhs == rhs);

        public bool Equals(Color other) => this == other;
        public override bool Equals(object obj) => obj is Color other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(r, g, b, a);
    }

    [Serializable]
    public struct Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Vector2 position => new Vector2(x, y);
        public Vector2 size => new Vector2(width, height);
        public float xMin => x;
        public float yMin => y;
        public float xMax => x + width;
        public float yMax => y + height;
    }

    [Serializable]
    public struct Keyframe
    {
        public float time;
        public float value;

        public Keyframe(float time, float value)
        {
            this.time = time;
            this.value = value;
        }
    }

    [Serializable]
    public class AnimationCurve
    {
        public Keyframe[] keys;

        public AnimationCurve()
        {
            keys = new[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) };
        }

        public AnimationCurve(params Keyframe[] keys)
        {
            this.keys = keys ?? Array.Empty<Keyframe>();
        }

        public static AnimationCurve Linear(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            return new AnimationCurve(new Keyframe(timeStart, valueStart), new Keyframe(timeEnd, valueEnd));
        }

        public float Evaluate(float time)
        {
            if (keys == null || keys.Length == 0) return 0f;
            if (keys.Length == 1 || time <= keys[0].time) return keys[0].value;
            if (time >= keys[keys.Length - 1].time) return keys[keys.Length - 1].value;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (time >= keys[i].time && time <= keys[i + 1].time)
                {
                    float span = keys[i + 1].time - keys[i].time;
                    float t = span > 1e-6f ? (time - keys[i].time) / span : 0f;
                    return Mathf.Lerp(keys[i].value, keys[i + 1].value, t);
                }
            }
            return keys[keys.Length - 1].value;
        }
    }

    public static class Mathf
    {
        public const float PI = MathF.PI;
        public const float Deg2Rad = MathF.PI / 180f;
        public const float Rad2Deg = 180f / MathF.PI;
        public const float Epsilon = 1.1920929e-7f;
        public const float Infinity = float.PositiveInfinity;

        public static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
        public static int Clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
        public static float Clamp01(float value) => value < 0f ? 0f : (value > 1f ? 1f : value);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float LerpUnclamped(float a, float b, float t) => a + (b - a) * t;
        public static float InverseLerp(float a, float b, float value) => MathF.Abs(b - a) > 1e-6f ? Clamp01((value - a) / (b - a)) : 0f;

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (MathF.Abs(target - current) <= maxDelta) return target;
            return current + MathF.Sign(target - current) * maxDelta;
        }

        public static float DeltaAngle(float current, float target)
        {
            float delta = Repeat(target - current, 360f);
            if (delta > 180f) delta -= 360f;
            return delta;
        }

        public static float Repeat(float t, float length) => Clamp(t - MathF.Floor(t / length) * length, 0f, length);
        public static float SmoothStep(float from, float to, float t)
        {
            t = Clamp01(t);
            t = -2f * t * t * t + 3f * t * t;
            return to * t + from * (1f - t);
        }

        public static float SmoothDamp(
            float current,
            float target,
            ref float currentVelocity,
            float smoothTime,
            float maxSpeed = float.PositiveInfinity,
            float deltaTime = 0.016f)
        {
            smoothTime = MathF.Max(0.0001f, smoothTime);
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            float change = current - target;
            float maxChange = maxSpeed * smoothTime;
            if (!float.IsInfinity(maxChange))
                change = Clamp(change, -maxChange, maxChange);
            float adjustedTarget = current - change;
            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            return adjustedTarget + (change + temp) * exp;
        }

        public static float Abs(float f) => MathF.Abs(f);
        public static int Abs(int value) => Math.Abs(value);
        public static float Min(float a, float b) => a < b ? a : b;
        public static int Min(int a, int b) => a < b ? a : b;
        public static float Max(float a, float b) => a > b ? a : b;
        public static int Max(int a, int b) => a > b ? a : b;
        public static float Sqrt(float f) => MathF.Sqrt(f);
        public static float Sin(float f) => MathF.Sin(f);
        public static float Cos(float f) => MathF.Cos(f);
        public static float Tan(float f) => MathF.Tan(f);
        public static float Asin(float f) => MathF.Asin(f);
        public static float Acos(float f) => MathF.Acos(f);
        public static float Atan2(float y, float x) => MathF.Atan2(y, x);
        public static float Pow(float f, float p) => MathF.Pow(f, p);
        public static float Sign(float f) => f >= 0f ? 1f : -1f;
        public static int RoundToInt(float f) => (int)MathF.Round(f);
        public static int FloorToInt(float f) => (int)MathF.Floor(f);
        public static int CeilToInt(float f) => (int)MathF.Ceiling(f);
        public static float Floor(float f) => MathF.Floor(f);
        public static float Ceil(float f) => MathF.Ceiling(f);
        public static bool Approximately(float a, float b) => MathF.Abs(b - a) < Max(1e-6f * Max(Abs(a), Abs(b)), Epsilon * 8f);
    }

    public static class Random
    {
        private static System.Random _rng = new System.Random(1337);
        public static void InitState(int seed) => _rng = new System.Random(seed);
        public static float value => (float)_rng.NextDouble();
        public static float Range(float minInclusive, float maxInclusive) => minInclusive + (maxInclusive - minInclusive) * value;
        public static int Range(int minInclusive, int maxExclusive) => minInclusive >= maxExclusive ? minInclusive : _rng.Next(minInclusive, maxExclusive);
        public static Vector3 insideUnitSphere
        {
            get
            {
                Vector3 v = new Vector3(Range(-1f, 1f), Range(-1f, 1f), Range(-1f, 1f));
                return v.sqrMagnitude > 1f ? v.normalized : v;
            }
        }
    }

    public static class Time
    {
        public static float time { get; set; } = 0f;
        public static float unscaledTime { get; set; } = 0f;
        public static float deltaTime { get; set; } = 0.016f;
        public static float unscaledDeltaTime { get; set; } = 0.016f;
        public static float fixedDeltaTime { get; set; } = 0.02f;
        public static float timeScale { get; set; } = 1f;
        public static int frameCount { get; set; } = 0;
    }

    public static class Debug
    {
        public static Action<string> OnLogWarning;
        public static Action<string> OnLogError;
        public static void Log(object message) { }
        public static void LogWarning(object message) => OnLogWarning?.Invoke(message?.ToString() ?? string.Empty);
        public static void LogError(object message) => OnLogError?.Invoke(message?.ToString() ?? string.Empty);
    }

    public class Object
    {
        public string name { get; set; } = string.Empty;
        private bool _destroyed;
        public int GetInstanceID() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);


        public static T Instantiate<T>(T original) where T : Object
        {
            if (original is GameObject go)
            {
                GameObject copy = new GameObject(go.name + "(Clone)");
                return copy as T;
            }
            return Activator.CreateInstance<T>();
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            T instance = Instantiate(original);
            if (instance is GameObject go)
            {
                go.transform.position = position;
                go.transform.rotation = rotation;
            }
            else if (instance is Component comp)
            {
                comp.transform.position = position;
                comp.transform.rotation = rotation;
            }
            return instance;
        }

        public static T Instantiate<T>(T original, Transform parent) where T : Object
        {
            T instance = Instantiate(original);
            if (instance is GameObject go)
                go.transform.SetParent(parent, false);
            else if (instance is Component comp)
                comp.transform.SetParent(parent, false);
            return instance;
        }

        public static void Destroy(Object obj)
        {
            if (obj != null) obj._destroyed = true;
        }

        public static void DestroyImmediate(Object obj) => Destroy(obj);
        public static void DontDestroyOnLoad(Object target) { }
        public static T FindFirstObjectByType<T>() where T : Object => null;
        public static T[] FindObjectsByType<T>(FindObjectsSortMode sortMode) where T : Object => Array.Empty<T>();

        public static implicit operator bool(Object exists) => !ReferenceEquals(exists, null) && !exists._destroyed;
    }

    public enum FindObjectsSortMode
    {
        None = 0,
        InstanceID = 1
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
        public static ScriptableObject CreateInstance(Type type) => (ScriptableObject)Activator.CreateInstance(type);
    }

    public class Component : Object
    {
        private GameObject _gameObject;

        public GameObject gameObject
        {
            get
            {
                if (_gameObject == null)
                    _gameObject = new GameObject(GetType().Name);
                return _gameObject;
            }
            internal set => _gameObject = value;
        }

        public Transform transform => gameObject.transform;

        public T GetComponent<T>() => gameObject.GetComponent<T>();
        public bool TryGetComponent<T>(out T component) => gameObject.TryGetComponent(out component);
        public T[] GetComponents<T>() => gameObject.GetComponents<T>();
        public T[] GetComponentsInChildren<T>(bool includeInactive = false) => gameObject.GetComponentsInChildren<T>(includeInactive);
        public T GetComponentInChildren<T>(bool includeInactive = false) => gameObject.GetComponentInChildren<T>(includeInactive);
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; } = true;
        public bool isActiveAndEnabled => enabled && gameObject.activeInHierarchy;
    }

    public class YieldInstruction { }

    public sealed class WaitForSeconds : YieldInstruction
    {
        public float Seconds { get; }
        public WaitForSeconds(float seconds) => Seconds = seconds;
    }

    public sealed class WaitForSecondsRealtime : YieldInstruction
    {
        public float Seconds { get; }
        public WaitForSecondsRealtime(float seconds) => Seconds = seconds;
    }

    public sealed class Coroutine : YieldInstruction { }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine != null)
            {
                int safety = 64;
                while (safety-- > 0 && routine.MoveNext())
                {
                    if (routine.Current is YieldInstruction)
                        break;
                }
            }
            return new Coroutine();
        }

        public void StopCoroutine(Coroutine routine) { }
        public void StopAllCoroutines() { }
    }

    public class Transform : Component, IEnumerable<Transform>
    {
        private readonly List<Transform> _children = new List<Transform>();
        private Transform _parent;

        public Vector3 localPosition { get; set; } = Vector3.zero;
        public Quaternion localRotation { get; set; } = Quaternion.identity;
        public Vector3 localScale { get; set; } = Vector3.one;

        public Transform parent
        {
            get => _parent;
            set => SetParent(value, true);
        }

        public Vector3 position
        {
            get => _parent != null ? _parent.position + (_parent.rotation * localPosition) : localPosition;
            set => localPosition = _parent != null ? value - _parent.position : value;
        }

        public Quaternion rotation
        {
            get => _parent != null ? _parent.rotation * localRotation : localRotation;
            set => localRotation = value;
        }

        public Vector3 eulerAngles
        {
            get => rotation.eulerAngles;
            set => rotation = Quaternion.Euler(value);
        }

        public Vector3 localEulerAngles
        {
            get => localRotation.eulerAngles;
            set => localRotation = Quaternion.Euler(value);
        }

        public Vector3 forward => rotation * Vector3.forward;
        public Vector3 right => rotation * Vector3.right;
        public Vector3 up => rotation * Vector3.up;

        public int childCount => _children.Count;
        public Transform GetChild(int index) => _children[index];

        public void SetParent(Transform newParent, bool worldPositionStays = true)
        {
            Vector3 worldPos = position;
            Quaternion worldRot = rotation;
            _parent?._children.Remove(this);
            _parent = newParent;
            _parent?._children.Add(this);
            if (worldPositionStays)
            {
                position = worldPos;
                rotation = worldRot;
            }
        }

        public Vector3 TransformDirection(Vector3 direction) => rotation * direction;
        public Vector3 TransformPoint(Vector3 pt) => position + (rotation * pt);
        public Vector3 InverseTransformDirection(Vector3 direction) => Quaternion.Euler(-eulerAngles) * direction;

        public IEnumerator<Transform> GetEnumerator() => _children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class RectTransform : Transform
    {
        public Vector2 anchorMin { get; set; } = Vector2.zero;
        public Vector2 anchorMax { get; set; } = Vector2.one;
        public Vector2 anchoredPosition { get; set; } = Vector2.zero;
        public Vector2 sizeDelta { get; set; } = new Vector2(100f, 100f);
        public Vector2 pivot { get; set; } = new Vector2(0.5f, 0.5f);
        public Vector2 offsetMin { get; set; } = Vector2.zero;
        public Vector2 offsetMax { get; set; } = Vector2.zero;
    }

    public sealed class GameObject : Object
    {
        private readonly List<Component> _components = new List<Component>();
        private readonly Transform _transform;
        private bool _activeSelf = true;

        public int layer { get; set; }
        public string tag { get; set; } = "Untagged";

        public GameObject() : this("GameObject") { }

        public GameObject(string name)
        {
            this.name = name;
            _transform = new Transform { gameObject = this, name = name };
            _components.Add(_transform);
        }

        public Transform transform => _transform;
        public bool activeSelf => _activeSelf;

        public bool activeInHierarchy
        {
            get
            {
                if (!_activeSelf) return false;
                Transform p = _transform.parent;
                while (p != null)
                {
                    if (!p.gameObject._activeSelf) return false;
                    p = p.parent;
                }
                return true;
            }
        }

        public void SetActive(bool value) => _activeSelf = value;
        public static GameObject Find(string name) => null;
        public static GameObject CreatePrimitive(PrimitiveType type)
        {
            GameObject go = new GameObject(type.ToString());
            if (type == PrimitiveType.Capsule)
                go.AddComponent<CapsuleCollider>();
            else if (type == PrimitiveType.Sphere)
                go.AddComponent<SphereCollider>();
            else
                go.AddComponent<BoxCollider>();
            return go;
        }


        public T AddComponent<T>() where T : Component, new()
        {
            T comp = new T { gameObject = this, name = name };
            _components.Add(comp);
            return comp;
        }

        public Component AddComponent(Type componentType)
        {
            Component comp = (Component)Activator.CreateInstance(componentType);
            comp.gameObject = this;
            comp.name = name;
            _components.Add(comp);
            return comp;
        }

        public T GetComponent<T>()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T match)
                    return match;
            }
            return default;
        }

        public bool TryGetComponent<T>(out T component)
        {
            component = GetComponent<T>();
            return component != null;
        }

        public T[] GetComponents<T>()
        {
            List<T> list = new List<T>();
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T match)
                    list.Add(match);
            }
            return list.ToArray();
        }

        public T GetComponentInChildren<T>(bool includeInactive = false)
        {
            if (includeInactive || _activeSelf)
            {
                T self = GetComponent<T>();
                if (self != null) return self;
            }
            for (int i = 0; i < _transform.childCount; i++)
            {
                Transform child = _transform.GetChild(i);
                if (!includeInactive && !child.gameObject.activeSelf) continue;
                T found = child.gameObject.GetComponentInChildren<T>(includeInactive);
                if (found != null) return found;
            }
            return default;
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive = false)
        {
            List<T> results = new List<T>();
            CollectInChildren(this, includeInactive, results);
            return results.ToArray();
        }

        private static void CollectInChildren<T>(GameObject go, bool includeInactive, List<T> acc)
        {
            if (!includeInactive && !go.activeSelf) return;
            acc.AddRange(go.GetComponents<T>());
            for (int i = 0; i < go.transform.childCount; i++)
            {
                CollectInChildren(go.transform.GetChild(i).gameObject, includeInactive, acc);
            }
        }
    }

    public enum ForceMode
    {
        Force = 0,
        Impulse = 1,
        VelocityChange = 2,
        Acceleration = 5
    }

    public enum CollisionDetectionMode
    {
        Discrete = 0,
        Continuous = 1,
        ContinuousDynamic = 2,
        ContinuousSpeculative = 3
    }

    public enum RigidbodyInterpolation
    {
        None = 0,
        Interpolate = 1,
        Extrapolate = 2
    }

    // Unity 6.3: linearVelocity only; NO velocity property (Landmine #2).
    public class Rigidbody : Component
    {
        public Vector3 linearVelocity { get; set; } = Vector3.zero;
        public Vector3 angularVelocity { get; set; } = Vector3.zero;
        public Vector3 centerOfMass { get; set; } = Vector3.zero;
        public float mass { get; set; } = 1f;
        public float linearDamping { get; set; } = 0f;
        public float angularDamping { get; set; } = 0.05f;
        public bool isKinematic { get; set; }
        public bool useGravity { get; set; } = true;
        public CollisionDetectionMode collisionDetectionMode { get; set; }
        public RigidbodyInterpolation interpolation { get; set; }
        private bool _sleeping;

        public Vector3 position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public Quaternion rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            if (isKinematic) return;
            _sleeping = false;
            float invMass = mass > 1e-4f ? 1f / mass : 1f;
            switch (mode)
            {
                case ForceMode.Impulse:
                    linearVelocity += force * invMass;
                    break;
                case ForceMode.VelocityChange:
                    linearVelocity += force;
                    break;
                case ForceMode.Acceleration:
                    linearVelocity += force * Time.fixedDeltaTime;
                    break;
                default:
                    linearVelocity += force * invMass * Time.fixedDeltaTime;
                    break;
            }
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force) => AddForce(force, mode);
        public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force) { }
        public void MovePosition(Vector3 pos) => transform.position = pos;
        public void MoveRotation(Quaternion rot) => transform.rotation = rot;
        public void Sleep() => _sleeping = true;
        public void WakeUp() => _sleeping = false;
        public bool IsSleeping() => _sleeping;
    }

    // CharacterController MUST derive from Collider so enabled exists (§4.1).
    public class Collider : Component
    {
        public bool enabled { get; set; } = true;
        public bool isTrigger { get; set; }
        public Rigidbody attachedRigidbody => GetComponent<Rigidbody>();
        public Vector3 ClosestPoint(Vector3 position) => transform.position;
    }

    public class BoxCollider : Collider
    {
        public Vector3 center { get; set; } = Vector3.zero;
        public Vector3 size { get; set; } = Vector3.one;
    }

    public class SphereCollider : Collider
    {
        public Vector3 center { get; set; } = Vector3.zero;
        public float radius { get; set; } = 0.5f;
    }

    public class CapsuleCollider : Collider
    {
        public Vector3 center { get; set; } = Vector3.zero;
        public float radius { get; set; } = 0.5f;
        public float height { get; set; } = 2f;
    }

    [Flags]
    public enum CollisionFlags
    {
        None = 0,
        Sides = 1,
        Above = 2,
        Below = 4
    }

    public class CharacterController : Collider
    {
        public float height { get; set; } = 1.8f;
        public float radius { get; set; } = 0.35f;
        public Vector3 center { get; set; } = new Vector3(0f, 0.9f, 0f);
        public float slopeLimit { get; set; } = 45f;
        public float stepOffset { get; set; } = 0.3f;
        public bool isGrounded { get; set; } = true;
        public Vector3 velocity { get; private set; } = Vector3.zero;

        public CollisionFlags Move(Vector3 motion)
        {
            if (!enabled) return CollisionFlags.None;
            transform.position += motion;
            velocity = Time.deltaTime > 1e-6f ? motion / Time.deltaTime : Vector3.zero;
            if (transform.position.y <= 0f)
            {
                Vector3 p = transform.position;
                p.y = 0f;
                transform.position = p;
                isGrounded = true;
                return CollisionFlags.Below;
            }
            isGrounded = false;
            return CollisionFlags.None;
        }
    }

    public class Collision
    {
        public Collider collider { get; set; }
        public GameObject gameObject => collider != null ? collider.gameObject : null;
        public Vector3 relativeVelocity { get; set; }
    }

    [Serializable]
    public struct LayerMask
    {
        public int value;
        public static implicit operator int(LayerMask mask) => mask.value;
        public static implicit operator LayerMask(int intVal) => new LayerMask { value = intVal };
    }

    public struct Ray
    {
        public Vector3 origin;
        public Vector3 direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction.normalized;
        }

        public Vector3 GetPoint(float distance) => origin + direction * distance;
    }

    public struct RaycastHit
    {
        public Collider collider;
        public Vector3 point;
        public Vector3 normal;
        public float distance;
        public Transform transform => collider != null ? collider.transform : null;
        public Rigidbody rigidbody => collider != null ? collider.attachedRigidbody : null;
    }

    public enum QueryTriggerInteraction
    {
        UseGlobal = 0,
        Ignore = 1,
        Collide = 2
    }

    public static class Physics
    {
        public static bool autoSyncTransforms { get; set; }
        public static readonly List<Collider> RegisteredColliders = new List<Collider>();

        public static bool Raycast(
            Vector3 origin,
            Vector3 direction,
            out RaycastHit hitInfo,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            RaycastHit[] buf = new RaycastHit[8];
            int count = RaycastNonAlloc(origin, direction, buf, maxDistance, layerMask, queryTriggerInteraction);
            if (count > 0)
            {
                int best = 0;
                for (int i = 1; i < count; i++)
                {
                    if (buf[i].distance < buf[best].distance) best = i;
                }
                hitInfo = buf[best];
                return true;
            }
            hitInfo = default;
            return false;
        }

        public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -1)
            => Raycast(ray.origin, ray.direction, out hitInfo, maxDistance, layerMask);

        public static int RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            RaycastHit[] results,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (results == null || results.Length == 0) return 0;
            Vector3 dir = direction.normalized;
            if (dir.sqrMagnitude < 1e-6f) return 0;
            int written = 0;
            for (int i = 0; i < RegisteredColliders.Count && written < results.Length; i++)
            {
                Collider col = RegisteredColliders[i];
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy) continue;
                if (queryTriggerInteraction == QueryTriggerInteraction.Ignore && col.isTrigger) continue;
                Vector3 toCol = col.transform.position - origin;
                float proj = Vector3.Dot(toCol, dir);
                if (proj < 0f || proj > maxDistance) continue;
                Vector3 closest = origin + dir * proj;
                float perpDist = Vector3.Distance(closest, col.transform.position);
                float hitRadius = col is SphereCollider sc ? sc.radius : 0.75f;
                if (perpDist <= hitRadius)
                {
                    results[written++] = new RaycastHit
                    {
                        collider = col,
                        point = closest,
                        normal = -dir,
                        distance = proj
                    };
                }
            }
            return written;
        }

        public static int OverlapSphereNonAlloc(
            Vector3 position,
            float radius,
            Collider[] results,
            int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (results == null || results.Length == 0) return 0;
            int written = 0;
            float rSqr = radius * radius;
            for (int i = 0; i < RegisteredColliders.Count && written < results.Length; i++)
            {
                Collider col = RegisteredColliders[i];
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy) continue;
                if (queryTriggerInteraction == QueryTriggerInteraction.Ignore && col.isTrigger) continue;
                if ((col.transform.position - position).sqrMagnitude <= rSqr)
                {
                    results[written++] = col;
                }
            }
            return written;
        }
    }

    public class Camera : Behaviour
    {
        public static Camera main { get; set; }
        public float fieldOfView { get; set; } = 60f;
        public float nearClipPlane { get; set; } = 0.05f;
        public float farClipPlane { get; set; } = 500f;
    }

    public enum LightType
    {
        Spot = 0,
        Directional = 1,
        Point = 2
    }

    public enum LightShadows
    {
        None = 0,
        Hard = 1,
        Soft = 2
    }

    public class Light : Behaviour
    {
        public LightType type { get; set; } = LightType.Point;
        public float intensity { get; set; } = 1f;
        public float range { get; set; } = 10f;
        public Color color { get; set; } = Color.white;
        public LightShadows shadows { get; set; } = LightShadows.None;
    }

    public class MaterialPropertyBlock
    {
        private readonly Dictionary<int, Color> _colors = new Dictionary<int, Color>();
        private readonly Dictionary<int, float> _floats = new Dictionary<int, float>();

        public void SetColor(int nameID, Color value) => _colors[nameID] = value;
        public void SetColor(string name, Color value) => _colors[Shader.PropertyToID(name)] = value;
        public Color GetColor(int nameID) => _colors.TryGetValue(nameID, out Color c) ? c : Color.clear;
        public void SetFloat(int nameID, float value) => _floats[nameID] = value;
        public void Clear()
        {
            _colors.Clear();
            _floats.Clear();
        }
    }

    public static class Shader
    {
        public static int PropertyToID(string name) => name != null ? Animator.StringToHash(name) : 0;
    }

    public class Material : Object { }

    public class Renderer : Component
    {
        public bool enabled { get; set; } = true;
        public UnityEngine.Rendering.ShadowCastingMode shadowCastingMode { get; set; }
        private MaterialPropertyBlock _block;
        public void GetPropertyBlock(MaterialPropertyBlock properties) { }
        public void SetPropertyBlock(MaterialPropertyBlock properties) => _block = properties;
        public Material sharedMaterial { get; set; }
    }

    public class MeshRenderer : Renderer { }

    public class Animator : Behaviour
    {
        public RuntimeAnimatorController runtimeAnimatorController { get; set; }
        private readonly Dictionary<int, float> _floats = new Dictionary<int, float>();
        private readonly Dictionary<int, bool> _bools = new Dictionary<int, bool>();
        private readonly HashSet<int> _triggers = new HashSet<int>();

        public static int StringToHash(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            unchecked
            {
                uint hash = 2166136261u;
                for (int i = 0; i < name.Length; i++)
                {
                    hash ^= name[i];
                    hash *= 16777619u;
                }
                return (int)hash;
            }
        }

        public void SetTrigger(int id) => _triggers.Add(id);
        public void ResetTrigger(int id) => _triggers.Remove(id);
        public void SetBool(int id, bool value) => _bools[id] = value;
        public bool GetBool(int id) => _bools.TryGetValue(id, out bool v) && v;
        public void SetFloat(int id, float value) => _floats[id] = value;
        public float GetFloat(int id) => _floats.TryGetValue(id, out float v) ? v : 0f;
    }

    public class RuntimeAnimatorController : Object { }
    public class AnimationClip : Object
    {
        public float length { get; set; } = 1f;
    }

    public class AudioClip : Object
    {
        public float length { get; set; } = 1f;
    }

    public class AudioListener : Behaviour
    {
        public static float volume { get; set; } = 1f;
        public static bool pause { get; set; }
    }

    public class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; }
        public float volume { get; set; } = 1f;
        public float pitch { get; set; } = 1f;
        public float spatialBlend { get; set; } = 1f;
        public float minDistance { get; set; } = 1f;
        public float maxDistance { get; set; } = 90f;
        public bool loop { get; set; }
        public bool playOnAwake { get; set; }
        public bool isPlaying { get; private set; }

        public void Play() => isPlaying = clip != null;
        public void Stop() => isPlaying = false;
        public void PlayOneShot(AudioClip oneShotClip, float volumeScale = 1f)
        {
            if (oneShotClip != null)
            {
                clip = oneShotClip;
                volume = volumeScale;
                isPlaying = true;
            }
        }
    }

    public enum FogMode
    {
        Linear = 1,
        Exponential = 2,
        ExponentialSquared = 3
    }

    public static class RenderSettings
    {
        public static bool fog { get; set; }
        public static FogMode fogMode { get; set; } = FogMode.Exponential;
        public static float fogDensity { get; set; }
        public static Color fogColor { get; set; } = new Color(0.5f, 0.55f, 0.65f, 1f);
        public static Color ambientLight { get; set; } = Color.white;
        public static UnityEngine.Rendering.AmbientMode ambientMode { get; set; } = UnityEngine.Rendering.AmbientMode.Flat;
    }

    public enum PrimitiveType
    {
        Sphere = 0,
        Capsule = 1,
        Cylinder = 2,
        Cube = 3,
        Plane = 4,
        Quad = 5
    }

    public enum ShadowQuality
    {
        Disable = 0,
        HardOnly = 1,
        All = 2
    }

    public static class QualitySettings
    {
        private static int _qualityLevel = 5;
        public static string[] names { get; set; } = new[] { "Very Low", "Low", "Medium", "High", "Very High", "Ultra" };
        public static ShadowQuality shadows { get; set; } = ShadowQuality.All;
        public static float shadowDistance { get; set; } = 50f;
        public static float lodBias { get; set; } = 1f;
        public static int antiAliasing { get; set; } = 1;
        public static int vSyncCount { get; set; } = 0;
        public static UnityEngine.Rendering.RenderPipelineAsset renderPipeline { get; set; }
        public static int GetQualityLevel() => _qualityLevel;
        public static void SetQualityLevel(int index, bool applyExpensiveChanges = true) => _qualityLevel = index;
    }

    public static class Application
    {
        public static string unityVersion { get; set; } = "6000.3.28f1";
        public static int targetFrameRate { get; set; } = 60;
        public static bool isPlaying { get; set; } = true;
        public static void Quit() { }
    }

    public static class SystemInfo
    {
        public static int systemMemorySize { get; set; } = 4096;
        public static int processorCount { get; set; } = 6;
    }

    public static class Screen
    {
        public static int width { get; set; } = 1920;
        public static int height { get; set; } = 1080;
        public static Rect safeArea { get; set; } = new Rect(0f, 0f, 1920f, 1080f);
    }

    public static class PlayerPrefs
    {
        private static readonly Dictionary<string, object> _store = new Dictionary<string, object>();

        public static bool HasKey(string key) => key != null && _store.ContainsKey(key);
        public static void SetString(string key, string value) => _store[key] = value ?? string.Empty;
        public static string GetString(string key, string defaultValue = "") =>
            key != null && _store.TryGetValue(key, out object v) && v is string s ? s : defaultValue;

        public static void SetInt(string key, int value) => _store[key] = value;
        public static int GetInt(string key, int defaultValue = 0) =>
            key != null && _store.TryGetValue(key, out object v) && v is int i ? i : defaultValue;

        public static void SetFloat(string key, float value) => _store[key] = value;
        public static float GetFloat(string key, float defaultValue = 0f) =>
            key != null && _store.TryGetValue(key, out object v) && v is float f ? f : defaultValue;

        public static void DeleteKey(string key)
        {
            if (key != null) _store.Remove(key);
        }

        public static void DeleteAll() => _store.Clear();
        public static void Save() { }
    }

    // Real field-based JSON serializer matching Unity's JsonUtility (§4.1: must do a real round trip).
    public static class JsonUtility
    {
        private static JsonSerializerOptions CreateOptions(bool indented)
        {
            var resolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver();
            resolver.Modifiers.Add(typeInfo =>
            {
                if (typeInfo.Kind != System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)
                    return;
                for (int i = typeInfo.Properties.Count - 1; i >= 0; i--)
                {
                    if (!(typeInfo.Properties[i].AttributeProvider is FieldInfo))
                    {
                        typeInfo.Properties.RemoveAt(i);
                    }
                }
            });

            return new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = indented,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
                TypeInfoResolver = resolver
            };
        }

        private static readonly JsonSerializerOptions Options = CreateOptions(false);
        private static readonly JsonSerializerOptions PrettyOptions = CreateOptions(true);

        public static string ToJson(object obj, bool prettyPrint = false)
        {
            if (obj == null) return string.Empty;
            return JsonSerializer.Serialize(obj, obj.GetType(), prettyPrint ? PrettyOptions : Options);
        }

        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Activator.CreateInstance<T>();
            return JsonSerializer.Deserialize<T>(json, Options);
        }

        public static void FromJsonOverwrite(string json, object objectToOverwrite)
        {
            if (string.IsNullOrWhiteSpace(json) || objectToOverwrite == null) return;
            object parsed = JsonSerializer.Deserialize(json, objectToOverwrite.GetType(), Options);
            if (parsed == null) return;
            foreach (FieldInfo f in objectToOverwrite.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
                {
                    f.SetValue(objectToOverwrite, f.GetValue(parsed));
                }
            }
        }
    }

    public class Canvas : Behaviour { }
    public class CanvasGroup : Behaviour
    {
        public float alpha { get; set; } = 1f;
        public bool interactable { get; set; } = true;
        public bool blocksRaycasts { get; set; } = true;
    }
}

namespace UnityEngine.Rendering
{
    public enum AmbientMode
    {
        Skybox = 0,
        Trilight = 1,
        Flat = 3,
        Custom = 4
    }

    public enum ShadowCastingMode
    {
        Off = 0,
        On = 1,
        TwoSided = 2,
        ShadowsOnly = 3
    }

    public class RenderPipelineAsset : ScriptableObject
    {
        public int msaaSampleCount { get; set; } = 1;
    }

    public static class GraphicsSettings
    {
        public static RenderPipelineAsset defaultRenderPipeline { get; set; }
        public static RenderPipelineAsset currentRenderPipeline => QualitySettings.renderPipeline ?? defaultRenderPipeline;
    }
}

namespace UnityEngine.EventSystems
{
    public class PointerEventData
    {
        public Vector2 position { get; set; }
        public Vector2 pressPosition { get; set; }
        public Camera pressEventCamera { get; set; }
    }

    public interface IPointerDownHandler
    {
        void OnPointerDown(PointerEventData eventData);
    }

    public interface IDragHandler
    {
        void OnDrag(PointerEventData eventData);
    }

    public interface IPointerUpHandler
    {
        void OnPointerUp(PointerEventData eventData);
    }
}

namespace UnityEngine.Events
{
    public class UnityEvent
    {
        private event Action _listeners;
        public void AddListener(Action call) => _listeners += call;
        public void RemoveListener(Action call) => _listeners -= call;
        public void RemoveAllListeners() => _listeners = null;
        public void Invoke() => _listeners?.Invoke();
    }
}

namespace UnityEngine.UI
{
    public class Graphic : Behaviour
    {
        public Color color { get; set; } = Color.white;
        public RectTransform rectTransform
        {
            get
            {
                RectTransform rt = GetComponent<RectTransform>();
                return rt ?? gameObject.AddComponent<RectTransform>();
            }
        }
    }

    public class Image : Graphic
    {
        public float fillAmount { get; set; } = 1f;
        public bool raycastTarget { get; set; } = true;
    }

    public class Slider : Behaviour
    {
        public float minValue { get; set; } = 0f;
        public float maxValue { get; set; } = 1f;
        public float value { get; set; }
    }

    public class Button : Behaviour
    {
        public bool interactable { get; set; } = true;
        public UnityEngine.Events.UnityEvent onClick { get; } = new UnityEngine.Events.UnityEvent();
    }

    public class CanvasScaler : Behaviour
    {
        public Vector2 referenceResolution { get; set; } = new Vector2(1920f, 1080f);
    }

    public class GraphicRaycaster : Behaviour { }
}

namespace TMPro
{
    public class TMP_Text : UnityEngine.UI.Graphic
    {
        public virtual string text { get; set; } = string.Empty;
        public float fontSize { get; set; } = 24f;
    }

    public class TextMeshProUGUI : TMP_Text { }
}

namespace Unity.Cinemachine
{
    [Serializable]
    public struct LensSettings
    {
        public float FieldOfView;
        public float NearClipPlane;
        public float FarClipPlane;

        public static LensSettings Default => new LensSettings
        {
            FieldOfView = 60f,
            NearClipPlane = 0.05f,
            FarClipPlane = 500f
        };
    }

    // Lens is a struct property just like Cinemachine 3 (§1.9.5).
    public class CinemachineCamera : UnityEngine.MonoBehaviour
    {
        public LensSettings Lens { get; set; } = LensSettings.Default;
        public UnityEngine.Transform Follow { get; set; }
        public UnityEngine.Transform LookAt { get; set; }
        public int Priority { get; set; } = 10;
    }

    public class CinemachineBrain : UnityEngine.MonoBehaviour { }
}

namespace UnityEngine.InputSystem.Controls
{
    public class ButtonControl
    {
        public bool isPressed { get; set; }
        public bool wasPressedThisFrame { get; set; }
        public bool wasReleasedThisFrame { get; set; }
        public float ReadValue() => isPressed ? 1f : 0f;
    }

    public class Vector2Control
    {
        public UnityEngine.Vector2 value { get; set; }
        public UnityEngine.Vector2 ReadValue() => value;
    }
}

namespace UnityEngine.InputSystem
{
    using UnityEngine.InputSystem.Controls;

    // Landmine #3: Keyboard has NO 'mouse' member; Mouse.current is separate.
    public class Keyboard
    {
        public static Keyboard current { get; set; }

        public ButtonControl wKey { get; } = new ButtonControl();
        public ButtonControl aKey { get; } = new ButtonControl();
        public ButtonControl sKey { get; } = new ButtonControl();
        public ButtonControl dKey { get; } = new ButtonControl();
        public ButtonControl leftShiftKey { get; } = new ButtonControl();
        public ButtonControl cKey { get; } = new ButtonControl();
        public ButtonControl leftCtrlKey { get; } = new ButtonControl();
        public ButtonControl spaceKey { get; } = new ButtonControl();
        public ButtonControl rKey { get; } = new ButtonControl();
        public ButtonControl eKey { get; } = new ButtonControl();
        public ButtonControl fKey { get; } = new ButtonControl();
        public ButtonControl vKey { get; } = new ButtonControl();
        public ButtonControl qKey { get; } = new ButtonControl();
        public ButtonControl escapeKey { get; } = new ButtonControl();
    }

    public class Mouse
    {
        public static Mouse current { get; set; }

        public ButtonControl leftButton { get; } = new ButtonControl();
        public ButtonControl rightButton { get; } = new ButtonControl();
        public ButtonControl middleButton { get; } = new ButtonControl();
        public Vector2Control delta { get; } = new Vector2Control();
        public Vector2Control scroll { get; } = new Vector2Control();
    }

    public class Gamepad
    {
        public static Gamepad current { get; set; }

        public Vector2Control leftStick { get; } = new Vector2Control();
        public Vector2Control rightStick { get; } = new Vector2Control();
        public ButtonControl buttonSouth { get; } = new ButtonControl();
        public ButtonControl buttonEast { get; } = new ButtonControl();
        public ButtonControl buttonWest { get; } = new ButtonControl();
        public ButtonControl buttonNorth { get; } = new ButtonControl();
        public ButtonControl leftShoulder { get; } = new ButtonControl();
        public ButtonControl rightShoulder { get; } = new ButtonControl();
        public ButtonControl leftTrigger { get; } = new ButtonControl();
        public ButtonControl rightTrigger { get; } = new ButtonControl();
        public ButtonControl startButton { get; } = new ButtonControl();
        public ButtonControl leftStickButton { get; } = new ButtonControl();
    }
}
