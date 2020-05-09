﻿using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.SampleCircuits;
using CompactMPC.UnitTests.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class SecureComputationTest
    {
        private static readonly BitArray[] Inputs = new[]
            {
                "010101",
                "110111",
                "010111",
                "110011",
                "110111"
            }.Select(BitArray.FromBinaryString).ToArray();

        [TestMethod]
        public void TestTwoPartySetIntersection()
        {
            RunSecureComputationParties(2, "010101110");
        }

        [TestMethod]
        public void TestThreePartySetIntersection()
        {
            RunSecureComputationParties(3, "010101110");
        }

        [TestMethod]
        public void TestFourPartySetIntersection()
        {
            RunSecureComputationParties(4, "010001010");
        }

        [TestMethod]
        public void TestFivePartySetIntersection()
        {
            RunSecureComputationParties(5, "010001010");
        }

        private static void RunSecureComputationParties(int numberOfParties, string expectedOutputString)
        {
            BitArray expectedOutput = BitArray.FromBinaryString(expectedOutputString);

            Task[] tasks = new Task[numberOfParties];
            for (int i = 0; i < numberOfParties; ++i)
            {
                int localPartyId = i;
                BitArray localInput = new BitArray(Inputs[localPartyId].ToArray());

                tasks[i] = Task.Factory.StartNew(
                    () => RunSecureComputationParty(numberOfParties, localPartyId, localInput, expectedOutput),
                    TaskCreationOptions.LongRunning
                );
            }

            Task.WaitAll(tasks);
        }

        private static void RunSecureComputationParty(int numberOfParties, int localPartyId, BitArray localInput, BitArray expectedOutput)
        {
            using IMultiPartyNetworkSession session = TestNetworkSession.EstablishMultiParty(localPartyId, numberOfParties);
            using CryptoContext cryptoContext = CryptoContext.CreateDefault();
            
            IObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                new SecurityParameters(47, 23, 4, 1, 1),
                cryptoContext
            );

            IMultiplicativeSharing multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(
                obliviousTransfer,
                cryptoContext
            );

            GMWSecureComputation computation = new GMWSecureComputation(session, multiplicativeSharing, cryptoContext);
                    
            SetIntersectionCircuitRecorder circuitRecorder = new SetIntersectionCircuitRecorder(numberOfParties, localInput.Length);
            CircuitBuilder circuitBuilder = new CircuitBuilder();
            circuitRecorder.Record(circuitBuilder);

            ForwardCircuit circuit = new ForwardCircuit(circuitBuilder.CreateCircuit());
            BitArray output = computation.EvaluateAsync(circuit, circuitRecorder.InputMapping, circuitRecorder.OutputMapping, localInput).Result;

            CollectionAssert.AreEqual(
                expectedOutput,
                output,
                "Incorrect output {0} (should be {1}).",
                output.ToBinaryString(),
                expectedOutput.ToBinaryString()
            );
        }
    }
}
