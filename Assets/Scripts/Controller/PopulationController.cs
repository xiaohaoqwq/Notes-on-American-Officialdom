using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmericanOfficialNotes.Model;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Controller
{
    /// <summary>
    /// 人口管理 - 分配、劳动力、人口流入
    /// 人口只有在有工作的时候才会流入
    /// 一个人只会在一个地方工作，长期不会辞职
    /// </summary>
    public class PopulationController
    {
        private readonly GameSaveData _gameData;
        private readonly GameManager _gameManager;

        public PopulationController(GameSaveData gameData, GameManager gameManager)
        {
            _gameData = gameData;
            _gameManager = gameManager;
        }

        /// <summary>添加人口到指定居民区地块</summary>
        public PopulationData AddPopulation(int homeTileX, int homeTileY)
        {
            bool isMale = _gameData.NameLibrary.RandomGender();
            string name = _gameData.NameLibrary.GenerateName(isMale);
            var pop = new PopulationData
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                IsMale = isMale,
                HomeTileX = homeTileX,
                HomeTileY = homeTileY,
                Health = 100f,
                Mood = 80f,
                ConsumptionPower = 50f,
                Labor = 100f,
                LaborMax = 100f,
                Money = 100f,
                Savings = 0f
            };
            _gameData.Population.Add(pop);
            return pop;
        }

        /// <summary>移除人口（按UUID）</summary>
        public bool RemovePopulation(string id)
        {
            var pop = _gameData.Population.Find(p => p.Id == id);
            if (pop == null) return false;
            _gameData.Population.Remove(pop);
            return true;
        }

        /// <summary>分配工作（按UUID）</summary>
        public bool AssignJob(string id, int workTileX, int workTileY)
        {
            var pop = _gameData.Population.Find(p => p.Id == id);
            if (pop == null) return false;
            pop.WorkTileX = workTileX;
            pop.WorkTileY = workTileY;
            return true;
        }

        /// <summary>取消工作分配（按UUID）</summary>
        public bool UnassignJob(string id)
        {
            var pop = _gameData.Population.Find(p => p.Id == id);
            if (pop == null) return false;
            pop.WorkTileX = -1;
            pop.WorkTileY = -1;
            return true;
        }

        /// <summary>获取地块上的居民</summary>
        public List<PopulationData> GetResidentsAt(int tileX, int tileY)
        {
            return _gameData.Population
                .Where(p => p.HomeTileX == tileX && p.HomeTileY == tileY)
                .ToList();
        }

        /// <summary>获取地块上的工作人员</summary>
        public List<PopulationData> GetWorkersAt(int tileX, int tileY)
        {
            return _gameData.Population
                .Where(p => p.WorkTileX == tileX && p.WorkTileY == tileY)
                .ToList();
        }

        /// <summary>获取总人口数</summary>
        public int GetTotalPopulation() => _gameData.Population.Count;

        /// <summary>获取已就业人口数</summary>
        public int GetEmployedPopulation()
        {
            return _gameData.Population.Count(p => p.IsEmployed);
        }

        /// <summary>
        /// 获取建筑在职率（0-1，不会超过1）
        /// 员工在职率 = 在这个建筑工作的人数 / 该建筑的职位数量
        /// </summary>
        public float GetEmploymentRate(int tileX, int tileY)
        {
            if (GameManager.Instance == null) return 0f;
            var tile = _gameManager.TileController.GetTile(tileX, tileY);
            if (tile == null || !tile.HasBuilding) return 0f;

            int jobs = (int)_gameManager.BuildingController.GetParameter(
                tileX, tileY, BuildingParameterType.JobCount);
            int workers = GetWorkersAt(tileX, tileY).Count;

            if (jobs <= 0) return 0f;
            return Mathf.Min(1f, (float)workers / jobs);
        }

        /// <summary>
        /// 人口流入：有用工需求时自动新增人口到居民区
        /// </summary>
        public void ProcessPopulationInflow()
        {
            foreach (var tile in _gameData.Tiles)
            {
                if (!tile.HasBuilding) continue;
                if (tile.Building.Category == TileCategory.Residential ||
                    tile.Building.Category == TileCategory.SocialService) continue;

                int jobs = (int)_gameManager.BuildingController.GetParameter(
                    tile.GridX, tile.GridY, BuildingParameterType.JobCount);
                int workers = GetWorkersAt(tile.GridX, tile.GridY).Count;

                if (workers >= jobs) continue;

                var residential = _gameData.Tiles
                    .Where(t => t.HasBuilding && t.Building.Category == TileCategory.Residential)
                    .OrderBy(t => t.DistanceTo(tile))
                    .FirstOrDefault();

                if (residential == null) continue;

                int cap = (int)residential.Building.GetBaseParameter(BuildingParameterType.PopulationCap);
                int current = GetResidentsAt(residential.GridX, residential.GridY).Count;
                if (current >= cap) continue;

                var pop = AddPopulation(residential.GridX, residential.GridY);
                pop.WorkTileX = tile.GridX;
                pop.WorkTileY = tile.GridY;
            }
        }

        /// <summary>
        /// 每小时劳动力结算
        /// 工作消耗劳动力，不工作时在居民区回复劳动力（互斥）
        /// </summary>
        public void ProcessHourlyLabor()
        {
            foreach (var pop in _gameData.Population)
            {
                pop.PreviousLabor = pop.Labor;

                if (pop.IsEmployed)
                {
                    var workTile = _gameManager.TileController.GetTile(pop.WorkTileX, pop.WorkTileY);
                    if (workTile != null && workTile.HasBuilding)
                    {
                        float consumption = _gameManager.BuildingController.GetParameter(
                            pop.WorkTileX, pop.WorkTileY, BuildingParameterType.LaborConsumption);
                        float actual = System.Math.Min(consumption, pop.Labor);
                        pop.Labor -= actual;
                    }
                }
                else
                {
                    var homeTile = _gameManager.TileController.GetTile(pop.HomeTileX, pop.HomeTileY);
                    if (homeTile != null && homeTile.HasBuilding &&
                        homeTile.Building.Category == TileCategory.Residential)
                    {
                        float recovery = _gameManager.BuildingController.GetParameter(
                            pop.HomeTileX, pop.HomeTileY, BuildingParameterType.LaborRecovery);
                        pop.Labor += recovery;
                    }
                }

                pop.Labor = Mathf.Clamp(pop.Labor, 0f, pop.LaborMax);
            }
        }
    }
}
