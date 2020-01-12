using CnMedicineServer.Models;
using OW.Data.Entity;

namespace CnMedicineServer.Bll
{
    public abstract class CnMedicineAlgorithm
    {
        protected abstract SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db);

        public SurveysConclusion GetResult(Surveys surveys, ApplicationDbContext db)
        {
            if(null== surveys.Template)
                surveys.Template =db.SurveysTemplates.Find(surveys.TemplateId);
            return GetResultCore(surveys, db);
        }
    }
}