using System;
using System.Linq;
using System.Text;

namespace NModbus.Extensions
{
    public static class NModbusHelper
    {
        private static bool _isLittleEndian = BitConverter.IsLittleEndian;

        public static UInt16 GetUInt16(ushort src)
        {
            return src;
        }

        public static Int16 GetInt16(ushort src)
        {
            return (Int16)src;
        }

        public static double[] GetMultiDouble(ushort[] src, DataFormat dataFormat = DataFormat.ABCDEFGH)
        {
            byte[] bytes = ToSequenceBytes(src);
            bytes = ToDataFormatBytes(bytes, dataFormat);

            double[] result = new double[src.Length / 4];

            for (int i = 0; i < bytes.Length; i += 8)
            {
                result[i / 8] = BitConverter.ToDouble(bytes, i);
            }
            return result.ToArray();
        }

        public static double GetDouble(ushort[] src, DataFormat dataFormat = DataFormat.ABCDEFGH)
        {
            return GetMultiDouble(src, dataFormat)[0];
        }

        public static float[] GetMultiFloat(ushort[] src, DataFormat dataFormat = DataFormat.ABCD)
        {
            byte[] bytes = ToSequenceBytes(src);
            bytes = ToDataFormatBytes(bytes, dataFormat);
            float[] result = new float[src.Length / 2];

            for (int i = 0; i < bytes.Length; i += 4)
            {
                result[i / 4] = BitConverter.ToSingle(bytes, i);
            }
            return result;
        }

        public static float GetFloat(ushort[] src, DataFormat dataFormat = DataFormat.ABCD)
        {
            return GetMultiFloat(src, dataFormat)[0];
        }

        public static Int32[] GetMultiInt32(ushort[] src, DataFormat dataFormat = DataFormat.ABCD)
        {
            byte[] bytes = ToSequenceBytes(src);
            bytes = ToDataFormatBytes(bytes, dataFormat);
            Int32[] result = new Int32[src.Length / 2];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                result[i / 4] = BitConverter.ToInt32(bytes, i);
            }
            return result;
        }

        public static Int32 GetInt32(ushort[] src, DataFormat dataFormat = DataFormat.ABCD)
        {
            return GetMultiInt32(src, dataFormat)[0];
        }

