/*
 * 刘刚医师算法基础文件
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using CnMedicineServer.Models;
using OW;
using OW.Data.Entity;

namespace CnMedicineServer.Bll
{
    /// <summary>
    /// 刘刚医师评分表。
    /// </summary>
    public class LiuGangP1Base : IMatchableItem<int, List<(string, float)>>
    {
        public LiuGangP1Base()
        {

        }

        [TextFieldName("症状编号")]
        public string NumberString { get; set; }

        List<(int, float)> _Numbers;

        /// <summary>
        /// 症状编号集合。
        /// </summary>
        public IEnumerable<(int, float)> Keys
        {
            get
            {
                lock (this)
                    if (null == _Numbers)
                    {
                        var tmpList = new List<(string, decimal)>();
                        EntityUtility.FileListInArrayWithPower(NumberString, tmpList, 1);
                        int tmpI;
                        var coll = from tmp in tmpList
                                   where int.TryParse(tmp.Item1, out tmpI)
                                   select (int.Parse(tmp.Item1), (float)tmp.Item2);
                        _Numbers = coll.ToList();
                    }
                return _Numbers;
            }
        }

        [TextFieldName("诊断")]
        public string OutString { get; set; }

        List<(string, float)> _Outs;

        public List<(string, float)> Value
        {
            get
            {
                lock (this)
                    if (null == _Outs)
                    {
                        List<(string, decimal)> tmpList = new List<(string, decimal)>();
                        EntityUtility.FillValueTuples(OutString, tmpList);
                        _Outs = tmpList.Select(c => (c.Item1, (float)c.Item2)).ToList();
                    }
                return _Outs;
            }
        }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public float Threshold { get; set; }

        public int Group { get; set; } = 0;
    }

    /// <summary>
    /// 刘刚医师症型结论表。
    /// </summary>
    public class LiuGangCnDrugOutBase
    {
        public LiuGangCnDrugOutBase()
        {

        }

        /// <summary>
        /// 证型结论。
        /// </summary>
        [TextFieldName("输出症型结论")]
        public string Key { get; set; }

        [TextFieldName("当其为第一诊断时")]
        public string CnDrug1String { get; set; }

        List<(string, decimal)> _CnDrug1s;
        /// <summary>
        /// 为第一诊断的药物
        /// </summary>
        public List<(string, decimal)> CnDrug1s
        {
            get
            {
                lock (this)
                {
                    if (null == _CnDrug1s)
                    {
                        _CnDrug1s = new List<(string, decimal)>();
                        EntityUtility.FillValueTuples(CnDrug1String, _CnDrug1s);
                    }
                }
                return _CnDrug1s;
            }
        }

        [TextFieldName("当其为第二诊断时")]
        public string CnDrug2String { get; set; }

        List<(string, decimal)> _CnDrug2s;
        /// <summary>
        /// 为第二诊断的药物
        /// </summary>
        public List<(string, decimal)> CnDrug2s
        {
            get
            {
                lock (this)
                {
                    if (null == _CnDrug2s)
                    {
                        _CnDrug2s = new List<(string, decimal)>();
                        EntityUtility.FillValueTuples(CnDrug2String, _CnDrug2s);
                    }
                }
                return _CnDrug2s;
            }
        }

        [TextFieldName("当其为并列第一诊断时")]
        public string CnDrug3String { get; set; }

        List<(string, decimal)> _CnDrug3s;
        /// <summary>
        /// 为并列第一诊断时药物
        /// </summary>
        public List<(string, decimal)> CnDrug3s
        {
            get
            {
                lock (this)
                {
                    if (null == _CnDrug3s)
                    {
                        _CnDrug3s = new List<(string, decimal)>();
                        EntityUtility.FillValueTuples(CnDrug3String, _CnDrug3s);
                    }
                }
                return _CnDrug3s;
            }
        }
    }

    /// <summary>
    /// 刘刚医师算法基础类
    /// </summary>
    public abstract class LiuGangAlgorithmBase : CnMedicineAlgorithmBase
    {
        public LiuGangAlgorithmBase()
        {

        }

        List<Tuple<string, decimal>> _Results;

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers
        {
            get
            {
                return Enumerable.Empty<GeneratedNumeber>();
            }
        }

        /// <summary>
        /// 最终药物结果列表。
        /// </summary>
        public override List<Tuple<string, decimal>> Results
        {
            get
            {
                lock (SyncLocker)
                    if (null == _Results)
                    {
                        var coll = CnDrugCorrection.GetMatches(AllNumbers, (c1, c2) => c1 as CnDrugCorrectionBase);
                        var corrAdd = coll.SelectMany(c => c.CnDrugOfAdd).Select(c => (c.Item1, Math.Abs(c.Item2)));   //取增加的药
                        var corrSub = coll.SelectMany(c => c.CnDrugOfSub).ToLookup(c => c);

                        var added = from tmp in OriginalCnDrugs.Concat(corrAdd)
                                    group tmp by tmp.Item1 into g
                                    select Tuple.Create(g.Key, g.Max(c => c.Item2));
                        _Results = added.ToList();
                        _Results.RemoveAll(c => corrSub.Contains(c.Item1)); //去掉要减去的药

                        var ageTemplate = QuestionTemplates.Where(c => c.Value.QuestionTitle == "年龄");
                        if (ageTemplate.Any())
                        {
                            var ageTemplateId = ageTemplate.First().Value.Id;
                            var ageString = Answers.FirstOrDefault(c => c.TemplateId == ageTemplateId)?.Guts;
                            if (decimal.TryParse(ageString, out decimal age)) //若获得了年龄数据
                            {
                                if (age < 6)   //若是幼儿
                                    _Results = _Results.Select(c => Tuple.Create(c.Item1, c.Item2 / 3)).ToList();
                                else if (age <= 12) //若是儿童
                                    _Results = _Results.Select(c => Tuple.Create(c.Item1, c.Item2 * 2 / 3)).ToList();
                            }
                        }
                    }
                return _Results;
            }
        }

        private List<(LiuGangP1Base, float)> _PingFens;

        /// <summary>
        /// 获取匹配的评分项
        /// </summary>
        public List<(LiuGangP1Base, float)> PingFen
        {
            get
            {
                lock (SyncLocker)
                    if (null == _PingFens)
                    {
                        _PingFens = LiuGangP1.GetMatches(AllNumbers, (c1, c2) => (c1 as LiuGangP1Base, c2)).ToList();
                    }
                return _PingFens;
            }
        }

        List<(string, float)> _ZhenDuan;

        /// <summary>
        /// 获取有效的诊断。按诊断值降序排序。
        /// </summary>
        public List<(string, float)> ZhenDuan
        {
            get
            {
                lock (SyncLocker)
                    if (null == _ZhenDuan)
                    {
                        var coll = from tmp in PingFen.SelectMany(c => c.Item1.Value)
                                   group tmp by tmp.Item1 into g
                                   let val = g.Sum(c => c.Item2)
                                   orderby val descending
                                   select (g.Key, val);
                        _ZhenDuan = new List<(string, float)>(coll);
                        if (2 < _ZhenDuan.Count)    //若有三个或更多
                        {
                            var maxVal = _ZhenDuan.First().Item2; //取最大匹配
                            var val2 = _ZhenDuan.SkipWhile(c => c.Item2 >= maxVal).Take(1);   //第二大
                            switch (_ZhenDuan.Count(c => c.Item2 >= maxVal))
                            {
                                case 1: //1大多次则只取最大
                                    _ZhenDuan.RemoveRange(1, _ZhenDuan.Count - 1);
                                    break;
                                default:    //若有2个或更多并列第一
                                    _ZhenDuan.RemoveAll(c => c.Item2 < maxVal);
                                    break;
                            }
                        }
                    }
                return _ZhenDuan;
            }
        }

        List<(string, decimal)> _OriginalCnDrugs;
        /// <summary>
        /// 获取初始的药物项，此处未增减等做处理。
        /// </summary>
        public List<(string, decimal)> OriginalCnDrugs
        {
            get
            {
                lock (SyncLocker)
                    if (null == _OriginalCnDrugs)
                    {
                        LiuGangCnDrugOutBase lgcd;
                        IEnumerable<(string, decimal)> result = Array.Empty<(string, decimal)>();
                        switch (ZhenDuan.Count)
                        {
                            case 0: //无诊断
                                break;
                            case 1: //单一诊断
                                lgcd = LiuGangCnDrugOut.FirstOrDefault(c => c.Key == ZhenDuan.First().Item1);
                                if (null != lgcd)
                                    result = result.Concat(lgcd.CnDrug1s);
                                break;
                            case 2: //双诊断
                                if (Math.Abs(ZhenDuan[0].Item2 - ZhenDuan[1].Item2) < 0.0001)   //若并列第一
                                {
                                    var tmp = LiuGangCnDrugOut.FirstOrDefault(c => c.Key == ZhenDuan[0].Item1)?.CnDrug3s;
                                    if (null != tmp)
                                        result = result.Concat(tmp);
                                    tmp = LiuGangCnDrugOut.FirstOrDefault(c => c.Key == ZhenDuan[1].Item1)?.CnDrug3s;
                                    if (null != tmp)
                                        result = result.Concat(tmp);
                                }
                                else //一大一小
                                {
                                    var tmp = LiuGangCnDrugOut.FirstOrDefault(c => c.Key == ZhenDuan[0].Item1)?.CnDrug1s;
                                    if (null != tmp)
                                        result = result.Concat(tmp);
                                    tmp = LiuGangCnDrugOut.FirstOrDefault(c => c.Key == ZhenDuan[1].Item1)?.CnDrug2s;
                                    if (null != tmp)
                                        result = result.Concat(tmp);
                                }
                                break;
                            default:    //1大多次大仅取最大，所以此处必是n个同样最大
                                result = result.Concat(LiuGangCnDrugOut.Join(ZhenDuan, c => c.Key, c => c.Item1, (c1, c2) => c1.CnDrug3s).SelectMany(c => c));
                                break;
                        }
                        var coll = from tmp in result
                                   group tmp.Item2 by tmp.Item1 into g
                                   select (g.Key, g.Max());
                        _OriginalCnDrugs = new List<(string, decimal)>(coll);
                    }
                return _OriginalCnDrugs;
            }
        }


        List<LiuGangP1Base> _LiuGangP1;
        /// <summary>
        /// 获取评分表。
        /// </summary>
        virtual public IEnumerable<LiuGangP1Base> LiuGangP1
        {
            get
            {
                lock (SyncLocker)
                    if (null == _LiuGangP1)
                    {
                        _LiuGangP1 = CnMedicineLogicBase.GetOrCreateAsync<LiuGangP1Base>($"~/{GetDataFilePath(GetType())}/{GetCnName(GetType())}-评分表.txt").Result;
                    }
                return _LiuGangP1;
            }
        }

        List<LiuGangCnDrugOutBase> _LiuGangCnDrugOut;
        /// <summary>
        /// 获取症型结论表。
        /// </summary>
        virtual public IEnumerable<LiuGangCnDrugOutBase> LiuGangCnDrugOut
        {
            get
            {
                lock (SyncLocker)
                    if (null == _LiuGangCnDrugOut)
                    {
                        _LiuGangCnDrugOut = CnMedicineLogicBase.GetOrCreateAsync<LiuGangCnDrugOutBase>($"~/{GetDataFilePath(GetType())}/{GetCnName(GetType())}-症型结论表.txt").Result;
                    }
                return _LiuGangCnDrugOut;
            }
        }

        List<CnDrugCorrectionBase> _CnDrugCorrection;
        /// <summary>
        /// 获取药物加减表。
        /// </summary>
        virtual public IEnumerable<CnDrugCorrectionBase> CnDrugCorrection
        {
            get
            {
                lock (SyncLocker)
                    if (null == _CnDrugCorrection)
                    {
                        _CnDrugCorrection = CnMedicineLogicBase.GetOrCreateAsync<CnDrugCorrectionBase>($"~/{GetDataFilePath(GetType())}/{GetCnName(GetType())}-加减表.txt").Result;
                    }
                return _CnDrugCorrection;
            }
        }

        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var cnName = GetCnName(GetType());
            var sy = db.SurveysTemplates.Where(c => c.Name == cnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            else //若有病的则填写诊断说明
            {
                switch (ZhenDuan.Count)
                {
                    case 1:
                    case 3:
                        result.ExtendedInfomation = $"{string.Join(",", ZhenDuan.Select(c => c.Item1))}";
                        break;
                    case 2:
                        if (ZhenDuan[0].Item2 > ZhenDuan[1].Item2) //若是主从
                            result.ExtendedInfomation = $"主症:{ZhenDuan[0].Item1},兼{ZhenDuan[1].Item1}";
                        else
                            result.ExtendedInfomation = $"{string.Join(",", ZhenDuan.Select(c => c.Item1))}";
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

    }
}