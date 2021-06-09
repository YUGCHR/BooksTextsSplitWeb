using System.Collections.Generic;
using ConstantData.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Library.Models;

namespace ConstantDataTests.Services
{
    [TestClass()]
    public class OnKeysEventsSubscribeServiceTests
    {
        [TestMethod()]
        [DataRow(new int[] { 6, 300, 1000000, 2, 3, 2 }, new string[] { "RecordActualityLevel", "TaskEmulatorDelayTimeInMilliseconds", "RandomRangeExtended", "BalanceOfTasksAndProcesses", "MaxProcessesCountOnServer", "MinBackProcessesServersCount" }, new int[] { 6, 300, 1000000, 2, 3, 2 })]
        [DataRow(new int[] { 6, 300, 1000000, 2, 3, 2 }, new string[] { "RecordActualityLevel", "TaskEmulatorDelayTimeInMilliseconds", "RandomRangeExtended", "BalanceOfTasksAndProcesses", "MaxProcessesCountOnServer", "MinBackProcessesServersCount" }, new int[] { 8, 500, 1000001, 3, 5, 3 })]
        [DataRow(new int[] { 6, 300, 1000000, 2, 3, 2 }, new string[] { "RecordActualityLevel", "TaskEmulatorDelayTimeInMilliseconds", "RandomRangeExtended", "BalanceOfTasksAndProcesses", "MaxProcessesCountOnServer", "MinBackProcessesServersCount" }, new int[] { 6, 100, 1000000, 5, 3, 7 })]
        // до появления ошибочного имени обновление констант происходит
        [DataRow(new int[] { 6, 300, 1000000, 2, 3, 2 }, new string[] { "RecordActualityLevel", "TaskEmulatorDelayTimeInilliseconds", "RandomRangeExtended", "BalanceOfTasksAndProcesses", "MaxProcessesCountOnServer", "MinBackProcessesServersCount" }, new int[] { 7, 300, 1000000, 2, 3, 2 })]

        public void UpdatedValueAssignsToPropertyTest(int[] source, string[] updateKey, int[] updateValue)
        {
            // сделать вариант неполного заполнения словаря - 1-2 значения
            IDictionary<string, int> updatedConstants = new Dictionary<string, int>()
            {
                {updateKey[0], updateValue[0]},
                {updateKey[1], updateValue[1]},
                {updateKey[2], updateValue[2]},
                {updateKey[3], updateValue[3]},
                {updateKey[4], updateValue[4]},
                {updateKey[5], updateValue[5]},
            };

            ConstantsSet constantsSet = new ConstantsSet()
            {
                RecordActualityLevel = new ConstantType()
                {
                    Description = "Description",
                    Value = source[0],
                    LifeTime = -1
                },
                TaskEmulatorDelayTimeInMilliseconds = new ConstantType()
                {
                    Description = "Description",
                    Value = source[1],
                    LifeTime = -1
                },
                RandomRangeExtended = new ConstantType()
                {
                    Description = "Description",
                    Value = source[2],
                    LifeTime = -1
                },
                BalanceOfTasksAndProcesses = new ConstantType()
                {
                    Description = "Description",
                    Value = source[3],
                    LifeTime = -1
                },
                MaxProcessesCountOnServer = new ConstantType()
                {
                    Description = "Description",
                    Value = source[4],
                    LifeTime = -1
                },
                MinBackProcessesServersCount = new ConstantType()
                {
                    Description = "Description",
                    Value = source[5],
                    LifeTime = -1
                },
                FinalPropertyToSet = new KeyType()
                {
                    Description = "ConstantsSet.ConstantType.Value",
                    Value = "Value",
                    LifeTime = 0.999
                }
            };

            bool setWasUpdated;
            (setWasUpdated, constantsSet) = OnKeysEventsSubscribeService.UpdatedValueAssignsToProperty(constantsSet, updatedConstants);

            int[] result = new[]
            {
                constantsSet.RecordActualityLevel.Value,
                constantsSet.TaskEmulatorDelayTimeInMilliseconds.Value,
                constantsSet.RandomRangeExtended.Value,
                constantsSet.BalanceOfTasksAndProcesses.Value,
                constantsSet.MaxProcessesCountOnServer.Value,
                constantsSet.MinBackProcessesServersCount.Value
            };

            CollectionAssert.AreEqual(updateValue, result);
        }
    }
}