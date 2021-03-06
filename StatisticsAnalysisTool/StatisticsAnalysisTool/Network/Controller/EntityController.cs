﻿using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class EntityController
    {
        private readonly ConcurrentDictionary<long, GameObject> _knownEntities = new ConcurrentDictionary<long, GameObject>();

        public void AddEntity(long objectId, string name, GameObjectType objectType, GameObjectSubType objectSubType, bool isInParty = false)
        {
            AddEntity(objectId, null, name, objectType, objectSubType, isInParty);
        }

        public void AddEntity(long objectId, Guid? userGuid, string name, GameObjectType objectType, GameObjectSubType objectSubType, bool isInParty = false)
        {
            if (_knownEntities.ContainsKey(objectId))
            {
                return;
            }

            var gameObject = new GameObject(objectId)
            {
                Name = name,
                ObjectType = objectType,
                UserGuid = userGuid,
                ObjectSubType = objectSubType,
                IsInParty = isInParty
            };

            _knownEntities.TryAdd(objectId, gameObject);
            OnAddEntity?.Invoke(gameObject);
        }

        public void RemoveEntity(long objectId)
        {
            _knownEntities.TryRemove(objectId, out _);
        }

        public void RemoveAll()
        {
            _knownEntities.Clear();
        }

        public void ResetPartyMember()
        {
            foreach (var entity in _knownEntities.Where(x => x.Value.IsInParty))
            {
                entity.Value.IsInParty = false;
            }
        }

        public void SetInParty(Guid guid)
        {
            var userObject = _knownEntities.FirstOrDefault(x => x.Value.UserGuid == guid);
            if (userObject.Value?.IsInParty == false)
            {
                userObject.Value.IsInParty = true;
            }
        }

        public void SetInParty(Dictionary<string, Guid> party)
        {
            foreach (var member in party)
            {
                SetInParty(member.Value);
            }
        }

        public KeyValuePair<long, GameObject>? GetEntity(long objectId)
        {
            return _knownEntities?.FirstOrDefault(x => x.Key == objectId);
        }

        public void SetCharacterEquipment(long objectId, CharacterEquipment equipment)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Key == objectId);
            if (entity?.Value != null)
            {
                entity.Value.Value.CharacterEquipment = equipment;
            }
        }

        public IEnumerable<GameObject> GetEntities()
        {
            return new List<GameObject>(_knownEntities.Values);
        }

        public event Action<GameObject> OnAddEntity;

        public void HealthUpdate(
            long objectId,
            GameTimeStamp TimeStamp,
            double HealthChange,
            double NewHealthValue,
            EffectType EffectType,
            EffectOrigin EffectOrigin,
            long CauserId,
            int CausingSpellType
        )
        {
            OnHealthUpdate?.Invoke(
                objectId,
                TimeStamp,
                HealthChange,
                NewHealthValue,
                EffectType,
                EffectOrigin,
                CauserId,
                CausingSpellType
            );
        }

        public event Action<long, GameTimeStamp, double, double, EffectType, EffectOrigin, long, int> OnHealthUpdate;
    }
}