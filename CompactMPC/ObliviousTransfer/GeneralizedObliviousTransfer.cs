﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public abstract class GeneralizedObliviousTransfer : IObliviousTransfer, IGeneralizedObliviousTransfer
    {
        public Task SendAsync(Stream stream, BitQuadruple[] options, int numberOfInvocations)
        {
            return SendAsync(stream, ToOptionMessages(options), numberOfInvocations, 1);
        }

        private Quadruple<byte[]>[] ToOptionMessages(BitQuadruple[] options)
        {
            Quadruple<byte[]>[] optionMessages = new Quadruple<byte[]>[options.Length];
            for (int i = 0; i < optionMessages.Length; ++i)
            {
                optionMessages[i] = new Quadruple<byte[]>(
                    new[] { (byte)options[i][0] },
                    new[] { (byte)options[i][1] },
                    new[] { (byte)options[i][2] },
                    new[] { (byte)options[i][3] }
                );
            }

            return optionMessages;
        }

        public Task<BitArray> ReceiveAsync(Stream stream, int[] selectionIndices, int numberOfInvocations)
        {
            return ReceiveAsync(stream, selectionIndices, numberOfInvocations, 1).ContinueWith(task => FromResultMessages(task.Result));
        }

        private BitArray FromResultMessages(byte[][] resultMessages)
        {
            BitArray result = new BitArray(resultMessages.Length);
            for (int i = 0; i < result.Length; ++i)
                result[i] = ((Bit)resultMessages[i][0]).Value;

            return result;
        }

        public abstract Task SendAsync(Stream stream, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        public abstract Task<byte[][]> ReceiveAsync(Stream stream, int[] selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
