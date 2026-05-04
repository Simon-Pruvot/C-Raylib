using System;
using System.Net;
using System.Net.Sockets;

partial class Program
{
    class NetManager
    {
        const int Port = 7777;

        UdpClient udp = null!;
        IPEndPoint? remoteEP;

        public bool IsHost   { get; private set; }
        public bool Connected { get; private set; }

        // ── Host side ────────────────────────────────────────────────────────

        public void StartHost()
        {
            IsHost = true;
            udp = new UdpClient(Port);
            udp.Client.Blocking = false;
        }

        // Call every frame while waiting. Returns true + seed the moment a client connects.
        public bool HostPoll(out int seed)
        {
            seed = 0;
            if (Connected) return true;
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udp.Receive(ref ep);
                if (data.Length == 1 && data[0] == 0xAB)
                {
                    remoteEP = ep;
                    seed = new Random().Next(1, int.MaxValue);
                    byte[] seedBytes = BitConverter.GetBytes(seed);
                    udp.Send(seedBytes, seedBytes.Length, remoteEP);
                    Connected = true;
                    return true;
                }
            }
            catch (SocketException) { }
            return false;
        }

        // ── Client side ───────────────────────────────────────────────────────

        public void StartClient(string hostIp)
        {
            IsHost = false;
            remoteEP = new IPEndPoint(IPAddress.Parse(hostIp), Port);
            udp = new UdpClient();
            udp.Client.Blocking = false;
        }

        // Call every frame while waiting. Keeps pinging host and returns true + seed when handshake completes.
        public bool ClientPoll(out int seed)
        {
            seed = 0;
            if (Connected) return true;

            // Keep sending hello so the host notices us
            try { udp.Send(new byte[] { 0xAB }, 1, remoteEP); }
            catch { }

            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udp.Receive(ref ep);
                if (data.Length == 4)
                {
                    seed = BitConverter.ToInt32(data, 0);
                    remoteEP = ep;
                    Connected = true;
                    return true;
                }
            }
            catch (SocketException) { }
            return false;
        }

        // ── Per-frame input exchange ──────────────────────────────────────────

        public void SendInput(PlayerInput input)
        {
            if (remoteEP == null) return;
            byte[] data = Pack(input);
            try { udp.Send(data, data.Length, remoteEP); }
            catch { }
        }

        // Non-blocking: returns false if nothing arrived this frame.
        public bool TryReceiveInput(out PlayerInput input)
        {
            input = default;
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udp.Receive(ref ep);
                if (data.Length >= PacketSize)
                {
                    input = Unpack(data);
                    return true;
                }
            }
            catch (SocketException) { }
            return false;
        }

        public void Close()
        {
            try { udp?.Close(); } catch { }
            Connected = false;
        }

        // ── Serialisation ─────────────────────────────────────────────────────
        // Layout: 1 byte flags | 4 bytes mouseX | 4 bytes mouseY | 4 bytes selectedSlot = 13 bytes

        const int PacketSize = 13;

        static byte[] Pack(PlayerInput i)
        {
            byte flags = 0;
            if (i.MoveLeft)      flags |= 0x01;
            if (i.MoveRight)     flags |= 0x02;
            if (i.ClimbUp)       flags |= 0x04;
            if (i.JumpPressed)   flags |= 0x08;
            if (i.ActionPressed) flags |= 0x10;
            if (i.PickupPressed) flags |= 0x20;

            byte[] buf = new byte[PacketSize];
            buf[0] = flags;
            Buffer.BlockCopy(BitConverter.GetBytes(i.MouseX),       0, buf, 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(i.MouseY),       0, buf, 5, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(i.SelectedSlot), 0, buf, 9, 4);
            return buf;
        }

        static PlayerInput Unpack(byte[] buf)
        {
            byte flags = buf[0];
            return new PlayerInput
            {
                MoveLeft      = (flags & 0x01) != 0,
                MoveRight     = (flags & 0x02) != 0,
                ClimbUp       = (flags & 0x04) != 0,
                JumpPressed   = (flags & 0x08) != 0,
                ActionPressed = (flags & 0x10) != 0,
                PickupPressed = (flags & 0x20) != 0,
                MouseX        = BitConverter.ToSingle(buf, 1),
                MouseY        = BitConverter.ToSingle(buf, 5),
                SelectedSlot  = BitConverter.ToInt32(buf, 9),
            };
        }
    }
}
