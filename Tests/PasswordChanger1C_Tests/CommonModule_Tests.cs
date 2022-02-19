using PasswordChanger1C;
using System;
using Xunit;

namespace PasswordChanger1C.Tests
{
    using static CommonModule;

    public class CommonModule_Tests
    {
        private static readonly byte[] Test_Data = FromBase64(@"
                GPJBHWrITsJOq25wcpiLMoP5YTI4Nr85ER36ohH8L/YozllGS7W+BeebTAYNBtoUKMF5eUfwLaB/mApBRa2+AbrVQ2dLU80
                bPdCuotfqYuAKzh0TALqnArPJUQIIBo8UIcJxLUf4fvJ+hl5AQqimArPJUQIIBo8JIcJxMWfCNfNinwpCSqDpAOfUUFNaBZI
                NIMF4MAuqLaFjnwgSF6+6ULvMBQdaS5Nad8F1f1ipe+92nV5GX6zpB7rUAwYLUJIOd8cjfFrxK/J+mFxcQ7S6Hq/JTQIUFPF
                ufqgKLgGcPYc2/jhAQsH8XbK+VFhUY/RyYs9jMUiGGa0U4F0bJuvOStavUQJhQdAIVscrcT+DBbFziUJCXqmnALPLUAIABo0
                IKcdyKV/kfu5+h2N6Caj2HrLVbDhDBsIVIN5xMVvkbOAz"
        );

        private const string Test_DataStr =
            "﻿{4a4fe769-57db-450e-938d-8cb13d175539,\"User\",\"�\",\"Descr\",00000000-0000-0000-0000-000000000000,\r\n" +
            "{1,4d288b2d-1ab3-4139-abcc-4fbe71b85d5b},cf34b2a5-8606-4b59-b43f-7f5ba09e0032,1,1,,0,0," +
            "\"NWoZK3kTsExUV00Ywo1G5jlUKKs=\",\"NWoZK3kTsExUV00Ywo1G5jlUKKs=\",2,1,20210802185345,0,0,\r\n{0},1,\r\n{0},1,0,1,\"\"}\0";

        private static readonly byte[] Test_Key = FromBase64("8kEdashOwk6rbnBymIsyg/lhMjg2vzkR");

        private static byte[] FromBase64(in string data)
        {
            var replaces = "\n,\r, ,\t".Split(',');
            string base64str = data;
            foreach (var replace in replaces)
            {
                base64str = base64str.Replace(replace, "");
            }
            return Convert.FromBase64String(base64str);
        }

        [Theory()]
        [InlineData("", "2jmj7l5rSw0yVb/vlWAYkK/YBwk=")]
        [InlineData("1", "NWoZK3kTsExUV00Ywo1G5jlUKKs=")]
        [InlineData("test1", "tESsBmE/yNY3lb6a0L6vVQEZNqw=")]
        [InlineData("TEST1", "/+tKGP8cN+WSkMhrkt8o9l21hNk=")]
        [InlineData("TeSt1", "X5pltlMitHnQjMO9ZteC3800pf8=")]
        [InlineData("234234234SDFsrew$%2", "oIWJb4uuvo46fYk0GLHAhcEM1is=")]
        public void EncryptStringSHA1_Test(string input, string hash)
        {
            Assert.Equal(hash, EncryptStringSHA1(input));
        }

        [Theory()]
        [InlineData("2jmj7l5rSw0yVb/vlWAYkK/YBwk=", true)]
        [InlineData("/+tKGP8cN+WSkMhrkt8o9l21hNk=", true)]
        [InlineData("2jmj7l5r'wyVb/vYkK/YBwk=", false)]
        [InlineData("2jmj7l5r'wyVb/vlWAYksdfsdf345sfdK/YBwk=", false)]
        [InlineData("2jm j7l5r'wyVb /vl WAYkK/YBwk=", false)]
        [InlineData("2jmj7l5r'w\tyVb/vlWAYkK/YBwk=", false)]
        [InlineData("2jmj7l5r'wyVb/vlWAYk\rK/YBwk=", false)]
        [InlineData("2jmj7l\n5r'wyVb/vWAYk4K/YBwk=", false)]
        [InlineData("2jmj,l5rSw0yVb/vlWAY,K/YBwk=", false)]
        [InlineData("2jmj7l5'Sw0yVb/vlWAYkK/YBwk=", false)]
        [InlineData("", false)]
        [InlineData("X5pltlMitHnQjMO9ZteC3800pf8=", true)]
        [InlineData("oIWJb4uuvo46fYk0GLHAhcEM1is=", true)]
        public void IsPasswordHash_Test(string input, bool is_valid)
        {
            Assert.Equal(is_valid, IsPasswordHash(input));
        }

