using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ConstantData.Services;
using Shared.Library.Models;
using Shared.Library.Services;

namespace ConstantData
{
    public class MonitorLoop
    {
        private readonly IConstantsCollectionService _collection;
        private readonly ISharedDataAccess _data;
        private readonly ICacheManageService _cache;
        private readonly CancellationToken _cancellationToken;
        private readonly IOnKeysEventsSubscribeService _subscribe;
        private readonly string _guid;

        public MonitorLoop(
            GenerateThisInstanceGuidService thisGuid,
            ISharedDataAccess data,
            ICacheManageService cache,
            IHostApplicationLifetime applicationLifetime,
            IOnKeysEventsSubscribeService subscribe,
            IConstantsCollectionService collection)
        {
            _data = data;
            _subscribe = subscribe;
            _collection = collection;
            _cache = cache;
            _cancellationToken = applicationLifetime.ApplicationStopping;
            _guid = thisGuid.ThisBackServerGuid();
        }

        private static Serilog.ILogger Logs => Serilog.Log.ForContext<MonitorLoop>();

        public void StartMonitorLoop()
        {
            Logs.Here().Information("ConstantsMountingMonitor Loop is starting.");
            
            Task.Run(ConstantsMountingMonitor, _cancellationToken);
        }

        public async Task ConstantsMountingMonitor()
        {
            ConstantsSet constantsSet = _collection.SettingConstants;
            Logs.Here().Debug("ConstantCheck EventKeyFrontGivesTaskTimeDays = {0}.", constantsSet.EventKeyFrontGivesTask.LifeTime);

            (string startConstantKey, string constantsStartLegacyField, string constantsStartGuidField) = _data.FetchBaseConstants();
            
            string dataServerPrefixGuid = $"{constantsSet.PrefixDataServer.Value}:{_guid}";
            double baseLifeTime = constantsSet.PrefixDataServer.LifeTime;
            constantsSet.ConstantsVersionBase.Value = startConstantKey;
            constantsSet.ConstantsVersionBase.LifeTime = baseLifeTime;
            constantsSet.ConstantsVersionBaseField.Value = constantsStartGuidField;
            
            // записываем константы в стартовый ключ и старое поле (для совместимости)
            await _cache.SetStartConstants(constantsSet.ConstantsVersionBase, constantsStartLegacyField, constantsSet);
            Logs.Here().Information("ConstantData sent constants to {@K} / {@F}.", new { Key = constantsSet.ConstantsVersionBase.Value }, new { Field = constantsStartLegacyField });


            // сервер констант имеет свой гуид и это ключ обновляемых констант
            // его он пишет в поле для нового гуид-ключа для всех
            // на этот ключ уже можно подписаться, он стабильный на всё время существования сервера
            // если этот ключ исчезнет(сервер перезапустился), то надо перейти на базовый ключ и искать там
            // на этом ключе будут сменяемые поля с константами - новое появилась, старое удалили
            // тогда будет смысл в подписке
            // в подписке всё равно мало смысла, даже если есть известие от подписки, надо проверять наличие гуид-ключа -
            // может же сервер исчезнуть к этому времени, забрав с собой ключ
            // можно ключ не удалять, даже нужно - если сервер упадёт неожиданно, то ключи всё равно останутся
            // но ключ может и исчезнуть сам по себе, надо проверять
            // наверное, подписка имеет смысл для мгновенной реакции или для длительного ожидания
            // если сервер простаивает, то обновления констант ему всё равно не нужны
            // если, конечно, не обновятся какие-то базовые ключи, но это допускать нельзя
            // можно разделить набор на два - изменяемый и постоянный
            // постоянные инициализовать через инит, а остальные добавлять по ходу - по ключам изменения
            // поэтому сервер получит новые константы после захвата пакета


            // проверяем наличие старого ключа гуид-констант и если он есть, удаляем его
            string oldGuidConstants = await _cache.FetchHashedAsync<string>(constantsSet.ConstantsVersionBase.Value, constantsStartGuidField);
            if (oldGuidConstants != null)
            {
                bool oldGuidConstantsWasDeleted = await _cache.DelKeyAsync(oldGuidConstants);
                Logs.Here().Information("Old Constants {0} was deleted - {1}.", oldGuidConstants, oldGuidConstantsWasDeleted);
            }

            // записываем в стартовый ключ и новое поле гуид-ключ обновляемых констант
            await _cache.SetConstantsStartGuidKey(constantsSet.ConstantsVersionBase, constantsStartGuidField, dataServerPrefixGuid);

            // записываем в строку версии констант основной гуид-ключ
            constantsSet.ConstantsVersionBase.Value = dataServerPrefixGuid;

            // записываем константы в новый гуид-ключ и новое поле (надо какое-то всем известное поле)
            // потом может быть будет поле-версия, а может будет меняться ключ

            // передавать переменную класса с временем жизни вместо строки
            await _cache.SetStartConstants(constantsSet.ConstantsVersionBase, constantsStartGuidField, constantsSet);
            Logs.Here().Information("ConstantData sent constants to {@K} / {@F}.", new { Key = constantsSet.ConstantsVersionBase.Value }, new { Field = constantsStartGuidField });

            // подписываемся на ключ сообщения о необходимости обновления констант
            _subscribe.SubscribeOnEventUpdate(constantsSet, constantsStartGuidField, _cancellationToken);
            Logs.Here().Debug("SettingConstants ConstantsVersionBase = {0}, ConstantsVersionNumber = {1}.", constantsSet.ConstantsVersionBase.Value, constantsSet.ConstantsVersionNumber.Value);

            while (true)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    bool res = await _cache.DeleteKeyIfCancelled(startConstantKey);
                    Logs.Here().Warning("Cancellation Token was received, key was removed = {KeyStroke}.", res);

                    return;
                }

                var keyStroke = Console.ReadKey();

                if (keyStroke.Key == ConsoleKey.W)
                {
                    Logs.Here().Information("ConsoleKey was received {KeyStroke}.", keyStroke.Key);
                }

                await Task.Delay(10, _cancellationToken);
            }
        }
    }
}
