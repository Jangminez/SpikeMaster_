using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner
{
    private const float MIN_DISTANCE = 1f;
    private RallyConfigSO _config;
    private Vector2 _spikerPos;

    private List<IPlayer> _activeEnemies = new List<IPlayer>();
    private Queue<IPlayer> _deActiveEnemies = new Queue<IPlayer>();

    public EnemySpawner(RallyConfigSO config, Vector2 pos)
    {
        _config = config;
        _spikerPos = pos;

        _activeEnemies = new List<IPlayer>();
        _deActiveEnemies = new Queue<IPlayer>();
    }

    public void SetEnemy(int count)
    {
        ResetEnemies();

        if (count > _deActiveEnemies.Count)
        {
            SpawnEnemy(count - _deActiveEnemies.Count);
        }

        int curSpawnCnt = 0;

        while (curSpawnCnt < count)
        {
            IPlayer enemy = _deActiveEnemies.Dequeue();
            Vector2 randPos = GetRandomPos();
            Vector2 direction = _spikerPos - randPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion lookRot = Quaternion.Euler(0, 0, angle);

            enemy.SetTransform(randPos, lookRot);
            enemy.SetActive(true);

            _activeEnemies.Add(enemy);
            curSpawnCnt++;
        }
    }

    private void SpawnEnemy(int count)
    {
        for (int i = 0; i < count; i++)
        {
            IPlayer enemy = EntityFactory.CreateEntity<IPlayer>(EntityPath.Enemy_Libero);
            enemy.SetActive(false);
            _deActiveEnemies.Enqueue(enemy);
        }
    }

    private Vector2 GetRandomPos()
    {
        while (true)
        {
            Vector2 temp = new Vector2(
                Random.Range(_config.CourtMinX + 0.5f, _config.CourtMaxX - 0.5f),
                Random.Range(_config.CourtMinY + 2f, _config.CourtMaxY - 1f)
            );

            bool isOverlap = false;
            foreach (var enemy in _activeEnemies)
            {
                if (Vector2.Distance(temp, enemy.GetPosition()) < MIN_DISTANCE)
                {
                    isOverlap = true;
                    break;
                }
            }

            if (!isOverlap) return temp;
        }
    }

    private void ResetEnemies()
    {
        foreach (var enemy in _activeEnemies)
        {
            enemy.SetActive(false);
            _deActiveEnemies.Enqueue(enemy);
        }

        _activeEnemies.Clear();
    }
}