        public static UInt32[] GetMultiUInt32(ushort[] src, DataFormat dataFormat = DataFormat.ABCD)
        {
            byte[] bytes = ToSequenceBytes(src);
            bytes = ToDataFormatBytes(bytes, dataFormat);
            UInt32[] result = new UInt32[src.Length / 2];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                result[i / 4] = BitConverter.ToUInt32(bytes, i);
            }
            return result;
        }

        public static UInt32 GetUInt32(ushort[] src, DataFormat dataFormat = DataFormat.ABCD)
        {
            return GetMultiUInt32(src, dataFormat)[0];
        }

        public static string GetAsciiString(ushort[] src)
        {
            byte[] bytes = ToSequenceBytes(src);
            StringBuilder builder = new StringBuilder();
            foreach (var item in bytes)
            {
                builder.Append((Char)item);
            }
            return builder.ToString();
        }

        /// <summary>
        /// 返回CRC16 Modbus. 第一个字节是低位，第二个字节是高位
        /// </summary>
        /// <param name="bytes">可以对bytes或者其部分片段进行校验</param>
        /// <param name="startIndex">需要校验的字节片段在bytes中的起始索引</param>
        /// <param name="length">需要校验的字节的个数</param>
        /// <returns>第一个字节是低位，第二个字节是高位</returns>
        public static byte[] GetCrc16Modbus(byte[] bytes, int startIndex, int length)
        {
            byte[] CRC = new byte[2];

            UInt16 wCrc = 0xFFFF;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                wCrc ^= Convert.ToUInt16(bytes[i]);
                for (int j = 0; j < 8; j++)
                {
                    if ((wCrc & 0x0001) == 1)
                    {
                        wCrc >>= 1;
                        wCrc ^= 0xA001;//异或多项式
                    }
                    else
                    {
                        wCrc >>= 1;
                    }
                }
            }

            CRC[1] = (byte)((wCrc & 0xFF00) >> 8);//高位在后
            CRC[0] = (byte)(wCrc & 0x00FF);       //低位在前
            return CRC;
        }

        /// <summary>
        /// 返回CRC16 Modbus. 第一个字节是低位，第二个字节是高位
        /// </summary>
        /// <param name="bytes">待校验的字节数组</param>
        /// <returns>第一个字节是低位，第二个字节是高位</returns>
        public static byte[] GetCrc16Modbus(byte[] bytes)
        {
            return GetCrc16Modbus(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 返回CRC16 Modbus. 第一个字节是低位，第二个字节是高位
        /// </summary>
        /// <param name="bytes">对bytes中的字节片段进行校验，字节片段在bytes的起始索引默认是0</param>
        /// <param name="length">需要校验的字节的个数</param>
        /// <returns>第一个字节是低位，第二个字节是高位</returns>
        public static byte[] GetCrc16Modbus(byte[] bytes, int length)
        {
            return GetCrc16Modbus(bytes, 0, length);
        }

        /// <summary>
        /// 将一个保持寄存器或输入寄存器的字转换成2个字节
        /// </summary>
        /// <param name="src">字</param>
        /// <param name="high">高位</param>
        /// <param name="low">低位</param>
        private static void UInt16ToBytes(ushort src, out byte high, out byte low)
        {
            high = (byte)(src >> 8);
            low = (byte)(src >> 0);
        }

        /// <summary>
        /// 将读取到的多个连续的保持寄存器或输入寄存器的字转换成字节流，每个字高位在前，低位在后
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte[] ToSequenceBytes(ushort[] src)
        {
            byte[] bytes = new byte[src.Length * 2];
            for (int i = 0; i < src.Length; i++)
            {
                UInt16ToBytes(src[i], out byte high, out byte low);
                bytes[2 * i] = high;
                bytes[2 * i + 1] = low;
            }
            return bytes;
        }

        private static byte[] ToDataFormatBytes(byte[] src, DataFormat dataFormat)
        {
            if (src.Length == 0 || src == null)
            {
                throw new ArgumentException($"{nameof(src)}不能为NULL或空集合。");
            }
            if ((int)dataFormat < 5)
            {
                if (src.Length % 4 != 0)
                {
                    throw new ArgumentException($"{nameof(src)}的长度必须是4的整数倍");
                }
            }
            else
            {
                if (src.Length % 8 != 0)
                {
                    throw new ArgumentException($"{nameof(src)}的长度必须是8的整数倍");
                }
            }
            byte[] temp = new byte[8];

            switch (dataFormat)
            {
                case DataFormat.ABCDEFGH:
                    if (_isLittleEndian)
                    {
                        for (int i = 0; i < src.Length; i += 8)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            temp[4] = src[i + 4];
                            temp[5] = src[i + 5];
                            temp[6] = src[i + 6];
                            temp[7] = src[i + 7];
                            src[i + 0] = temp[7];
                            src[i + 1] = temp[6];
                            src[i + 2] = temp[5];
                            src[i + 3] = temp[4];
                            src[i + 4] = temp[3];
                            src[i + 5] = temp[2];
                            src[i + 6] = temp[1];
                            src[i + 7] = temp[0];
                        }
                    }
                    break;

                case DataFormat.GHEFCDAB:
                    if (_isLittleEndian)
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            temp[4] = src[i + 4];
                            temp[5] = src[i + 5];
                            temp[6] = src[i + 6];
                            temp[7] = src[i + 7];
                            src[i + 0] = temp[7];
                            src[i + 1] = temp[6];
                            src[i + 2] = temp[5];
                            src[i + 3] = temp[4];
                            src[i + 4] = temp[3];
                            src[i + 5] = temp[2];
                            src[i + 6] = temp[1];
                            src[i + 7] = temp[0];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            temp[4] = src[i + 4];
                            temp[5] = src[i + 5];
                            temp[6] = src[i + 6];
                            temp[7] = src[i + 7];
                            src[i + 0] = temp[6];
                            src[i + 1] = temp[7];
                            src[i + 2] = temp[4];
                            src[i + 3] = temp[5];
                            src[i + 4] = temp[2];
                            src[i + 5] = temp[3];
                            src[i + 6] = temp[0];
                            src[i + 7] = temp[1];
                        }
                    }
                    break;

                case DataFormat.BADCFEHG:
                    if (_isLittleEndian)
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            temp[4] = src[i + 4];
                            temp[5] = src[i + 5];
                            temp[6] = src[i + 6];
                            temp[7] = src[i + 7];
                            src[i + 0] = temp[6];
                            src[i + 1] = temp[7];
                            src[i + 2] = temp[4];
                            src[i + 3] = temp[5];
                            src[i + 4] = temp[2];
                            src[i + 5] = temp[3];
                            src[i + 6] = temp[0];
                            src[i + 7] = temp[1];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            temp[4] = src[i + 4];
                            temp[5] = src[i + 5];
                            temp[6] = src[i + 6];
                            temp[7] = src[i + 7];
                            src[i + 0] = temp[1];
                            src[i + 1] = temp[0];
                            src[i + 2] = temp[3];
                            src[i + 3] = temp[2];
                            src[i + 4] = temp[5];
                            src[i + 5] = temp[4];
                            src[i + 6] = temp[7];
                            src[i + 7] = temp[6];
                        }
                    }
                    break;

                case DataFormat.HGFEDCBA:
                    if (_isLittleEndian)
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            temp[4] = src[i + 4];
                            temp[5] = src[i + 5];
                            temp[6] = src[i + 6];
                            temp[7] = src[i + 7];
                            src[i + 0] = temp[0];
                            src[i + 1] = temp[1];
                            src[i + 2] = temp[2];
                            src[i + 3] = temp[3];
                            src[i + 4] = temp[4];
                            src[i + 5] = temp[5];
                            src[i + 6] = temp[6];
                            src[i + 7] = temp[7];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            temp[4] = src[i + 4];
                            temp[5] = src[i + 5];
                            temp[6] = src[i + 6];
                            temp[7] = src[i + 7];
                            src[i + 0] = temp[7];
                            src[i + 1] = temp[6];
                            src[i + 2] = temp[5];
                            src[i + 3] = temp[4];
                            src[i + 4] = temp[3];
                            src[i + 5] = temp[2];
                            src[i + 6] = temp[1];
                            src[i + 7] = temp[0];
                        }
                    }
                    break;

                case DataFormat.ABCD:
                    if (_isLittleEndian)
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            src[i + 0] = temp[3];
                            src[i + 1] = temp[2];
                            src[i + 2] = temp[1];
                            src[i + 3] = temp[0];
                        }
                    }
                    break;

                case DataFormat.CDAB:
                    if (_isLittleEndian)
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            src[i + 0] = temp[1];
                            src[i + 1] = temp[0];
                            src[i + 2] = temp[3];
                            src[i + 3] = temp[2];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            src[i + 0] = temp[2];
                            src[i + 1] = temp[3];
                            src[i + 2] = temp[0];
                            src[i + 3] = temp[1];
                        }
                    }
                    break;

                case DataFormat.BADC:
                    if (_isLittleEndian)
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            src[i + 0] = temp[2];
                            src[i + 1] = temp[3];
                            src[i + 2] = temp[0];
                            src[i + 3] = temp[1];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            src[i + 0] = temp[1];
                            src[i + 1] = temp[0];
                            src[i + 2] = temp[3];
                            src[i + 3] = temp[2];
                        }
                    }
                    break;

                case DataFormat.DCBA:
                    if (_isLittleEndian)
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            src[i + 0] = temp[0];
                            src[i + 1] = temp[1];
                            src[i + 2] = temp[2];
                            src[i + 3] = temp[3];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < src.Length; i += 4)
                        {
                            temp[0] = src[i];
                            temp[1] = src[i + 1];
                            temp[2] = src[i + 2];
                            temp[3] = src[i + 3];
                            src[i + 0] = temp[3];
                            src[i + 1] = temp[2];
                            src[i + 2] = temp[1];
                            src[i + 3] = temp[0];
                        }
                    }
                    break;
            }

            return src;
        }
    }

    public enum DataFormat
    {
        ABCD = 1,
        CDAB = 2,
        BADC = 3,
        DCBA = 4,
        ABCDEFGH = 5,
        GHEFCDAB = 6,
        BADCFEHG = 7,
        HGFEDCBA = 8
    }
}