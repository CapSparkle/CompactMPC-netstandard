﻿using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class SecureComputationTest
    {
        [TestMethod]
        public void TestSetIntersection()
        {
            BitArray[] inputs = new string[]
            {
                "010101",
                "110111",
                "010111",
                "110011",
                "110111"
            }.Select(input => BitArray.FromBinaryString(input)).ToArray();

            int startPort = 12348;
            int numberOfParties = inputs.Length;
            int numberOfElements = inputs[0].Length;

            BitArray expectedOutput = inputs.Aggregate((left, right) => left & right);
            
            Task[] tasks = new Task[numberOfParties];
            for (int i = 0; i < numberOfParties; ++i)
            {
                int localPartyId = i;
                tasks[i] = Task.Factory.StartNew(
                    () => RunSecureComputationParty(startPort, numberOfParties, numberOfElements, localPartyId, inputs[localPartyId], expectedOutput),
                    TaskCreationOptions.LongRunning
                );
            }

            Task.WaitAll(tasks);
        }

        private static void RunSecureComputationParty(int startPort, int numberOfParties, int numberOfElements, int localPartyId, BitArray localInput, BitArray expectedOutput)
        {
            using (TcpMultiPartyNetworkSession session = new TcpMultiPartyNetworkSession(startPort, numberOfParties, localPartyId))
            {
                using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
                {
                    IObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                        new SecurityParameters(47, 23, 4, 1, 1),
                        cryptoContext
                    );

                    IPairwiseMultiplicationScheme multiplicationScheme = new ObliviousTransferMultiplicationScheme(
                        obliviousTransfer,
                        cryptoContext
                    );

                    GMWSecureComputation computation = new GMWSecureComputation(session, multiplicationScheme, cryptoContext);
                    
                    SetIntersectionCircuitRecorder circuitRecorder = new SetIntersectionCircuitRecorder(numberOfParties, numberOfElements);
                    CircuitBuilder circuitBuilder = new CircuitBuilder();
                    circuitRecorder.Record(circuitBuilder);

                    ForwardCircuit circuit = new ForwardCircuit(circuitBuilder.CreateCircuit());

                    BitArray output = computation.EvaluateAsync(circuit, circuitRecorder.InputMapping, circuitRecorder.OutputMapping, localInput).Result;

                    Assert.AreEqual(
                        expectedOutput.Length,
                        output.Length,
                        "Incorrect output length {0} (should be {1}).",
                        output.Length,
                        expectedOutput.Length
                    );

                    Assert.IsTrue(
                        Enumerable.SequenceEqual(expectedOutput, output),
                        "Incorrect output {0} (should be {1}).",
                        output.ToBinaryString(),
                        expectedOutput.ToBinaryString()
                    );
                }
            }
        }
    }
}
