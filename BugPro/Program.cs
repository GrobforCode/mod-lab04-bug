using Stateless;
using System;

namespace BugPro
{
    public enum BugState
    {
        New,           // Новый дефект
        Triage,        // Разбор дефектов
        Fixing,        // Исправление
        Verification,  // Проверка исправления
        Closed,        // Закрытие
        Reopened       // Переоткрытие
    }

    public enum BugTrigger
    {
        AssignToTriage,
        NoTimeNow,
        SeparateSolution,
        OtherProduct,
        NeedMoreInfo,
        NotDefect,
        NotFix,
        Duplicate,
        NotReproducible,
        StartFix,
        FixCompleted,
        VerifyOk,
        VerifyNotOk
    }

    public class Bug
    {
        private readonly StateMachine<BugState, BugTrigger> _machine;
        private BugState _state;

        public BugState CurrentState => _state;

        // Действия для логирования/консоли
        public event Action<string> OnTransition;

        public Bug()
        {
            _machine = new StateMachine<BugState, BugTrigger>(BugState.New);
            _state = BugState.New;

            // Настройка автомата
            ConfigureMachine();
        }

        private void ConfigureMachine()
        {
            // Начальное состояние New
            _machine.Configure(BugState.New)
                .Permit(BugTrigger.AssignToTriage, BugState.Triage);

            // Состояние Triage
            _machine.Configure(BugState.Triage)
                .Permit(BugTrigger.StartFix, BugState.Fixing)
                .Permit(BugTrigger.NotDefect, BugState.Closed)
                .Permit(BugTrigger.NotFix, BugState.Closed)
                .Permit(BugTrigger.Duplicate, BugState.Closed)
                .Permit(BugTrigger.NotReproducible, BugState.Closed)
                // Информационные триггеры - остаются в Triage
                .Ignore(BugTrigger.NoTimeNow)
                .Ignore(BugTrigger.SeparateSolution)
                .Ignore(BugTrigger.OtherProduct)
                .Ignore(BugTrigger.NeedMoreInfo);

            // Состояние Fixing
            _machine.Configure(BugState.Fixing)
                .Permit(BugTrigger.FixCompleted, BugState.Verification);

            // Состояние Verification
            _machine.Configure(BugState.Verification)
                .Permit(BugTrigger.VerifyOk, BugState.Closed)
                .Permit(BugTrigger.VerifyNotOk, BugState.Reopened);

            // Состояние Reopened
            _machine.Configure(BugState.Reopened)
                .Permit(BugTrigger.StartFix, BugState.Fixing);

            // Состояние Closed - конечное, никаких переходов (кроме гипотетического Reopen, но не требуется)
            _machine.Configure(BugState.Closed)
                .Ignore(BugTrigger.AssignToTriage); // можно игнорировать
        }

        public void Fire(BugTrigger trigger)
        {
            try
            {
                _machine.Fire(trigger);
                _state = _machine.State;
                OnTransition?.Invoke($"Переход по триггеру {trigger} -> новое состояние {_state}");
            }
            catch (Exception ex)
            {
                OnTransition?.Invoke($"Ошибка: {ex.Message}");
                throw;
            }
        }

        public bool CanFire(BugTrigger trigger) => _machine.CanFire(trigger);
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Демонстрация работы автомата бага ===\n");
            var bug = new Bug();
            bug.OnTransition += msg => Console.WriteLine(msg);

            Console.WriteLine($"Начальное состояние: {bug.CurrentState}");

            // Пример сценария
            bug.Fire(BugTrigger.AssignToTriage);
            bug.Fire(BugTrigger.StartFix);
            bug.Fire(BugTrigger.FixCompleted);
            bug.Fire(BugTrigger.VerifyOk);

            Console.WriteLine($"\nКонечное состояние: {bug.CurrentState}");
        }
    }
}