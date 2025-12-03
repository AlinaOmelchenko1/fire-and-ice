using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using fire_and_ice.Interfaces;

namespace fire_and_ice.Systems
{
    /// <summary>
    /// Represents an entity lifecycle event
    /// </summary>
    public class EntityEventArgs : EventArgs
    {
        /// <summary>
        /// The entity involved in the event
        /// </summary>
        public IGameObject Entity { get; set; }

        /// <summary>
        /// Optional tag associated with the entity
        /// </summary>
        public string Tag { get; set; }
    }

    /// <summary>
    /// Manages all game entities including their lifecycle, updates, and queries.
    /// Provides entity grouping, tagging, and efficient batch operations.
    /// </summary>
    public class EntityManager
    {
        private readonly List<IGameObject> _entities;
        private readonly List<IGameObject> _entitiesToAdd;
        private readonly List<IGameObject> _entitiesToRemove;
        private readonly Dictionary<string, HashSet<IGameObject>> _entityTags;
        private readonly Dictionary<Type, List<IGameObject>> _entitiesByType;
        private bool _isUpdating;

        /// <summary>
        /// Event raised when an entity is added
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityAdded;

        /// <summary>
        /// Event raised when an entity is removed
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityRemoved;

        /// <summary>
        /// Gets the total number of entities
        /// </summary>
        public int EntityCount => _entities.Count;

        /// <summary>
        /// Gets the number of entities pending addition
        /// </summary>
        public int PendingAddCount => _entitiesToAdd.Count;

        /// <summary>
        /// Gets the number of entities pending removal
        /// </summary>
        public int PendingRemoveCount => _entitiesToRemove.Count;

        /// <summary>
        /// Gets the number of unique tags
        /// </summary>
        public int TagCount => _entityTags.Count;

        /// <summary>
        /// Creates a new entity manager
        /// </summary>
        public EntityManager()
        {
            _entities = new List<IGameObject>();
            _entitiesToAdd = new List<IGameObject>();
            _entitiesToRemove = new List<IGameObject>();
            _entityTags = new Dictionary<string, HashSet<IGameObject>>();
            _entitiesByType = new Dictionary<Type, List<IGameObject>>();
            _isUpdating = false;
        }

        /// <summary>
        /// Adds an entity to the manager
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <param name="tags">Optional tags to assign to the entity</param>
        public void AddEntity(IGameObject entity, params string[] tags)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (_isUpdating)
            {
                // Defer addition until after update
                _entitiesToAdd.Add(entity);
            }
            else
            {
                AddEntityInternal(entity);
            }

            // Add tags
            foreach (var tag in tags)
            {
                TagEntity(entity, tag);
            }
        }

        /// <summary>
        /// Internal method to add an entity immediately
        /// </summary>
        private void AddEntityInternal(IGameObject entity)
        {
            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);

                // Add to type cache
                var type = entity.GetType();
                if (!_entitiesByType.ContainsKey(type))
                {
                    _entitiesByType[type] = new List<IGameObject>();
                }
                _entitiesByType[type].Add(entity);

