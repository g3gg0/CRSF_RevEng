using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CRSF_Debug
{
    class Program
    {
        enum eCrsfState
        {
            STATE_WAIT_SYNC = 0,
            STATE_RECEIVE_LEN = 1,
            STATE_RECEIVE_DATA = 2
        };

        enum eCrsfFrameType
        {
            CRSF_FRAMETYPE_GPS = 0x02,
            CRSF_FRAMETYPE_BATTERY_SENSOR = 0x08,
            CRSF_FRAMETYPE_LINK_STATISTICS = 0x14,
            CRSF_FRAMETYPE_RC_CHANNELS_PACKED = 0x16,
            CRSF_FRAMETYPE_ATTITUDE = 0x1E,
            CRSF_FRAMETYPE_FLIGHT_MODE = 0x21,
            // Extended Header Frames, range: 0x28 to 0x96
            CRSF_FRAMETYPE_DEVICE_PING = 0x28,
            CRSF_FRAMETYPE_DEVICE_INFO = 0x29,
            CRSF_FRAMETYPE_PARAMETER_SETTINGS_ENTRY = 0x2B,
            CRSF_FRAMETYPE_PARAMETER_READ = 0x2C,
            CRSF_FRAMETYPE_PARAMETER_WRITE = 0x2D,
            CRSF_FRAMETYPE_COMMAND = 0x32,
            // MSP commands
            CRSF_FRAMETYPE_MSP_REQ = 0x7A,   // response request using msp sequence as command
            CRSF_FRAMETYPE_MSP_RESP = 0x7B,  // reply with 58 byte chunked binary
            CRSF_FRAMETYPE_MSP_WRITE = 0x7C  // write with 8 byte chunked binary (OpenTX outbound telemetry buffer limit)
        };


        static byte crc8(byte[] data, int start, int length)
        {
            uint crc = 0;
            for (uint i = 0; i < length; i++)
            {
                uint inbyte = data[start + i];
                for (uint j = 0; j < 8; j++)
                {
                    uint mix = (crc ^ inbyte) & 0x80;
                    crc <<= 1;
                    if (mix != 0)
                    {
                        crc ^= 0xD5;
                    }
                    inbyte <<= 1;
                }
            }
            return (byte)crc;
        }

        public static string ByteArrayToString(byte[] ba, int start, int length)
        {
            StringBuilder hex = new StringBuilder(length * 2);
            for (int pos = 0; pos < length; pos++)
            {
                hex.AppendFormat("{0:x2}", ba[start + pos]);
            }
            return hex.ToString();
        }

        private static SerialPort Port;

        static void Main(string[] args)
        {
            Port = new SerialPort("COM16", 420000, Parity.None, 8, StopBits.One);
            eCrsfState state = eCrsfState.STATE_WAIT_SYNC;

            Port.Open();

            int received = 0;
            byte[] buffer = new byte[0];

            while (true)
            {
                int data = Port.ReadByte();

                switch (state)
                {
                    case eCrsfState.STATE_WAIT_SYNC:
                        if (data == 0xC8)
                        {
                            state = eCrsfState.STATE_RECEIVE_LEN;
                        }
                        break;

                    case eCrsfState.STATE_RECEIVE_LEN:
                        buffer = new byte[data];
                        received = 0;
                        state = eCrsfState.STATE_RECEIVE_DATA;
                        break;

                    case eCrsfState.STATE_RECEIVE_DATA:
                        buffer[received++] = (byte)data;
                        if (received >= buffer.Length)
                        {
                            byte crc = buffer[buffer.Length - 1];
                            byte calcCrc = crc8(buffer, 0, buffer.Length - 1);
                            bool crcFail = (calcCrc != crc);

                            if (crcFail)
                            {
                                Console.WriteLine("CRC Failed");
                            }
                            else
                            {
                                Process(buffer, 0, buffer.Length - 1);
                            }


                            state = eCrsfState.STATE_WAIT_SYNC;
                        }
                        break;
                }
            }
        }

        private static byte dummy_crc = 0;

        private static Dictionary<eCrsfFrameType, int> ReceivedTypes = new Dictionary<eCrsfFrameType, int>();
        private static List<eCrsfFrameType> ReceivedTypesIndex = new List<eCrsfFrameType>();

        private static DateTime lastTime = DateTime.Now;
        private static int framesSent = 0;
        private static int framesReceived = 0;
        private static int framesReceivedLast = 0;

        private static void Process(byte[] buffer, int start, int length)
        {
            int pos = 8;
            DateTime nowTime = DateTime.Now;

            if (Enum.IsDefined(typeof(eCrsfFrameType), (int)buffer[start + 0]))
            {
                eCrsfFrameType type = (eCrsfFrameType)buffer[start + 0];

                if (!ReceivedTypes.ContainsKey(type))
                {
                    ReceivedTypes.Add(type, 0);
                    ReceivedTypesIndex.Add(type);
                }

                ReceivedTypes[type]++;
                pos = ReceivedTypesIndex.IndexOf(type);

                if (type == eCrsfFrameType.CRSF_FRAMETYPE_RC_CHANNELS_PACKED)
                {
                    uint[] channelValues = new uint[16];
                    byte chanBits = 0;
                    byte channel = 0;
                    uint value = 0;

                    framesReceived++;

                    /* go through all payload bytes */
                    for (int bitPos = 0; bitPos < 22; bitPos++)
                    {
                        /* fetch 8 bits */
                        value |= ((uint)buffer[start + 1 + bitPos]) << chanBits;
                        chanBits += 8;

                        /* when we got enough (11) bits, treat this as a sample */
                        if (chanBits >= 11)
                        {
                            channelValues[channel++] = (value & 0x7FF);
                            /* keep remaining bits */
                            value >>= 11;
                            chanBits -= 11;
                        }
                    }
                    Console.SetCursorPosition(4, 2 + 4 * pos);
                    Console.Write("Received  " + ReceivedTypes[type].ToString().PadLeft(6) + "x  0x" + buffer[start + 0].ToString("X2") + " (" + Enum.GetName(typeof(eCrsfFrameType), type) + ")");

                    Console.SetCursorPosition(4, 2 + 4 * pos + 2);
                    Console.Write("  parsed  " + string.Join(" ", channelValues.Select(v => v.ToString("D4"))));
                }
                else
                {
                    Console.SetCursorPosition(4, 2 + 4 * pos);
                    Console.Write("Received  " + ReceivedTypes[type].ToString().PadLeft(6) + "x  0x" + buffer[start + 0].ToString("X2") + " (" + Enum.GetName(typeof(eCrsfFrameType), type) + ")");
                }
            }
            else if ((int)buffer[start + 0] == 0xED)
            {
                int ms = 0;

                ms |= (int)buffer[start + 1];
                ms |= (int)buffer[start + 2] << 8;
                ms |= (int)buffer[start + 3] << 16;
                ms |= (int)buffer[start + 4] << 24;

                Console.SetCursorPosition(20, 0);
                Console.Write("latency:    " + (nowTime.Millisecond - ms) + " ms    ");
            }
            else
            {
                Console.SetCursorPosition(4, 2 + 4 * pos);
                Console.Write("Received   0x" + buffer[start + 0].ToString("X2") + "  ");
            }


            if ((nowTime - lastTime).TotalMilliseconds > 500)
            {
                /* calc rate */
                int rate = (framesReceived - framesReceivedLast) * 2;
                Console.SetCursorPosition(0, 0);
                Console.Write(" rate " + rate + " Hz   ");
                framesReceivedLast = framesReceived;

                /* send custom telemetry */
                framesSent++;
                lastTime = nowTime;
                byte[] buf = new byte[] { 0xC8, 0x06, 0xEC, 0xEC, 0xAA, 0x55, 0x11, 0x00 };
                buf[3] = (byte) (nowTime.Millisecond & 0xFF);
                buf[4] = (byte)((nowTime.Millisecond >> 8) & 0xFF);
                buf[5] = (byte)((nowTime.Millisecond >> 16) & 0xFF);
                buf[6] = (byte)((nowTime.Millisecond >> 24) & 0xFF);
                buf[7] = crc8(buf, 2, 5);

                Port.Write(buf, 0, buf.Length);
                Console.SetCursorPosition(20, 0);
                Console.Write(" sent " + framesSent);
            }

            var lines = ByteArrayToString(buffer, start, length).Split(60);
            int linenum = 0;
            foreach (string l in lines)
            {
                Console.SetCursorPosition(6, 2 + 4 * pos + 1 + linenum);
                Console.Write(l);
            }
        }
    }

    public static class Extensions
    {
        public static IEnumerable<string> Split(this string str, int n)
        {
            if (String.IsNullOrEmpty(str) || n < 1)
            {
                throw new ArgumentException();
            }

            return Enumerable.Range(0, (str.Length + n - 1) / n).Select(i => str.Substring(i * n, Math.Min(n, str.Length - i * n)));
        }
    }
}
