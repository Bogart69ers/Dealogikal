using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dealogikal.Database;
using Dealogikal.Utils;

namespace Dealogikal.Repository
{
    public class FeedbackManager
    {

        private BaseRepository<feedback> _feedback;


        public FeedbackManager()
        {
            _feedback = new BaseRepository<feedback>();
        }

        public List<feedback> GetAllDtrDesc()
        {
            return _feedback.GetAll()
                            .OrderBy(l => l.status != 0) // status 0 first (false < true)
                            .ThenByDescending(l => l.dateCreated) // newest dateFiled first
                            .ToList();
        }

        public List<feedback> GetAllDtr()
        {
            return _feedback.GetAll();
        }

        public ErrorCode CreateFeedback(feedback fb, ref string errMsg)
        {
            try
            {

                if (_feedback.Create(fb, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }

                return ErrorCode.Success;

            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return ErrorCode.Error;
            }
        }
    }
}