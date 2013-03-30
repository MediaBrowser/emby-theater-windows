using System;
using System.Collections.Generic;
using System.Text;

namespace BDInfo
{
    public abstract class TSCodecAVC
    {
        public static void Scan(
            TSVideoStream stream,
            TSStreamBuffer buffer,
            ref string tag)
        {
            uint parse = 0;
            byte accessUnitDelimiterParse = 0;
            byte sequenceParameterSetParse = 0;
            string profile = null;
            string level = null;
            byte constraintSet0Flag = 0;
            byte constraintSet1Flag = 0;
            byte constraintSet2Flag = 0;
            byte constraintSet3Flag = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                parse = (parse << 8) + buffer.ReadByte();

                if (parse == 0x00000109)
                {
                    accessUnitDelimiterParse = 1;
                }
                else if (accessUnitDelimiterParse > 0)
                {
                    --accessUnitDelimiterParse;
                    if (accessUnitDelimiterParse == 0)
                    {
                        switch ((parse & 0xFF) >> 5)
                        {
                            case 0: // I
                            case 3: // SI
                            case 5: // I, SI
                                tag = "I";
                                break;

                            case 1: // I, P
                            case 4: // SI, SP
                            case 6: // I, SI, P, SP
                                tag = "P";
                                break;

                            case 2: // I, P, B
                            case 7: // I, SI, P, SP, B
                                tag = "B";
                                break;
                        }
                        if (stream.IsInitialized) return;
                    }
                }
                else if (parse == 0x00000127 || parse == 0x00000167)
                {
                    sequenceParameterSetParse = 3;
                }
                else if (sequenceParameterSetParse > 0)
                {
                    --sequenceParameterSetParse;
                    switch (sequenceParameterSetParse)
                    {
                        case 2:
                            switch (parse & 0xFF)
                            {
                                case 66:
                                    profile = "Baseline Profile";
                                    break;
                                case 77:
                                    profile = "Main Profile";
                                    break;
                                case 88:
                                    profile = "Extended Profile";
                                    break;
                                case 100:
                                    profile = "High Profile";
                                    break;
                                case 110:
                                    profile = "High 10 Profile";
                                    break;
                                case 122:
                                    profile = "High 4:2:2 Profile";
                                    break;
                                case 144:
                                    profile = "High 4:4:4 Profile";
                                    break;
                                default:
                                    profile = "Unknown Profile";
                                    break;
                            }
                            break;

                        case 1:
                            constraintSet0Flag = (byte)
                                ((parse & 0x80) >> 7);
                            constraintSet1Flag = (byte)
                                ((parse & 0x40) >> 6);
                            constraintSet2Flag = (byte)
                                ((parse & 0x20) >> 5);
                            constraintSet3Flag = (byte)
                                ((parse & 0x10) >> 4);
                            break;

                        case 0:
                            byte b = (byte)(parse & 0xFF);
                            if (b == 11 && constraintSet3Flag == 1)
                            {
                                level = "1b";
                            }
                            else
                            {
                                level = string.Format(
                                    "{0:D}.{1:D}",
                                    b / 10, (b - ((b / 10) * 10)));
                            }
                            stream.EncodingProfile = string.Format(
                                "{0} {1}", profile, level);
                            stream.IsVBR = true;
                            stream.IsInitialized = true;
                            break;
                    }
                }
            }
            return;
        }
    }
}
