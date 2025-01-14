﻿using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.SampleCircuits;
using CompactMPC.Util;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class SecureComputationTest
    {
        private static readonly BitArray[] Inputs =
        {
            BitArray.FromBinaryString("010101"),
            BitArray.FromBinaryString("110111"),
            BitArray.FromBinaryString("010111"),
            BitArray.FromBinaryString("110011"),
            BitArray.FromBinaryString("110111")
        };

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
            TestNetworkRunner.RunMultiPartyNetwork(
                numberOfParties,
                session => PerformSecureComputation(session, expectedOutput)
            );
        }

        private static void PerformSecureComputation(IMultiPartyNetworkSession session, BitArray expectedOutput)
        {
            BitArray localInput = Inputs[session.LocalParty.Id];

            IBitObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                new SecurityParameters(47, 23, 4, 1, 1)
            );

            IMultiplicativeSharing multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(
                obliviousTransfer
            );

            SecretSharingSecureComputation computation = new SecretSharingSecureComputation(
                session,
                multiplicativeSharing
            );

            SetIntersectionCircuitRecorder circuitRecorder = new SetIntersectionCircuitRecorder(
                session.NumberOfParties,
                localInput.Length
            );

            CircuitBuilder circuitBuilder = new CircuitBuilder();
            circuitRecorder.Record(circuitBuilder);

            ForwardCircuit circuit = new ForwardCircuit(circuitBuilder.CreateCircuit());
            BitArray actualOutput = computation
                .EvaluateAsync(circuit, circuitRecorder.InputMapping, circuitRecorder.OutputMapping, localInput)
                .Result;

            actualOutput.Should().Equal(expectedOutput);
        }
    }
}