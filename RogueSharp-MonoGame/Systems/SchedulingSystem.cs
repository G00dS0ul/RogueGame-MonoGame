using RogueSharp_MonoGame.Interfaces;

namespace RogueSharp_MonoGame.Systems
{
    public class SchedulingSystem
    {
        #region Backing Variable

        private int _time;
        private readonly SortedDictionary<int, List<ISchedulable>> _schedulables;

        #endregion


        public SchedulingSystem()
        {
            _time = 0;
            _schedulables = new SortedDictionary<int, List<ISchedulable>>();
        }

        #region Public Methods

        public void Add(ISchedulable schedulable)
        {
            var key = _time + schedulable.Time;
            if (!_schedulables.ContainsKey(key))
            {
                _schedulables.Add(key, []);
            }

            _schedulables[key].Add(schedulable);
        }

        public void Remove(ISchedulable schedulable)
        {
            var schedulableListFound =
                new KeyValuePair<int, List<ISchedulable>>(-1, null);

            foreach (var schedulableList in _schedulables)
            {
                if (schedulableList.Value.Contains(schedulable))
                {
                    schedulableListFound = schedulableList;
                    break;
                }
            }

            if (schedulableListFound.Value != null)
            {
                schedulableListFound.Value.Remove(schedulable);
                if (schedulableListFound.Value.Count <= 0)
                {
                    _schedulables.Remove(schedulableListFound.Key);
                }
            }
        }

        public ISchedulable Get()
        {
            var firstSchedulableGroup = _schedulables.First();
            var firstSchedulable = firstSchedulableGroup.Value.First();
            Remove(firstSchedulable);
            _time = firstSchedulableGroup.Key;
            return firstSchedulable;
        }

        public bool HasItems()
        {
            return _schedulables.Count > 0;
        }

        public int GetTime()
        {
            return _time;
        }

        public void Clear()
        {
            _time = 0;
            _schedulables.Clear();
        }

        #endregion


    }
}
