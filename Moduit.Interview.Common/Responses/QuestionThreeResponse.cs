using System;
using System.Collections.Generic;

namespace Moduit.Interview.Common.Responses
{
    public class QuestionThreeResponse
    {
        public int Id { get; set; }
        public int Category { get; set; }
        public IList<ItemResponse> Items { get; set; }
        public IList<string> Tags { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ItemResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Footer { get; set; }
    }
}
