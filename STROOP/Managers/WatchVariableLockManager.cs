﻿using STROOP.Controls;
using STROOP.Managers;
using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STROOP.Structs
{
    public static class WatchVariableLockManager
    {
        private static List<WatchVariableLock> _lockList = new List<WatchVariableLock>();

        public static void AddLocks(WatchVariable variable, List<uint> addresses = null)
        {
            List<WatchVariableLock> newLocks = variable.GetLocks(addresses);
            foreach (WatchVariableLock newLock in newLocks)
            {
                if (!_lockList.Contains(newLock)) _lockList.Add(newLock);
            }
        }

        public static void RemoveLocks(WatchVariable variable, List<uint> addresses = null)
        {
            List<WatchVariableLock> newLocks = variable.GetLocks(addresses);
            foreach (WatchVariableLock newLock in newLocks)
            {
                _lockList.Remove(newLock);
            }
        }

        public static bool ContainsLocksBool(WatchVariable variable, List<uint> addresses = null)
        {
            return ContainsLocksCheckState(variable, addresses) != CheckState.Unchecked;
        }

        public static CheckState ContainsLocksCheckState(
            WatchVariable variable, List<uint> addresses = null)
        {
            if (!ContainsAnyLocks()) return CheckState.Unchecked;
            List<WatchVariableLock> newLocks = variable.GetLocks(addresses);

            if (newLocks.Count == 0) return CheckState.Unchecked;
            CheckState firstCheckState =
                _lockList.Contains(newLocks[0]) ? CheckState.Checked : CheckState.Unchecked;
            for (int i = 1; i < newLocks.Count; i++)
            {
                CheckState checkState =
                    _lockList.Contains(newLocks[i]) ? CheckState.Checked : CheckState.Unchecked;
                if (checkState != firstCheckState) return CheckState.Indeterminate;
            }
            return firstCheckState;
        }

        public static List<object> GetExistingLockValues(
            WatchVariable variable, List<uint> addresses = null)
        {
            if (LockConfig.LockingDisabled) return null;
            // don't get the locks with values, or there's a stack overflow error
            List<WatchVariableLock> locks = variable.GetLocksWithoutValues(addresses);
            List<object> returnValues = new List<object>();
            foreach (WatchVariableLock lok in locks)
            {
                WatchVariableLock existingLock = _lockList.FirstOrDefault(l => l.Equals(lok));
                object value = existingLock?.Value;
                returnValues.Add(value);
            }
            return returnValues;
        }

        public static void UpdateLockValues(
            WatchVariable variable, object newValue, List<uint> addresses = null)
        {
            if (LockConfig.LockingDisabled) return;
            if (!ContainsAnyLocks()) return;
            List<WatchVariableLock> newLocks = variable.GetLocks(addresses);
            foreach (WatchVariableLock newLock in newLocks)
            {
                foreach (WatchVariableLock currentLock in _lockList)
                {
                    if (currentLock.Equals(newLock))
                    {
                        currentLock.UpdateLockValue(newValue);
                    }
                }
            }
        }

        public static void UpdateLockValues(
            WatchVariable variable, List<object> newValues, List<uint> addresses = null)
        {
            if (LockConfig.LockingDisabled) return;
            if (!ContainsAnyLocks()) return;
            List<WatchVariableLock> newLocks = variable.GetLocks(addresses);
            for (int i = 0; i < newLocks.Count; i++)
            {
                if (newValues[i] == null) continue;
                foreach (WatchVariableLock currentLock in _lockList)
                {
                    if (currentLock.Equals(newLocks[i]))
                    {
                        currentLock.UpdateLockValue(newValues[i]);
                    }
                }
            }
        }

        public static void UpdateMemoryLockValue(
            object newValue, uint address, Type type, uint? mask, int? shift)
        {
            if (LockConfig.LockingDisabled) return;
            if (!ContainsAnyLocks()) return;
            foreach (WatchVariableLock currentLock in _lockList)
            {
                if (currentLock.EqualsMemorySignature(address, type, mask, shift))
                {
                    currentLock.UpdateLockValue(newValue);
                }
            }
        }

        public static void RemoveAllLocks()
        {
            _lockList.Clear();
        }

        public static bool ContainsAnyLocks()
        {
            return _lockList.Count > 0;
        }

        public static void Update()
        {
            if (LockConfig.LockingDisabled) return;
            bool shouldSuspend = _lockList.Count >= 2;
            if (shouldSuspend) Config.Stream.Suspend();
            _lockList.ForEach(varLock => varLock.Invoke());
            if (shouldSuspend) Config.Stream.Resume();
        }

    };
}
