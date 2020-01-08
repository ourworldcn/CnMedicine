using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CnMedicineServer.Models
{
    /// <summary>
    /// 向平台发送专病诊断结论的数据模型。
    /// </summary>
    [DataContract]
    public class SaveConclusioModel
    {
        /*
         * name	姓名	string		是
         * sex	性别	string		否
         * age	年龄	string		否
         * mobile	电话号码	string		是
         * result	问卷结果	string	问卷结果以JSON格式的字符串传输	是
         */

        /// <summary>
        /// 构造函数。
        /// </summary>
        public SaveConclusioModel()
        {

        }

        public SaveConclusioModel(Surveys surveys, SurveysConclusion conclusion, DbContext db)
        {
            Conclusion = conclusion;
            var template = db.Set<SurveysTemplate>().Find(surveys.TemplateId);
            UserId = surveys.UserId;
            var question = template.Questions.FirstOrDefault(c => c.QuestionTitle == "姓名");
            if (null != question)
            {
                Name = surveys.SurveysAnswers.FirstOrDefault(c => c.TemplateId == question.Id)?.Guts;
            }
            question = template.Questions.FirstOrDefault(c => c.QuestionTitle == "手机号");
            if (null != question)
            {
                Mobile = surveys.SurveysAnswers.FirstOrDefault(c => c.TemplateId == question.Id)?.Guts;
            }
            question = template.Questions.FirstOrDefault(c => c.QuestionTitle == "性别");
            if (null != question)
            {
                Sex = surveys.SurveysAnswers.FirstOrDefault(c => c.TemplateId == question.Id)?.Guts;
            }
        }

        [DataMember(IsRequired = true, Name = "name")]
        [MyValidation]
        [Required]
        public string Name { get; set; }

        [DataMember(IsRequired = false, Name = "sex")]
        public string Sex { get; set; }

        [DataMember(IsRequired = false, Name = "age")]
        public string Age { get; set; }

        [DataMember(IsRequired = true, Name = "mobile")]
        [Required]
        public string Mobile { get; set; }

        [DataMember(IsRequired = true, Name = "result")]
        public SurveysConclusion Conclusion { get; set; }

        [DataMember(IsRequired = true, Name = "userId")]
        public string UserId { get; set; }
    }
}