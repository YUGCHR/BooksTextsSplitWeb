using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using CachingFramework.Redis.Contracts;
using CachingFramework.Redis.Contracts.Providers;
using Shared.Library.Models;
using Shared.Library.Services;

namespace ConstantData.Services
{
    public interface IOnKeysEventsSubscribeService
    {
        public void SubscribeOnEventUpdate(ConstantsSet constantsSet, string constantsStartGuidField, CancellationToken stoppingToken);
    }

    public class OnKeysEventsSubscribeService : IOnKeysEventsSubscribeService
    {
        private readonly ICacheManageService _cache;
        private readonly IKeyEventsProvider _keyEvents;

        public OnKeysEventsSubscribeService(
            IKeyEventsProvider keyEvents, ICacheManageService cache)
        {
            _keyEvents = keyEvents;
            _cache = cache;
        }

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<OnKeysEventsSubscribeService>();

        private bool _flagToBlockEventUpdate;

        // подписываемся на ключ сообщения о появлении обновления констант
        public void SubscribeOnEventUpdate(ConstantsSet constantsSet, string constantsStartGuidField, CancellationToken stoppingToken)
        {
            string eventKeyUpdateConstants = constantsSet.EventKeyUpdateConstants.Value;

            Logs.Here().Information("ConstantsData subscribed on EventKey. \n {@E}", new { EventKey = eventKeyUpdateConstants });
            Logs.Here().Information("Constants version is {0}:{1}.", constantsSet.ConstantsVersionBase.Value, constantsSet.ConstantsVersionNumber.Value);

            _flagToBlockEventUpdate = true;

            _keyEvents.Subscribe(eventKeyUpdateConstants, async (string key, KeyEvent cmd) =>
            {
                if (cmd == constantsSet.EventCmd && _flagToBlockEventUpdate)
                {
                    _flagToBlockEventUpdate = false;
                    _ = CheckKeyUpdateConstants(constantsSet, constantsStartGuidField, stoppingToken);
                }
            });
        }

        private async Task CheckKeyUpdateConstants(ConstantsSet constantsSet, string constantsStartGuidField, CancellationToken stoppingToken) // Main of EventKeyFrontGivesTask key
        {
            // проверять, что константы может обновлять только админ

            string eventKeyUpdateConstants = constantsSet.EventKeyUpdateConstants.Value;
            Logs.Here().Debug("CheckKeyUpdateConstants started with key {0}.", eventKeyUpdateConstants);

            IDictionary<string, int> updatedConstants = await _cache.FetchUpdatedConstantsAndDeleteKey<string, int>(eventKeyUpdateConstants); ;
            int updatedConstantsCount = updatedConstants.Count;
            Logs.Here().Debug("Fetched updated constants count = {0}.", updatedConstantsCount);

            // выбирать все поля, присваивать по таблице, при присваивании поле удалять
            // все обновляемые константы должны быть одного типа или разные типы на разных ключах
            
            bool setWasUpdated;
            (setWasUpdated, constantsSet) = UpdatedValueAssignsToProperty(constantsSet, updatedConstants);
            if (setWasUpdated)
            {
                // версия констант обновится внутри SetStartConstants
                await _cache.SetStartConstants(constantsSet.ConstantsVersionBase, constantsStartGuidField, constantsSet);
            }

            // задержка, определяющая максимальную частоту обновления констант
            double timeToWaitTheConstants = constantsSet.EventKeyUpdateConstants.LifeTime;
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(timeToWaitTheConstants), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if the Delay is cancelled
            }
            // перед завершением обработчика разрешаем события подписки на обновления
            _flagToBlockEventUpdate = true;
        }
        
        public static (bool, ConstantsSet) UpdatedValueAssignsToProperty(ConstantsSet constantsSet, IDictionary<string, int> updatedConstants)
        {
            bool setWasUpdated = false;
            string finalPropertyToSet = constantsSet.FinalPropertyToSet.Value;
            // foreach перенесли внутрь метода, чтобы лишний раз не переписывать набор, если обновление имеет такое же значение
            // возможно, что со страницы всегда будет приезжать весь набор полей только с одним/несколькими изменёнными
            foreach (KeyValuePair<string, int> updatedConstant in updatedConstants)
            {
                var (key, value) = updatedConstant;
                
                int existsConstant = FetchValueOfPropertyOfProperty(constantsSet, finalPropertyToSet, key);
                // можно проверять предыдущее значение и, если новое такое же, не обновлять
                // но тогда надо проверять весь пакет и только если все не изменились, то не переписывать ключ
                // может быть когда-нибудь потом
                if (existsConstant != value)
                {
                    // но запись в ключ всё равно произойдёт, как это устранить?
                    //return constantsSet;

                    object constantType = FetchValueOfProperty(constantsSet, key);

                    if (constantType == null)
                    {
                        Logs.Here().Error("Wrong {@P} was used - update failed", new { PropertyName = key });
                        return (false, constantsSet);
                    }

                    constantType.GetType().GetProperty(finalPropertyToSet)?.SetValue(constantType, value);

                    int constantWasUpdated = FetchValueOfPropertyOfProperty(constantsSet, finalPropertyToSet, key);
                    if (constantWasUpdated == value)
                    {
                        setWasUpdated = true;
                    }
                }
                else
                {
                    // если не обновится ни одно поле, в setWasUpdated останется false и основной ключ не обновится
                    // ещё можно показать значения - бывшее и которое хотели обновить
                    Logs.Here().Warning("Constant {@K} will be left unchanged", new { Key = key });
                }
                // тут надо удалять поле, с которого считано обновление
                // или не удалять по одному, а на выходе всегда удалять ключ целиком - в любом случае
                // тогда юнит-тест останется живой
            }
            return (setWasUpdated, constantsSet);
        }

        private static int FetchValueOfPropertyOfProperty(ConstantsSet constantsSet, string finalPropertyToSet, string key)
        {
            int constantValue = Convert.ToInt32(FetchValueOfProperty(FetchValueOfProperty(constantsSet, key), finalPropertyToSet));
            Logs.Here().Information("The value of property {0} = {1}.", key, constantValue);
            return constantValue;
        }

        private static object FetchValueOfProperty(object classInstance, string propertyName)
        {
            return classInstance?.GetType().GetProperty(propertyName)?.GetValue(classInstance);
        }
    }
}
