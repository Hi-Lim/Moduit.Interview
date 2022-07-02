using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Moduit.Interview.Common.Commands;
using Moduit.Interview.Common.Responses;
using QSI.Common.Api.AspNetCore.Impl.HttpClientImpl;
using QSI.Common.HttpClientMessage;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Moduit.Interview.Service.Impl
{
    public class ModuitIntegrationServiceImpl : HttpClientImpl, IModuitIntegrationService
    {
        private readonly ModuitConfiguration moduitConfiguration;

        public ModuitIntegrationServiceImpl(ModuitConfiguration moduitConfiguration, IHttpContextAccessor httpContext) : base(httpContext)
        {
            this.moduitConfiguration = moduitConfiguration;
        }

        public QuestionOneResponse GetQuestionOne()
        {
            QuestionOneResponse questionOneResponse = null;
            var endpoint = $"{moduitConfiguration.ClientUrl}/backend/question/one";

            HttpClientResult response = this.Get("Get Question One From Moduit", null, $"{endpoint}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                questionOneResponse = JsonConvert.DeserializeObject<QuestionOneResponse>(response.Result);
            return questionOneResponse;
        }

        public IList<QuestionOneResponse> GetQuestionThree()
        {
            IList<QuestionOneResponse> questionOneResponses = new List<QuestionOneResponse>();
            var endpoint = $"{moduitConfiguration.ClientUrl}/backend/question/three";

            HttpClientResult response = this.Get("Get Question Three From Moduit", null, $"{endpoint}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IList<QuestionThreeResponse> questionThreeResponses = JsonConvert.DeserializeObject<IList<QuestionThreeResponse>>(response.Result);
                foreach (var item in questionThreeResponses)
                {
                    if (item?.Items?.Count > 0)
                    {
                        foreach (var cItem in item.Items)
                        {
                            QuestionOneResponse questionOneResponse = new QuestionOneResponse()
                            {
                                Id = item.Id,
                                Category = item.Category,
                                Title = cItem.Title,
                                Description = cItem.Description,
                                Footer = cItem.Footer,
                                Tags = item.Tags,
                                CreatedAt = item.CreatedAt
                            };

                            questionOneResponses.Add(questionOneResponse);
                        }
                    }
                }
            }

            return questionOneResponses;
        }

        public IList<QuestionTwoResponse> GetQuestionTwo()
        {
            IList<QuestionTwoResponse> questionTwoResponses = null;
            var endpoint = $"{moduitConfiguration.ClientUrl}/backend/question/two";

            HttpClientResult response = this.Get("Get Question Two From Moduit", null, $"{endpoint}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                questionTwoResponses = JsonConvert.DeserializeObject<IList<QuestionTwoResponse>>(response.Result);
                var query = questionTwoResponses
                    .Where(x =>
                        (
                            x.Title.ToLower().Contains("ergonomics") || x.Description.ToLower().Contains("ergonomics")
                        ) &&
                            x.Tags.Contains("Sports")
                    )
                    .OrderByDescending(x => x.Id)
                    .Skip(Math.Max(0, questionTwoResponses.Count() - 3))
                    .ToList();
                questionTwoResponses = query;
            }

            return questionTwoResponses;
        }
    }
}
