using Moduit.Interview.Common.Responses;
using System.Collections.Generic;

namespace Moduit.Interview.Service
{
    public interface IModuitIntegrationService
    {
        QuestionOneResponse GetQuestionOne();
        IList<QuestionTwoResponse> GetQuestionTwo();
        IList<QuestionOneResponse> GetQuestionThree();
    }
}
