using System;
using System.Collections.Generic;

namespace AmericanOfficialNotes.Model
{
    /// <summary>
    /// 中文姓名库 - 存储姓和名（男女分开），供生成人口时组合
    /// </summary>
    [Serializable]
    public class NameLibrary
    {
        public List<string> Surnames { get; set; } = new List<string>
        {
            "赵", "钱", "孙", "李", "周", "吴", "郑", "王",
            "冯", "陈", "褚", "卫", "蒋", "沈", "韩", "杨",
            "朱", "秦", "尤", "许", "何", "吕", "施", "张"
        };

        public List<string> MaleGivenNames { get; set; } = new List<string>
        {
            "伟", "强", "磊", "军", "勇", "涛", "明", "超",
            "鹏", "辉", "杰", "浩", "宇", "斌", "健", "飞"
        };

        public List<string> FemaleGivenNames { get; set; } = new List<string>
        {
            "芳", "娜", "敏", "静", "丽", "艳", "雪", "琳",
            "婷", "玲", "燕", "萍", "花", "梅", "红", "倩"
        };

        private static readonly Random _rng = new Random();

        /// <summary>生成一个随机全名</summary>
        public string GenerateName(bool isMale)
        {
            string surname = Surnames[_rng.Next(Surnames.Count)];
            var pool = isMale ? MaleGivenNames : FemaleGivenNames;
            string given = pool[_rng.Next(pool.Count)];
            return surname + given;
        }

        /// <summary>随机性别（true=男，false=女）</summary>
        public bool RandomGender()
        {
            return _rng.Next(2) == 0;
        }
    }
}
