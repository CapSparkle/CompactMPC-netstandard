﻿using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using CompactMPC.Cryptography;

namespace CompactMPC.UnitTests.Cryptography
{
    public class ConstantRandomOracle : RandomOracle, IDisposable
    {
        private byte[] _invokeResponse;

        public ConstantRandomOracle(byte[] invokeResponse)
        {
            _invokeResponse = invokeResponse;
        }

        public override IEnumerable<byte> Invoke(byte[] query)
        {
            return _invokeResponse;
        }

        public override void Dispose() { }
    }
}