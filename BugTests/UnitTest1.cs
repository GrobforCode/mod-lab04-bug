using Microsoft.VisualStudio.TestTools.UnitTesting;
using BugPro;

namespace BugTests
{
    [TestClass]
    public class BugStateMachineTests
    {
        private Bug _bug;

        [TestInitialize]
        public void Setup()
        {
            _bug = new Bug();
        }

        [TestMethod]
        public void InitialState_ShouldBeNew()
        {
            Assert.AreEqual(BugState.New, _bug.CurrentState);
        }

        [TestMethod]
        public void AssignToTriage_FromNew_ShouldGoToTriage()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            Assert.AreEqual(BugState.Triage, _bug.CurrentState);
        }

        [TestMethod]
        public void StartFix_FromTriage_ShouldGoToFixing()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.StartFix);
            Assert.AreEqual(BugState.Fixing, _bug.CurrentState);
        }

        [TestMethod]
        public void FixCompleted_FromFixing_ShouldGoToVerification()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.StartFix);
            _bug.Fire(BugTrigger.FixCompleted);
            Assert.AreEqual(BugState.Verification, _bug.CurrentState);
        }

        [TestMethod]
        public void VerifyOk_FromVerification_ShouldGoToClosed()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.StartFix);
            _bug.Fire(BugTrigger.FixCompleted);
            _bug.Fire(BugTrigger.VerifyOk);
            Assert.AreEqual(BugState.Closed, _bug.CurrentState);
        }

        [TestMethod]
        public void VerifyNotOk_FromVerification_ShouldGoToReopened()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.StartFix);
            _bug.Fire(BugTrigger.FixCompleted);
            _bug.Fire(BugTrigger.VerifyNotOk);
            Assert.AreEqual(BugState.Reopened, _bug.CurrentState);
        }

        [TestMethod]
        public void Reopened_ThenStartFix_ShouldGoToFixing()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.StartFix);
            _bug.Fire(BugTrigger.FixCompleted);
            _bug.Fire(BugTrigger.VerifyNotOk);
            Assert.AreEqual(BugState.Reopened, _bug.CurrentState);
            _bug.Fire(BugTrigger.StartFix);
            Assert.AreEqual(BugState.Fixing, _bug.CurrentState);
        }

        [TestMethod]
        public void NotDefect_FromTriage_ShouldGoToClosed()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.NotDefect);
            Assert.AreEqual(BugState.Closed, _bug.CurrentState);
        }

        [TestMethod]
        public void Duplicate_FromTriage_ShouldGoToClosed()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.Duplicate);
            Assert.AreEqual(BugState.Closed, _bug.CurrentState);
        }

        [TestMethod]
        public void CannotStartFix_FromNew_WithoutTriage()
        {
            Assert.IsFalse(_bug.CanFire(BugTrigger.StartFix));
            Assert.ThrowsException<InvalidOperationException>(() => _bug.Fire(BugTrigger.StartFix));
        }

        [TestMethod]
        public void CannotVerifyOk_FromFixing()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.StartFix);
            Assert.IsFalse(_bug.CanFire(BugTrigger.VerifyOk));
            Assert.ThrowsException<InvalidOperationException>(() => _bug.Fire(BugTrigger.VerifyOk));
        }

        [TestMethod]
        public void InformationalTriggers_StayInTriage()
        {
            _bug.Fire(BugTrigger.AssignToTriage);
            _bug.Fire(BugTrigger.NoTimeNow);
            Assert.AreEqual(BugState.Triage, _bug.CurrentState);
            _bug.Fire(BugTrigger.NeedMoreInfo);
            Assert.AreEqual(BugState.Triage, _bug.CurrentState);
        }
    }
}