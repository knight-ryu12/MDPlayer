﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDPlayer
{
    public static class Common
    {
        public const int DEV_WaveOut = 0;
        public const int DEV_DirectSound = 1;
        public const int DEV_WasapiOut = 2;
        public const int DEV_AsioOut = 3;
        public const int DEV_SPPCM = 4;
        public const int DEV_Null = 5;

        public static Int32 SampleRate = 44100;
        public static Int32 NsfClock = 1789773;
        public static string settingFilePath = "";

        public static UInt32 getBE16(byte[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 1)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr] * 0x100 + (UInt32)buf[adr + 1];

            return dat;
        }

        public static UInt32 getLE16(byte[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 1)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr] + (UInt32)buf[adr + 1] * 0x100;

            return dat;
        }

        public static UInt32 getLE24(byte[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 2)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr] + (UInt32)buf[adr + 1] * 0x100 + (UInt32)buf[adr + 2] * 0x10000;

            return dat;
        }

        public static UInt32 getLE32(byte[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 3)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr] + (UInt32)buf[adr + 1] * 0x100 + (UInt32)buf[adr + 2] * 0x10000 + (UInt32)buf[adr + 3] * 0x1000000;

            return dat;
        }

        public static byte[] getByteArray(byte[] buf, ref uint adr)
        {
            if (adr >= buf.Length) return null;

            List<byte> ary = new List<byte>();
            while (buf[adr] != 0 || buf[adr + 1] != 0)
            {
                ary.Add(buf[adr]);
                adr++;
                ary.Add(buf[adr]);
                adr++;
            }
            adr += 2;

            return ary.ToArray();
        }

        public static GD3 getGD3Info(byte[] buf, uint adr)
        {
            GD3 GD3 = new GD3();

            GD3.TrackName = "";
            GD3.TrackNameJ = "";
            GD3.GameName = "";
            GD3.GameNameJ = "";
            GD3.SystemName = "";
            GD3.SystemNameJ = "";
            GD3.Composer = "";
            GD3.ComposerJ = "";
            GD3.Converted = "";
            GD3.Notes = "";
            GD3.VGMBy = "";
            GD3.Version = "";
            GD3.UsedChips = "";

            try
            {
                //trackName
                GD3.TrackName = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //trackNameJ
                GD3.TrackNameJ = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //gameName
                GD3.GameName = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //gameNameJ
                GD3.GameNameJ = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //systemName
                GD3.SystemName = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //systemNameJ
                GD3.SystemNameJ = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //Composer
                GD3.Composer = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //ComposerJ
                GD3.ComposerJ = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //Converted
                GD3.Converted = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //VGMBy
                GD3.VGMBy = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //Notes
                GD3.Notes = Encoding.Unicode.GetString(Common.getByteArray(buf, ref adr));
                //Lyric(独自拡張)
                byte[] bLyric = Common.getByteArray(buf, ref adr);
                if (bLyric != null)
                {
                    GD3.Lyrics = new List<Tuple<int, int, string>>();
                    int i = 0;
                    int st = 0;
                    while (i < bLyric.Length)
                    {
                        byte h = bLyric[i];
                        byte l = bLyric[i + 1];
                        if ((h == 0x5b && l == 0x00 && i != 0) || i >= bLyric.Length - 2)
                        {
                            if ((i >= bLyric.Length - 2) || (bLyric[i + 2] != 0x5b || bLyric[i + 3] != 0x00))
                            {
                                string m = Encoding.Unicode.GetString(bLyric, st, i - st + ((i >= bLyric.Length - 2) ? 2 : 0));
                                st = i;

                                int cnt = int.Parse(m.Substring(1, m.IndexOf("]")-1));
                                m = m.Substring(m.IndexOf("]") + 1);
                                GD3.Lyrics.Add(new Tuple<int, int, string>(cnt, cnt, m));
                            }
                        }
                        i += 2;
                    }
                }
                else
                {
                    GD3.Lyrics = null;
                }

            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
            }

            return GD3;
        }

        public static string getNRDString(byte[] buf, ref uint index)
        {
            if (buf == null || buf.Length < 1 || index < 0 || index >= buf.Length) return "";

            try
            {
                List<byte> lst = new List<byte>();
                for (; buf[index] != 0; index++)
                {
                    lst.Add(buf[index]);
                }

                string n = System.Text.Encoding.GetEncoding(932).GetString(lst.ToArray());
                index++;

                return n;
            }
            catch (Exception e)
            {
                log.ForcedWrite(e);
            }
            return "";
        }

        public static string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public static int Range(int n, int min, int max)
        {
            return (n > max) ? max : (n < min ? min : n);
        }

        public static int getvv(byte[] buf, ref uint musicPtr)
        {
            int s = 0, n = 0;

            do
            {
                n |= (buf[musicPtr] & 0x7f) << s;
                s += 7;
            }
            while ((buf[musicPtr++] & 0x80) > 0);

            return n + 2;
        }

        public static int getv(byte[] buf, ref uint musicPtr)
        {
            int s = 0, n = 0;

            do
            {
                n |= (buf[musicPtr] & 0x7f) << s;
                s += 7;
            }
            while ((buf[musicPtr++] & 0x80) > 0);

            return n;
        }

        public static int getDelta(ref uint trkPtr, byte[] bs)
        {
            int delta = 0;
            while(true)
            {
                delta = (delta << 7) + (bs[trkPtr] & 0x7f);
                if ((bs[trkPtr] & 0x80) == 0)
                {
                    trkPtr++;
                    break;
                }
                trkPtr++;
            }

            return delta;
        }

        public static EnmFileFormat CheckExt(string filename)
        {
            if (filename.ToLower().LastIndexOf(".m3u") != -1) return EnmFileFormat.M3U;
            if (filename.ToLower().LastIndexOf(".mid") != -1) return EnmFileFormat.MID;
            if (filename.ToLower().LastIndexOf(".nrd") != -1) return EnmFileFormat.NRT;
            if (filename.ToLower().LastIndexOf(".nsf") != -1) return EnmFileFormat.NSF;
            if (filename.ToLower().LastIndexOf(".hes") != -1) return EnmFileFormat.HES;
            if (filename.ToLower().LastIndexOf(".sid") != -1) return EnmFileFormat.SID;
            if (filename.ToLower().LastIndexOf(".mnd") != -1) return EnmFileFormat.MND;
            if (filename.ToLower().LastIndexOf(".mdr") != -1) return EnmFileFormat.MDR;
            if (filename.ToLower().LastIndexOf(".mdx") != -1) return EnmFileFormat.MDX;
            if (filename.ToLower().LastIndexOf(".mub") != -1) return EnmFileFormat.MUB;
            if (filename.ToLower().LastIndexOf(".muc") != -1) return EnmFileFormat.MUC;
            if (filename.ToLower().LastIndexOf(".rcp") != -1) return EnmFileFormat.RCP;
            if (filename.ToLower().LastIndexOf(".s98") != -1) return EnmFileFormat.S98;
            if (filename.ToLower().LastIndexOf(".vgm") != -1) return EnmFileFormat.VGM;
            if (filename.ToLower().LastIndexOf(".vgz") != -1) return EnmFileFormat.VGM;
            if (filename.ToLower().LastIndexOf(".xgm") != -1) return EnmFileFormat.XGM;
            if (filename.ToLower().LastIndexOf(".zip") != -1) return EnmFileFormat.ZIP;
            if (filename.ToLower().LastIndexOf(".lzh") != -1) return EnmFileFormat.LZH;

            return EnmFileFormat.unknown;
        }

        public static int searchFMNote(int freq)
        {
            int m = int.MaxValue;
            int n = 0;
            for (int i = 0; i < 12 * 5; i++)
            {
                //if (freq < Tables.FmFNum[i]) break;
                //n = i;
                int a = Math.Abs(freq - Tables.FmFNum[i]);
                if (m > a)
                {
                    m = a;
                    n = i;
                }
            }
            return n - 12 * 3;
        }

        public static int searchSSGNote(float freq)
        {
            float m = float.MaxValue;
            int n = 0;
            for (int i = 0; i < 12 * 8; i++)
            {
                //if (freq < Tables.freqTbl[i]) break;
                //n = i;
                float a = Math.Abs(freq - Tables.freqTbl[i]);
                if (m > a)
                {
                    m = a;
                    n = i;
                }
            }
            return n;
        }

        public static int searchSegaPCMNote(double ml)
        {
            double m = double.MaxValue;
            int n = 0;
            for (int i = 0; i < 12 * 8; i++)
            {
                double a = Math.Abs(ml - (Tables.pcmMulTbl[i % 12 + 12] * Math.Pow(2, ((int)(i / 12) - 4))));
                if (m > a)
                {
                    m = a;
                    n = i;
                }
            }
            return n;
        }

        public static int searchPCMNote(int ml)
        {
            int m = int.MaxValue;
            ml = ml % 1024;
            int n = 0;
            for (int i = 0; i < 12; i++)
            {
                int a = Math.Abs(ml - Tables.pcmpitchTbl[i]);
                if (m > a)
                {
                    m = a;
                    n = i;
                }
            }
            return n;
        }

        public static int searchYM2608Adpcm(float freq)
        {
            float m = float.MaxValue;
            int n = 0;

            for (int i = 0; i < 12 * 8; i++)
            {
                if (freq < Tables.pcmMulTbl[i % 12 + 12] * Math.Pow(2, ((int)(i / 12) - 3))) break;
                n = i;
                float a = Math.Abs(freq - (float)(Tables.pcmMulTbl[i % 12 + 12] * Math.Pow(2, ((int)(i / 12) - 3))));
                if (m > a)
                {
                    m = a;
                    n = i;
                }
            }

            return n + 1;
        }

        public static int GetYM2151Hosei(float YM2151ClockValue,float baseClock)
        {
            int ret = 0;

            float delta = (float)YM2151ClockValue / baseClock;
            float d;
            float oldD = float.MaxValue;
            for (int i = 0; i < Tables.pcmMulTbl.Length; i++)
            {
                d = Math.Abs(delta - Tables.pcmMulTbl[i]);
                ret = i;
                if (d > oldD) break;
                oldD = d;
            }
            ret -= 12;

            return ret;
        }

        public static string GetApplicationFolder()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(path))
            {
                path += path[path.Length - 1] == '\\' ? "" : "\\";
            }
            return path;
        }

        public static string GetApplicationDataFolder(bool make = false)
        {
            string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            fullPath = System.IO.Path.Combine(fullPath, "KumaApp", AssemblyTitle);
            if (!System.IO.Directory.Exists(fullPath)) System.IO.Directory.CreateDirectory(fullPath);

            return fullPath;
        }
    }

    public enum EnmModel
    {
        None
        , VirtualModel
        , RealModel
    }

    public enum EnmDevice : int
    {
        None = 0x00000000
        //VGM Chips(VGMで使用されるエミュレーションチップ定義)
        , SN76489 = 0x0000000C
        , YM2413 = 0x00000010
        , YM2612 = 0x0000002C
        , YM2151 = 0x00000030
        , SegaPCM = 0x00000038
        , RF5C68 = 0x00000040
        , YM2203 = 0x00000044
        , YM2608 = 0x00000048
        , YM2610 = 0x0000004C
        , YM2610B = 0x0000004C
        , YM3812 = 0x00000050
        , YM3526 = 0x00000054
        , Y8950 = 0x00000058
        , YMF262 = 0x0000005C
        , YMF278B = 0x00000060
        , YMF271 = 0x00000064
        , YMZ280B = 0x00000068
        , RF5C164 = 0x0000006C
        , PWM = 0x00000070
        , AY8910 = 0x00000074
        , GameBoyDMG = 0x00000080
        , NESAPU = 0x00000084
        , MultiPCM = 0x00000088
        , uPD7759 = 0x0000008C
        , OKIM6258 = 0x00000090
        , OKIM6295 = 0x00000098
        , K051649 = 0x0000009C
        , K054539 = 0x000000A0
        , HuC6280 = 0x000000A4
        , C140 = 0x000000A8
        , K053260 = 0x000000AC
        , Pokey = 0x000000B0
        , QSound = 0x000000B4
        , SCSP = 0x000000B8
        , WonderSwan = 0x000000C0
        , VirtualBoyVSU = 0x000000C4
        , SAA1099 = 0x000000C8
        , ES5503 = 0x000000CC
        , ES5505 = 0x000000D0
        , ES5506 = 0x000000D0
        , X1_010 = 0x000000D8
        , C352 = 0x000000DC
        , GA20 = 0x000000E0
        //OtherChips(仮想チップや他のエミュレーションコア)
        , OtherChips = 0x00010000
        , AY8910B = 0x00010001
        , YM2609 = 0x00010002
        //XG
        , MIDIXG = 0x00020000
        //GS
        , MIDIGS = 0x00030000
        //GM
        , MIDIGM = 0x00040000
        //VSTi
        , MIDIVSTi = 0x00050000
        //Wave
        , Wave = 0x00060000
    }

    public enum EnmDataType
    {
        None 
        , Normal 
        , Block 
        , Loop
        , FadeOut
    }

    public enum EnmChip : int
    {
        Unuse = 0

        , SN76489
        , YM2612
        , YM2612Ch6
        , RF5C164
        , PWM
        , C140
        , OKIM6258
        , OKIM6295
        , SEGAPCM
        , YM2151
        , YM2608
        , YM2203
        , YM2610
        , AY8910
        , HuC6280
        , YM2413
        , NES
        , DMC
        , FDS
        , MMC5
        , YMF262
        , YMF278B
        , VRC7
        , C352
        , YM3526
        , Y8950
        , YM3812
        , K051649
        , N160
        , VRC6
        , FME7
        , RF5C68
        , MultiPCM
        , YMF271
        , YMZ280B
        , QSound
        , GA20
        , K053260
        , K054539
        , DMG

        , S_SN76489
        , S_YM2612
        , S_YM2612Ch6
        , S_RF5C164
        , S_PWM
        , S_C140
        , S_OKIM6258
        , S_OKIM6295
        , S_SEGAPCM
        , S_YM2151
        , S_YM2608
        , S_YM2203
        , S_YM2610
        , S_AY8910
        , S_HuC6280
        , S_YM2413
        , S_NES
        , S_DMC
        , S_FDS
        , S_MMC5
        , S_YMF262
        , S_YMF278B
        , S_VRC7
        , S_C352
        , S_YM3526
        , S_Y8950
        , S_YM3812
        , S_K051649
        , S_N160
        , S_VRC6
        , S_FME7
        , S_RF5C68
        , S_MultiPCM
        , S_YMF271
        , S_YMZ280B
        , S_QSound
        , S_GA20
        , S_K053260
        , S_K054539
        , S_DMG
    }

    public enum EnmRealChipType : int
    {
        YM2608 = 1
        , YM2151 = 2
        , YM2610 = 3
        , YM2203 = 4
        , YM2612 = 5
        , AY8910 = 6
        , SN76489 = 7
        , YM2413 = 10
        , SPPCM = 42
        , C140 = 43
        , SEGAPCM = 44
    }

    public enum EnmInstFormat : int
    {
        FMP7 = 0,
        MDX = 1,
        TFI = 2,
        MUSICLALF = 3,
        MUSICLALF2 = 4,
        MML2VGM = 5,
        NRTDRV = 6,
        HUSIC=7
    }

    public enum EnmFileFormat : int
    {
        unknown = 0,
        VGM = 1,
        NRT = 2,
        XGM = 3,
        S98 = 4,
        MID = 5,
        RCP = 6,
        NSF = 7,
        HES = 8,
        ZIP = 9,
        M3U = 10,
        SID = 11,
        MDR = 12,
        LZH = 13,
        MDX = 14,
        MND = 15,
        MUB = 16,
        MUC = 17
    }

    public enum EnmArcType : int
    {
        unknown = 0,
        ZIP = 1,
        LZH = 2
    }

    public enum EnmRealModel
    {
        unknown,
        SCCI,
        GIMIC
    }


}
