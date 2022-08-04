using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public partial class CharacterSource : DataSource
{
    protected InstanceHandler _instanceHandler;
    public InstanceHandler instanceHandler => _instanceHandler;

    public class InstanceHandler
    {
        public const int MAX_RESERVE_COUNT = 200;

        ~InstanceHandler()
        {
            DisposePool();
        }

        public InstanceHandler(CharacterSource source)
        {
            _source = source;
        }

        public virtual Character Create(Vector3 pos, Quaternion rot)
        {
            return Create(pos, rot, SceneManager.GetActiveScene());
        }

        public virtual Character Create(Vector3 pos, Quaternion rot, Scene scene)
        {
            Character instance = null;

            if (_pool != null)
            {
                instance = _pool.Get();
            }

            if (instance == null)
            {
                // try instantiating if pooling fails for some reason
                instance = Instantiate();
            }

            if (instance != null)
            {
                instance.transform.position = pos;
                instance.transform.rotation = rot;
                SceneManager.MoveGameObjectToScene(instance.gameObject, scene);
                instance.OnSpawn();
            }

            return instance;
        }

        public virtual void Dispose(Character character)
        {
            if (character != null)
            {
                character.OnDespawn();

                if (Count == MaxCount || _pool == null)
                {
                    OnDestroyCallback(character);
                }
                else
                {
                    _pool.Release(character);
                }
            }
        }

        public virtual void Reserve(int count)
        {
            // creates pool if NULL
            CreatePool();

            count = Mathf.Clamp(count, 0, _maxCount);
            int currentCount = _pool.CountInactive;

            if (count > currentCount)
            {
                for (int i = currentCount; i < count; i++)
                {
                    _pool.Release(Instantiate());
                }
            }

            if (count < currentCount)
            {
                for (int i = currentCount; i < count; i++)
                {
                    OnDestroyCallback(_pool.Get());
                }
            }
        }

        public virtual void Reserve(int count, int maxCount)
        {
            _maxCount = Mathf.Max(maxCount, 0);
            Reserve(count);
        }

        public virtual void CreatePool()
        {
            if (_pool == null)
            {
                _pool = new ObjectPool<Character>(Instantiate,
                    OnGetCallback, OnReleaseCallback, OnDestroyCallback,
                    Application.isEditor, _maxCount, MAX_RESERVE_COUNT);
            }

            if (_parent == null)
            {
                _parent = new GameObject($"{_source.characterName} Character Pool").transform;
                GameObject.DontDestroyOnLoad(_parent.gameObject);
            }
        }

        public virtual void DisposePool()
        {
            if (_pool != null)
            {
                _pool.Dispose();
            }

            if (_parent != null)
            {
                GameObject.Destroy(_parent.gameObject);
            }
        }

        protected virtual Character Instantiate()
        {
            var prefab = _source._tppPrefab;
            if (prefab == null)
            {
                return null;
            }

            // deactivate the prefab to avoid calling Awake()
            // on instantiated instances
            prefab.SetActive(false);
            GameObject instance = GameObject.Instantiate(prefab);
            prefab.SetActive(true);

            Character character = instance.GetComponent<Character>();

            // add the character initializer
            CharacterInitializer initializer = instance.AddComponent<CharacterInitializer>();
            initializer.destroyOnUse = true;
            initializer.source = _source;

            character.transform.SetParent(_parent);
            instance.SetActive(true);

            return character;
        }

        protected virtual void OnGetCallback(Character character)
        {
            if (character != null)
            {
                character.gameObject.SetActive(true);
                character.transform.SetParent(null);
                character.OnSpawn();
            }
        }

        protected virtual void OnReleaseCallback(Character character)
        {
            if (character != null)
            {
                character.gameObject.SetActive(false);
                character.transform.SetParent(_parent);
                character.OnDespawn();
            }
        }

        protected virtual void OnDestroyCallback(Character character)
        {
            if (character == null)
            {
            }

            if (character != null)
            {
                character.Destroy();

                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(character.gameObject);
                }
                else
                {
                    GameObject.Destroy(character.gameObject);
                }
            }
        }

        public int Count => _pool == null ? 0 : _pool.CountInactive;
        public int MaxCount => _maxCount;

        protected CharacterSource _source;
        public CharacterSource Source => _source;

        protected ObjectPool<Character> _pool;
        public IObjectPool<Character> Pool => _pool;

        protected Transform _parent;
        protected int _maxCount = 0;
    }
}