using System;
using UnityEngine;

namespace DudeChaos.Net
{
    // Fixed 32-byte client command struct with hand-rolled zero-alloc serialisation (§1.8 & P10).
    public struct NetworkCommand
    {
        public const int ByteSize = 32;

        public uint tick;            // 0..3   (4 bytes)
        public Vector3 position;     // 4..15  (12 bytes)
        public float moveX;          // 16..19 (4 bytes)
        public float moveY;          // 20..23 (4 bytes)
        public float aimYaw;         // 24..27 (4 bytes)
        public ushort buttonsMask;   // 28..29 (2 bytes)
        public byte weaponSlot;      // 30     (1 byte)
        public byte claimedDamage;   // 31     (1 byte)

        public void WriteTo(byte[] buffer, int offset = 0)
        {
            if (buffer == null || buffer.Length - offset < ByteSize)
                throw new ArgumentException("Buffer must hold at least 32 bytes.");

            WriteUInt32(buffer, offset + 0, tick);
            WriteSingle(buffer, offset + 4, position.x);
            WriteSingle(buffer, offset + 8, position.y);
            WriteSingle(buffer, offset + 12, position.z);
            WriteSingle(buffer, offset + 16, moveX);
            WriteSingle(buffer, offset + 20, moveY);
            WriteSingle(buffer, offset + 24, aimYaw);
            WriteUInt16(buffer, offset + 28, buttonsMask);
            buffer[offset + 30] = weaponSlot;
            buffer[offset + 31] = claimedDamage;
        }

        public static NetworkCommand ReadFrom(byte[] buffer, int offset = 0)
        {
            if (buffer == null || buffer.Length - offset < ByteSize)
                throw new ArgumentException("Buffer must hold at least 32 bytes.");

            return new NetworkCommand
            {
                tick = ReadUInt32(buffer, offset + 0),
                position = new Vector3(
                    ReadSingle(buffer, offset + 4),
                    ReadSingle(buffer, offset + 8),
                    ReadSingle(buffer, offset + 12)),
                moveX = ReadSingle(buffer, offset + 16),
                moveY = ReadSingle(buffer, offset + 20),
                aimYaw = ReadSingle(buffer, offset + 24),
                buttonsMask = ReadUInt16(buffer, offset + 28),
                weaponSlot = buffer[offset + 30],
                claimedDamage = buffer[offset + 31]
            };
        }

        private static void WriteUInt32(byte[] buf, int o, uint v)
        {
            buf[o + 0] = (byte)(v & 0xFF);
            buf[o + 1] = (byte)((v >> 8) & 0xFF);
            buf[o + 2] = (byte)((v >> 16) & 0xFF);
            buf[o + 3] = (byte)((v >> 24) & 0xFF);
        }

        private static uint ReadUInt32(byte[] buf, int o)
        {
            return (uint)(buf[o + 0] | (buf[o + 1] << 8) | (buf[o + 2] << 16) | (buf[o + 3] << 24));
        }

        private static void WriteUInt16(byte[] buf, int o, ushort v)
        {
            buf[o + 0] = (byte)(v & 0xFF);
            buf[o + 1] = (byte)((v >> 8) & 0xFF);
        }

        private static ushort ReadUInt16(byte[] buf, int o)
        {
            return (ushort)(buf[o + 0] | (buf[o + 1] << 8));
        }

        private static void WriteSingle(byte[] buf, int o, float f)
        {
            int bits = BitConverter.SingleToInt32Bits(f);
            WriteUInt32(buf, o, unchecked((uint)bits));
        }

        private static float ReadSingle(byte[] buf, int o)
        {
            uint bits = ReadUInt32(buf, o);
            return BitConverter.Int32BitsToSingle(unchecked((int)bits));
        }
    }
}
