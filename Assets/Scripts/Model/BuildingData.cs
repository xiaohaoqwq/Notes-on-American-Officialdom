using System;
using System.Collections.Generic;
using AmericanOfficialNotes.Model.Enums;

namespace AmericanOfficialNotes.Model
{
    /// <summary>
    /// 建筑数据 - 纯C#数据结构，不依赖Unity
    /// 不同地块分类的建筑具有不同的参数类型
    /// </summary>
    [Serializable]
    public class BuildingData
    {
        /// <summary>建筑类型</summary>
        public BuildingType Type { get; set; }

        /// <summary>所属地块分类</summary>
        public TileCategory Category { get; set; }

        /// <summary>产品名称（仅工业区使用）</summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 基础参数（原始值，未经修正）
        /// 只能通过GameManager更改
        /// </summary>
        private readonly Dictionary<BuildingParameterType, float> _baseParameters = new Dictionary<BuildingParameterType, float>();

        /// <summary>
        /// 获取基础参数值（未经修正）
        /// </summary>
        public float GetBaseParameter(BuildingParameterType type)
        {
            return _baseParameters.TryGetValue(type, out float value) ? value : 0f;
        }

        /// <summary>
        /// 设置基础参数值（仅GameManager应调用）
        /// </summary>
        public void SetBaseParameter(BuildingParameterType type, float value)
        {
            _baseParameters[type] = value;
        }

        /// <summary>
        /// 基础参数是否存在
        /// </summary>
        public bool HasParameter(BuildingParameterType type)
        {
            return _baseParameters.ContainsKey(type);
        }

        /// <summary>
        /// 获取该建筑拥有的所有参数类型
        /// </summary>
        public List<BuildingParameterType> GetParameterTypes()
        {
            return new List<BuildingParameterType>(_baseParameters.Keys);
        }
    }
}