using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public partial class CharacterAsset : DataAsset
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

        public InstanceHandler(CharacterAsset source)
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

            if (_pool is not null)
            {
                instance = _pool.Get();
            }

            if (instance is null)
            {
                // try instantiating if pooling fails for some reason
                instance = Instantiate();
            }

            if (instance is not null)
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
            if (character is not null)
            {
                character.OnDespawn();

                if (Count == MaxCount || _pool is null)
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

        public virtual void SetMaxReserve(int count)
        {
            Debug.Assert(count >= 0);
            _maxCount = count;
        }

        public virtual void CreatePool()
        {
            if (_pool is null)
            {
                _pool = new ObjectPool<Character>(Instantiate,
                    OnGetCallback, OnReleaseCallback, OnDestroyCallback,
                    Application.isEditor, _maxCount, MAX_RESERVE_COUNT);
            }

            if (_parent is null)
            {
                _parent = new GameObject($"{_source.characterName} Character Pool").transform;
                GameObject.DontDestroyOnLoad(_parent.gameObject);
            }
        }

        public virtual void DisposePool()
        {
            if (_pool is not null)
            {
                _pool.Dispose();
            }

            if (_parent is not null)
            {
                GameObject.Destroy(_parent.gameObject);
            }
        }

        protected virtual Character Instantiate()
        {
            var prefab = _source._tppPrefab;
            if (prefab is null)
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
            if (character is not null)
            {
                character.gameObject.SetActive(true);
                character.transform.SetParent(null);
                character.OnSpawn();
            }
        }

        protected virtual void OnReleaseCallback(Character character)
        {
            if (character is not null)
            {
                character.gameObject.SetActive(false);
                character.transform.SetParent(_parent);
                character.OnDespawn();
            }
        }

        protected virtual void OnDestroyCallback(Character character)
        {
            if (character is null)
            {
            }

            if (character is not null)
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

        public int Count => _pool is null ? 0 : _pool.CountInactive;
        public int MaxCount => _maxCount;

        protected CharacterAsset _source;
        public CharacterAsset Source => _source;

        protected ObjectPool<Character> _pool;
        public IObjectPool<Character> Pool => _pool;

        protected Transform _parent;
        protected int _maxCount = 0;
    }
}