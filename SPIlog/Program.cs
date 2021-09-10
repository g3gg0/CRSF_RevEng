using Mono.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPIlog
{
    class Program
    {
        public static SerialPort Port { get; private set; }
        public static bool IsLora
        {
            get
            {
                return (RegistersFsk[0] & 0x80) != 0;
            }
        }

        public static uint[] HopSequence = new uint[] { 0, 6, 28, 11, 39, 23, 17, 9, 37, 1, 48, 38, 31, 3, 49, 46, 25, 15, 44, 8, 27, 47, 24, 2, 29, 10, 43, 40, 26, 42, 7, 4, 19, 33, 12, 5, 36, 22, 45, 14, 20, 35, 32, 13, 16, 30, 21, 0, 18, 28, 11, 41, 23, 17, 34, 37, 1, 6, 38, 31, 39, 49, 46, 9, 15, 44, 48, 27, 47, 3, 2, 29, 25, 43, 40, 8, 42, 7, 24, 19, 33, 10, 5, 36, 26, 45, 14, 4, 35, 32, 12, 16, 30, 22, 0, 18, 20, 11, 41, 13, 17, 34, 21, 1, 6, 28, 31, 39, 23, 46, 9, 37, 44, 48, 38, 47, 3, 49, 29, 25, 15, 40, 8, 27, 7, 24, 2, 33, 10, 43, 36, 26, 42, 14, 4, 19, 32, 12, 5, 30, 22, 45, 18, 20, 35, 41, 13, 16, 34, 21 };
        public static int[] HopSequenceDetected = null;
        public static byte[] RegistersFsk = new byte[255];
        public static byte[] RegistersLora = new byte[255];
        public static uint[] SharedRegisters = new uint[] { 0x00, 0x01, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x4B, 0x58, 0x5A, 0x5C, 0x5E, 0x63, 0x6C, 0x70 };
        public static byte[] hopDownCrc = new byte[150];
        public static ushort[] hopUpCrc = new ushort[150];
        public static ulong ChannelSpacing = 260000;
        public static List<ulong> Frequencies = new List<ulong>();
        public static List<ulong> FrequenciesBinary = new List<ulong>();
        public static List<ulong> LastWrittenFrequenciesBinary = new List<ulong>();
        public static List<ulong> LastUplinkFrequenciesBinary = new List<ulong>();
        public static List<Tuple<double, byte>> FrequenciesLog = new List<Tuple<double, byte>>();
        public static ulong MinFreq = 0xFFFFFFFF;
        public static ulong MaxFreq = 0;

        public enum SX1272RegsFsk
        {
            RegFifo = 0x00,
            RegOpMode = 0x01,
            RegBitrateMsb = 0x02,
            RegBitrateLsb = 0x03,
            RegFdevMsb = 0x04,
            RegFdevLsb = 0x05,
            RegFrfMsb = 0x06,
            RegFrfMid = 0x07,
            RegFrfLsb = 0x08,
            RegPaConfig = 0x09,
            RegPaRamp = 0x0A,
            RegOcp = 0x0B,
            RegLna = 0x0C,
            RegRxConfig = 0x0D,
            RegRssiConfig = 0x0E,
            RegRssiCollision = 0x0F,
            RegRssiThresh = 0x10,
            RegRssiValue = 0x11,
            RegRxBw = 0x12,
            RegAfcBw = 0x13,
            RegOokPeak = 0x14,
            RegOokFix = 0x15,
            RegOokAvg = 0x16,
            RegAfcFei = 0x1A,
            RegAfcMsb = 0x1B,
            RegAfcLsb = 0x1C,
            RegFeiMsb = 0x1D,
            RegFeiLsb = 0x1E,
            RegPreambleDetect = 0x1F,
            RegRxTimeout1 = 0x20,
            RegRxTimeout2 = 0x21,
            RegRxTimeout3 = 0x22,
            RegRxDelay = 0x23,
            RegOsc = 0x24,
            RegPreambleMsb = 0x25,
            RegPreambleLsb = 0x26,
            RegSyncConfig = 0x27,
            RegSyncValue1 = 0x28,
            RegSyncValue2 = 0x29,
            RegSyncValue3 = 0x2A,
            RegSyncValue4 = 0x2B,
            RegSyncValue5 = 0x2C,
            RegSyncValue6 = 0x2D,
            RegSyncValue7 = 0x2E,
            RegSyncValue8 = 0x2F,
            RegPacketConfig1 = 0x30,
            RegPacketConfig2 = 0x31,
            RegPayloadLength = 0x32,
            RegNodeAdrs = 0x33,
            RegBroadcastAdrs = 0x34,
            RegFifoThresh = 0x35,
            RegSeqConfig1 = 0x36,
            RegSeqConfig2 = 0x37,
            RegTimerResol = 0x38,
            RegTimer1Coef = 0x39,
            RegTimer2Coef = 0x3A,
            RegImageCal = 0x3B,
            RegTemp = 0x3C,
            RegLowBat = 0x3D,
            RegIrqFlags1 = 0x3E,
            RegIrqFlags2 = 0x3F,
            RegDioMapping1 = 0x40,
            RegDioMapping2 = 0x41,
            RegVersion = 0x42,
            RegAgcRef = 0x43,
            RegAgcThresh1 = 0x44,
            RegAgcThresh2 = 0x45,
            RegAgcThresh3 = 0x46,
            RegPllHop = 0x4B,
            RegTcxo = 0x58,
            RegPaDac = 0x5A,
            RegPll = 0x5C,
            RegPllLowPn = 0x5E,
            RegPaManual = 0x63,
            RegFormerTemp = 0x6C,
            RegBitRateFrac = 0x70
        }

        public enum SX1272RegsLoRa
        {
            RegFifo = 0x00,
            RegOpMode = 0x01,
            RegFrfMsb = 0x06,
            RegFrfMid = 0x07,
            RegFrfLsb = 0x08,
            RegPaConfig = 0x09,
            RegPaRamp = 0x0A,
            RegOcp = 0x0B,
            RegLna = 0x0C,
            RegFifoAddrPtr = 0x0D,
            RegFifoTxBaseAddr = 0x0E,
            RegFifoRxBaseAddr = 0x0F,
            FifoRxCurrentAddr = 0x10,
            RegIrqFlagsMask = 0x11,
            RegIrqFlags = 0x12,
            RegRxNbBytes = 0x13,
            RegRxHeaderCntValueMsb = 0x14,
            RegRxHeaderCntValueLsb = 0x15,
            RegRxPacketCntMsb = 0x16,
            RegRxPacketCntLsb = 0x17,
            RegModemStat = 0x18,
            RegPktSnrValue = 0x19,
            RegPktRssiValue = 0x1A,
            RegRssiValue = 0x1B,
            RegHopChannel = 0x1C,
            RegModemConfig1 = 0x1D,
            RegModemConfig2 = 0x1E,
            RegSymbTimeoutLsb = 0x1F,
            RegPreambleMsb = 0x20,
            RegPreambleLsb = 0x21,
            RegPayloadLength = 0x22,
            RegMaxPayloadLength = 0x23,
            RegHopPeriod = 0x24,
            RegFifoRxByteAddr = 0x25,
            RegFeiMsb = 0x28,
            RegFeiMib = 0x29,
            RegFeiLsb = 0x2A,
            RegRssiWideband = 0x2C,
            RegDetectOptimize = 0x31,
            RegInvertIQ = 0x33,
            RegDetectionThreshold = 0x37,
            RegSyncWord = 0x39,
            RegInvertIQ2 = 0x3B,
            RegDioMapping1 = 0x40,
            RegDioMapping2 = 0x41,
            RegVersion = 0x42,
            RegAgcRef = 0x43,
            RegAgcThresh1 = 0x44,
            RegAgcThresh2 = 0x45,
            RegAgcThresh3 = 0x46,
            RegPllHop = 0x4B,
            RegTcxo = 0x58,
            RegPaDac = 0x5A,
            RegPll = 0x5C,
            RegPllLowPn = 0x5E,
            RegPaManual = 0x63,
            RegFormerTemp = 0x6C,
            RegBitRateFrac = 0x70
        }

        public static byte crc8(byte[] data, int start, int length, byte init)
        {
            uint crc = init;

            if (data == null || data.Length < start + length)
            {
                return 0;
            }

            for (uint i = 0; i < length; i++)
            {
                uint inData = data[start + i];
                for (uint j = 0; j < 8; j++)
                {
                    uint mix = (crc ^ inData) & 0x80;
                    crc <<= 1;
                    if (mix != 0)
                    {
                        crc ^= 0x07;
                    }
                    inData <<= 1;
                }
            }
            return (byte)crc;
        }

        public static ushort reflect(int inData, int width)
        {
            int resByte = 0;

            for (int i = 0; i < width; i++)
            {
                if ((inData & (1 << i)) != 0)
                {
                    resByte |= ((1 << (width - 1 - i)) & 0xFFFF);
                }
            }

            return (ushort)resByte;
        }

        public static ushort[] crc16_table =
        {
                0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
                0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6, 0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
                0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
                0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
                0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823, 0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
                0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
                0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
                0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70, 0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
                0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
                0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
                0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D, 0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
                0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
                0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
                0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A, 0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
                0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
                0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
            };

        public static ushort crc16(byte[] data, int start, int length, ushort init)
        {
            int crc = init;

            for (int i = 0; i < length; ++i)
            {
                crc ^= (reflect(data[start + i], 8) << (16 - 8));
                int pos = (crc >> (16 - 8)) & 0xFF;
                crc <<= 8;
                crc ^= crc16_table[pos];
            }
            return reflect(crc, 16);
        }

        static ulong GetFrequencyRaw()
        {
            return (GetRegister(SX1272RegsFsk.RegFrfMsb) << 16) | (GetRegister(SX1272RegsFsk.RegFrfMid) << 8) | (GetRegister(SX1272RegsFsk.RegFrfLsb) << 0);
        }

        /// <summary>
        /// Return the frequency in MHz
        /// </summary>
        static decimal GetFrequency()
        {
            return FreqToMhz(GetFrequencyRaw());
        }

        static int GetBitrateRaw()
        {
            return (RegistersFsk[(int)SX1272RegsFsk.RegBitrateMsb] << 8) | RegistersFsk[(int)SX1272RegsFsk.RegBitrateLsb];
        }

        static decimal GetBitrate()
        {
            if (IsFsk())
            {
                int br = GetBitrateRaw();
                decimal rate = (br > 0 ? (32000000.0m / br) : 0) / 1000.0m;

                return Math.Round(rate, 2);
            }

            return 0;
        }

        static int GetChiprate()
        {
            if (IsFsk())
            {
                return 0;
            }
            else
            {
                int rateReg = (RegistersLora[0x1E] >> 4) & 0x0F;
                /* check if anything was configured at all */
                if (rateReg < 6 || rateReg > 12)
                {
                    return 0;
                }
                int rate = 1 << rateReg;

                return rate;
            }
        }

        static int GetFreqShiftRaw()
        {
            return (RegistersFsk[(int)SX1272RegsFsk.RegFdevMsb] << 8) | RegistersFsk[(int)SX1272RegsFsk.RegFdevLsb];
        }

        static bool IsFsk()
        {
            return ((GetRegister(SX1272RegsFsk.RegOpMode) & 0x80) == 0);
        }

        static decimal GetFreqShift()
        {
            if (IsFsk())
            {
                decimal width = (GetFreqShiftRaw() * 32000000.0m / (1 << 19)) / 1000.0m;

                return Math.Round(width, 2);
            }
            else
            {
                /* check if anything was configured at all */
                if(((RegistersLora[0x1D] >> 3) & 7) == 0)
                {
                    return 0;
                }
                int width = 0;
                switch(RegistersLora[0x1D] >> 6)
                {
                    case 0:
                        width = 125000;
                        break;
                    case 1:
                        width = 250000;
                        break;
                    case 2:
                        width = 500000;
                        break;

                }

                return width / 1000;
            }
        }

        static string GetModulationParameters()
        {
            if(IsFsk())
            {
                return "FSK " + " F:" + GetFrequency().ToString("000.000") + "MHz S:" + GetFreqShift().ToString("00.000") + "kHz B:" + GetBitrate().ToString("00.00") + "k";
            }
            return "LoRa" + " F:" + GetFrequency().ToString("000.000") + "MHz S:" + GetFreqShift().ToString("00.000") + "kHz B:" + GetChiprate().ToString("00") + "c/s";
        }

        static void Main(string[] args)
        {
            string port = null;
            string outfile = null;
            string infile = null;
            bool show_help = false;
            bool hasKeys = true;
            int displayMode = 1;
            int sleepDelay = 0;
            string lastModulationParameters = "";

            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            var p = new OptionSet() {
                "Usage: SPILog [-p port] [-o log.bin] [-i log.bin]",
                "",
                "Options:",
                { "p|port=", "Serial port of the FPGA SPI logger.", v => port = v },
                { "o|output=", "Filename where to write SPI log to.", v => outfile = v },
                { "i|input=", "Filename where to read SPI log data from.", v => infile = v },
                { "d|display=", "Display mode (0 = nothing, 1 = register r/w, 2 = FIFO payload only, 3 = SPI data, 4 = raw SPI data).", v => displayMode = int.Parse(v) },
                { "s|sleep=", "sleep delay in ms after payload r/w in playback mode (0 = disable).", v => sleepDelay = int.Parse(v) },
                { "h|help",  "Show this message and exit", v => show_help = v != null },
            };
            List<string> extra;

            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("SPILog: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `SPILog --help' for more information.");
                return;
            }

            if (show_help)
            {
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (port != null && infile != null)
            {
                Console.Write("SPILog: ");
                Console.WriteLine("You cannot specify both -p and -i");
                Console.WriteLine("Try `SPILog --help' for more information.");
                return;
            }

            BinaryWriter logWriter = null;
            BinaryReader logReader = null;

            if (outfile != null)
            {
                Console.WriteLine("SPILog: Writing log to " + outfile);
                if (File.Exists(outfile))
                {
                    File.Delete(outfile);
                }
                logWriter = new BinaryWriter(File.OpenWrite(outfile));
            }

            if (infile != null)
            {
                Console.WriteLine("SPILog: Reading log from " + infile);
                logReader = new BinaryReader(File.OpenRead(infile));
            }

            if (port != null)
            {
                Console.WriteLine("SPILog: Reading data from " + port);
                Port = new SerialPort(port, 8000000, Parity.None, 8, StopBits.One);
                Port.ReadBufferSize = 8192000;
                Port.Open();

                logReader = new BinaryReader(Port.BaseStream);
                logReader.BaseStream.ReadTimeout = 50;
            }

            if (logReader == null)
            {
                Console.Write("SPILog: ");
                Console.WriteLine("You have to specify either -p and -i");
                Console.WriteLine("Try `SPILog --help' for more information.");
                return;
            }

            SX1272RegsFsk lastFskRegister = 0;
            SX1272RegsLoRa lastLoRaRegister = 0;
            List<ulong> lastData = new List<ulong>();
            bool write = false;
            uint currentRegister = 0;
            int currentHop = 0;
            int hopsMissing = 0;
            int hopsSuccess = 0;
            bool running = true;
            byte fwLastCounter = 0;

            DateTime StartTime = DateTime.Now;
            DateTime lastUpdate = DateTime.Now;

            try
            {
                bool test = Console.KeyAvailable;
            }
            catch(Exception ex)
            {
                hasKeys = false;
            }

            switch (displayMode)
            {
                case 5:
                    Console.Clear();
                    break;
            }

            try
            {
                while (running)
                {
                    DateTime thisTime = DateTime.Now;

                    if (hasKeys && Console.KeyAvailable)
                    {
                        string line = Console.In.ReadLine().Trim();

                        if (line.Length == 0)
                        {
                            continue;
                        }

                        switch (line.Split(' ')[0])
                        {
                            case "help":
                                Console.WriteLine("Available commands:");
                                Console.WriteLine(" help, display <n>, stats, regs, quit");
                                break;

                            case "stats":
                                PrintStatistics();
                                continue;

                            case "quit":
                                running = false;
                                continue;

                            case "regs":
                                Console.WriteLine(" Reg    FSK   LoRa");
                                Console.WriteLine("-------------------");
                                for (uint reg = 0; reg < 0x80; reg++)
                                {
                                    if (SharedRegisters.Contains(reg))
                                    {
                                        Console.WriteLine(" 0x" + reg.ToString("X2") + ":     0x" + RegistersFsk[reg].ToString("X2"));
                                    }
                                    else
                                    {
                                        Console.WriteLine(" 0x" + reg.ToString("X2") + ":  0x" + RegistersFsk[reg].ToString("X2") + "  0x" + RegistersLora[reg].ToString("X2"));
                                    }
                                }
                                break;

                            case "display":
                                try
                                {
                                    displayMode = int.Parse(line.Split(' ')[1]);
                                    Console.Clear();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Usage: display <n>  where n = 0..4");
                                }
                                break;
                        }

                    }

                    try
                    {
                        byte[] buf = logReader.ReadBytes(2);

                        if (buf == null || buf.Length == 0)
                        {
                            if ((logReader.BaseStream is FileStream))
                            {
                                running = false;
                            }
                            continue;
                        }
                        byte type = buf[0];
                        byte data = buf[1];

                        if (logWriter != null)
                        {
                            logWriter.Write(buf);
                        }

                        /* chip enable */
                        if (type == 0xDE)
                        {
                            if (write)
                            {
                                switch (lastFskRegister)
                                {
                                    case SX1272RegsFsk.RegFrfMsb:
                                        ulong frf = (GetRegister(SX1272RegsFsk.RegFrfMsb) << 16) + (GetRegister(SX1272RegsFsk.RegFrfMid) << 8) + (GetRegister(SX1272RegsFsk.RegFrfLsb) << 0);

                                        /* delay a few ms */
                                        if ((logReader.BaseStream is FileStream) && sleepDelay > 0)
                                        {
                                            Thread.Sleep(sleepDelay);
                                        }

                                        if (frf != 0)
                                        {
                                            int arfcn = FreqToArfcn(frf);

                                            LastWrittenFrequenciesBinary.Add(frf);

                                            if (MinFreq != LastWrittenFrequenciesBinary.Min())
                                            {
                                                MinFreq = LastWrittenFrequenciesBinary.Min();
                                                HopSequenceDetected = null;
                                            }
                                            if (MaxFreq != LastWrittenFrequenciesBinary.Max())
                                            {
                                                MaxFreq = LastWrittenFrequenciesBinary.Max();
                                                HopSequenceDetected = null;
                                            }

                                            if (LastWrittenFrequenciesBinary.Count > 300)
                                            {
                                                /* if the pattern does not repeat, reset the hop list */
                                                if (LastWrittenFrequenciesBinary.First() != LastWrittenFrequenciesBinary.Last())
                                                {
                                                    HopSequenceDetected = null;
                                                }

                                                /* only update the hop sequence if the current (and first) ARFCN is zero */
                                                if (HopSequenceDetected == null && arfcn == 0)
                                                {
                                                    HopSequenceDetected = LastWrittenFrequenciesBinary.Take(300).Select(v => FreqToArfcn(v)).ToArray();
                                                }

                                                LastWrittenFrequenciesBinary.RemoveAt(0);
                                            }
                                        }
                                        break;
                                }
                            }

                            switch (displayMode)
                            {
                                case 0:
                                    break;

                                case 1:
                                    {
                                        switch (lastLoRaRegister)
                                        {
                                            case SX1272RegsLoRa.RegModemConfig2:
                                                {
                                                    byte regVal = (byte)GetRegister(SX1272RegsLoRa.RegModemConfig2);

                                                    Console.WriteLine("  => SpreadingFactor  " + (1U << (regVal >> 4)).ToString());
                                                    Console.WriteLine("  => TxContinuousMode " + ((regVal & (1 << 3)) != 0));
                                                    Console.WriteLine("  => AgcAutoOn        " + ((regVal & (1 << 2)) != 0));
                                                    Console.WriteLine("  => SymbTimeout(9:8) " + ((regVal >> 0) & 3).ToString());

                                                }
                                                break;
                                        }

                                        switch (lastFskRegister)
                                        {
                                            case SX1272RegsFsk.RegFifo:
                                                {
                                                    if (LastWrittenFrequenciesBinary.Count == 0)
                                                    {
                                                        break;
                                                    }
                                                    ulong frf = LastWrittenFrequenciesBinary.Last();
                                                    int arfcn = FreqToArfcn(frf) % 50;

                                                    string miss = "";
                                                    if (arfcn == HopSequence[currentHop])
                                                    {
                                                        currentHop++;
                                                        hopsSuccess++;
                                                        currentHop %= HopSequence.Length;
                                                        hopsMissing = 0;
                                                    }
                                                    else
                                                    {
                                                        currentHop += 37;
                                                        currentHop %= HopSequence.Length;
                                                        hopsMissing++;
                                                        miss = ", miss " + hopsMissing + " after " + hopsSuccess;
                                                        hopsSuccess = 0;
                                                    }

                                                    Console.WriteLine(">  " + (((GetRegister(SX1272RegsFsk.RegOpMode) & 0x80) == 0) ? "FSK " : "LoRa") + ", " + frf.ToString("X6") + "(" + FreqToMhzString(frf) + "), " + arfcn.ToString("00") + ", " + string.Join(" ", lastData.Select(d => d.ToString("X2"))) + " " + miss);
                                                }
                                                break;

                                            case SX1272RegsFsk.RegOpMode:
                                                Console.WriteLine("  => Mode " + (((GetRegister(SX1272RegsFsk.RegOpMode) & 0x80) == 0) ? "FSK " : "LoRa"));
                                                break;

                                            case SX1272RegsFsk.RegFrfMsb:
                                                if (lastData.Count == 3)
                                                {
                                                    ulong frf = GetFrequencyRaw();
                                                    ulong freq = (ulong)(GetFrequency() * 1000000.0m);

                                                    {
                                                        Frequencies.Add(freq);
                                                        FrequenciesBinary.Add(frf);
                                                        FrequenciesLog.Add(new Tuple<double, byte>((DateTime.Now - StartTime).TotalMilliseconds, (byte)((frf - 0xD70AB0) / 4260)));

                                                        if ((FrequenciesBinary.Count % 2) == 0)
                                                        {
                                                            int width = FrequenciesBinary.Count / 2;

                                                            var part1 = FrequenciesBinary.Take(width);
                                                            var part2 = FrequenciesBinary.Skip(width);

                                                            //if(Enumerable.SequenceEqual(part1, part2))
                                                            {
                                                                //PrintStatistics();
                                                            }

                                                        }
                                                    }

                                                    Console.WriteLine("  => Frequency: " + (freq / 1000) + " kHz");
                                                }
                                                break;
                                        }
                                        break;
                                    }

                                case 2:
                                    {
                                        switch (lastFskRegister)
                                        {
                                            case SX1272RegsFsk.RegFifo:
                                                {
                                                    if (LastWrittenFrequenciesBinary.Count == 0)
                                                    {
                                                        break;
                                                    }
                                                    ulong frf = LastWrittenFrequenciesBinary.Last();
                                                    int arfcn = FreqToArfcn(frf);
                                                    string payload = string.Join(" ", lastData.Select(d => d.ToString("X2")));

                                                    string miss = "@" + currentHop.ToString().PadLeft(3);
                                                    //if (write)
                                                    {
                                                        if (arfcn == HopSequence[currentHop])
                                                        {
                                                            currentHop++;
                                                            hopsSuccess++;
                                                            currentHop %= HopSequence.Length;
                                                            hopsMissing = 0;
                                                        }
                                                        else
                                                        {
                                                            currentHop = 0;
                                                            currentHop %= HopSequence.Length;
                                                            hopsMissing++;
                                                            miss = hopsMissing.ToString().PadLeft(4);
                                                            hopsSuccess = 0;
                                                        }
                                                    }

                                                    bool isFsk = ((GetRegister(SX1272RegsFsk.RegOpMode) & 0x80) == 0);

                                                    if (frf != 0 || payload.Length > 0)
                                                    {
                                                        byte[] binData = lastData.Select(d => (byte)d).ToArray();
                                                        bool crc8InitZero = false;

                                                        if (binData.Length > 1)
                                                        {
                                                            byte crc = crc8(binData, 0, binData.Length - 1, 0);
                                                            crc8InitZero = (crc == binData[binData.Length - 1]);
                                                        }

                                                        string modulationParameters = GetModulationParameters();

                                                        if(lastModulationParameters != modulationParameters)
                                                        {
                                                            lastModulationParameters = modulationParameters;
                                                            //Console.WriteLine("Modulation: " + modulationParameters);
                                                        }

                                                        Console.WriteLine(">  " + modulationParameters + " " + arfcn.ToString("00") + " " + miss + " " + (write ? "Tx" : "Rx") + "  " + payload + (crc8InitZero ? "  (CRC8 zero-init)":""));

                                                        if (!write && !isFsk && crc8InitZero)
                                                        {
                                                            /* then check for a fw packet 00000101 / 00001101  */
                                                            if ((binData[0] == 0x05) || (binData[0] == 0x0D))
                                                            {
                                                                byte len = binData[1];
                                                                byte[] fwData = binData.Skip(2).Take(len).ToArray();

                                                                if (fwLastCounter != binData[0])
                                                                {
                                                                    fwLastCounter = binData[0];
                                                                    //Console.WriteLine("FW>  " + string.Join(" ", fwData.Select(d => d.ToString("X2"))));
                                                                }
                                                            }

                                                        }
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }

                                case 3:
                                    Console.WriteLine("");
                                    Console.Write("" + (((data & 0x80) != 0) ? "W" : "R") + " " + (data & 0x7F).ToString("X2"));
                                    break;

                                case 4:
                                    Console.WriteLine("");
                                    Console.Write("" + data.ToString("X2"));
                                    break;

                                case 5:
                                    {
                                        switch (lastFskRegister)
                                        {
                                            case SX1272RegsFsk.RegFifo:
                                                {
                                                    if (LastWrittenFrequenciesBinary.Count == 0 || HopSequenceDetected == null)
                                                    {
                                                        break;
                                                    }
                                                    ulong frf = LastWrittenFrequenciesBinary.Last();
                                                    int arfcn = FreqToArfcn(frf);

                                                    if (arfcn == HopSequenceDetected[currentHop])
                                                    {
                                                        currentHop++;
                                                        hopsSuccess++;
                                                        currentHop %= HopSequenceDetected.Length;
                                                        hopsMissing = 0;

                                                    }
                                                    else
                                                    {
                                                        currentHop = 0;
                                                        hopsMissing++;
                                                        hopsSuccess = 0;
                                                    }

                                                    if (hopsSuccess > 10)
                                                    {
                                                        if (write)
                                                        {
                                                            for (int test_value = 0; test_value < 0x100; test_value++)
                                                            {
                                                                int pkt_start = 0;
                                                                int pkt_len = 12;
                                                                byte[] buffer = lastData.Select(d => (byte)d).ToArray();
                                                                byte initValue = (byte)(test_value + hopDownCrc[currentHop / 2]);

                                                                if (crc8(buffer, 0, pkt_start + pkt_len, initValue) == buffer[pkt_start + pkt_len])
                                                                {
                                                                    hopDownCrc[currentHop / 2] = initValue;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            for (int test_value = 0; test_value < 0x10000; test_value++)
                                                            {
                                                                int pkt_start = 0;
                                                                int pkt_len = 21;
                                                                byte[] buffer = lastData.Select(d => (byte)d).ToArray();
                                                                ushort initValue = (ushort)(test_value + hopUpCrc[currentHop / 2]);
                                                                ushort pktCrc = (ushort)((buffer[pkt_start + pkt_len + 1] << 8) | buffer[pkt_start + pkt_len]);

                                                                if (crc16(buffer, 0, pkt_start + pkt_len, initValue) == pktCrc)
                                                                {
                                                                    hopUpCrc[currentHop / 2] = initValue;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                break;
                                        }
                                        break;
                                    }
                            }

                            lastData.Clear();
                            currentRegister = data & ~0x80U;
                            write = (data & 0x80) != 0;

                            lastFskRegister = (SX1272RegsFsk) 0xff;
                            lastLoRaRegister = (SX1272RegsLoRa) 0xff;

                            if (IsFsk())
                            {
                                lastFskRegister = (SX1272RegsFsk)currentRegister;
                            }
                            else
                            {
                                lastLoRaRegister = (SX1272RegsLoRa)currentRegister;
                            }
                        }
                        else if (type == 0xDD)
                        {
                            /* data */
                            string regName = "unk";

                            if (IsFsk())
                            {
                                if (Enum.IsDefined(typeof(SX1272RegsFsk), (int)currentRegister))
                                {
                                    regName = Enum.GetName(typeof(SX1272RegsFsk), (int)currentRegister);
                                }
                            }
                            else
                            {
                                if (Enum.IsDefined(typeof(SX1272RegsLoRa), (int)currentRegister))
                                {
                                    regName = Enum.GetName(typeof(SX1272RegsLoRa), (int)currentRegister);
                                }
                            }

                            lastData.Add((ulong)data);

                            switch (displayMode)
                            {
                                case 0:
                                    break;

                                case 1:
                                    {
                                        if (write)
                                        {
                                            Console.Write("W Reg: 0x" + currentRegister.ToString("X2") + " " + regName.PadRight(16) + " < " + data.ToString("X2"));
                                            Console.WriteLine();
                                        }
                                        else
                                        {
                                            Console.Write("R Reg: 0x" + currentRegister.ToString("X2") + " " + regName.PadRight(16) + " > " + data.ToString("X2"));
                                            Console.WriteLine();
                                        }
                                        break;
                                    }

                                case 2:
                                    break;

                                case 3:
                                case 4:
                                    Console.Write(" " + data.ToString("X2"));
                                    break;

                                case 5:
                                    if ((thisTime - lastUpdate).TotalMilliseconds > 50)
                                    {

                                        lastUpdate = thisTime;
                                        var frfs = LastWrittenFrequenciesBinary.Distinct().ToList();
                                        List<ulong> deltas = new List<ulong>();

                                        frfs.Sort();

                                        for (int pos = 0; pos < frfs.Count - 1; pos++)
                                        {
                                            ulong delta = frfs[pos + 1] - frfs[pos];
                                            deltas.Add(delta);
                                        }
                                        decimal avgDelta = 0;

                                        if (deltas.Count > 0)
                                        {
                                            avgDelta = (decimal)deltas.Average(s => (double)s);
                                        }
                                        decimal avgDeltaFreq = (avgDelta * 32000000.0m / (1 << 19));

                                        ChannelSpacing = (ulong)avgDeltaFreq;

                                        Console.SetCursorPosition(0, 0);
                                        Console.WriteLine("Hop: " + currentHop.ToString().PadLeft(3));
                                        Console.WriteLine("");
                                        Console.WriteLine("Freq min:  0x" + MinFreq.ToString("X6") + " (" + FreqToMhzString(MinFreq) + " MHz)      ");
                                        Console.WriteLine("Freq max:  0x" + MaxFreq.ToString("X6") + " (" + FreqToMhzString(MaxFreq) + " MHz)      ");
                                        Console.WriteLine("Spacing:   " + (avgDeltaFreq / 1000.0m).ToString("0.00") + " kHz      ");
                                        Console.WriteLine("FreqShift: 0x" + GetFreqShiftRaw().ToString("X6") + " (" + GetFreqShift().ToString("0.00") + " kHz)      ");
                                        Console.WriteLine("Bitrate:   0x" + GetBitrateRaw().ToString("X6") + " (" + GetBitrate().ToString("0.00") + " kBaud)      ");
                                        Console.WriteLine("");
                                        Console.WriteLine("");
                                        Console.WriteLine(" hop sequence");
                                        Console.WriteLine("--------------");
                                        for (int hopNum = 0; hopNum < 300; hopNum++)
                                        {
                                            if ((hopNum % 30) == 0)
                                            {
                                                Console.WriteLine();
                                                Console.Write(" " + hopNum.ToString().PadLeft(3) + " ");
                                            }
                                            if (HopSequenceDetected != null)
                                            {
                                                Console.Write(" " + HopSequenceDetected[hopNum].ToString().PadLeft(2));
                                            }
                                        }
                                        Console.WriteLine("");
                                        Console.WriteLine("");
                                        Console.WriteLine(" Downlink CRC8 table per hop");
                                        Console.WriteLine("-----------------------------");
                                        for (int hopNum = 0; hopNum < 150; hopNum++)
                                        {
                                            if ((hopNum % 30) == 0)
                                            {
                                                Console.WriteLine();
                                                Console.Write(" " + hopNum.ToString().PadLeft(3) + " ");
                                            }
                                            Console.Write(" " + hopDownCrc[hopNum].ToString("X2"));
                                        }
                                        Console.WriteLine("");
                                        Console.WriteLine("");
                                        Console.WriteLine(" Uplink CRC16 table per hop");
                                        Console.WriteLine("----------------------------");
                                        for (int hopNum = 0; hopNum < 150; hopNum++)
                                        {
                                            if ((hopNum % 20) == 0)
                                            {
                                                Console.WriteLine();
                                                Console.Write(" " + hopNum.ToString().PadLeft(3) + " ");
                                            }
                                            Console.Write(" " + hopUpCrc[hopNum].ToString("X4"));
                                        }
                                        Console.WriteLine("");
                                    }
                                    break;
                            }

                            if (write)
                            {
                                WriteRegister(currentRegister, data);
                            }

                            if (currentRegister != 0)
                            {
                                currentRegister++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Lost a few bytes");
                            lastFskRegister = 0;
                            lastData.Clear();
                            logReader.ReadByte();
                        }
                    }
                    catch (TimeoutException ex)
                    {
                    }
                }
            }
            catch (EndOfStreamException ex)
            {
                Console.WriteLine("End of file");
            }
        }

        private static int FreqToArfcn(ulong frf)
        {
            double spacingFrf = ChannelSpacing * (1 << 19) / 32000000; //4260.0f
            return (int)Math.Round((frf - MinFreq) / spacingFrf, 0);
        }

        private static decimal FreqToMhz(ulong frf)
        {
            return Math.Round(((frf * 32000000) >> 19) / 1000000.0m, 3);
        }

        private static string FreqToMhzString(ulong frf)
        {
            return FreqToMhz(frf).ToString("0.000");
        }

        private static void PrintStatistics()
        {
            ulong min = FrequenciesBinary.Min();
            ulong max = FrequenciesBinary.Max();
            decimal minFreq = ((min * 32000000) >> 19) / 1000000.0m;
            decimal maxFreq = ((max * 32000000) >> 19) / 1000000.0m;

            var freqs = FrequenciesBinary.OrderBy(s => s).Distinct();

            List<ulong> deltas = new List<ulong>();

            ulong prevFreq = freqs.First();

            foreach (var freq in freqs.Skip(1))
            {
                ulong delta = freq - prevFreq;

                deltas.Add(delta);

                prevFreq = freq;
            }

            if (deltas.Count > 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Min: " + min.ToString("X6") + " (" + minFreq.ToString("0.000") + ")" + " Max: " + max.ToString("X6") + " (" + maxFreq.ToString("0.000") + ")" + " Distance: " + deltas.First() + " Hz");

                if (!IsLora)
                {
                    uint frac = GetRegister(0x70);
                    uint bitRate = (GetRegister(2) << 8 | GetRegister(3));

                    if (bitRate > 0)
                    {
                        Console.WriteLine("Rate: " + (32000000 / (bitRate + frac / 16)));
                    }
                }
            }

            /*

            Console.WriteLine("Frequencies: ");
            foreach (ulong frf in FrequenciesBinary)
            {
                var chan = (frf - FrequenciesBinary.Min()) / deltas.First();
                Console.Write("0x" + chan.ToString("X2")+", ");
            }
            Console.WriteLine("");

            */

            Console.WriteLine("Frequencies: ");
            foreach (var pair in FrequenciesLog.OrderBy(p => p.Item1))
            {
                Console.Write(pair.Item1 + " " + pair.Item2.ToString("X2") + ", ");
            }
            Console.WriteLine("");
        }

        private static void WriteRegister(uint currentRegister, uint data)
        {
            if (IsLora && !SharedRegisters.Contains(currentRegister))
            {
                RegistersLora[currentRegister] = (byte)data;
            }
            else
            {
                RegistersFsk[currentRegister] = (byte)data;
            }
        }

        private static uint GetRegister(SX1272RegsFsk reg)
        {
            return GetRegister((uint)reg);
        }

        private static uint GetRegister(SX1272RegsLoRa reg)
        {
            return GetRegister((uint)reg);
        }

        private static uint GetRegister(uint currentRegister)
        {
            if (IsLora && !SharedRegisters.Contains(currentRegister))
            {
                return RegistersLora[currentRegister];
            }
            return RegistersFsk[currentRegister];
        }
    }
}