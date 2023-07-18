using System.Numerics;

namespace CryptoEat.Modules.Crypto;

internal static class BytesToUtf
{
    private const int CheckSumSize = 4;
    private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly BigInteger Alg = 58;

    internal static string Encode(byte[] data)
    {
        return EncodePlain(AddCheckSum(data));
    }

    internal static string EncodePlain(byte[] data)
    {
        var intData = data.Aggregate(BigInteger.Zero, (current, t) => current * 256 + t);

        var result = string.Empty;
        while (intData > 0)
        {
            var remainder = (int) (intData % Alg);
            intData /= Alg;
            result = Digits[remainder] + result;
        }

        for (var i = 0; i < data.Length && data[i] == 0; i++) result = '1' + result;

        return result;
    }

    internal static byte[] Decode(string data)
    {
        var dataWithoutCheckSum = VerifyAndRemoveCheckSum(DecodePlain(data));

        return dataWithoutCheckSum ?? throw new FormatException("Checksum is invalid");
    }

    internal static byte[] DecodePlain(string data)
    {
        var intData = BigInteger.Zero;
        for (var i = 0; i < data.Length; i++)
        {
            var digit = Digits.IndexOf(data[i]);

            if (digit < 0) throw new FormatException($"Invalid character `{data[i]}` at position {i}");
            intData = intData * Alg + digit;
        }

        var leadingZeroCount = data.TakeWhile(c => c == '1').Count();
        var leadingZeros = Enumerable.Repeat((byte) 0, leadingZeroCount);
        var bytesWithoutLeadingZeros =
            intData.ToByteArray()
                .Reverse()
                .SkipWhile(b => b == 0);
        var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();

        return result;
    }

    private static byte[] AddCheckSum(byte[] data)
    {
        var checkSum = GetCheckSum(data);
        var dataWithCheckSum = ConcatArrays(data, checkSum);

        return dataWithCheckSum;
    }

    private static byte[]? VerifyAndRemoveCheckSum(byte[] data)
    {
        var result = data.AsSpan()[..^CheckSumSize].ToArray();
        var givenCheckSum = data.AsSpan()[result.Length..];
        var correctCheckSum = GetCheckSum(result);

        return givenCheckSum.SequenceEqual(correctCheckSum) ? result : null;
    }

    internal static byte[] GetCheckSum(IEnumerable<byte> bytes)
    {
        var crcTable = new uint[256];
        uint crc;

        for (uint i = 0; i < 256; i++)
        {
            crc = i;
            for (uint j = 0; j < 8; j++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0x8F6E37A0 : crc >> 1;

            crcTable[i] = crc;
        }

        crc = bytes.Aggregate(0xFFFFFFFF, (current, s) => crcTable[(current ^ s) & 0xFF] ^ (current >> 8));

        crc ^= 0xFFFFFFFF;
        return BitConverter.GetBytes(crc);
    }

    private static T[] ConcatArrays<T>(T[] arr1, T[] arr2)
    {
        var result = new T[arr1.Length + arr2.Length];
        Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
        Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);

        return result;
    }
}