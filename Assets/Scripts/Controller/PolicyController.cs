using System.Collections.Generic;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Controller
{
    /// <summary>
    /// 政策系统 - 仅有少部分政策会更改地块并放置独特建筑
    /// 政策系统可更改地块分类、放置独特建筑
    /// </summary>
    public class PolicyController
    {
        private readonly GameManager _gameManager;
        private readonly List<string> _activePolicies = new List<string>();

        public PolicyController(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        /// <summary>实施政策</summary>
        public bool EnactPolicy(string policyId)
        {
            if (_activePolicies.Contains(policyId)) return false;
            _activePolicies.Add(policyId);
            // TODO: 根据策划案补充具体政策效果
            return true;
        }

        /// <summary>废除政策</summary>
        public bool RepealPolicy(string policyId)
        {
            return _activePolicies.Remove(policyId);
        }

        /// <summary>获取所有已激活政策</summary>
        public List<string> GetActivePolicies() => new List<string>(_activePolicies);

        /// <summary>检查政策是否已激活</summary>
        public bool IsPolicyActive(string policyId) => _activePolicies.Contains(policyId);
    }
}