                OnEntityAdded(entity);
            }
        }

        /// <summary>
        /// Removes an entity from the manager
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        public void RemoveEntity(IGameObject entity)
        {
            if (entity == null)
                return;

            if (_isUpdating)
            {
                // Defer removal until after update
                if (!_entitiesToRemove.Contains(entity))
                {
                    _entitiesToRemove.Add(entity);
                }
            }
            else
            {
                RemoveEntityInternal(entity);
            }
        }

        /// <summary>
        /// Internal method to remove an entity immediately
        /// </summary>
        private void RemoveEntityInternal(IGameObject entity)
        {
            if (_entities.Remove(entity))
            {
                // Remove from type cache
                var type = entity.GetType();
                if (_entitiesByType.ContainsKey(type))
                {
                    _entitiesByType[type].Remove(entity);
                    if (_entitiesByType[type].Count == 0)
                    {
                        _entitiesByType.Remove(type);
                    }
                }

                // Remove from all tags
                RemoveAllTags(entity);

                OnEntityRemoved(entity);
            }
        }

        /// <summary>
        /// Removes all entities
        /// </summary>
        public void Clear()
        {
            var entitiesToRemove = new List<IGameObject>(_entities);
            foreach (var entity in entitiesToRemove)
            {
                RemoveEntity(entity);
            }

            ProcessPendingChanges();

            _entities.Clear();
            _entitiesToAdd.Clear();
            _entitiesToRemove.Clear();
            _entityTags.Clear();
            _entitiesByType.Clear();
        }

        /// <summary>
        /// Tags an entity with a string identifier
        /// </summary>
        /// <param name="entity">The entity to tag</param>
        /// <param name="tag">The tag to assign</param>
        public void TagEntity(IGameObject entity, string tag)
        {
            if (entity == null || string.IsNullOrEmpty(tag))
                return;

            if (!_entityTags.ContainsKey(tag))
            {
                _entityTags[tag] = new HashSet<IGameObject>();
            }

            _entityTags[tag].Add(entity);
        }

        /// <summary>
        /// Removes a tag from an entity
        /// </summary>
        /// <param name="entity">The entity to untag</param>
        /// <param name="tag">The tag to remove</param>
        public void UntagEntity(IGameObject entity, string tag)
        {
            if (entity == null || string.IsNullOrEmpty(tag))
                return;

            if (_entityTags.ContainsKey(tag))
            {
                _entityTags[tag].Remove(entity);
                if (_entityTags[tag].Count == 0)
                {
                    _entityTags.Remove(tag);
                }
            }
        }

        /// <summary>
        /// Removes all tags from an entity
        /// </summary>
        /// <param name="entity">The entity to untag</param>
        private void RemoveAllTags(IGameObject entity)
        {
            var tagsToRemove = new List<string>();
            foreach (var kvp in _entityTags)
            {
                if (kvp.Value.Contains(entity))
                {
                    tagsToRemove.Add(kvp.Key);
                }
            }

            foreach (var tag in tagsToRemove)
            {
                UntagEntity(entity, tag);
            }
        }

        /// <summary>
        /// Gets all entities with a specific tag
        /// </summary>
        /// <param name="tag">The tag to search for</param>
        /// <returns>List of entities with the tag</returns>
        public List<IGameObject> GetEntitiesWithTag(string tag)
        {
            if (_entityTags.TryGetValue(tag, out var entities))
            {
                return new List<IGameObject>(entities);
            }
            return new List<IGameObject>();
        }

        /// <summary>
        /// Checks if an entity has a specific tag
        /// </summary>
        /// <param name="entity">The entity to check</param>
        /// <param name="tag">The tag to check for</param>
        /// <returns>True if the entity has the tag</returns>
        public bool HasTag(IGameObject entity, string tag)
        {
            if (_entityTags.TryGetValue(tag, out var entities))
            {
                return entities.Contains(entity);
            }
            return false;
        }

        /// <summary>
        /// Gets all entities of a specific type
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <returns>List of entities of the specified type</returns>
        public List<T> GetEntitiesOfType<T>() where T : class, IGameObject
        {
            var type = typeof(T);
            if (_entitiesByType.TryGetValue(type, out var entities))
            {
                return entities.Cast<T>().ToList();
            }
            return new List<T>();
        }

        /// <summary>
        /// Gets all entities matching a predicate
        /// </summary>
        /// <param name="predicate">The predicate to match</param>
        /// <returns>List of matching entities</returns>
        public List<IGameObject> GetEntities(Func<IGameObject, bool> predicate)
        {
            return _entities.Where(predicate).ToList();
        }

        /// <summary>
        /// Gets the first entity matching a predicate
        /// </summary>
        /// <param name="predicate">The predicate to match</param>
        /// <returns>The first matching entity, or null</returns>
        public IGameObject GetFirstEntity(Func<IGameObject, bool> predicate)
        {
            return _entities.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets all entities as a read-only list
        /// </summary>
        /// <returns>Read-only list of all entities</returns>
        public IReadOnlyList<IGameObject> GetAllEntities()
        {
            return _entities.AsReadOnly();
        }

        /// <summary>
        /// Counts entities matching a predicate
        /// </summary>
        /// <param name="predicate">The predicate to match</param>
        /// <returns>Number of matching entities</returns>
        public int CountEntities(Func<IGameObject, bool> predicate)
        {
            return _entities.Count(predicate);
        }

        /// <summary>
        /// Checks if any entity matches a predicate
        /// </summary>
        /// <param name="predicate">The predicate to match</param>
        /// <returns>True if any entity matches</returns>
        public bool AnyEntity(Func<IGameObject, bool> predicate)
        {
            return _entities.Any(predicate);
        }

        /// <summary>
        /// Updates all entities that implement IUpdateable
        /// </summary>
        /// <param name="gameTime">Game time information</param>
        public void Update(GameTime gameTime)
        {
            _isUpdating = true;

            // Update all updateable entities
            foreach (var entity in _entities)
            {
                if (entity is Interfaces.IUpdateable updateable && updateable.Enabled)
                {
                    updateable.Update(gameTime);
                }
            }

            _isUpdating = false;

            // Process pending additions and removals
            ProcessPendingChanges();
        }

        /// <summary>
        /// Updates entities of a specific type that implement IUpdateable
        /// </summary>
        /// <typeparam name="T">The type of entities to update</typeparam>
        /// <param name="gameTime">Game time information</param>
        public void UpdateEntitiesOfType<T>(GameTime gameTime) where T : class, IGameObject
        {
            var entities = GetEntitiesOfType<T>();
            foreach (var entity in entities)
            {
                // Check if entity implements IUpdateable and is enabled
                if (entity is Interfaces.IUpdateable updateable && updateable.Enabled)
                {
                    entity.Update(gameTime);
                }
                else
                {
                    // Just call Update if it doesn't implement IUpdateable
                    entity.Update(gameTime);
                }
            }
        }

        /// <summary>
        /// Updates entities with a specific tag
        /// </summary>
        /// <param name="tag">The tag of entities to update</param>
        /// <param name="gameTime">Game time information</param>
        public void UpdateEntitiesWithTag(string tag, GameTime gameTime)
        {
            var entities = GetEntitiesWithTag(tag);
            foreach (var entity in entities)
            {
                if (entity is Interfaces.IUpdateable updateable && updateable.Enabled)
                {
                    updateable.Update(gameTime);
                }
            }
        }

        /// <summary>
        /// Performs an action on all entities
        /// </summary>
        /// <param name="action">The action to perform</param>
        public void ForEach(Action<IGameObject> action)
        {
            foreach (var entity in _entities)
            {
                action(entity);
            }
        }

        /// <summary>
        /// Performs an action on entities of a specific type
        /// </summary>
        /// <typeparam name="T">The type of entities</typeparam>
        /// <param name="action">The action to perform</param>
        public void ForEachOfType<T>(Action<T> action) where T : class, IGameObject
        {
            var entities = GetEntitiesOfType<T>();
            foreach (var entity in entities)
            {
                action(entity);
            }
        }

        /// <summary>
        /// Performs an action on entities with a specific tag
        /// </summary>
        /// <param name="tag">The tag of entities</param>
        /// <param name="action">The action to perform</param>
        public void ForEachWithTag(string tag, Action<IGameObject> action)
        {
            var entities = GetEntitiesWithTag(tag);
            foreach (var entity in entities)
            {
                action(entity);
            }
        }

        /// <summary>
        /// Processes pending entity additions and removals
        /// </summary>
        private void ProcessPendingChanges()
        {
            // Add pending entities
            foreach (var entity in _entitiesToAdd)
            {
                AddEntityInternal(entity);
            }
            _entitiesToAdd.Clear();

            // Remove pending entities
            foreach (var entity in _entitiesToRemove)
            {
                RemoveEntityInternal(entity);
            }
            _entitiesToRemove.Clear();
        }

        /// <summary>
        /// Raises the EntityAdded event
        /// </summary>
        private void OnEntityAdded(IGameObject entity)
        {
            EntityAdded?.Invoke(this, new EntityEventArgs { Entity = entity });
        }

        /// <summary>
        /// Raises the EntityRemoved event
        /// </summary>
        private void OnEntityRemoved(IGameObject entity)
        {
            EntityRemoved?.Invoke(this, new EntityEventArgs { Entity = entity });
        }

        /// <summary>
        /// Gets all unique tags currently in use
        /// </summary>
        /// <returns>List of all tags</returns>
        public List<string> GetAllTags()
        {
            return new List<string>(_entityTags.Keys);
        }

        /// <summary>
        /// Gets all registered entity types
        /// </summary>
        /// <returns>List of all entity types</returns>
        public List<Type> GetAllEntityTypes()
        {
            return new List<Type>(_entitiesByType.Keys);
        }

        /// <summary>
        /// Gets diagnostic information about the entity manager
        /// </summary>
        /// <returns>Diagnostic string</returns>
        public string GetDiagnostics()
        {
            int updateableCount = _entities.Count(e => e is Interfaces.IUpdateable);
            return $"Entities: {EntityCount}, Updateable: {updateableCount}, " +
                   $"Tags: {TagCount}, Types: {_entitiesByType.Count}, " +
                   $"Pending Add: {PendingAddCount}, Pending Remove: {PendingRemoveCount}";
        }

        /// <summary>
        /// Gets detailed statistics about the entity manager
        /// </summary>
        /// <returns>Statistics object</returns>
        public EntityManagerStats GetStats()
        {
            return new EntityManagerStats
            {
                TotalEntities = EntityCount,
                UpdateableEntities = _entities.Count(e => e is Interfaces.IUpdateable),
                UniqueTypes = _entitiesByType.Count,
                UniqueTags = TagCount,
                PendingAdditions = PendingAddCount,
                PendingRemovals = PendingRemoveCount
            };
        }
    }

    /// <summary>
    /// Statistics about the entity manager
    /// </summary>
    public struct EntityManagerStats
    {
        /// <summary>
        /// Total number of entities
        /// </summary>
        public int TotalEntities { get; set; }

        /// <summary>
        /// Number of entities that implement IUpdateable
        /// </summary>
        public int UpdateableEntities { get; set; }

        /// <summary>
        /// Number of unique entity types
        /// </summary>
        public int UniqueTypes { get; set; }

        /// <summary>
        /// Number of unique tags in use
        /// </summary>
        public int UniqueTags { get; set; }

        /// <summary>
        /// Number of entities pending addition
        /// </summary>
        public int PendingAdditions { get; set; }

        /// <summary>
        /// Number of entities pending removal
        /// </summary>
        public int PendingRemovals { get; set; }
    }
}
