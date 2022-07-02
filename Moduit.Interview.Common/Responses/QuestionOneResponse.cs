using System;
using System.Collections.Generic;

namespace Moduit.Interview.Common.Responses
{
    public class QuestionOneResponse
    {
        public int Id { get; set; }
        public int Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Footer { get; set; }
        public IList<string> Tags { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
