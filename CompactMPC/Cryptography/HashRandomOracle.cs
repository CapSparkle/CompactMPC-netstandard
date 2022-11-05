﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace CompactMPC.Cryptography
{
    public class HashRandomOracle : RandomOracle
    {
        public override IEnumerable<byte> Invoke(byte[] query)
        {
            byte[] seed = SHA256.HashData(query);

            using (MemoryStream stream = new MemoryStream(seed.Length + 4))
            {
                stream.Write(seed, 0, seed.Length);

                int counter = 0;
                while (counter < int.MaxValue)
                {
                    stream.Position = seed.Length;
                    stream.Write(BitConverter.GetBytes(counter), 0, 4);
                    stream.Position = 0;

                    byte[] block = SHA256.HashData(stream.GetBuffer());

                    foreach (byte blockByte in block)
                        yield return blockByte;

                    counter++;
                }
            }

            throw new InvalidOperationException("Random oracle cannot provide more data since the counter has reached its maximum value.");
        }
    }
}