        [Fact()]
        public void EncodePasswordStructure_Test()
        {
            byte[] result = EncodePasswordStructure(Test_DataStr, Test_Key.Length, Test_Key);
            Assert.Equal(result, Test_Data);
        }

        [Fact()]
        public void DecodePasswordStructure_Test()
        {
            int KeySize = 0;
            byte[] KeyData = null;
            string result = DecodePasswordStructure(Test_Data, ref KeySize, ref KeyData);
            Assert.Equal(result, Test_DataStr);
            Assert.Equal(KeySize, Test_Key.Length);
            Assert.Equal(KeyData, Test_Key);
        }

        [Fact()]
        public void GeneratePasswordHashes_Test()
        {
            var Hashes = GeneratePasswordHashes("teST1");
            Assert.Equal("IgULlSR7RU6pXmqYPKARyoXwr+k=", Hashes.Item1);
            Assert.Equal("/+tKGP8cN+WSkMhrkt8o9l21hNk=", Hashes.Item2);
        }

        [Fact()]
        public void GetPasswordHashTuple_Test()
        {
            var AuthStructure = ParserServices.ParsesClass.ParseString(Test_DataStr)[0];
            var Hashes = GetPasswordHashTuple(AuthStructure);
            Assert.Equal("NWoZK3kTsExUV00Ywo1G5jlUKKs=", Hashes.Item1);
            Assert.Equal("NWoZK3kTsExUV00Ywo1G5jlUKKs=", Hashes.Item2);
        }

        [Fact()]
        public void ReplaceHashes_Test()
        {
            var OldHashes = Tuple.Create("NWoZK3kTsExUV00Ywo1G5jlUKKs=", "NWoZK3kTsExUV00Ywo1G5jlUKKs=");
            var NewHashes = Tuple.Create("IgULlSR7RU6pXmqYPKARyoXwr+k=", "/+tKGP8cN+WSkMhrkt8o9l21hNk=");

            string OldHashesStr = "\"NWoZK3kTsExUV00Ywo1G5jlUKKs=\",\"NWoZK3kTsExUV00Ywo1G5jlUKKs=\"";
            string NewHashesStr = "\"IgULlSR7RU6pXmqYPKARyoXwr+k=\",\"/+tKGP8cN+WSkMhrkt8o9l21hNk=\"";
            string Expected = Test_DataStr.Replace(OldHashesStr, NewHashesStr);

            Assert.Equal(Expected, ReplaceHashes(Test_DataStr, OldHashes, NewHashes));
        }

        [Theory()]
        [InlineData("B", 222, 1, 223)]
        [InlineData("B", 222, 0, 222)]
        [InlineData("N", 222, 1, 113)]
        [InlineData("NC", 222, 1, 445)]
        [InlineData("NVC", 222, 1, 447)]
        [InlineData("RV", 222, 1, 17)]
        [InlineData("I", 222, 1, 9)]
        [InlineData("T", 222, 1, 9)]
        [InlineData("DT", 222, 1, 8)]
        [InlineData("NT", 222, 1, 9)]
        public void GetFieldSize_Test(string FieldType, int FieldLength, int CouldBeNull, int ExpectedResult)
        {
            var Field = new AccessFunctions.TableFields
            {
                Type = FieldType,
                Length = FieldLength,
                CouldBeNull = CouldBeNull
            };
            Assert.Equal(ExpectedResult, GetFieldSize(Field));
        }
    }
}
