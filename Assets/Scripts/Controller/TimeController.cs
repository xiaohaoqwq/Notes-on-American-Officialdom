using System;
using AmericanOfficialNotes.Model;

namespace AmericanOfficialNotes.Controller
{
    /// <summary>
    /// 游戏时间管理 - 推进日期，每小时/每日触发结算
    /// </summary>
    public class TimeController
    {
        private readonly GameSaveData _gameData;
        private readonly GameManager _gameManager;

        private int _currentHour;

        public TimeController(GameSaveData gameData, GameManager gameManager)
        {
            _gameData = gameData;
            _gameManager = gameManager;
        }

        /// <summary>推进一小时，触发每小时结算</summary>
        public void AdvanceHour()
        {
            _gameManager.SettlementController.SettleHour();
            _currentHour++;
            if (_currentHour >= 24)
            {
                _currentHour = 0;
                AdvanceDay();
            }
        }

        /// <summary>推进一天，触发每日结算</summary>
        public void AdvanceDay()
        {
            _gameData.CurrentDay++;
            _gameManager.SettlementController.Settle();
            _gameManager.InvokeDayAdvanced(_gameData.CurrentDay);
        }

        /// <summary>当前天数</summary>
        public int GetCurrentDay() => _gameData.CurrentDay;

        /// <summary>当前小时（0-23）</summary>
        public int GetCurrentHour() => _currentHour;
    }
}
