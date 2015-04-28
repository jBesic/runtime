// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public static class BarrierCancellationTests
    {
        [Fact]
        public static void BarrierCancellationTestsCancelBeforeWait()
        {
            Barrier barrier = new Barrier(3);

            CancellationTokenSource cs = new CancellationTokenSource();
            cs.Cancel();
            CancellationToken ct = cs.Token;

            const int millisec = 100;
            TimeSpan timeSpan = new TimeSpan(100);

            EnsureOperationCanceledExceptionThrown(
               () => barrier.SignalAndWait(ct), ct,
               "CancelBeforeWait:  An OCE should have been thrown.");
            EnsureOperationCanceledExceptionThrown(
               () => barrier.SignalAndWait(millisec, ct), ct,
               "CancelBeforeWait:  An OCE should have been thrown.");
            EnsureOperationCanceledExceptionThrown(
               () => barrier.SignalAndWait(timeSpan, ct), ct,
               "CancelBeforeWait:  An OCE should have been thrown.");

            barrier.Dispose();
        }

        [Fact]
        public static void BarrierCancellationTestsCancelAfterWait_Negative()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            const int numberParticipants = 3;
            Barrier barrier = new Barrier(numberParticipants);

            Task.Factory.StartNew(() => cancellationTokenSource.Cancel());

            //Now wait.. the wait should abort and an exception should be thrown
            EnsureOperationCanceledExceptionThrown(
               () => barrier.SignalAndWait(cancellationToken),
               cancellationToken,
               "CancelAfterWait:  An OCE(null) should have been thrown that references the cancellationToken.");

            // the token should not have any listeners.
            // currently we don't expose this.. but it was verified manually
        }

        [Fact]
        public static void BarrierCancellationTestsCancelAfterWait()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            const int numberParticipants = 3;
            Barrier barrier = new Barrier(numberParticipants);

            Task.Factory.StartNew(() => cancellationTokenSource.Cancel());

            //Test that backout occured.
            Assert.Equal(numberParticipants, barrier.ParticipantsRemaining);

            // the token should not have any listeners.
            // currently we don't expose this.. but it was verified manually
        }

        private static void EnsureOperationCanceledExceptionThrown(Action action, CancellationToken token, string message)
        {
            OperationCanceledException operationCanceledEx =
                Assert.Throws<OperationCanceledException>(action);

            if (operationCanceledEx.CancellationToken != token)
            {
                Assert.True(false, string.Format("BarrierCancellationTests: Failed.  " + message));
            }
        }
    }
}

